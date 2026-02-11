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

using _BaseModule.AssetDefinitions.BaseStats;
using _BaseModule.Registry;
using RPGCreator.SDK;
using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.ECS;
using RPGCreator.SDK.ECS.Systems;
using RPGCreator.SDK.Modules.Features.Entity;
using RPGCreator.SDK.Types;
using RPGCreator.SDK.Types.Collections;

namespace _BaseModule.Features.Entity;


public enum StatModifierType
{
    Flat, // A flat modifier adds a fixed value to the stat. For example, a flat modifier of +10 would add 10 to the stat's current value.
    Percent, // A percent modifier increases the stat by a percentage of its current value. For example, a percent modifier of +20% would increase the stat's current value by 20%.
    Multiplier // A multiplier modifier multiplies the stat by a certain factor. For example, a multiplier modifier of x1.5 would increase the stat's current value by 50%, while a multiplier modifier of x0.5 would decrease the stat's current value by 50%.
}

public interface IStatsModifier : ISlabItem
{
    public TimeSpan Duration { get; }
    public int ModifierId { get; set; } // Unique identifier for this modifier, used to prevent stacking of the same modifier multiple times.
    public long InstanceId { get; set; } // Unique identifier for this modifier INSIDE the Slabs, this is used by the ModifierExpirationData!
}

public record struct BaseStatFlatModifier : IStatsModifier
{
    public float FlatValue { get; set; } // 10 => +10, -5 => -5
    public TimeSpan Duration { get; set; }
    public int? SlabPointerIndex { get; set; } // Slab pointer index. Required by Slabs<T>.
    public int ModifierId { get; set; } // Unique identifier for this modifier, used to prevent stacking of the same modifier multiple times.
    public long InstanceId { get; set; } // Unique identifier for this modifier INSIDE the Slabs, this is used by the ModifierExpirationData!
}

public record struct BaseStatPercentModifier : IStatsModifier
{
    public float PercentValue { get; set; } // 10 => +10%, -20 => -20%
    public TimeSpan Duration { get; set; }
    public int? SlabPointerIndex { get; set; } // Slab pointer index. Required by Slabs<T>.
    public int ModifierId { get; set; } // Unique identifier for this modifier, used to prevent stacking of the same modifier multiple times.
    public long InstanceId { get; set; } // Unique identifier for this modifier INSIDE the Slabs, this is used by the ModifierExpirationData!
}

public record struct BaseStatMultiplierModifier : IStatsModifier
{
    public float MultiplierValue { get; set; } // 1.5 => +50%, 0.8 => -20%
    public TimeSpan Duration { get; set; }
    public int? SlabPointerIndex { get; set; } // Slab pointer index. Required by Slabs<T>.
    public int ModifierId { get; set; } // Unique identifier for this modifier, used to prevent stacking of the same modifier multiple times.
    public long InstanceId { get; set; } // Unique identifier for this modifier INSIDE the Slabs, this is used by the ModifierExpirationData!
}

// Stats modifiers are used to modify stats of other entities (e.g. buffs, debuffs, etc.).
// Here is the formula used to calculate the final stat value of an entity with stats modifiers:
// FinalStatValue = Math.Max(0, (BaseStatValue + Sum(FlatModifiers)) * (1 + Sum(PercentModifiers) / 100) * Product(MultiplierModifiers))
// The Math.Max(0, ...) is used to prevent negative final stat values, which could cause issues in the game (e.g. negative health, negative damage, etc.).
// The Math.Max(0, ...) is only applied IF the stat is marked as "NonNegative" in the stat editor, otherwise negative final stat values are allowed (e.g. for debuffs that reduce stats below 0).

[EntityFeature]
public class StatsModifierFeature : BaseEntityFeature
{
    public override string FeatureName => "Stats Modifier";

