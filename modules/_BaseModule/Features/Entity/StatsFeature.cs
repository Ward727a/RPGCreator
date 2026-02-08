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

using System.Runtime.InteropServices;
using _BaseModule.AssetDefinitions.BaseResistance;
using _BaseModule.AssetDefinitions.BaseStats;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Stats;
using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.ECS;
using RPGCreator.SDK.ECS.Systems;
using RPGCreator.SDK.EngineService;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Modules.Features.Entity;
using RPGCreator.SDK.Types;

namespace _BaseModule.Features.Entity;

[EntityFeature]
public class StatsFeature : BaseEntityFeature
{
    public static readonly URN StatsTag = new URN("rpgc", TagsUrnModule, "stats");
    public static readonly URN Urn = new URN("rpgc", FeatureUrnModule, "stats");
    
    public override string FeatureName => "Stats Feature";
    public override string FeatureDescription => "Adds basic stats to the entity, such as health, mana, and stamina.\n" +
                                                 "This will also enable a custom assets menu for stats management.";

    public override URN FeatureUrn => Urn;

    private URN MakePath(string statName) => new URN("rpgc", "stats", statName);

    private readonly List<BaseStatDefinition> _statDefinitions = [];
    
    public override void OnSetup()
    {
        EngineServices.OnceServiceReady((IProjectsManager projectManager) =>
        {
            projectManager.OnProjectOpened += _ =>
            {
                // First we register each path for each stat, if they don't already exist.
                EngineServices.OnceServiceReady((IAssetsManager manager) =>
                {
                    var pack = manager.GetDefaultPack();
                    
                    // If the paths for stats don't exist yet,
                    // This either means that the feature is being set up for the very first time,
                    // or that the paths have been deleted (which shouldn't happen, but just in case).
                    if(BaseModule.FirstTime)
                    {
                        EngineServices.OnceServiceReady((IAssetsManager assetManager) =>
                        {
                            var hpStatDef = assetManager.CreateAsset<StatDefinition>();
                            hpStatDef.Name = "Health";
                            hpStatDef.Description = "The health stat represents the amount of damage an entity can take before being defeated.";
                            hpStatDef.DefaultValue = 100;
                            var mpStatDef = assetManager.CreateAsset<StatDefinition>();
                            mpStatDef.Name = "Mana";
                            mpStatDef.Description = "The mana stat, used for casting spells and using special abilities.";
                            mpStatDef.DefaultValue = 50;
                            var spStatDef = assetManager.CreateAsset<StatDefinition>();
                            spStatDef.Name = "Stamina";
                            spStatDef.Description = "The stamina stat, used for performing physical actions like running or attacking.";
                            spStatDef.DefaultValue = 75;
                            
                            _statDefinitions.Add(hpStatDef);
                            _statDefinitions.Add(mpStatDef);
                            _statDefinitions.Add(spStatDef);
                            
                            pack.AddOrUpdateAsset(hpStatDef);
                            pack.AddOrUpdateAsset(mpStatDef);
                            pack.AddOrUpdateAsset(spStatDef);
                        });
                    }
                });
            };
        });
    }

    public override void OnWorldSetup(IEcsWorld world)
    {
        GetAllStats();
        PopulateStatDefIdToIndexCache();
        world.SystemManager.AddSystem(new StatSystem(_statDefIdToIndexCache, _resistanceDefIdToIndexCache, _regenerationStatIndexToTargetStatIndexCache));
    }

    private Dictionary<Ulid, int> _statDefIdToIndexCache = new Dictionary<Ulid, int>();
    private Dictionary<URN, int> _resistanceDefIdToIndexCache = new Dictionary<URN, int>();
    private Dictionary<Ulid, Ulid> _regenerationStatToTargetStatCache = new Dictionary<Ulid, Ulid>();
    private Dictionary<int, int> _regenerationStatIndexToTargetStatIndexCache = new Dictionary<int, int>();
    
