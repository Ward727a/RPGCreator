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
using System.Collections.Generic;
using RPGCreator.SDK.Graph;
using RPGCreator.SDK.Graph.ConnectorLogics;
using RPGCreator.SDK.Graph.LOGIC;
using RPGCreator.SDK.Types;

namespace RPGCreator.UI.Blueprints.Nodes.FlowControl;

public class CombineExecNode : BaseNodeLogic
{
    public override FlowType FlowType => FlowType.Linear;
    public override string Title { get; } = "Combine Execution";
    public override PipedPath Category { get; } = "Flow Control".ToPipedPath();
    public override URN Urn { get; } = DefaultUrnModule.ToUrnModule("rpgc").ToUrn("combine_exec_node");
    
    public override IReadOnlyList<IConnectorLogic> Inputs {get; set;} = [new ExecConnectorLogic("Input1"), new ExecConnectorLogic("Input2")];
    public override IReadOnlyList<IConnectorLogic> Outputs {get; set;} = [new ExecConnectorLogic("Output")];
    public override void GenerateCode(CodeContext context, IndentedTextWriter writer)
    {
        writer.WriteLine("// COMBINED EXECUTION");
    }
}