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
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Types;

namespace RPGCreator.SDK.Assets.Skills;


[TypeDiscriminator("rpgc", "sdk", "definitions", "skill")]
[AssetType("skills", "rskill")]
public class SkillDefinition : IAssetDto
{
    public Ulid AssetId { get; init; } = Ulid.NewUlid();
    public List<Ulid> Dependencies { get; init; } = [];

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Ulid IconResource { get; set; } = Ulid.Empty;

    /// <summary>
    /// Resource StatDefinition AssetId consumed when using this skill (e.g. MP or Stamina).
    /// Leave Ulid.Empty for no resource cost.
    /// </summary>
    [AssetReference]
    public Ulid CostStatId { get; set; } = Ulid.Empty;

    /// <summary>
    /// Amount of CostStatId consumed (supports Pratt formulas or flat numbers, e.g. "15").
    /// </summary>
    public double CostAmount { get; set; } = 0;

    /// <summary>
    /// Cooldown duration in seconds (or turns) before the skill can be used again.
    /// </summary>
    public float Cooldown { get; set; } = 0f;

    public SkillTargetType TargetType { get; set; } = SkillTargetType.SingleEnemy;

    /// <summary>
    /// Maximum distance in pixels (or tiles) to reach the target.
    /// </summary>
    public float Range { get; set; } = 48f;

    /// <summary>
    /// Animation slot played by the caster when using this skill (e.g. "rpgc://anim_slots/attack").
    /// </summary>
    public URN CasterAnimationSlot { get; set; } = new("rpgc", "anim_slots", "attack");

    /// <summary>
    /// SpriteAnimationDefinition played on the target upon impact (e.g. slash, fireball explosion).
    /// </summary>
    [AssetReference]
    public Ulid HitSpriteAnimationId { get; set; } = Ulid.Empty;

    /// <summary>
    /// Audio resource ID played when the skill hits.
    /// </summary>
    public Ulid HitSoundResourceId { get; set; } = Ulid.Empty;

    [AssetContainer]
    public List<SkillEffect> Effects { get; set; } = [];
}