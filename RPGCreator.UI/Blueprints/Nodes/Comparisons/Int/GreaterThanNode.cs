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

namespace RPGCreator.UI.Blueprints.Nodes.Comparisons.Int;

public class GreaterThanNode : BaseNodeLogic
{
    public override FlowType FlowType => FlowType.Pure;
    public override string Title { get; } = "Greater Than";
    public override PipedPath Category { get; } = "Comparisons".ToPipedPath().Extend("Integer");
    public override URN Urn { get; } = DefaultUrnModule.ToUrnModule("rpgc").ToUrn("comparisons_int_greater_than_node");
    public override IReadOnlyList<IConnectorLogic> Inputs { get; set; } =
        [new IntConnectorLogic("A"), new IntConnectorLogic("B")];
    public override IReadOnlyList<IConnectorLogic> Outputs { get; set; } = [new BoolConnectorLogic("Result")];
    public override void GenerateCode(CodeContext context, IndentedTextWriter writer)
    {
        var valueInput = GetConnectorValue(context, Inputs[0]);
        var valueInput2 = GetConnectorValue(context, Inputs[1]);
        var variableName = $"var_{Ulid.NewUlid()}";
        context.SetVariableName(Outputs[0].RuntimeId, variableName);
        writer.WriteLine($"bool {variableName} = {valueInput} > {valueInput2};");
    }
}