    private void PopulateStatDefIdToIndexCache()
    {
        _statDefIdToIndexCache.Clear();
        _resistanceDefIdToIndexCache.Clear();
        _regenerationStatToTargetStatCache.Clear();
        for (int i = 0; i < _statDefinitions.Count; i++)
        {
            if(_statDefinitions[i] is ResistanceDefinition resistanceDef)
            {
                _resistanceDefIdToIndexCache[resistanceDef.DamageType] = i;
            }
            if(_statDefinitions[i] is RegenerationDefinition regenerationDef)
            {
                _regenerationStatToTargetStatCache[regenerationDef.Unique] = regenerationDef.TargetStat;
            }
            _statDefIdToIndexCache[_statDefinitions[i].Unique] = i;
        }
        
        foreach (var kvp in _regenerationStatToTargetStatCache)
        {
            var regenIndex = _statDefIdToIndexCache[kvp.Key];
            var targetIndex = _statDefIdToIndexCache[kvp.Value];
            _regenerationStatIndexToTargetStatIndexCache[regenIndex] = targetIndex;
        }
    }
    
    public override void OnInject(BufferedEntity entity, IEntityDefinition entityDefinition)
    {
        var comp = new StatComponent();
        foreach (var stat in _statDefinitions)
        {
            comp.Stats.Add(new StatData(stat.Unique, stat.DefaultValue, stat.DefaultValue, stat.CanBeNegative, stat.CapSettings, stat.TypeKind, stat.MinValue, stat.DefaultValue));
        }
        entity.AddComponent(comp);
    }
    
    public void GetAllStats()
    {
        if(_statDefinitions.Count > 0)
        {
            _statDefinitions.Clear();
        }
        
        var stats = EngineServices.AssetsManager.GetAssetsOfType<BaseStatDefinition>();
        _statDefinitions.Clear();
        _statDefinitions.AddRange(stats);
    }

    public void AddStat(BaseStatDefinition stat)
    {
        _statDefinitions.Add(stat);
    }
}

public struct StatComponent : IComponent
{
    public List<StatData> Stats { get; set; } // Key is the stat definition unique ID, value is the current value of the stat. It can be null if the stat is not initialized yet.
}

public record struct StatData(Ulid StatDefId, double BaseValue, double FinalValue, bool CanBeNegative, StatCapSettings CapSettings, EStatTypeKind TypeKind, double MinValue = 0, double ActualValue = 0);

public class StatSystem : ISystem
{
    public override int Priority => 100; // Priority can be adjusted based on when you want this system to run in the update loop.
    public override bool IsDrawingSystem => false; // This system is not responsible for drawing, it's purely for logic updates.
    
    private ComponentManager _componentManager = null!;
    private StatsModifierSystem? _statsModifierSystem = null!;
    private EcsEventBus _eventBus = null!;

    private Dictionary<Ulid, int> _statDefIdToIndexCache;
    private Dictionary<URN, int> _resistanceDefIdToIndexCache;
    private Dictionary<int, int> _regenerationStatIndexToTargetStatIndexCache;
    
    public StatSystem(
        Dictionary<Ulid, int> statDefIdToIndexCache,
        Dictionary<URN, int> resistanceDefIdToIndexCache,
        Dictionary<int, int> regenerationStatIndexToTargetStatIndexCache)
    {
        _statDefIdToIndexCache = statDefIdToIndexCache;
        _resistanceDefIdToIndexCache = resistanceDefIdToIndexCache;
        _regenerationStatIndexToTargetStatIndexCache = regenerationStatIndexToTargetStatIndexCache;
    }
    
    public override void Initialize(IEcsWorld ecsWorld)
    {
        _componentManager = ecsWorld.ComponentManager;
        _eventBus = ecsWorld.EventBus;
        ecsWorld.SystemManager.GetSystem<StatsModifierSystem>(out _statsModifierSystem);
        
        ecsWorld.EventBus.Subscribe((DamageEvent damageEvent) =>
        {
            Logger.Debug("Received DamageEvent for entity {0} with damage amount {1} and damage type {2}", damageEvent.TargetEntityId, damageEvent.DamageAmount, damageEvent.DamageType);
        });
    }
    