    public override string FeatureDescription =>
        "Allows the entity to modify stats of other entities (e.g. buffs, debuffs, etc.).\n" +
        "The actual logic of how the stats are modified is not implemented by this feature, it simply adds the necessary component for other systems to work with.\n" +
        "Note: This feature is dependent on the Stats Feature, and will not work properly if the target entity does not have the Stats Feature.";
    public override URN FeatureUrn => FeatureUrnModule.ToUrnModule("rpgc").ToUrn("stats_modifier_feature");

    public override URN[] DependentFeatures => [StatsFeature.Urn];

    public override void OnWorldSetup(IEcsWorld world)
    {
        world.SystemManager.AddSystem(new StatsModifierSystem());
    }

    public override void OnInject(BufferedEntity entity, IEntityDefinition entityDefinition)
    {
        entity.AddComponent(new StatsModifierComponent
        {
            StatModifiers = new Dictionary<Ulid, StatBlocks>(),
            ModifierDurations = new PriorityQueue<ModifierExpirationData, double>()
        });
    }
}

public struct StatsModifierComponent : IComponent
{
    public Dictionary<Ulid, StatBlocks> StatModifiers; // Key: StatId, Value: The indices of the modifiers for that stat in the slabs.
    public PriorityQueue<ModifierExpirationData, double> ModifierDurations;
}

public struct StatBlocks()
{
    public int FlatModifiersIdx = -1;
    public int PercentModifiersIdx = -1;
    public int MultiplierModifiersIdx = -1;

    public StatBlocks(Slabs<BaseStatFlatModifier> slabsFlat, Slabs<BaseStatPercentModifier> slabsPercent, Slabs<BaseStatMultiplierModifier> slabsMutliplier) : this()
    {
        FlatModifiersIdx = slabsFlat.AllocateEmpty();
        PercentModifiersIdx = slabsPercent.AllocateEmpty();
        MultiplierModifiersIdx = slabsMutliplier.AllocateEmpty();
    }
}

public record struct ModifierExpirationData(int SlabIdx, long InstanceId, StatModifierType Type, int ModifierId);

public class StatsModifierSystem : ISystem
{
    
    private long _nextModifierInstanceId; // Used to assign unique instance IDs to modifiers inside the slabs, this is used to track modifiers for expiration and removal.
    
    public override int Priority => 90; // Before the Stats System, which is at 100, to ensure that the modifiers are applied before the stats are calculated.
    public override bool IsDrawingSystem => false;

    // Initial capacity of 20 modifiers BY stats BY entity, it will automatically resize if needed
    public readonly Slabs<BaseStatFlatModifier> FlatModifiers = new(20);
    public readonly Slabs<BaseStatPercentModifier> PercentModifiers = new(20);
    public readonly Slabs<BaseStatMultiplierModifier> MultiplierModifiers = new(20);
    
    private ComponentManager _componentManager = null!;
    
    private double _totalTimeElapsed = 0;

    public override void Initialize(IEcsWorld ecsWorld)
    {
        _componentManager = ecsWorld.ComponentManager;
    }

    public override void Update(TimeSpan deltaTime)
    {
        _totalTimeElapsed += deltaTime.TotalMilliseconds;
        foreach (var entityId in _componentManager.Query<StatsModifierComponent>())
        {
            ref var statsModifierComponent = ref _componentManager.GetComponent<StatsModifierComponent>(entityId);
            
            if (statsModifierComponent.ModifierDurations.Count == 0)
                continue;
            
            bool hasChanged = false;

            while (statsModifierComponent.ModifierDurations.Count > 0 && statsModifierComponent.ModifierDurations.TryPeek(out var data, out double expiration))
            {
                if(expiration > _totalTimeElapsed)
                    break; // The next modifier to expire is still active, so we can stop checking further.
                
                var expiredModifier = statsModifierComponent.ModifierDurations.Dequeue();
                hasChanged = true;
                
                switch (expiredModifier.Type)
                {
                    case StatModifierType.Flat:
                        RemoveModifierFromSlab(expiredModifier.SlabIdx, expiredModifier.InstanceId, FlatModifiers);
                        break;
                    case StatModifierType.Percent:
                        RemoveModifierFromSlab(expiredModifier.SlabIdx, expiredModifier.InstanceId, PercentModifiers);
                        break;
                    case StatModifierType.Multiplier:
                        RemoveModifierFromSlab(expiredModifier.SlabIdx, expiredModifier.InstanceId, MultiplierModifiers);
                        break;
                }
            }

            if (!hasChanged) continue;
            
            if (_componentManager.HasComponent<StatComponent>(entityId))
            {
                _componentManager.MarkDirty<StatComponent>(entityId);
            }
        }
    }

