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

using _BaseModule.Features.Entity;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions;
using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.Serializer;
using RPGCreator.SDK.Types;
using RPGCreator.SDK.Types.Internals;

namespace _BaseModule.AssetDefinitions.BaseStats;

public enum StatStackingPolicy
{
    /// <summary>
    /// Modifiers of the same type will stack by summing their values.
    /// </summary>
    Stack,
    
    /// <summary>
    /// Only the strongest modifier of the same type will apply.
    /// </summary>
    Strongest,
    
    /// <summary>
    /// Modifiers of the same type will not stack and will be applied in order, with later modifiers overriding earlier ones.
    /// </summary>
    Override,
    
    /// <summary>
    /// Modifiers of the same type will not stack and only the first applied modifier will be applied, with later modifiers being ignored.
    /// </summary>
    Ignore
}

[SerializingType("StatModifier")]
public class StatModifierDefinition : BaseAssetDef, IHasSavePath, ISerializable, IDeserializable
{
    public override UrnSingleModule UrnModule => "stats_modifiers".ToUrnSingleModule();
    public string SavePath { get; set; } = "";

    public int RegistryId { get; set; } = -1;
    
    public Ulid StatId { get; set; }
    public StatModifierType ModifierType { get; set; }
    public StatStackingPolicy StackingPolicy { get; set; }
    public float Value { get; set; }
    public TimeSpan Duration { get; set; }
    public string DisplayName => string.IsNullOrEmpty(Name) ? $"Stat_Modifier-{StatId}" : Name;

    public string Description = "";

    public SerializationInfo GetObjectData()
    {
        var info = new SerializationInfo(typeof(StatModifierDefinition))
            .AddValue(nameof(Unique), Unique)
            .AddValue(nameof(StatId), StatId)
            .AddValue(nameof(ModifierType), ModifierType)
            .AddValue(nameof(StackingPolicy), StackingPolicy)
            .AddValue(nameof(Value), Value)
            .AddValue(nameof(Duration), Duration)
            .AddValue(nameof(Name), Name)
            .AddValue(nameof(Description), Description);
        return info;
    }

    public List<Ulid> GetReferencedAssetIds()
    {
        return [StatId];
    }

    public void SetObjectData(DeserializationInfo info)
    {
        info.TryGetValue(nameof(Unique), out var unique, Ulid.Empty);
        info.TryGetValue(nameof(StatId), out var statId, Ulid.Empty);
        info.TryGetValue(nameof(ModifierType), out var modifierType, StatModifierType.Flat);
        info.TryGetValue(nameof(StackingPolicy), out var stackingPolicy, StatStackingPolicy.Stack);
        info.TryGetValue(nameof(Value), out var value, 0f);
        info.TryGetValue(nameof(Duration), out var duration, TimeSpan.Zero);
        info.TryGetValue(nameof(Name), out var name, "");
        info.TryGetValue(nameof(Description), out var description, "");
        
        Unique = unique;
        StatId = statId;
        ModifierType = modifierType;
        StackingPolicy = stackingPolicy;
        Value = value;
        Duration = duration;
        Name = name;
        Description = description;
    }
}