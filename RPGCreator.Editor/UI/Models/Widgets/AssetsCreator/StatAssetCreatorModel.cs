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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;
using Hexa.NET.ImGui;
using Microsoft.Xna.Framework;
using RPGCreator.SDK.Assets.Stats;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Types;

namespace RPGCreator.Editor.UI.Models.Widgets.AssetsCreator;

[EditorUiModel]
public class StatAssetCreatorModel
{
    public ValidationState ValidationState = new();
    
    public string Name = string.Empty;
    public string Code = string.Empty;
    public string Description = string.Empty;
    
    public string KindStr => Enum.GetName(Kind) ?? "Unknown";
    public StatKind Kind = StatKind.Attribute;
    public ReadOnlyDictionary<string, string> KindTooltip = new(new Dictionary<string, string>
    {
        { "Attribute", "Simple stat (e.g. strength, intelligence, etc.)." },
        { "Resource", "Gauge stat that lives inside a min and a max (e.g. health, mana, etc.)." },
        { "Derived", "Calculated stat with a formula (e.g. dodge = agility * 0.5%)." }
    });

    public string IconSourcePath = string.Empty;
    public Ulid IconId = Ulid.Empty;
    public Point IconSize = new(32, 32);
    public ImTextureRef? IconTexture = null;
    
}