    public void ApplyStatModifier(int targetEntityId, StatModifierDefinition definition)
    {
        if(definition.RegistryId == -1)
            throw new InvalidOperationException("StatModifierDefinition must have a valid RegistryId to be applied as a modifier.");
        
        var slabIdx = GetSlabIndexFromUnique(targetEntityId, definition.StatId, definition.ModifierType);
        
        int spanExistingId = -1;
        long existingInstanceId = -1;
        
        Slabs<BaseStatFlatModifier>? slabsFlat = null;
        Slabs<BaseStatPercentModifier>? slabsPercent = null;
        Slabs<BaseStatMultiplierModifier>? slabsMultiplier = null;

        switch (definition.ModifierType)
        {
            case StatModifierType.Flat:
            {
                slabsFlat = FlatModifiers;
                var span = FlatModifiers.GetSpan(slabIdx);
                (spanExistingId, existingInstanceId) = SpanExistingId(definition, span);
                break;
            }
            case StatModifierType.Percent:
            {
                slabsPercent = PercentModifiers;
                var span = PercentModifiers.GetSpan(slabIdx);
                (spanExistingId, existingInstanceId) = SpanExistingId(definition, span);
                break;
            }
            case StatModifierType.Multiplier:
            {
                slabsMultiplier = MultiplierModifiers;
                var span = MultiplierModifiers.GetSpan(slabIdx);
                (spanExistingId, existingInstanceId) = SpanExistingId(definition, span);
                break;
            }
        }

        if (spanExistingId != -1)
        {
            switch (definition.StackingPolicy)
            {
                case StatStackingPolicy.Ignore:
                    return; // We do nothing, the existing modifier takes precedence and the new one is ignored.
                case StatStackingPolicy.Override:
                    slabsFlat?.RemoveItem(slabIdx, spanExistingId);
                    slabsPercent?.RemoveItem(slabIdx, spanExistingId);
                    slabsMultiplier?.RemoveItem(slabIdx, spanExistingId);
                    break; // We remove the existing modifier, and continue to apply the new one as normal.
                case StatStackingPolicy.Strongest:
                    IStatsModifier? existingModifier = null;
                    switch (definition.ModifierType)
                    {
                        case StatModifierType.Flat:
                            existingModifier = GetModifierFromSlab(slabIdx, existingInstanceId, FlatModifiers);
                            if (existingModifier is BaseStatFlatModifier existingFlat &&
                                Math.Abs(existingFlat.FlatValue) >= Math.Abs(definition.Value))
                                return; // The existing modifier is stronger or equal, so we ignore the new one.
                            break;
                        case StatModifierType.Percent:
                            existingModifier = GetModifierFromSlab(slabIdx, existingInstanceId, PercentModifiers);
                            if (existingModifier is BaseStatPercentModifier existingPercent &&
                                Math.Abs(existingPercent.PercentValue) >= Math.Abs(definition.Value))
                                return; // The existing modifier is stronger or equal, so we ignore the new one.
                            
                            break;
                        case StatModifierType.Multiplier:
                            existingModifier = GetModifierFromSlab(slabIdx, existingInstanceId, MultiplierModifiers);
                            if (existingModifier is BaseStatMultiplierModifier existingMultiplier &&
                                Math.Abs(existingMultiplier.MultiplierValue - 1) >= Math.Abs(definition.Value - 1))
                                return; // The existing modifier is stronger or equal, so we ignore the new one.
                            break;
                    }
                    // Else, we remove the existing modifier and continue to apply the new one as normal.
                    slabsFlat?.RemoveItem(slabIdx, spanExistingId);
                    slabsPercent?.RemoveItem(slabIdx, spanExistingId);
                    slabsMultiplier?.RemoveItem(slabIdx, spanExistingId);
                    break;
                case StatStackingPolicy.Stack:
                    // We allow stacking, so we do nothing and continue to apply the new modifier as normal.
                    break;
                default:
                    throw new InvalidOperationException("Unknown stacking policy.");
            }
        }

        switch (definition.ModifierType)
        {
            case StatModifierType.Flat:
                var flatModifier = new BaseStatFlatModifier
                {
                    FlatValue = definition.Value,
                    Duration = definition.Duration,
                    ModifierId = definition.RegistryId,
                    InstanceId = _nextModifierInstanceId++
                };
                InternalApplyModifier(flatModifier, slabIdx, targetEntityId);
                break;
            case StatModifierType.Percent:
                var percentModifier = new BaseStatPercentModifier
                {
                    PercentValue = definition.Value,
                    Duration = definition.Duration,
                    ModifierId = definition.RegistryId,
                    InstanceId = _nextModifierInstanceId++
                };
                InternalApplyModifier(percentModifier, slabIdx, targetEntityId);
                break;
            case StatModifierType.Multiplier:
                var multiplierModifier = new BaseStatMultiplierModifier
                {
                    MultiplierValue = definition.Value,
                    Duration = definition.Duration,
                    ModifierId = definition.RegistryId,
                    InstanceId = _nextModifierInstanceId++
                };
                InternalApplyModifier(multiplierModifier, slabIdx, targetEntityId);
                break;
            
        }
        
        _componentManager.MarkDirty<StatComponent>(targetEntityId);
    }

