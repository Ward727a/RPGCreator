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

using System.CodeDom.Compiler;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Graph.CompilerLogic;
using RPGCreator.SDK.Graph.LOGIC;

namespace RPGCreator.SDK.Graph;


public class CodeContext
{
    public List<Ulid> GeneratedNodes { get; } = new();
    public Dictionary<Ulid, INodeLogic> Nodes { get; } = new();
    public Dictionary<Ulid, IConnectorLogic> Connectors { get; } = new();
    public Dictionary<Ulid, string> VariablesName { get; } = new();
    
    /// <summary>
    /// Global parameters of the blueprint.<br/>
    /// This can be used to pass data to the blueprint and then be used by the whole blueprint.
    /// </summary>
    public List<BlueprintParameters> Parameters { get; } = new();
    
    public bool InPureMode { get; private set; } = true;
    public bool InDebugMode { get; internal set; } = false;

    public Action<string> AddUsing { get; internal set; } = s => { };
    
    private Dictionary<Ulid, NodeExecutionMetadata> _metadataMap = new();
    
    public CodeContext(BlueprintCompilerData data, Dictionary<Ulid, NodeExecutionMetadata> metadataMap)
    {
        _metadataMap = metadataMap;
        
        var nodeList = data.Nodes.ToList();
        foreach (var node in nodeList)
        {
            Nodes.Add(node.RuntimeId, node);
            foreach (var output in node.Outputs)
            {
                Connectors.Add(output.RuntimeId, output);
            }
            foreach (var input in node.Inputs)
            {
                Connectors.Add(input.RuntimeId, input);
            }
        }
    }
    
    public void GenerateCodeFromOutput(CodeContext context, IndentedTextWriter writer, Ulid outputId)
    {
        if (_metadataMap.TryGetValue(Connectors[outputId].RuntimeNodeId, out var meta) && meta.OutputConnections.TryGetValue(outputId, out var targetNodeId))
        {
            GenerateCode(context, writer, targetNodeId);
        }
    }

    private int _branchCounter = 0;
    
    public void EnterBranch()
    {
        _branchCounter++;
    }

    public void ExitBranch()
    {
        if(_branchCounter <= 0)
        {
            Logger.Error("ERROR: Attempted to exit branch without entering one. This should not happen.");
            return;
        }
        _branchCounter--;
    }
    
    public bool IsInsideBranch => _branchCounter > 0;
    private Stack<Ulid> _junctionStack = new();
    public void GenerateCode(CodeContext context, IndentedTextWriter writer, Ulid nodeId)
    {
        if (!Nodes.TryGetValue(nodeId, out var node) || !_metadataMap.TryGetValue(nodeId, out var meta))
            return;
        
        if (_junctionStack.Count > 0 && _junctionStack.Peek() == nodeId)
        {
            return; 
        }
        
        if (GeneratedNodes.Contains(nodeId)) return;

        GeneratedNodes.Add(nodeId);
    

        switch (meta.FlowType)
        {
            case FlowType.Linear:
                node.GenerateCode(context, writer);
                var next = meta.OutputConnections.Values.FirstOrDefault();
                if (next != Ulid.Empty) GenerateCode(context, writer, next);
                break;

            case FlowType.Conditional:
                if (meta.JunctionNodeId.HasValue)
                    _junctionStack.Push(meta.JunctionNodeId.Value);
                
                node.GenerateCode(context, writer);

                if (meta.JunctionNodeId.HasValue)
                {
                    _junctionStack.Pop();
                    GenerateCode(context, writer, meta.JunctionNodeId.Value);
                }
                break;

            case FlowType.Terminal:
                node.GenerateCode(context, writer);
                break;
        }
    }

    public void GeneratePure(CodeContext context, IndentedTextWriter writer, Ulid nodeId)
    {

        if (GeneratedNodes.Contains(nodeId))
            return;
        
        if (_metadataMap.TryGetValue(nodeId, out var nodeMeta))
        {
            if (nodeMeta.InputConnections.Count > 0)
            {
                foreach (var input in nodeMeta.InputConnections)
                {
                    if(input.Value == Ulid.Empty || GeneratedNodes.Contains(input.Value))
                        continue;
                    GeneratePure(context, writer, input.Value);
                }
            }
        }
        
        if (!Nodes.TryGetValue(nodeId, out var node))
        {
            Logger.Error("FATAL ERROR: Node with runtime ID '{0}' not found in context", nodeId);
            return;
        }
        
        GeneratePure(context, writer,node);
    }
    
    public void GeneratePure(CodeContext context, IndentedTextWriter writer, INodeLogic node)
    {
        if (!InPureMode)
        {
            Logger.Error("Cannot generate pure code outside of pure mode.");
            Logger.Error("Please, do not use GeneratePure method if you already called GenerateCode even once (as it exit pureMode instantly)");
            return;
        }
        
        if(node.Inputs.Any(i => i.ValueType == typeof(void)) || node.Outputs.Any(o => o.ValueType == typeof(void)))
        {
            Logger.Error("Cannot generate pure code for node with execution input or output.");
            return;
        }
        
        node.GenerateCode(this, writer);
        GeneratedNodes.Add(node.RuntimeId);
    }
    
    public void SetVariableName(Ulid id, string value)
    {
        VariablesName[id] = value;
    }
    
    public void GetVariableName(Ulid id, out string value)
    {
        value = VariablesName[id];
    }
}