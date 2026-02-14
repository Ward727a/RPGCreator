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
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Stats;
using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.ECS;
using RPGCreator.SDK.ECS.Systems;
using RPGCreator.SDK.EngineService;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Modules.Features.Entity;
using RPGCreator.SDK.Types;
using RPGCreator.UI.Contexts;
using Ursa.Controls;
using Thickness = Avalonia.Thickness;

namespace _BaseModule.Features.Entity;

[EntityFeature]
public class StatsFeature : BaseEntityFeature
{
    public static readonly URN StatsTag = new URN("rpgc", TagsUrnModule, "stats");
    public static readonly URN Urn = FeatureUrnModule.ToUrnModule("rpgc").ToUrn("stats");
    
    public override string FeatureName => "Stats Feature";
    public override string FeatureDescription => "Adds basic stats to the entity, such as health, mana, and stamina.";
    public override URN FeatureUrn => Urn;

    private URN MakePath(string statName) => new URN("rpgc", "stats", statName);

    private readonly List<BaseStatDefinition> _statDefinitions = [];

    private HashSet<Ulid> _usingStats
    {
        get => GetConfig(new HashSet<Ulid>());
        set => SetConfig(value);
    }

    private Dictionary<Ulid, double> _statCustomDefaultValueCache
    {
        get => GetConfig(new Dictionary<Ulid, double>());
        set => SetConfig(value);
    }

    private Dictionary<Ulid, int> _statDefIdToIndexCache = new Dictionary<Ulid, int>();
    private Dictionary<URN, int> _resistanceDefIdToIndexCache = new Dictionary<URN, int>();
    private Dictionary<Ulid, Ulid> _regenerationStatToTargetStatCache = new Dictionary<Ulid, Ulid>();
    private Dictionary<int, int> _regenerationStatIndexToTargetStatIndexCache = new Dictionary<int, int>();
    
    public class StatToggleItem : UserControl
    {
        public event Action<Ulid, bool>? OnToggleChanged;
        public event Action<Ulid, double>? OnValueChanged;
        private StackPanel _panel = null!;
        private CheckBox _checkBox = null!;
        private TextBlock _textBlock = null!;
        private NumericDoubleUpDown _valueInput = null!;
        private Ulid? _statDefId = null;
        private string _statName = string.Empty;
        private double _currentValue = 0;
        private double _minValue = 0;
        private bool _isDerived;

        public StatToggleItem(BaseStatDefinition statDef, bool isToggled = false)
        {
            _statDefId = statDef.Unique;
            _statName = statDef.DisplayName;
            _currentValue = statDef.DefaultValue;
            _minValue = statDef.MinValue;
            _isDerived = statDef.TypeKind == EStatTypeKind.Derived;
            CreateComponents(isToggled);
            RegisterEvents();
        }

        private void CreateComponents(bool toggle)
        {
            
            _panel = new StackPanel() { Orientation = Orientation.Horizontal, Margin = new Thickness(5) };
            _checkBox = new CheckBox()
            {
                IsChecked = toggle,
                VerticalAlignment = VerticalAlignment.Center
            };
            _textBlock = new TextBlock()
            {
                Text = _statName,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5, 0, 0, 0),
                Cursor = new Cursor(StandardCursorType.Hand)
            };
            _valueInput = new NumericDoubleUpDown()
            {
                Minimum = _minValue,
                Value = _currentValue,
                Margin = new Thickness(10, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                IsEnabled = toggle
            };
            var label = new Label
            {
                Content = _textBlock,
                Target = _checkBox,
                Background = Brushes.Transparent
            };
            _textBlock.PointerPressed += (s, e) =>
            {
                _checkBox.IsChecked = !_checkBox.IsChecked;
            };

            _panel.Children.Add(_checkBox);
            _panel.Children.Add(label);
            if(!_isDerived)
                _panel.Children.Add(_valueInput);
            else
            {
                _panel.Children.Add(new TextBlock()
                {
                    Text="(Derived Stat - Default value can't be set)",
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(10, 0, 0, 0),
                    FontStyle = FontStyle.Italic,
                    Foreground = Brushes.Gray
                });
            }
            this.Content = _panel;
        }

