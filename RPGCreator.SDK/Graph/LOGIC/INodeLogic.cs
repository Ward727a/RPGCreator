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
using RPGCreator.SDK.Modules.Definition;
using RPGCreator.SDK.Types;

namespace RPGCreator.SDK.Graph.LOGIC;

public enum FlowType
{
    /// <summary>
    /// Has ONE OR MORE execution input, and exactly ONE execution output.<br/>
    /// For example: 'Print Node', 'start node', 'combine exec node'<br/>
    /// The start node is a special case of this type, as it's the first node in a graph and has no incoming connections, but will be executed at first.
    /// </summary>
    Linear,
    /// <summary>
    /// Has exactly ONE execution input, and TWO OR MORE execution outputs. Each execution output CAN join together<br/>
    /// For example: 'If Node', 'Switch Node'
    /// </summary>
    Conditional,
    /// <summary>
    /// Has exactly ONE execution input, and exactly TWO execution outputs. The first execution output DO NOT JOIN the second execution output.<br/>
    /// For example: 'While node'
    /// </summary>
    Loop,
    /// <summary>
    /// Has exactly ONE execution input and ZERO execution outputs.<br/>
    /// For example: 'End Node'
    /// </summary>
    Terminal,
    /// <summary>
    /// Has exactly ZERO execution inputs, and ZERO execution outputs.<br/>
    /// For example: 'Add Integer node', 'Equal Integer Node'
    /// </summary>
    Pure
}

public interface INodeLogic
{
    
    public FlowType FlowType { get; }
    
    /// <summary>
    /// Please do not touch this property.<br/>
    /// The engine uses this property in an internal process while compiling the graph.
    /// </summary>
    public Ulid RuntimeId { get; internal set; }
    
    /// <summary>
    /// Inputs available to this node.
    /// </summary>
    public IReadOnlyList<IConnectorLogic> Inputs { get; internal set; }
    
    /// <summary>
    /// Outputs available from this node.
    /// </summary>
    public IReadOnlyList<IConnectorLogic> Outputs { get; internal set; }
    
    /// <summary>
    /// Title of the node.
    /// </summary>
    public string Title { get; }
    
    /// <summary>
    /// Category of the node.
    /// </summary>
    public PipedPath Category { get; }
    
    /// <summary>
    /// Unique identifier of the node.<br/>
    /// You should set this property to a unique value for each node, this will be used to register your node in the engine registry.
    /// </summary>
    public URN Urn { get; }
    
    /// <summary>
    /// OPTIONAL: URN of the help page for this node.<br/>
    /// If set, this will be used to open the help page when the user clicks on the '?' icon next to the node title.
    /// </summary>
    public URN HelpUrn { get; }
    
    /// <summary>
    /// Custom data that can be used by the node.<br/>
    /// It can store specific node-related data, such as configuration settings or additional information.
    /// </summary>
    public CustomData CustomData { get; set; }

    /// <summary>
    /// This should return a valid C# code snippet, that will be executed when the node is executed.
    /// </summary>
    /// <param name="context">The code generation context, providing necessary information for code generation.</param>
    /// <param name="writer">The IndentedTextWriter to write the generated code to.</param>
    public void GenerateCode(CodeContext context, IndentedTextWriter writer);

    void PlacedInGraph(object? nodeEditorViewModel);
    
    public INodeLogic Clone();
}