    public override void Update(TimeSpan deltaTime)
    {
        // Here you would implement the logic for updating stats, such as regenerating health or mana over time, applying damage, etc.
        // For example, you could loop through all entities with a StatComponent and apply regeneration based on the stat definition's properties.

        foreach (var entityId in _componentManager.QueryDirty<StatComponent>())
        {
            ref var statComponent = ref _componentManager.GetComponent<StatComponent>(entityId);

            if (_componentManager.HasComponent<StatsModifierComponent>(entityId))
            {
                ApplyModifier(entityId, statComponent);
            }
            ApplyCapSettings(entityId, statComponent);
            ApplyChangeToStat(entityId, statComponent);
        }
        double seconds = deltaTime.TotalSeconds;

        foreach (var entityId in _componentManager.Query<StatComponent>())
        {
            ref var statComp = ref _componentManager.GetComponent<StatComponent>(entityId);

            ApplyRegeneration(entityId, statComp, seconds);
        }
        
        _componentManager.ClearDirty<StatComponent>();
    }
    
    private void ApplyModifier(int entityId, StatComponent statComponent)
    {
        ref var modifierComponent = ref _componentManager.GetComponent<StatsModifierComponent>(entityId);

        foreach (ref var stat in CollectionsMarshal.AsSpan(statComponent.Stats))
        {
            var statId = stat.StatDefId;
            double baseValue = stat.BaseValue;

            if (modifierComponent.StatModifiers.TryGetValue(statId, out var blocks))
            {
                double totalFlat = SumFlatModifiers(blocks.FlatModifiersIdx);
                double totalPercent = SumPercentModifiers(blocks.PercentModifiersIdx);
                double totalMultiplier = SumMultiModifiers(blocks.MultiplierModifiersIdx);

                double finalValue = (baseValue + totalFlat) * (1 + totalPercent / 100) * totalMultiplier;
                if (!stat.CanBeNegative)
                    finalValue = Math.Max(0, finalValue);
                
                // If the stat cap is simply a fixed value, we apply it here.
                // If it's a cap settings by stat WE WAIT, the 'ApplyCapSettings' method will make it just after this.
                if (stat.CapSettings.CapType == EStatTypeCap.ByValue)
                {
                    finalValue = Math.Min(finalValue, stat.CapSettings.CapValue);
                }
                
                // Update the final value in the stat component
                stat.FinalValue = finalValue;
            }
        }
    }
    
    private double SumFlatModifiers(int flatModifiersIdx)
    {
        if (_statsModifierSystem == null)
        {
            Logger.Error("StatsModifierSystem is not initialized. Cannot sum flat modifiers.");
            return 0;
        }
        var span = _statsModifierSystem.FlatModifiers.GetSpan(flatModifiersIdx);
        double totalFlat = 0;

        // Utiliser une boucle for avec ref readonly évite la copie des structs
        for (int i = 0; i < span.Length; i++)
        {
            // On accède directement à la mémoire du Slab par référence
            ref readonly var modifier = ref span[i];
        
            // On ignore les slots vides (ModifierId == 0) si tu as désactivé le Swap-and-Pop
            // ou si tu as des "trous" dans tes Slabs.
            if (modifier.ModifierId != 0)
            {
                totalFlat += modifier.FlatValue;
            }
        }
    
        return totalFlat;
    }
    
    private double SumPercentModifiers(int percentModifiersIdx)
    {
        if (_statsModifierSystem == null)
        {
            Logger.Error("StatsModifierSystem is not initialized. Cannot sum percent modifiers.");
            return 0;
        }
        var span = _statsModifierSystem.PercentModifiers.GetSpan(percentModifiersIdx);
        double totalPercent = 0;

        for (int i = 0; i < span.Length; i++)
        {
            ref readonly var modifier = ref span[i];
        
            if (modifier.ModifierId != 0)
            {
                totalPercent += modifier.PercentValue;
            }
        }
    
        return totalPercent;
    }
    
