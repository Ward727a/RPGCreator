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

using System.Runtime.CompilerServices;
using CommunityToolkit.Diagnostics;
using RPGCreator.SDK.Assets.Definitions.Characters;
using RPGCreator.SDK.ECS;
using RPGCreator.SDK.ECS.Components;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Modules.Definition;
using RPGCreator.SDK.Types;

namespace RPGCreator.SDK.Modules.Features.Entity;

public abstract class BaseMacroEntityFeature : BaseEntityFeature
{
    
    /// <summary>
    /// Just the module URN where all entity macro features are stored.<br/>
    /// This is mainly a constant to avoid hardcoding the string everywhere.
    /// </summary>
    protected const string MacroFeatureUrnModule = "entity_macro_features";

    /// <summary>
    /// The unique identifier for this macro feature instance.
    /// </summary>
    protected Ulid MacroFeatureId;
    
    /// <summary>
    /// A list of required features for this macro feature.<br/>
    /// Those will be put automatically when this feature is added to an entity.
    /// </summary>
    private readonly Dictionary<int, IEntityFeature> _requiredFeatures = new();
    private readonly Dictionary<IEntityFeature, CustomData> _featureData = new();
    
    /// <summary>
    /// A read-only list of required features for this macro feature.<br/>
    /// Those will be put automatically when this feature is added to an entity.
    /// </summary>
    public IReadOnlyList<IEntityFeature> RequiredFeatures => _requiredFeatures.Values.ToList().AsReadOnly();
    
    /// <summary>
    /// A mapping of feature URNs to their respective index in the entity's feature list.<br/>
    /// This allows for quick access to specific features within the macro feature context.
    /// </summary>
    private readonly Dictionary<URN, int> _featureToIndex = new();
    
    private Dictionary<int, EntityFeatureData> _addedFeatureData = new();
    private List<IEntityFeature> _activeFeatures = new();

    private int _nextIndex = 0;
    
    /// <summary>
    /// Allow to define features that cannot be present at the same time as this feature.<br/>
    /// For exemple, a "living being" macro feature could conflict with an "equipable item" macro feature.<br/>
    /// A living being cannot be an equipable item and vice versa.<br/>
    /// This can also be a list of features (not macro features) that cannot be present at the same time as this macro feature.
    /// </summary>
    public readonly List<URN> ConflictingFeatures = new();
    
    public void RegisterConflictingFeature(URN featureUrn)
    {
        Guard.IsNotNull(featureUrn, nameof(featureUrn));
        ConflictingFeatures.Add(featureUrn);
    }

    public bool IsConflictingFeature(URN featureUrn)
    {
        return ConflictingFeatures.Contains(featureUrn);
    }
    
    public void UnregisterConflictingFeature(URN featureUrn)
    {
        Guard.IsNotNull(featureUrn, nameof(featureUrn));
        ConflictingFeatures.Remove(featureUrn);
    }
    
    public void RegisterSubFeature(URN featureUrn) => RegisterSubFeature(featureUrn, -1);
    
    public void RegisterSubFeature(URN featureUrn, int specificIndex)
    {
        if (!EngineServices.FeaturesManager.HasEntityFeature(featureUrn))
        {
            Logger.Error($"Cannot register sub-feature '{featureUrn}' to macro feature '{FeatureUrn}': feature not found.");
            return;
        }

        if (IsConflictingFeature(featureUrn))
        {
            Logger.Error($"Cannot register sub-feature '{featureUrn}' to macro feature '{FeatureUrn}': feature is marked as conflicting.");
            return;
        }
        var feature = EngineServices.FeaturesManager.GetEntityFeature(featureUrn);
        RegisterSubFeature(feature, new CustomData(), specificIndex);
    }
    
    public void RegisterSubFeature(IEntityFeature feature, CustomData configuration, int specificIndex = -1)
    {
        Guard.IsNotNull(feature, nameof(feature));

        if (IsConflictingFeature(feature.FeatureUrn))
        {
            Logger.Error($"Cannot register sub-feature '{feature.FeatureUrn}' to macro feature '{FeatureUrn}': feature is marked as conflicting.");
            return;
        }
        
        var index = specificIndex;
        
        // We are doing this, so that if the macro_feature add 1 feature with automatic index (0), a specific index (2), then another automatic index (3).
        // Without this system, the last automatic index would be 1, and if the user try to add another automatic index, it would be 2, conflicting with the specific index.
        if (index == -1 || index < _nextIndex)
            index = _nextIndex++;
        else
            _nextIndex = index + 1;

        if (index != specificIndex && specificIndex != -1)
        {
            Logger.Warning("The specific index ({specific_index}) provided for the sub-feature '{feature_urn}' in macro feature '{macro_feature_urn}' is less than the next available index ({next_index}). " +
                           "The index has been adjusted to avoid conflicts.", specificIndex, feature.FeatureUrn, FeatureUrn, _nextIndex);
        }
        
        
        _featureData.Add(feature, configuration);
        _requiredFeatures.Add(index, feature);
        _featureToIndex.Add(feature.FeatureUrn, index);
    }