        private void RegisterEvents()
        {
            _checkBox.IsCheckedChanged += (s, e) =>
            {
                if (_statDefId == null) return;
                OnToggleChanged?.Invoke(_statDefId.Value, _checkBox.IsChecked == true);
                
                _textBlock.TextDecorations = _checkBox.IsChecked == false ? TextDecorations.Strikethrough : null;
                _textBlock.Foreground = _checkBox.IsChecked == false ? Brushes.Gray : Brushes.White;
                _valueInput.IsEnabled = _checkBox.IsChecked == true;
            };

            _valueInput.ValueChanged += (s, e) =>
            {
                if (_statDefId == null) return;
                double newValue = _valueInput.Value ?? 0;
                if (newValue < _minValue)
                {
                    _valueInput.Value = _minValue;
                    newValue = _minValue;
                }

                OnValueChanged?.Invoke(_statDefId.Value, newValue);
            };
        }

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
                    else
                    {
                        GetAllStats();
                    }
                });
            };
        });
        var urn = ISignalRegistry.SignalModuleUrn.ToUrnModule("rpgc").ToUrn("stat_changed");
        RegistryServices.SignalRegistry.RegisterSignal(urn);
    }

    // When this feature is added to an entity definition, we want to add a toggle for each stat definition, allowing the user to choose which stats they want to use for this entity.
    // As the actual API of the editor doesn't allow us to put a specific UI for a property, we have to do it in this method, which is called when the feature is added to the definition, and we have access to the item control of the feature in the editor, which is a StackPanel that we can add controls to.
    public override void OnAddingToDefinition(IEntityDefinition definition, object controlOrContext)
    {
        var tempUsingStats = new HashSet<Ulid>(_usingStats);
        foreach (var statDef in _statDefinitions)
        {
            tempUsingStats.Add(statDef.Unique);
        }
        _usingStats = tempUsingStats;

        if (controlOrContext is not CharacterFeaturesEditorFeatureItemContext ctx) return;
        
        StackPanel panel = ctx.ExpanderContent;
                
        Expander statsExpander = new Expander() { Header = "Stats", IsExpanded = true };
        StackPanel statsPanel = new StackPanel() { Margin = new Thickness(10) };
        statsExpander.Content = statsPanel;
        panel.Children.Add(statsExpander);
                
        foreach (var stat in _statDefinitions)
        {
            var toggleItem = new StatToggleItem(stat, _usingStats.Contains(stat.Unique));
            statsPanel.Children.Add(toggleItem);
            toggleItem.OnToggleChanged += (statDefId, isToggled) =>
            {
                var tempUsingStats = new HashSet<Ulid>(_usingStats);
                switch (isToggled)
                {
                    case true:
                        tempUsingStats.Add(statDefId);
                        break;
                    case false:
                        tempUsingStats.Remove(statDefId);
                        break;
                }
                _usingStats = tempUsingStats;
            };
            toggleItem.OnValueChanged += (statDefId, newDefaultValue) =>
            {
                var tempCache = new Dictionary<Ulid, double>(_statCustomDefaultValueCache);
                tempCache[statDefId] = newDefaultValue;
                _statCustomDefaultValueCache = tempCache;
            };
        }
    }

    public override void OnWorldSetup(IEcsWorld world)
    {
        GetAllStats();
        PopulateStatDefIdToIndexCache();
        world.SystemManager.AddSystem(new StatSystem(_statDefIdToIndexCache, _resistanceDefIdToIndexCache, _regenerationStatIndexToTargetStatIndexCache));
        new BaseSubscriber(new URN("rpgc", "events", "test"), 0, @event =>
        {
            var modifier = EngineServices.AssetsManager.GetAssetsOfType<StatModifierDefinition>().ToList();

            if (modifier.Count == 0)
            {
                Logger.Debug("No StatModifierDefinition found, skipping test event.");
                return;
            }

            foreach (var entityId in world.ComponentManager.Query<PlayerTagComponent>())
            {
                world.SystemManager.GetSystem<StatsModifierSystem>(out var statsModifierSystem);
                statsModifierSystem?.ApplyStatModifier(entityId, modifier.First());
            }
        }).Subscribe();
    }
    
    private void PopulateStatDefIdToIndexCache()
    {
        _statDefIdToIndexCache.Clear();
        _resistanceDefIdToIndexCache.Clear();
        _regenerationStatToTargetStatCache.Clear();
        for (int i = 0; i < _statDefinitions.Count; i++)
        {
            _resistanceDefIdToIndexCache[_statDefinitions[i].Urn] = i;
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
        var tempStats = new List<StatData>(_statDefinitions.Count); 
    
        foreach (var stat in _statDefinitions)
        {
            if (!_usingStats.Contains(stat.Unique)) continue;
        
            var defaultValue = _statCustomDefaultValueCache.TryGetValue(stat.Unique, out var customDefault) 
                ? customDefault 
                : stat.DefaultValue;
        
            tempStats.Add(new StatData(stat.Unique, defaultValue, defaultValue, stat.CanBeNegative, 
                stat.CapSettings, stat.TypeKind, stat.MinValue, defaultValue));
        }
        var comp = new StatComponent();
        
        comp.Stats = tempStats.ToArray(); 
    
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
}

public struct StatComponent : IComponent
{
    public StatData[] Stats { get; set; } // Key is the stat definition unique ID, value is the current value of the stat. It can be null if the stat is not initialized yet.
}

public record struct StatData(Ulid StatDefId, double BaseValue, double FinalValue, bool CanBeNegative, StatCapSettings CapSettings, EStatTypeKind TypeKind, double MinValue = 0, double ActualValue = 0);

public class StatSystem : ISystem
{
    private URN SignalChanged = ISignalRegistry.SignalModuleUrn.ToUrnModule("rpgc").ToUrn("stat_changed");
    public override int Priority => 100; // Priority can be adjusted based on when you want this system to run in the update loop.
    public override bool IsDrawingSystem => false; // This system is not responsible for drawing, it's purely for logic updates.
    
    private ComponentManager _componentManager = null!;
    private StatsModifierSystem? _statsModifierSystem = null!;
    private EcsEventBus _eventBus = null!;
    private SystemManager _systemManager = null!;

    private Dictionary<Ulid, int> _statDefIdToIndexCache;
    private Dictionary<URN, int> _statDefUrnToIndexCache;
    private Dictionary<int, int> _regenerationStatIndexToTargetStatIndexCache;
    
    public StatSystem(
        Dictionary<Ulid, int> statDefIdToIndexCache,
        Dictionary<URN, int> statDefUrnToIndexCache,
        Dictionary<int, int> regenerationStatIndexToTargetStatIndexCache)
    {
        _statDefIdToIndexCache = statDefIdToIndexCache;
        _statDefUrnToIndexCache = statDefUrnToIndexCache;
        _regenerationStatIndexToTargetStatIndexCache = regenerationStatIndexToTargetStatIndexCache;
    }
    
    public override void Initialize(IEcsWorld ecsWorld)
    {
        _componentManager = ecsWorld.ComponentManager;
        _eventBus = ecsWorld.EventBus;
        _systemManager = ecsWorld.SystemManager;
        _systemManager.GetSystem<StatsModifierSystem>(out _statsModifierSystem);
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

            if (_componentManager.HasComponent<SignalsComponent>(entityId))
            {
                ref var signalComponent = ref _componentManager.GetComponent<SignalsComponent>(entityId);
                signalComponent.EmitSignal(SignalChanged);
                _componentManager.MarkDirty<SignalsComponent>(entityId);
            }
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

        foreach (ref var stat in statComponent.Stats.AsSpan())
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
            _systemManager.GetSystem<StatsModifierSystem>(out _statsModifierSystem);
            if(_statsModifierSystem == null)
                Logger.Error("StatsModifierSystem is not initialized. Cannot sum flat modifiers.");
            return 0;
        }
        var span = _statsModifierSystem.FlatModifiers.GetSpan(flatModifiersIdx);
        double totalFlat = 0;

        for (int i = 0; i < span.Length; i++)
        {
            ref readonly var modifier = ref span[i];
        
            totalFlat += modifier.FlatValue;
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
        var statSpan = statComponent.Stats.AsSpan();
        
        foreach (ref var stat in statSpan)
        {
            if(stat.CapSettings.CapType != EStatTypeCap.ByStat) continue;
            
            var capStatId = stat.CapSettings.CapStatUnique;
            var capStatIndex = _statDefIdToIndexCache[capStatId];
            if(statSpan.Length <= capStatIndex)
            {
                continue;
            }
            var capValue = statSpan[capStatIndex].FinalValue;
            
            stat.FinalValue = Math.Min(stat.FinalValue, capValue);
        }
    }

    private void ApplyChangeToStat(int entityId, StatComponent statComponent)
    {
        var statSpan = statComponent.Stats.AsSpan();
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
                        var id = stat.StatDefId;
                        _eventBus.Publish(Events.OnResourceReachedLimitEvent.EventId, entityId, data =>
                        {
                            data.Set("target", entityId);
                            data.Set("statId", id);
                        });
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
            var statDefId = stat.StatDefId;
            var finalValue = stat.FinalValue;
            var actualValue = stat.ActualValue;
            Logger.Debug("Stat changed for entity {0}, statDefId {1}: finalValue={2}, actualValue={3}", args: [entityId, statDefId, finalValue, actualValue]);
            _eventBus.Publish(new URN("rpgc", "events", "on_stat_changed"), entityId, data =>
            {
                data.Set("statDefId", statDefId);
                data.Set("finalValue", finalValue);
                data.Set("actualValue", actualValue);
            });
        }
    }

    private void ApplyRegeneration(int entityId, StatComponent statComponent, double seconds)
    {
        var span = statComponent.Stats.AsSpan();

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

    public int GetStatIndex(URN statUrn)
    {
        return _statDefUrnToIndexCache.GetValueOrDefault(statUrn, -1); // No stat found for this urn, we return -1 as default (which means no stat found).
    }
}

#region EcsEvents
    
public readonly record struct DamageEvent(int TargetEntityId, double DamageAmount, URN DamageType);
public readonly record struct ResourceReachedLimitEvent(int TargetEntityId, Ulid StatDefId);
    
#endregion
