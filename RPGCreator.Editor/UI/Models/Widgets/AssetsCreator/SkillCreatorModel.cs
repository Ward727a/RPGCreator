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
using Hexa.NET.ImGui;
using RPGCreator.SDK.Assets.Skills;
using RPGCreator.Shared.Attributes;

namespace RPGCreator.Editor.UI.Models.Widgets.AssetsCreator;

public enum SkillPreset : byte
{
    DamageSkill = 0,
    HealSkill = 1,
    SelfBuff = 2,
    Blank = 3
}

[EditorUiModel]
public class SkillCreatorModel
{
    public string Name = string.Empty;
    public string Description = string.Empty;
    public SkillTargetType TargetType = SkillTargetType.SingleEnemy;
    public SkillPreset Preset = SkillPreset.DamageSkill;

    public Ulid IconResourceId = Ulid.Empty;
    public ImTextureRef? IconTexture = null;
}