    private double SumMultiModifiers(int multiplierModifiersIdx)
    {
        if (_statsModifierSystem == null)
        {
            Logger.Error("StatsModifierSystem is not initialized. Cannot sum multiplier modifiers.");
            return 0;
        }
        var span = _statsModifierSystem.MultiplierModifiers.GetSpan(multiplierModifiersIdx);
        double totalMultiplier = 1;

        for (int i = 0; i < span.Length; i++)
        {
            ref readonly var modifier = ref span[i];
        
            if (modifier.ModifierId != 0)
            {
                totalMultiplier *= modifier.MultiplierValue;
            }
        }
    
        return totalMultiplier;
    }
    
    private void ApplyCapSettings(int entityId, StatComponent statComponent)
    {
        var statSpan = CollectionsMarshal.AsSpan(statComponent.Stats);
        
        foreach (ref var stat in statSpan)
        {
            if(stat.CapSettings.CapType != EStatTypeCap.ByStat) continue;
            
            var capStatId = stat.CapSettings.CapStatUnique;
            var capStatIndex = _statDefIdToIndexCache[capStatId];
            var capValue = statSpan[capStatIndex].FinalValue;
            
            stat.FinalValue = Math.Min(stat.FinalValue, capValue);
        }
    }

    private void ApplyChangeToStat(int entityId, StatComponent statComponent)
    {
        var statSpan = CollectionsMarshal.AsSpan(statComponent.Stats);

        foreach (ref var stat in statSpan)
        {
            switch (stat.TypeKind)
            {
                case EStatTypeKind.Resource:
                {
                    double difference = Math.Max(0, stat.FinalValue - stat.ActualValue);
                    
                    stat.ActualValue = stat.FinalValue - difference;
                    
                    stat.ActualValue = Math.Clamp(stat.ActualValue, stat.MinValue, stat.FinalValue);
                    
                    if (stat.ActualValue <= stat.MinValue)
                    {
                        _eventBus.Publish(new ResourceReachedLimitEvent(entityId, stat.StatDefId));
                    }
                    break;
                }
                case EStatTypeKind.Attribute:
                {
                    stat.ActualValue = stat.FinalValue;
                    break;
                }
                case EStatTypeKind.Derived:
                { // Not implemented yet, but planned...
                    stat.ActualValue = stat.FinalValue;
                    break;
                }
            }
        }
    }

    private void ApplyRegeneration(int entityId, StatComponent statComponent, double seconds)
    {
        var span = CollectionsMarshal.AsSpan(statComponent.Stats);

        foreach (var (regenIdx, resourceIdx) in _regenerationStatIndexToTargetStatIndexCache)
        {
            ref var regenStat = ref span[regenIdx];
            ref var resourceStat = ref span[resourceIdx];
            
            if (resourceStat.ActualValue < resourceStat.FinalValue)
            {
                resourceStat.ActualValue += regenStat.FinalValue * seconds;
                
                if (resourceStat.ActualValue > resourceStat.FinalValue)
                    resourceStat.ActualValue = resourceStat.FinalValue;
                
                _componentManager.MarkDirty<StatComponent>(entityId);
            }
        }
        
    }

    public int GetResistance(URN damageType)
    {
        return _resistanceDefIdToIndexCache.GetValueOrDefault(damageType, -1); // No resistance found for this damage type, we return -1 as default (which means no resistance).
    }
}

#region EcsEvents
    
public readonly record struct DamageEvent(int TargetEntityId, double DamageAmount, URN DamageType);
public readonly record struct ResourceReachedLimitEvent(int TargetEntityId, Ulid StatDefId);
    
#endregion
