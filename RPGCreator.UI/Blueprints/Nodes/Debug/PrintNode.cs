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
using RPGCreator.SDK.EditorUiService;
using RPGCreator.SDK.Graph;
using RPGCreator.SDK.Graph.ConnectorLogics;
using RPGCreator.SDK.Graph.LOGIC;
using RPGCreator.SDK.Types;

namespace RPGCreator.UI.Blueprints.Nodes.Debug;

public class PrintNode : BaseNodeLogic
{
    public override FlowType FlowType => FlowType.Linear;
    public override IReadOnlyList<IConnectorLogic> Inputs { get; set; }
    public override IReadOnlyList<IConnectorLogic> Outputs { get; set; }

    public PrintNode()
    {
        Inputs = new List<IConnectorLogic> 
        { 
            new ExecConnectorLogic(), 
            new StringConnectorLogic("Text") 
        }.AsReadOnly();

        Outputs = new List<IConnectorLogic> 
        { 
            new ExecConnectorLogic() 
        }.AsReadOnly();
    }
    
    public override string Title => "Print";
    public override PipedPath Category => "Debug".ToPipedPath();
    public override URN Urn => DefaultUrnModule.ToUrnModule("rpgc").ToUrn("print_node");
    public override URN HelpUrn => IDocService.DocsUrnModule.ToUrnModule("rpgc").ToUrn("print_node");
    public override void GenerateCode(CodeContext context, IndentedTextWriter writer)
    {
        if (Inputs[1].RuntimeHasParent)
        {
            context.GetVariableName(Inputs[1].RuntimeParentConnector, out var variableName);
            if (context.InDebugMode)
                writer.WriteLine($"Guard.IsNotNull({variableName});");
            writer.WriteLine($"Logger.Debug({variableName});");
        }
        else
        {
            var value = Inputs[1].GetStringValue();
            writer.WriteLine($"Logger.Debug(\"{value}\");");
        }
    }
}