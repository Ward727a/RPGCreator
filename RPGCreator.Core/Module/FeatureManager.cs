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

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.EngineService;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Modules.Features.Entity;
using RPGCreator.SDK.Modules.Features.Game;
using RPGCreator.SDK.Modules.Features.World;
using RPGCreator.SDK.Types;

namespace RPGCreator.Core.Module;

public class FeatureManager : IFeaturesManager
{
    private static readonly ScopedLogger Logger = SDK.Logging.Logger.ForContext<FeatureManager>();
    
    private readonly Dictionary<URN, IEntityFeature> _featuresTemplates = new();
    private readonly Dictionary<URN, Stack<IEntityFeature>> _featuresPools = new();
    private readonly Dictionary<URN, List<EntityFeaturePropertyMetadata>> _featureEntityPropertiesMetadata = new();
    
    private readonly Dictionary<URN, IWorldFeature> _worldFeatures = new();
    private readonly Dictionary<URN, List<WorldFeaturePropertyMetadata>> _featureWorldPropertiesMetadata = new();
    
    private readonly Dictionary<URN, IGameFeature> _gameFeatures = new();
    private readonly Dictionary<URN, List<GameFeaturePropertyMetadata>> _featureGamePropertiesMetadata = new();
    
    public void RegisterEntityFeature<T>() where T : IEntityFeature, new()
    {
        var type = typeof(T);
        T feature;
        try
        {
            feature = new T();
            feature.OnSetup();
        }
        catch (Exception ex)
        {
            Logger.Error("Failed to setup entity feature {featureUrn}: {exception}", args: [type.FullName, ex]);
            return;
        }
        
        _featuresTemplates[feature.FeatureUrn] = feature;
        _featuresPools[feature.FeatureUrn] = new Stack<IEntityFeature>();
        
        if (!_featureEntityPropertiesMetadata.ContainsKey(feature.FeatureUrn))
        {
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => new { 
                    Prop = p, 
                    Attr = p.GetCustomAttribute<EntityFeaturePropertyAttribute>() 
                })
                .Where(x => x.Attr != null)
                .Select(x => new EntityFeaturePropertyMetadata(x.Prop, x.Attr!, x.Prop.PropertyType))
                .ToList();

            _featureEntityPropertiesMetadata[feature.FeatureUrn] = props;
            Logger.Debug("Registered feature {feature} with {count} editable properties.", args: [feature.FeatureUrn, props.Count]);
        }
    }

    public void RegisterWorldFeature<T>() where T : IWorldFeature, new()
    {
        var type = typeof(T);
        T feature;
        try
        {
            feature = new T();
            feature.OnSetup();
        }
        catch (Exception ex)
        {
            Logger.Error("Failed to setup world feature {featureUrn}: {exception}", args: [type.FullName, ex]);
            return;
        }
        _worldFeatures[feature.FeatureUrn] = feature;
        
        if (!_featureWorldPropertiesMetadata.ContainsKey(feature.FeatureUrn))
        {
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => new { 
                    Prop = p, 
                    Attr = p.GetCustomAttribute<WorldFeaturePropertyAttribute>() 
                })
                .Where(x => x.Attr != null)
                .Select(x => new WorldFeaturePropertyMetadata(x.Prop, x.Attr!, x.Prop.PropertyType))
                .ToList();

            _featureWorldPropertiesMetadata[feature.FeatureUrn] = props;
            Logger.Debug("Registered world feature {feature} with {count} editable properties.", args: [feature.FeatureUrn, props.Count]);
        }
    }

    public void RegisterGameFeature<T>() where T : IGameFeature, new()
    {
        var type = typeof(T);
        T feature;
        try
        {
            feature = new T();
            feature.OnSetup();
        }
        catch (Exception ex)
        {
            Logger.Error("Failed to setup game feature {featureUrn}: {exception}", args: [type.FullName, ex]);
            return;
        }

        _gameFeatures[feature.FeatureUrn] = feature;
        
        if (!_featureGamePropertiesMetadata.ContainsKey(feature.FeatureUrn))
        {
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => new { 
                    Prop = p, 
                    Attr = p.GetCustomAttribute<GameFeaturePropertyAttribute>() 
                })
                .Where(x => x.Attr != null)
                .Select(x => new GameFeaturePropertyMetadata(x.Prop, x.Attr!, x.Prop.PropertyType))
                .ToList();

            _featureGamePropertiesMetadata[feature.FeatureUrn] = props;
            Logger.Debug("Registered game feature {feature} with {count} editable properties.", args: [feature.FeatureUrn, props.Count]);
        }
    }

    public bool HasEntityFeature(URN featureUrn)
    {
        return _featuresTemplates.ContainsKey(featureUrn);
    }

    public bool HasWorldFeature(URN featureUrn)
    {
        return _worldFeatures.ContainsKey(featureUrn);
    }

    public bool HasGameFeature(URN featureUrn)
    {
        return _gameFeatures.ContainsKey(featureUrn);
    }

    public void UnregisterEntityFeature(URN featureUrn)
    {
        _featuresTemplates.Remove(featureUrn);
        _featuresPools.Remove(featureUrn);
        _featureEntityPropertiesMetadata.Remove(featureUrn);
    }

    public void UnregisterWorldFeature(URN featureUrn)
    {
        _worldFeatures.Remove(featureUrn);
        _featureWorldPropertiesMetadata.Remove(featureUrn);
    }

    public void UnregisterGameFeature(URN featureUrn)
    {
        _gameFeatures.Remove(featureUrn);
        _featureGamePropertiesMetadata.Remove(featureUrn);
    }

    public IEntityFeature GetEntityFeature(URN featureUrn)
    {
        return _featuresTemplates[featureUrn];
    }

    public IWorldFeature GetWorldFeature(URN featureUrn)
    {
        return _worldFeatures[featureUrn];
    }

    public IGameFeature GetGameFeature(URN featureUrn)
    {
        return _gameFeatures[featureUrn];
    }

    public bool TryGetEntityFeature(URN featureUrn, [NotNullWhen(true)] out IEntityFeature? feature)
    {
        if (HasEntityFeature(featureUrn))
        {
            feature = CreateEntityFeatureInstance(featureUrn);
            return true;
        }
        feature = null;
        return false;
    }

    public bool TryGetWorldFeature(URN featureUrn, [NotNullWhen(true)] out IWorldFeature? feature)
    {
        return _worldFeatures.TryGetValue(featureUrn, out feature);
    }

    public bool TryGetGameFeature(URN featureUrn, [NotNullWhen(true)] out IGameFeature? feature)
    {
        return _gameFeatures.TryGetValue(featureUrn, out feature);
    }

    public void ReturnFeatureEntityToPool(IEntityFeature feature)
    {
        
        if(!HasEntityFeature(feature.FeatureUrn))
            return;
        
        if (!_featuresPools.TryGetValue(feature.FeatureUrn, out var stack))
        {
            stack = new Stack<IEntityFeature>();
            _featuresPools[feature.FeatureUrn] = stack;
        }

        stack.Push(feature);
    }

    public IEntityFeature CreateEntityFeatureInstance(URN featureUrn)
    {
        if (_featuresPools.TryGetValue(featureUrn, out var pool) && pool.Count > 0)
        {
            var instance = pool.Pop();
            instance.Reset();
            return instance;
        }

        if (_featuresTemplates.TryGetValue(featureUrn, out var template))
        {
            try
            {
                return template.Clone();
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to clone entity feature {featureUrn}: {exception}", args: [featureUrn, ex]);
                throw;
            }
        }
        
        throw new KeyNotFoundException($"Feature with URN {featureUrn} not found.");
    }

    public IEnumerable<EntityFeaturePropertyMetadata> GetEntityProperties(URN feature)
    {
        return _featureEntityPropertiesMetadata.TryGetValue(feature, out var props)
            ? props
            : Enumerable.Empty<EntityFeaturePropertyMetadata>();
    }

    public IEnumerable<WorldFeaturePropertyMetadata> GetWorldProperties(URN feature)
    {
        return _featureWorldPropertiesMetadata.TryGetValue(feature, out var props)
            ? props
            : Enumerable.Empty<WorldFeaturePropertyMetadata>();
    }

    public IEnumerable<GameFeaturePropertyMetadata> GetGameProperties(URN feature)
    {
        return _featureGamePropertiesMetadata.TryGetValue(feature, out var props)
            ? props
            : Enumerable.Empty<GameFeaturePropertyMetadata>();
    }

    public List<IEntityFeature> GetAllEntityFeatures()
    {
        return _featuresTemplates.Values.ToList();
    }

    public List<IWorldFeature> GetAllWorldFeatures(bool inPriorityOrder = false)
    {
        return _worldFeatures.Values.ToList();
    }

    public List<IGameFeature> GetAllGameFeatures(bool inPriorityOrder = false)
    {
        return _gameFeatures.Values.ToList();
    }

    public void ClearAllEntityFeatures()
    {
        _featuresTemplates.Clear();
        _featuresPools.Clear();
        _featureEntityPropertiesMetadata.Clear();
    }

    public void ClearAllWorldFeatures()
    {
        _worldFeatures.Clear();
        _featureWorldPropertiesMetadata.Clear();
    }

    public void ClearAllGameFeatures()
    {
        _gameFeatures.Clear();
        _featureGamePropertiesMetadata.Clear();
    }

    public void ClearAllFeatures()
    {
        ClearAllEntityFeatures();
        ClearAllWorldFeatures();
        ClearAllGameFeatures();
    }
}