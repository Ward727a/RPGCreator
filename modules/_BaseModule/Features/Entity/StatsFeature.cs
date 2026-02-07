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
    
    public override string FeatureName => "Stats Feature";
    public override string FeatureDescription => "Adds basic stats to the entity, such as health, mana, and stamina.\n" +
                                                 "This will also enable a custom assets menu for stats management.";
    public override URN FeatureUrn => new URN("rpgc", FeatureUrnModule, "stats");

    private URN MakePath(string statName) => new URN("rpgc", "stats", statName);

    private readonly List<BaseStatDefinition> _statDefinitions = [];
    
    private IGlobalPathData _pathsData = null!;

    [EntityFeatureProperty(
        "Optimize Stats When Launched",
        "If enabled, the engine will optimize the stats data when the game is launched, which can improve performance, at the cost of not being able to modify the stats data at runtime.\n" +
        "This is recommended for release builds, but can be disabled for development builds to allow for more flexibility when testing and debugging.", IsShared = true)]
    public bool OptimizeStatsWhenLaunched
    {
        get => GetShared(true);
        set => SetShared(value);
    }
    
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
        world.SystemManager.AddSystem(new StatSystem(OptimizeStatsWhenLaunched));
    }

    public override void OnInject(BufferedEntity entity, IEntityDefinition entityDefinition)
    {
        GetAllStats();
        foreach (var stat in _statDefinitions)
        {
            entity.AddComponent(new StatComponent()
            {
                StatDefId = stat.Unique,
                CurrentValue = stat.DefaultValue
            });
        }
    }
    
    public void GetAllStats()
    {
        if(_pathsData.TryGetValues(StatsTag, out var statIds))
        {
            if (((HashSet<Ulid>)statIds).Count == _statDefinitions.Count)
                return;
            _statDefinitions.Clear();
            foreach (var stat in statIds)
            {
                if (EngineServices.AssetsManager.TryResolveAsset(stat, out BaseStatDefinition? statDef))
                {
                    _statDefinitions.Add(statDef);
                }
            }
        }
    }

    public void AddStat(BaseStatDefinition stat)
    {
        _statDefinitions.Add(stat);
        _pathsData.RegisterPath(MakePath(stat.Name), stat.Unique, StatsTag);
    }

}

public struct StatComponent : IComponent
{
    /// <summary>
    /// Reference to the stat definition, which contains the stat's name, description, icon, etc.
    /// </summary>
    public Ulid StatDefId;
    
    /// <summary>
    /// The current value of the stat. This can be modified by the game logic, such as when the entity takes damage or uses a skill that consumes mana.
    /// </summary>
    public double CurrentValue;
}

public class StatSystem : ISystem
{
    public override int Priority => 100; // Priority can be adjusted based on when you want this system to run in the update loop.
    public override bool IsDrawingSystem => false; // This system is not responsible for drawing, it's purely for logic updates.
    
    private ComponentManager _componentManager = null!;

    private Dictionary<Ulid, BaseStatDefinition> _cacheTemplatedStats = new();
    
    private HashSet<Ulid> _entitiesWithMissingStats = new();

    private readonly bool _shouldOptimize;
    private bool _isOptimized = false;

    private Func<StatComponent, bool> _updateAction;
    
    public StatSystem(bool shouldOptimize)
    {
        _shouldOptimize = shouldOptimize;
        _updateAction = UpdateStatsTemplate;
    }
    
    public override void Initialize(IEcsWorld ecsWorld)
    {
        _componentManager = ecsWorld.ComponentManager;
        
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
            if (!_updateAction(statComponent))
                continue; // If the update action returns false, it means that the stat definition is missing, so we skip updating this stat for now.
            
            // Here we can apply any logic we want to update the stat, such as regeneration or decay over time.
            
        }

        if (_shouldOptimize && !_isOptimized)
        {
            _updateAction = CheckMissingStats;
            _isOptimized = true;
        }
    }
    
    
    #region Helpers

    private bool UpdateStatsTemplate(StatComponent statComponent)
    {
        if(!_cacheTemplatedStats.TryGetValue(statComponent.StatDefId, out var statDef))
        {
            if(EngineServices.AssetsManager.TryResolveAsset(statComponent.StatDefId, out BaseStatDefinition? resolvedStatDef))
            {
                statDef = resolvedStatDef;
                _cacheTemplatedStats[statComponent.StatDefId] = statDef;
                return true;
            }
        }
        _entitiesWithMissingStats.Add(statComponent.StatDefId);
        return false;
    }

    private bool CheckMissingStats(StatComponent statComponent)
    {
        if(_entitiesWithMissingStats.Contains(statComponent.StatDefId))
            return false;
        return true;
    }
    
    #endregion

    #region EcsEvents
    
    public readonly record struct DamageEvent(int TargetEntityId, double DamageAmount, URN DamageType);
    
    #endregion
}