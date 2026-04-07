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

namespace RPGCreator.UI.Blueprints.Nodes.Game.Player;

public class MovePlayerToNode : BaseNodeLogic
{
    public override FlowType FlowType => FlowType.Linear;
    public override string Title => "Move Player To";
    public override PipedPath Category => "Game".ToPipedPath().Extend("Player");
    public override URN Urn => DefaultUrnModule.ToUrnModule("rpgc").ToUrn("move_player_to_node");

    public override IReadOnlyList<IConnectorLogic> Inputs { get; set; } =
    [
        new ExecConnectorLogic(),
        new Vector2ConnectorLogic("Position to move to")
    ];

    public override IReadOnlyList<IConnectorLogic> Outputs { get; set; } =
    [
        new ExecConnectorLogic()
    ];

    public override void GenerateCode(CodeContext context, IndentedTextWriter writer)
    {
        var input = GetConnectorValue(context, Inputs[1]);

        writer.WriteLine();
        writer.WriteLine("// MOVE PLAYER TO");
        writer.WriteLine($"RuntimeServices.GameContext.GetPlayer().MoveTo({input});");
    }
}