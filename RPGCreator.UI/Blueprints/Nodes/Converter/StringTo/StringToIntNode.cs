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
using System.Globalization;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Graph;
using RPGCreator.SDK.Graph.ConnectorLogics;
using RPGCreator.SDK.Graph.LOGIC;
using RPGCreator.SDK.Types;
using RPGCreator.Shared.Types;

namespace RPGCreator.UI.Blueprints.Nodes.Converter.StringTo;

public class StringToIntNode : BaseNodeLogic
{
    public override FlowType FlowType => FlowType.Pure;
    public override string Title => "String to Int";
    public override PipedPath Category { get; } = "Converter".ToPipedPath().Extend("String To");
    public override URN Urn { get; } = DefaultUrnModule.ToUrnModule("rpgc").ToUrn("string_to_int_node");

    public override IReadOnlyList<IConnectorLogic> Inputs { get; set; } = new List<IConnectorLogic>
        { new StringConnectorLogic("Value to convert") };

    public override IReadOnlyList<IConnectorLogic> Outputs { get; set; } = new List<IConnectorLogic>
        { new IntConnectorLogic("Converted value") };

    public override void GenerateCode(CodeContext context, IndentedTextWriter writer)
    {
        var varName = $"var_{Ulid.NewUlid()}";
        var input = GetConnectorValue(context, Inputs[0]);

        context.SetVariableName(Outputs[0].RuntimeId, varName);
        
        writer.WriteLine();
        writer.WriteLine("// STRING TO INT");
        if (Inputs[0].RuntimeHasParent)
        {
            if (context.InDebugMode)
            {
                writer.WriteLine($"Guard.IsNotNull({input});");
                writer.WriteLine($"Guard.IsNotNullOrWhiteSpace({input});");
            }
            writer.WriteLine($"int {varName} = int.TryParse({input}.AsSpan(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) ? result : 0;");
        }
        else
        {
            var rawValue = (string?)Inputs[0].GetRawValue();

            if (rawValue == null)
            {
                Logger.Error("ERROR WHILE GENERATING CODE: STRING TO INT NODE: NO VALUE FOUND IN INPUT");
                writer.WriteLine("// ERROR WHILE GENERATING CODE: STRING TO INT NODE: NO VALUE FOUND IN INPUT");
                writer.WriteLine($"int {varName} = 0;");
                return;
            };
            
            if(context.InDebugMode)
                writer.WriteLine("// Optimized by compiler: pin was not linked, using node value");
            writer.WriteLine($"int {varName} = {(int.TryParse(rawValue.AsSpan(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) ? result : 0)};");
        }
    }
}