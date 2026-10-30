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

namespace RPGCreator.SDK.Assets.Skills;

public enum SkillEffectKind : byte
{
    DamageStat = 0,         // Subtracts the formula result from TargetStatId (e.g. HP)
    HealStat = 1,           // Adds the formula result to TargetStatId (e.g. HP, MP)
    ApplyStatusEffect = 2,  // Applies a status effect (buff/debuff)
    RemoveStatusEffect = 3, // Cleanses a status effect
    EmitSignal = 4          // Emits a custom URN signal on the target entity
}