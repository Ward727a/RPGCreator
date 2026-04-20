// RPG Creator - Open-source RPG Engine.
// (c) 2026 Ward
// 
// This file is part of RPG Creator and is distributed under the Apache 2.0 License.
// You are free to use, modify, and distribute this file under the terms of the Apache 2.0 License.
// See LICENSE for details.
// 
// ---
// 
// Ce fichier fait partie de RPG Creator et est distribué sous licence Apache 2.0.
// Vous êtes libre de l'utiliser, de le modifier et de le distribuer sous les termes de la licence Apache 2.0.
// Voir LICENSE pour plus de détails.
// 
// Contact:
// => Mail: Ward727a@gmail.com
//    Please use this object: "RPG Creator [YourObject]"
// => Discord: ward727
// 
// For urgent inquiries, sending both an email and a message on Discord is highly recommended for a quicker response.

using System.Diagnostics.CodeAnalysis;
using RPGCreator.SDK.Graph.ConnectorLogics;
using RPGCreator.SDK.Graph.LOGIC;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Types;

namespace RPGCreator.SDK.Graph.CompilerLogic;

public class NodeExecutionMetadata
{
    public Ulid RuntimeId { get; set; }
    public FlowType FlowType { get; set; }
    public Ulid? JunctionNodeId { get; set; } // For CONDITIONAL flow type

    /// <summary>
    /// ConnectorId -> TargetNodeId
    /// </summary>
    public Dictionary<Ulid, Ulid> OutputConnections { get; } = new(); // ConnectorId -> TargetNodeId

    /// <summary>
    /// ConnectorId -> SourceNodeId
    /// </summary>
    public Dictionary<Ulid, Ulid> InputConnections { get; } = new();

    /// <summary>
    /// Return the next node to execute, based on the flow type.
    /// </summary>
    /// <returns>
    /// Returns the next node to execute, based on the flow type.
    /// Or null if no default next node is available.
    /// </returns>
    public Ulid? GetDefaultNextNode()
    {
        if (FlowType == FlowType.Linear)
            return OutputConnections.Values.FirstOrDefault();

        return JunctionNodeId;
    }
}

public record struct CompilationSortingResults(
    bool Success,
    INodeLogic? StartNode,
    Dictionary<Ulid, int> InitialInDegree,
    Dictionary<Ulid, NodeExecutionMetadata> Metadata);


public static class GraphExtensions
{
    public static IEnumerable<T> TraverseBfs<T>(this T root, Func<T, IEnumerable<T>> getChildren)
    {
        var queue = new Queue<T>();
        var visited = new HashSet<T>();

        queue.Enqueue(root);
        visited.Add(root);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            
            yield return current;

            foreach (var child in getChildren(current))
            {
                if (visited.Add(child)) 
                {
                    queue.Enqueue(child);
                }
            }
        }
    }
}

/// <summary>
/// This pre-compiler is used to PREPARE a blueprint for compilation.
/// It sorts the nodes in a topological order, and removes unused nodes (no inputs or outputs).
/// </summary>
public static class CodePreCompiler
{
    public static CompilationSortingResults SortConnectors(BlueprintCompilerData data)
    {
        var connectionsByOutput = data.Connections.ToLookup(c => c.SourceConnectorId);
        var connectionsByInput = data.Connections.ToDictionary(c => c.TargetConnectorId, c => c);

        var sortedNodes = new List<INodeLogic>();
        INodeLogic? startNode = null;
        if (!TryTopologicalSort(data, out startNode, out var initialInDegrees, out sortedNodes))
        {
            return new CompilationSortingResults(false, null, initialInDegrees, new());
        }
        
        if(!ValidateGraph(data, sortedNodes, startNode))
        {
            return new CompilationSortingResults(false, null, initialInDegrees, new());
        }
        
        var metadataMap = BuildMetadataMap(data, connectionsByOutput, connectionsByInput);
        
        ResolveJunctionPoints(metadataMap);
        
        
        
        #if DEBUG
                
        PrintGraph(data);
        PrintMetadata(data, metadataMap);
        PrintGraphvizDot(data, metadataMap);
                
        #endif

        return new CompilationSortingResults(true, startNode, initialInDegrees, metadataMap);
    }

