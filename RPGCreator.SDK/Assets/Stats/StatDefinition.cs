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

using System.Text.Json.Serialization;
using RPGCreator.SDK.Services.Assets;
using RPGCreator.Shared.Attributes;

namespace RPGCreator.SDK.Assets.Stats;

[TypeDiscriminator("rpgc", "sdk", "definitions", "stat")]
[AssetType("stats", "rstat")]
public class StatDefinition : IAssetDto
{
    public Ulid AssetId { get; init; } = Ulid.NewUlid();
    public List<Ulid> Dependencies { get; init; } = [];
    
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Short name of the stat. (e.g. "HP", "MP", "STR", "DEF", "AGI", "INT", "LUK")<br/>
    /// It's also used as the identifier for the stat in PrattFormulas.
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    [JsonPropertyName("Icon")]
    public Ulid IconResource { get; set; } = Ulid.Empty;
    
    public StatKind Kind { get; set; } = StatKind.Attribute;
    
    public double DefaultValue { get; set; } = 10.0;
    public double MinValue { get; set; } = 0.0;
    public double MaxValue { get; set; } = 999.0;
    
    /// <summary>
    /// Specific for <see cref="StatKind.Resource"/> where it needs another stat to set its max value.<br/>
    /// e.g. "HP" stat will have "MaxHP" as its max stat.
    /// </summary>
    [AssetReference]
    [JsonPropertyName("MaxStat")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public Ulid MaxStatId { get; set; } = default;
    
    public string DerivedFormula { get; set; } = string.Empty;
    
    public List<StatThresholdRule> ThresholdRules { get; set; } = [];
}