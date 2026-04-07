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
using RPGCreator.UI.Blueprints.Connectors;

namespace RPGCreator.UI.Blueprints.Nodes.Game.Player;

public class GetPlayerNode : BaseNodeLogic
{
    public override FlowType FlowType => FlowType.Pure;
    public override string Title => "Get Player";
    public override PipedPath Category { get; } = "Game".ToPipedPath().Extend("Player");
    public override URN Urn => DefaultUrnModule.ToUrnModule("rpgc").ToUrn("get_player_node");
    public override IReadOnlyList<IConnectorLogic> Outputs { get; set; } = [
    new PlayerConnectorLogic("Player")];

    public override void GenerateCode(CodeContext context, IndentedTextWriter writer)
    {
        string varName = $"var_player_{Ulid.NewUlid()}";
        writer.WriteLine("// GET PLAYER");
        writer.WriteLine($"var {varName} = RuntimeServices.GameContext.GetPlayer();");
    }
}