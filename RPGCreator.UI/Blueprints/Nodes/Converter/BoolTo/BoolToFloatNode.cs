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
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Graph;
using RPGCreator.SDK.Graph.ConnectorLogics;
using RPGCreator.SDK.Graph.LOGIC;
using RPGCreator.SDK.Types;
using RPGCreator.Shared.Types;

namespace RPGCreator.UI.Blueprints.Nodes.Converter.BoolTo;

public class BoolToFloatNode : BaseNodeLogic
{
    public override FlowType FlowType => FlowType.Pure;
    public override string Title => "Bool to Float";
    public override PipedPath Category { get; } = "Converter".ToPipedPath().Extend("Bool To");
    public override URN Urn { get; } = DefaultUrnModule.ToUrnModule("rpgc").ToUrn("bool_to_float_node");
    
    public override IReadOnlyList<IConnectorLogic> Inputs { get; set; } = new List<IConnectorLogic> { new BoolConnectorLogic("Value to convert") };
    public override IReadOnlyList<IConnectorLogic> Outputs { get; set; } = new List<IConnectorLogic> { new FloatConnectorLogic("Converted value") };
    public override void GenerateCode(CodeContext context, IndentedTextWriter writer)
    {
        var varName = $"var_{Ulid.NewUlid()}";
        var input = GetConnectorValue(context, Inputs[0]);
        
        
        context.SetVariableName(Outputs[0].RuntimeId, varName);
        
        writer.WriteLine();
        writer.WriteLine("// BOOL TO FLOAT");
        if (Inputs[0].RuntimeHasParent)
        {
            writer.WriteLine($"float {varName} = {input} ? 1f : 0f;");
        }
        else
        {
            var rawValue = (bool?)Inputs[0].GetRawValue();
            if (!rawValue.HasValue)
            {
                Logger.Error("ERROR WHILE GENERATING CODE: BOOL TO FLOAT NODE: NO VALUE FOUND IN INPUT");
                writer.WriteLine("// ERROR WHILE GENERATING CODE: BOOL TO FLOAT NODE: NO VALUE FOUND IN INPUT");
                writer.WriteLine($"float {varName} = 0f;");
                return;
            }
            if(context.InDebugMode)
                writer.WriteLine("// Optimized by compiler: pin was not linked, using node value (true: 1f, false: 0f)");
            writer.WriteLine($"float {varName} = {(rawValue.Value ? "1f" : "0f")};");
        }
    }
}