    private static bool TryTopologicalSort(
        BlueprintCompilerData data, 
        [NotNullWhen(true)] out INodeLogic? startNode,
        out Dictionary<Ulid, int> initialInDegrees, 
        out List<INodeLogic> sortedNodes)
    {
        startNode = null;
        sortedNodes = new List<INodeLogic>(data.Nodes.Count);
        
        var inDegree = data.Nodes.ToDictionary(
            n => n.RuntimeId, 
            n => n.Inputs.Count(i => i is ExecConnectorLogic && i.RuntimeHasParent)
        );
        
        initialInDegrees = new Dictionary<Ulid, int>(inDegree);
        
        var queue = new Queue<INodeLogic>(data.Nodes.Where(n => inDegree[n.RuntimeId] == 0));
        var ouputChildren = BuildChildMap(data);
        
        var startNodeUrn = "bp_nodes".ToUrnSingleModule().ToUrnModule("rpgc").ToUrn("start_node");

        while (queue.TryDequeue(out var current))
        {
            sortedNodes.Add(current);

            if (startNode == null && current.Urn == startNodeUrn)
            {
                startNode = current;
            }
            
            foreach (var output in current.Outputs)
            {
                if (!ouputChildren.TryGetValue(output.RuntimeId, out var childrenIds)) continue;
                
                foreach (var childId in childrenIds)
                {
                    inDegree[childId]--;
                    if (inDegree[childId] == 0)
                    {
                        queue.Enqueue(data.Nodes.First(n => n.RuntimeId == childId));
                    }
                }
            }
        }

        return startNode != null;
    }
    
    #if DEBUG
    private static void PrintGraph(BlueprintCompilerData data)
    {
        Logger.Debug("--- Raw Graph Structure ---");
        foreach (var node in data.Nodes)
        {
            var outputTitles = node.Outputs
                .SelectMany(o => data.Connections.Where(c => c.SourceConnectorId == o.RuntimeId))
                .Select(c => data.Nodes.FirstOrDefault(n => n.RuntimeId == c.TargetNodeId)?.Title ?? "Unknown")
                .ToList();

            Logger.Debug($"[{node.Title}] -> {(outputTitles.Count > 0 ? string.Join(", ", outputTitles) : "End")}");
        }
    }

    private static void PrintMetadata(BlueprintCompilerData data, Dictionary<Ulid, NodeExecutionMetadata> metadataMap)
    {
        Logger.Debug("--- Execution Metadata (Resolved) ---");
    
        var titles = data.Nodes.ToDictionary(n => n.RuntimeId, n => n.Title);

        foreach (var (nodeId, meta) in metadataMap)
        {
            var title = titles.GetValueOrDefault(nodeId, "Unknown");
            var nextNodes = meta.OutputConnections.Values
                .Where(id => id != Ulid.Empty)
                .Select(id => titles.GetValueOrDefault(id, id.ToString()))
                .ToList();

            var junctionInfo = meta.JunctionNodeId.HasValue && meta.JunctionNodeId.Value != Ulid.Empty
                ? $" [Junction => {titles.GetValueOrDefault(meta.JunctionNodeId.Value, "Unknown")}]"
                : "";

            var flowTypeStr = meta.FlowType == FlowType.Conditional ? "[CONDITIONAL]" : "";

            Logger.Debug($"{flowTypeStr} [{title}]{junctionInfo} -> {(nextNodes.Count > 0 ? string.Join(" | ", nextNodes) : "End")}");
        }  
    }
    
    /// <summary>
    /// Generate a Graphviz DOT file for the graph.
    /// Copy/paste in https://dreampuf.github.io/GraphvizOnline/ to visualize the graph structure.
    /// </summary>
    private static void PrintGraphvizDot(BlueprintCompilerData data, Dictionary<Ulid, NodeExecutionMetadata> metadataMap)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("digraph Blueprint {");
        sb.AppendLine("  node [fontname=\"Arial\", shape=box, style=filled, fillcolor=\"#f0f0f0\"];");
        sb.AppendLine("  edge [fontname=\"Arial\"];");

        var titles = data.Nodes.ToDictionary(n => n.RuntimeId, n => n.Title.Replace("\"", "\\\""));

        foreach (var node in data.Nodes)
        {
            var meta = metadataMap[node.RuntimeId];
            string shape = meta.FlowType == FlowType.Conditional ? "diamond" : "box";
            string color = meta.FlowType == FlowType.Conditional ? "\"#fff2cc\"" : "\"#e1d5e7\"";
        
            sb.AppendLine($"  \"{node.RuntimeId}\" [label=\"{titles[node.RuntimeId]}\", shape={shape}, fillcolor={color}];");
        }

        foreach (var (nodeId, meta) in metadataMap)
        {
            foreach (var (connectorId, targetId) in meta.OutputConnections)
            {
                if (targetId != Ulid.Empty)
                {
                    sb.AppendLine($"  \"{nodeId}\" -> \"{targetId}\";");
                }
            }
        
            if (meta.JunctionNodeId.HasValue && meta.JunctionNodeId.Value != Ulid.Empty)
            {
                sb.AppendLine($"  \"{nodeId}\" -> \"{meta.JunctionNodeId.Value}\" [style=dotted, color=red, label=\"Junction\"];");
            }
        }

