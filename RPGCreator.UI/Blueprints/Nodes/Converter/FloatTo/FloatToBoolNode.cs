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
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Types;

namespace RPGCreator.UI.Blueprints.Nodes.Converter.FloatTo;

public class FloatToBoolNode : BaseNodeLogic
{
    public override FlowType FlowType => FlowType.Pure;
    public override string Title => "Float to Bool";
    public override PipedPath Category { get; } = "Converter".ToPipedPath().Extend("Float To");
    public override URN Urn { get; } = DefaultUrnModule.ToUrnModule("rpgc").ToUrn("float_to_bool_node");
    public override IReadOnlyList<IConnectorLogic> Inputs { get; set; } = new List<IConnectorLogic> { new FloatConnectorLogic("Value to convert") };
    public override IReadOnlyList<IConnectorLogic> Outputs { get; set; } = new List<IConnectorLogic> { new BoolConnectorLogic("Converted value") };

    public override void GenerateCode(CodeContext context, IndentedTextWriter writer)
    {
        var varName = $"var_{Ulid.NewUlid()}";
        var input = GetConnectorValue(context, Inputs[0]);
        
        context.SetVariableName(Outputs[0].RuntimeId, varName);
        
        writer.WriteLine();
        writer.WriteLine("// FLOAT TO BOOL");
        if(Inputs[0].RuntimeHasParent)
        {
            writer.WriteLine($"bool {varName} = (int){input} != 0;");
        }
        else
        {
            var rawValue = (float?)Inputs[0].GetRawValue();

            if (!rawValue.HasValue)
            {
                Logger.Error("ERROR WHILE GENERATING CODE: FLOAT TO BOOL NODE: NO VALUE FOUND IN INPUT");
                writer.WriteLine("// ERROR WHILE GENERATING CODE: FLOAT TO BOOL NODE: NO VALUE FOUND IN INPUT");
                writer.WriteLine($"bool {varName} = false;");
                return;
            };
            
            if(context.InDebugMode)
                writer.WriteLine("// Optimized by compiler: pin was not linked, using node value (value IS NOT 0: true, value IS EXACTLY 0: false)");
            writer.WriteLine($"bool {varName} = {((int)rawValue.Value != 0 ? "true" : "false")};");
        }
    }
}