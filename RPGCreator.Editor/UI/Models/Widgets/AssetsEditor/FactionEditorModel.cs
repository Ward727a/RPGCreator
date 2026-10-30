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
using System.Numerics;
using Hexa.NET.ImGui;
using RPGCreator.SDK.Assets.Factions;
using RPGCreator.Shared.Attributes;

namespace RPGCreator.Editor.UI.Models.Widgets.AssetsEditor;

[EditorUiModel]
public class FactionEditorModel
{
    public FactionDefinition Faction { get; set; }

    public string Name = string.Empty;
    public string Description = string.Empty;
    
    public Ulid IconId = Ulid.Empty;
    public ImTextureRef? IconTexture = null;

    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    // Will be converted to a uint value on save
    public Vector3 ColorRgbEditable = new(255);
    
    // Will be converted to a short on save
    public int DefaultRelation = 0;
    
    public Dictionary<Ulid, short> Relations = new();
}