        sb.AppendLine("}");

        Logger.Debug("---------- GRAPHVIZ DOT (Copy/Paste to a Graphviz viewer) ---------");
        Logger.Debug("--- Graphviz Online: https://dreampuf.github.io/GraphvizOnline/ ---");
        Logger.Debug("\n" + sb.ToString() + "\n");
    }
    
    #endif
    
    private static bool ValidateGraph(BlueprintCompilerData data, List<INodeLogic> sortedNodes, INodeLogic? startNode)
    {
        var unused = data.Nodes.Except(sortedNodes).Where(n => n.Inputs.Count == 0).ToList();
        foreach(var node in unused) 
        {
            Logger.Warning($"Node [{node.Title}]({node.RuntimeId}) is unused and will be ignored.");
        }
    
        if (sortedNodes.Count < data.Nodes.Count - unused.Count)
        {
            Logger.Error("Not all nodes were sorted - Infinite loop detected in Blueprint!");
            return false;
        }

        if (startNode == null)
        {
            Logger.Error("Start node not found in the graph, cannot compile.");
            return false;
        }

        return true;
    }
    
    private static Dictionary<Ulid, List<Ulid>> BuildChildMap(BlueprintCompilerData data)
    {
        var map = new Dictionary<Ulid, List<Ulid>>();
        foreach (var n in data.Nodes)
        {
            foreach (var input in n.Inputs.Where(i => i.RuntimeHasParent))
            {
                if (!map.TryGetValue(input.RuntimeParentConnector, out var list))
                {
                    list = new List<Ulid>();
                    map[input.RuntimeParentConnector] = list;
                }
                list.Add(n.RuntimeId);
            }
        }
        return map;
    }

    private static Dictionary<Ulid, NodeExecutionMetadata> BuildMetadataMap(BlueprintCompilerData data,
        ILookup<Ulid, ConnectionLogic> connectionByOutput,
        Dictionary<Ulid, ConnectionLogic> connectionByInput)
    {
        var metaDataMap = new Dictionary<Ulid, NodeExecutionMetadata>();

        foreach (var node in data.Nodes)
        {
            var meta = new NodeExecutionMetadata()
            {
                RuntimeId = node.RuntimeId,
                FlowType = node.FlowType
            };

            foreach (var output in node.Outputs)
            {
                var targetNodeId = connectionByOutput[output.RuntimeId].FirstOrDefault()?.TargetNodeId ?? Ulid.Empty;
                meta.OutputConnections[output.RuntimeId] = targetNodeId;
            }

            foreach (var input in node.Inputs)
            {
                var sourceNodeId = connectionByInput.TryGetValue(input.RuntimeId, out var connection) ? connection.SourceNodeId : Ulid.Empty;
                meta.InputConnections[input.RuntimeId] = sourceNodeId;
            }
            
            metaDataMap.Add(node.RuntimeId, meta);
        }
        
        
        return metaDataMap;
    }
    
    private static void ResolveJunctionPoints(Dictionary<Ulid, NodeExecutionMetadata> metadataMap)
    {
        foreach (var meta in metadataMap.Values.Where(m => m.FlowType == FlowType.Conditional))
        {
            meta.JunctionNodeId = FindJunctionPoint(meta, metadataMap);
        }
    }
    
    private static Ulid? FindJunctionPoint(
    NodeExecutionMetadata startNodeMeta, 
    Dictionary<Ulid, NodeExecutionMetadata> metadataMap)
    {
        var branchesStartIds = startNodeMeta.OutputConnections.Values.ToList();

        if (branchesStartIds.Count < 2) return null;

        Func<Ulid, IEnumerable<Ulid>> getChildren = currentId =>
        {
            if (metadataMap.TryGetValue(currentId, out var meta) && meta.FlowType != FlowType.Terminal)
                return meta.OutputConnections.Values;
                
            return Enumerable.Empty<Ulid>();
        };

        var branchesReachableNodes = new List<HashSet<Ulid>>();
        
        foreach (var branchStartId in branchesStartIds)
        {
            var reachableInBranch = branchStartId.TraverseBfs(getChildren).ToHashSet();
            branchesReachableNodes.Add(reachableInBranch);
        }

        var junctionId = branchesStartIds[0]
            .TraverseBfs(getChildren)
            .FirstOrDefault(nodeId => 
            {
                return branchesReachableNodes.Skip(1).All(otherBranch => otherBranch.Contains(nodeId));
            });

        return junctionId != Ulid.Empty ? junctionId : null;
    }
}