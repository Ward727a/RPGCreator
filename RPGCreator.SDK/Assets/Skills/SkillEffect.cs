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

using RPGCreator.SDK.Services.Assets;
using RPGCreator.Shared.Types;

namespace RPGCreator.SDK.Assets.Skills;

public class SkillEffect
{
    public SkillEffectKind Kind { get; set; } = SkillEffectKind.DamageStat;

    /// <summary>
    /// Target StatDefinition AssetId affected by DamageStat or HealStat (e.g. HP stat).
    /// </summary>
    [AssetReference]
    public Ulid TargetStatId { get; set; } = Ulid.Empty;

    /// <summary>
    /// Pratt formula evaluated when the effect triggers (e.g. "STR * 2 - DEF" or "50 + INT * 1.5").
    /// </summary>
    public string Formula { get; set; } = "10";

    /// <summary>
    /// Variance percentage applied to the formula result (0.0 = exact value, 0.2 = +/- 20%).
    /// </summary>
    public float Variance { get; set; } = 0.1f;

    /// <summary>
    /// Chance for this specific effect to trigger (1.0 = 100%).
    /// </summary>
    public float Chance { get; set; } = 1.0f;

    /// <summary>
    /// StatusEffect AssetId if Kind is Apply/RemoveStatusEffect.
    /// </summary>
    [AssetReference]
    public Ulid StatusEffectId { get; set; } = Ulid.Empty;

    /// <summary>
    /// Signal URN emitted if Kind is EmitSignal.
    /// </summary>
    public URN SignalToEmit { get; set; } = URN.Empty;
}