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
using Hexa.NET.ImGui;
using RPGCreator.SDK.Assets.Actors;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Types;

namespace RPGCreator.Editor.UI.Models.Widgets.AssetsEditor;

[EditorUiModel]
public class CharacterEditorModel
{
    public ActorDefinition Actor { get; set; } = null!;
    public bool IsDirty { get; set; }

    public string Name = string.Empty;
    public string Description = string.Empty;
    public Ulid PortraitResourceId = Ulid.Empty;
    public ImTextureRef? PortraitTexture = null;

    public Dictionary<URN, DirectionalAnimationSet> Animations = [];
    public List<IActorFeatureData> Features = [];

    // Temporary UI state for adding a new animation slot
    public string NewAnimSlotNamespace = "rpgc";
    public string NewAnimSlotName = "walk";
}