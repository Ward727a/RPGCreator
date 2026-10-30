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
using Microsoft.Xna.Framework;
using RPGCreator.SDK.Assets.Stats;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Types;

namespace RPGCreator.Editor.UI.Models.Widgets.AssetsEditor;

[EditorUiModel]
public class StatAssetEditorModel
{
    public bool IsDirty;
    public ValidationState ValidationState = new();
    
    public StatDefinition Stat = null!;
    public StatKind Kind => Stat.Kind;
    
    public string Name = string.Empty;
    public string Code = string.Empty;
    public string Description = string.Empty;
    public Ulid IconId;

    public double DefaultValue;
    
    public double MinValue;
    public double MaxValue;

    public Ulid MaxStatId;
    
    public string DerivedStatFormula = string.Empty;
    
    public List<StatThresholdRule> ThresholdRules = [];

    public ImTextureRef? IconTexture = null;
    public string IconSourcePath = string.Empty;
    public Point IconSize;
}