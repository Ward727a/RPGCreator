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
using RPGCreator.SDK.Assets.Skills;
using RPGCreator.Shared.Attributes;

namespace RPGCreator.Editor.UI.Models.Widgets.AssetsEditor;


[EditorUiModel]
public class SkillEditorModel
{
    public SkillDefinition Skill { get; set; } = null!;
    public bool IsDirty { get; set; }

    public string Name = string.Empty;
    public string Description = string.Empty;
    public Ulid IconResourceId = Ulid.Empty;
    public ImTextureRef? IconTexture = null;

    public Ulid CostStatId = Ulid.Empty;
    public double CostAmount = 0;
    public float Cooldown = 0f;
    public SkillTargetType TargetType = SkillTargetType.SingleEnemy;
    public float Range = 48f;

    public string CasterAnimNamespace = "rpgc";
    public string CasterAnimSlotName = "attack";
    public Ulid HitSpriteAnimationId = Ulid.Empty;
    public Ulid HitSoundResourceId = Ulid.Empty;
    public string HitSoundDisplayName = "None";

    public List<SkillEffect> Effects = [];
}