    private static (int spanId, long instanceId) SpanExistingId<T>(StatModifierDefinition definition, Span<T> span) where T : IStatsModifier
    {
        long existingInstanceId = -1;
        int spanExistingId = -1;
        for(int i = 0; i < span.Length; i++) {
            if(span[i].ModifierId == definition.RegistryId) {
                spanExistingId = i;
                existingInstanceId = span[i].InstanceId;
                break;
            }
        }

        return (spanExistingId, existingInstanceId);
    }

    public void ApplyStatModifier(int targetEntityId, Ulid definitionId)
    {
        if (!EngineServices.AssetsManager.TryResolveAsset<StatModifierDefinition>(definitionId, out var definition))
            throw new InvalidOperationException($"No StatModifierDefinition found with ID {definitionId}.");
        
        ApplyStatModifier(targetEntityId, definition);
    }

    public void ApplyStatModifier(int targetEntityId, int definitionMappingId)
    {
        if (EngineServices.AssetsManager.TryResolveRegistry<StatModifierRegistry>("stat_modifiers",
                out var statModifierRegistry))
        {
            var unique = statModifierRegistry.GetStatModifierUnique(definitionMappingId);
            
            if(unique == Ulid.Empty)
                throw new InvalidOperationException($"No StatModifierDefinition found with mapping ID {definitionMappingId} in the StatModifierRegistry.");
            
            ApplyStatModifier(targetEntityId, unique);
        }
        
    }
    
