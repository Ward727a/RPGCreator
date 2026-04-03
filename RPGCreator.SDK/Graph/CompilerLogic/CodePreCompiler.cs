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

/// <summary>
/// This pre-compiler is used to PREPARE a blueprint for compilation.
/// It sorts the nodes in a topological order, and removes unused nodes (no inputs or outputs).
/// </summary>
public static class CodePreCompiler
{
    public static (bool success, INodeLogic? startNode, Dictionary<Ulid, int> inDegree, Dictionary<Ulid, NodeExecutionMetadata> metadata) SortConnectors(BlueprintCompilerData data)
    {
        INodeLogic? startNode = null;
        var sortedNodes = new List<INodeLogic>();
        var nodes = data.Nodes.ToDictionary(n => n.RuntimeId, n => n);
        
        var inDegree = data.Nodes.ToDictionary(
            n => n.RuntimeId, 
            n => n.Inputs.Where(i => i is ExecConnectorLogic).Count(i => i.RuntimeHasParent)
        );
        
        var copyInDegree = inDegree.ToDictionary(kv => kv.Key, kv => kv.Value);

        var queue = new Queue<INodeLogic>(data.Nodes.Where(n => inDegree[n.RuntimeId] == 0));

        var outputToChildren = new Dictionary<Ulid, List<Ulid>>();
        foreach (var n in data.Nodes)
        {
            foreach (var input in n.Inputs.Where(i => i.RuntimeHasParent))
            {
                if (!outputToChildren.TryGetValue(input.RuntimeParentConnector, out var list))
                    outputToChildren[input.RuntimeParentConnector] = list = new List<Ulid>();
                list.Add(n.RuntimeId);
            }
        }

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            sortedNodes.Add(current);
            
            if(current.Urn == "bp_nodes".ToUrnSingleModule().ToUrnModule("rpgc").ToUrn("start_node") && startNode == null)
                startNode = current;
            
            foreach (var output in current.Outputs)
            {
                if (outputToChildren.TryGetValue(output.RuntimeId, out var childrenIds))
                {
                    foreach (var childId in childrenIds)
                    {
                        inDegree[childId]--;

                        if (inDegree[childId] == 0)
                        {
                            queue.Enqueue(nodes[childId]);
                        }
                    }
                }
            }
        }
        
        var unused = data.Nodes.Except(sortedNodes).Where(n => n.Inputs.Count == 0).ToList();
        foreach(var node in unused) {
            Logger.Warning($"Node [{node.Title}]({node.RuntimeId}) is unused (has no inputs or outputs), and will be ignored.");
        }
        
        if (sortedNodes.Count < data.Nodes.Count - unused.Count)
        {
            throw new Exception("Not all nodes were sorted - Infinite loop?");
        }

        if (startNode == null)
        {
            Logger.Error("Start node not found in the graph, cannot compile.");
            return (false, null, new Dictionary<Ulid, int>(),new());
        }
        
        var metadataMap = new Dictionary<Ulid, NodeExecutionMetadata>();

        foreach (var node in data.Nodes)
        {
            var meta = new NodeExecutionMetadata()
            {
                RuntimeId = node.RuntimeId,
                FlowType = node.FlowType
            };

            foreach (var output in node.Outputs)
            {
                var targetNodeId = data.Connections.FirstOrDefault(c => c.SourceConnectorId == output.RuntimeId)?.TargetNodeId ?? Ulid.Empty;
                meta.OutputConnections[output.RuntimeId] = targetNodeId;
            }
            
            foreach (var input in node.Inputs)
            {
                var connectionLogic = data.Connections.FirstOrDefault(c => c.TargetConnectorId == input.RuntimeId)?.SourceNodeId ?? Ulid.Empty;
                meta.InputConnections[input.RuntimeId] = connectionLogic;
            }

            metadataMap.Add(node.RuntimeId, meta);
        }
        
        foreach (var meta in metadataMap.Values.Where(m => m.FlowType == FlowType.Conditional))
        {
            meta.JunctionNodeId = FindJunctionPoint(meta, metadataMap);
        }

        return (true, startNode, copyInDegree, metadataMap);
    }
    
    private static Ulid? FindJunctionPoint(NodeExecutionMetadata startNode, Dictionary<Ulid, NodeExecutionMetadata> map)
    {
        if (startNode.OutputConnections.Count < 2) return null;

        var branches = startNode.OutputConnections.Values.ToList();
    
        var paths = new List<HashSet<Ulid>>();

        foreach (var branchStartNodeId in branches)
        {
            var path = new HashSet<Ulid>();
            var currentId = branchStartNodeId;

            while (currentId != Ulid.Empty && map.TryGetValue(currentId, out var meta))
            {
                if (!path.Add(currentId)) break;
                if (meta.FlowType == FlowType.Terminal) break;

                currentId = meta.OutputConnections.Values.FirstOrDefault();
            }
            paths.Add(path);
        }

        foreach (var nodeId in paths[0])
        {
            if (paths.Skip(1).All(p => p.Contains(nodeId)))
            {
                return nodeId;
            }
        }

        return null;
    }
}