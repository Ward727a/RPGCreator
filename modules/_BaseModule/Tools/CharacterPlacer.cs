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

using System.Collections.ObjectModel;
using System.Numerics;
using RPGCreator.SDK.GlobalState;
using RPGCreator.SDK.Inputs;
using RPGCreator.SDK.Types;
using Logger = RPGCreator.SDK.Logging.Logger;

namespace _BaseModule.Tools;

public class CharacterPlacer : ToolLogic
{
    public override URN ToolUrn { get; protected set; } = ToolUrnModule.ToUrnModule("rpgc").ToUrn("character_placer");
    public override string DisplayName => "Character Placer";
    public override string Description => "A tool for placing characters on the map.";
    public override string Icon => "mdi-account-plus";
    public override PipedPath Category { get; } = EntityCategory.Extend("Characters");
    public override EPayloadType PayloadType => EPayloadType.Character;
    public override URN HelpKey => "rpgc".ToUrnNamespace().ToUrnModule("docs").ToUrn("tool_character_placer");

    public override ObservableCollection<IToolParameter> GetParameters()
    {
        return [];
    }

    public override void UseAt(Vector2? absolutePosition = null, MouseButton button = MouseButton.Left)
    {
        Logger.Warning("The Character Placer tool is not yet implemented.");
    }
}