    public void UnregisterSubFeature(URN featureUrn)
    {
        if (!_featureToIndex.TryGetValue(featureUrn, out var index))
            throw new KeyNotFoundException($"The feature URN '{featureUrn}' is not registered as a required feature in the macro feature '{FeatureUrn}'.");

        var feature = _requiredFeatures[index];
        _featureData.Remove(feature);
        _requiredFeatures.Remove(index);
        _featureToIndex.Remove(featureUrn);
    }
    
    public IEntityFeature GetRequiredFeature(URN featureUrn)
    {
        if (!_featureToIndex.TryGetValue(featureUrn, out var index))
            throw new KeyNotFoundException($"The feature URN '{featureUrn}' is not registered as a required feature in the macro feature '{FeatureUrn}'.");

        return _requiredFeatures[index];
    }
    
    public CustomData GetRequiredFeatureData(URN featureUrn)
    {
        var featureType = GetRequiredFeature(featureUrn);
        return _featureData[featureType];
    }
    
    public bool HasRequiredFeature(URN featureUrn)
    {
        return _featureToIndex.ContainsKey(featureUrn);
    }

    public void RemoveRequiredFeature(URN featureUrn)
    {
        if (!_featureToIndex.TryGetValue(featureUrn, out var index))
            throw new KeyNotFoundException($"The feature URN '{featureUrn}' is not registered as a required feature in the macro feature '{FeatureUrn}'.");

        _requiredFeatures.Remove(index);
        _featureToIndex.Remove(featureUrn);
    }

    public void SetConfiguration(URN featureUrn, CustomData configuration)
    {
        var featureType = GetRequiredFeature(featureUrn);
        _featureData[featureType] = configuration;
    }
    
    public void SetConfigurationValue<T>(URN featureUrn, T value, [CallerMemberName] string key = "")
    {
        var featureData = GetRequiredFeatureData(featureUrn);
        featureData.Set(key, value);
    }

    public T GetConfigurationValue<T>(URN featureUrn, T defaultValue, [CallerMemberName] string key = "")
    {
        var featureData = GetRequiredFeatureData(featureUrn);
        return featureData.GetAsOrDefault(key, defaultValue);
    }
    
    public void SetSharedConfigurationValue<T>(URN featureUrn, T value, [CallerMemberName] string key = "")
    {
        SharedDataFeatures.SetValueShared(featureUrn, key, value);
    }
    
    public T GetSharedConfigurationValue<T>(URN featureUrn, T defaultValue, [CallerMemberName] string key = "")
    {
        return SharedDataFeatures.GetValueShared(featureUrn, key, defaultValue);
    }
    
    private EntityFeatureData MakeFeatureData(IEntityFeature feature)
    {
        var featureData = new EntityFeatureData(feature.FeatureUrn, _featureData[feature]);
        featureData.IsSubFeature = true;
        featureData.ParentFeatureId = MacroFeatureId;
        return featureData;
    }

    public override bool OnAddedToDefinition(IEntityDefinition definition)
    {
        MacroFeatureId = Ulid.NewUlid();

        if (definition.Features.Any(f => IsConflictingFeature(f.FeatureUrn)))
        {
            Logger.Error($"Cannot add macro feature '{FeatureUrn}' to entity definition '{definition.Name}': conflicting features are already present.");
            return false;
        }

        foreach (var feature in _requiredFeatures.Values)
        {
            feature.OnAddedToDefinition(definition);
            var featureData = MakeFeatureData(feature);
            definition.Features.Add(featureData);
            
            var idx = _featureToIndex[feature.FeatureUrn];
            _addedFeatureData.Add(idx, featureData);
        }

        return true;
    }

    public override void OnRemovedFromDefinition(IEntityDefinition definition)
    {
        foreach (var feature in _requiredFeatures.Values)
        {
            feature.OnRemovedFromDefinition(definition);
            var idx = _featureToIndex[feature.FeatureUrn];
            var featureData = _addedFeatureData[idx];
            definition.Features.Remove(featureData);
        }
        _addedFeatureData.Clear();
    }

    public override void OnInject(BufferedEntity entity, IEntityDefinition entityDefinition)
    {
        entity.AddComponent(new CharStateComponent
        {
            AnimationsMapping = entityDefinition.AnimationsMapping
        });
    }

    public override void OnDestroy(BufferedEntity entity)
    {
        foreach (var feature in _activeFeatures)
        {
            feature.OnDestroy(entity);
        }
        _activeFeatures.Clear();
    }
}