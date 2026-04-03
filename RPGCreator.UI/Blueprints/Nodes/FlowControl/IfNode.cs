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

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using RPGCreator.SDK.Graph;
using RPGCreator.SDK.Graph.ConnectorLogics;
using RPGCreator.SDK.Graph.LOGIC;
using RPGCreator.SDK.Types;

namespace RPGCreator.UI.Blueprints.Nodes.FlowControl;

public class IfNode : BaseNodeLogic
{
    public override FlowType FlowType => FlowType.Conditional;
    public override IReadOnlyList<IConnectorLogic> Inputs { get; set; } = [new ExecConnectorLogic(), new BoolConnectorLogic("Condition")];
    public override IReadOnlyList<IConnectorLogic> Outputs { get; set; } = [new ExecConnectorLogic("True"), new ExecConnectorLogic("False")];
    public override string Title => "If";
    public override PipedPath Category => ("Flow Control").ToPipedPath();
    public override URN Urn { get; } = DefaultUrnModule.ToUrnModule("rpgc").ToUrn("if_node");
    public override void GenerateCode(CodeContext context, IndentedTextWriter writer)
    {
        string condition = "";
        if (Inputs[1].RuntimeHasParent)
        {
            context.GetVariableName(Inputs[1].RuntimeParentConnector, out var variableName);
            condition = $"{variableName}";
        }
        else
        {
            condition = Inputs[1].GetStringValue();
        }


        if (condition == "false" || condition == "true")
        {
            writer.WriteLine("/* == DISABLED BY COMPILER - Branching is useless here ==");
        }

        context.EnterBranch();
        writer.WriteLine($"if ({condition}) {{");
        writer.Indent++;
        
        if (condition == "true")
        {
            writer.WriteLine("*/");
        }
        
        context.GenerateCodeFromOutput(context, writer, Outputs[0].RuntimeId);
        writer.Indent--;
        
        if (condition == "true")
        {
            writer.WriteLine("/* == DISABLED BY COMPILER - Branching is useless here ==");
        }
        
        writer.WriteLine("}");
        writer.WriteLine($"else {{");
        
        if(condition == "false")
            writer.WriteLine("*/");
        
        writer.Indent++;
        context.GenerateCodeFromOutput(context, writer, Outputs[1].RuntimeId);
        writer.Indent--;
        
        if(condition == "false")
            writer.WriteLine("/* == DISABLED BY COMPILER - Branching is useless here ==");
        
        writer.WriteLine("}");
        
        if (condition == "true" || condition == "false")
        {
            writer.WriteLine("*/");
        }
        
        context.ExitBranch();
    }
}