    private void InternalApplyModifier(IStatsModifier modifier, int slabIdx, int entityId)
    {
        switch (modifier)
        {
            case BaseStatFlatModifier flatModifier:
                FlatModifiers.AddItem(slabIdx, flatModifier);
                flatModifier.SlabPointerIndex = slabIdx;
                ApplyExpirationModifierData(flatModifier, StatModifierType.Flat, entityId);
                break;
            case BaseStatPercentModifier percentModifier:
                PercentModifiers.AddItem(slabIdx, percentModifier);
                percentModifier.SlabPointerIndex = slabIdx;
                ApplyExpirationModifierData(percentModifier, StatModifierType.Percent, entityId);
                break;
            case BaseStatMultiplierModifier multiplierModifier:
                MultiplierModifiers.AddItem(slabIdx, multiplierModifier);
                multiplierModifier.SlabPointerIndex = slabIdx;
                ApplyExpirationModifierData(multiplierModifier, StatModifierType.Multiplier, entityId);
                break;
            default:
                throw new InvalidOperationException("Unknown modifier type.");
        }
    }
    
    private void ApplyExpirationModifierData(IStatsModifier modifier, StatModifierType type, int entityId)
    {
        var expirationTime = _totalTimeElapsed + modifier.Duration.TotalMilliseconds;
        var expirationData = new ModifierExpirationData(modifier.SlabPointerIndex.Value, modifier.InstanceId, type, modifier.ModifierId);
        
        var statsModifierComponent = _componentManager.GetComponent<StatsModifierComponent>(entityId);
        statsModifierComponent.ModifierDurations.Enqueue(expirationData, expirationTime);
    }

    private IStatsModifier? GetModifierFromSlab<T>(int slabIdx, long modifierInstanceId, Slabs<T> slabs) where T : struct, IStatsModifier
    {
        var span = slabs.GetSpan(slabIdx);

        for (int i = 0; i < span.Length; i++)
        {
            if (span[i].InstanceId == modifierInstanceId)
            {
                return span[i];
            }
        }
        
        return null;
    }

    private void RemoveModifierFromSlab<T>(int slabIdx, long modifierInstanceId, Slabs<T> slabs)
        where T : struct, IStatsModifier
    {
        var span = slabs.GetSpan(slabIdx);

        for (int i = 0; i < span.Length; i++)
        {
            if (span[i].InstanceId == modifierInstanceId)
            {
                slabs.RemoveItem(slabIdx, i);
                return;
            }
        }
    }


    private int GetSlabIndexFromUnique(int entityId, Ulid statId, StatModifierType type)
    {
        var statsModifierComponent = _componentManager.GetComponent<StatsModifierComponent>(entityId);
        if (!statsModifierComponent.StatModifiers.TryGetValue(statId, out var blocks)) {
            blocks = new StatBlocks(FlatModifiers, PercentModifiers, MultiplierModifiers);
            statsModifierComponent.StatModifiers[statId] = blocks;
        }
        
        return type switch
        {
            StatModifierType.Flat => blocks.FlatModifiersIdx,
            StatModifierType.Percent => blocks.PercentModifiersIdx,
            StatModifierType.Multiplier => blocks.MultiplierModifiersIdx,
            _ => throw new InvalidOperationException("Unknown modifier type.")
        };
    }
    
    
}

#region EcsEvents

/// <summary>
/// When a stat modifier expires on an entity, this event is triggered to notify other systems that may need to react to the expiration of the modifier (e.g. updating the UI, recalculating stats, etc.).
/// </summary>
/// <param name="TargetEntityId">The ID of the entity on which the modifier expired.</param>
/// <param name="ModifierId">The unique identifier of the modifier that expired, used to identify which modifier expired and prevent stacking of the same modifier multiple times.</param>
public readonly record struct OnModifierExpired(int TargetEntityId, int ModifierId);

/// <summary>
/// When a stat modifier is applied to an entity, this event is triggered to notify other systems that may need to react to the application of the modifier (e.g. updating the UI, recalculating stats, etc.).
/// </summary>
/// <param name="TargetEntityId">The ID of the entity on which the modifier was applied.</param>
/// <param name="ModifierId">The unique identifier of the modifier that was applied, used to identify which modifier was applied and prevent stacking of the same modifier multiple times.</param>
public readonly record struct OnModifierApplied(int TargetEntityId, int ModifierId);

#endregion