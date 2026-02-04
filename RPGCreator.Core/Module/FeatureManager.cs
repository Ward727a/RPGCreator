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
using RPGCreator.SDK;
using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.ECS.Features;
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
    
    #region EntityFeature
    private readonly Dictionary<URN, IEntityFeature> _featuresTemplates = new();
    private readonly Dictionary<URN, Stack<IEntityFeature>> _featuresPools = new();
    private readonly Dictionary<URN, List<EntityFeaturePropertyMetadata>> _featureEntityPropertiesMetadata = new();
    private readonly Dictionary<URN, List<EntityFeatureAnimationRequirement>> _featureEntityAnimationMetadata = new();
    
    private readonly Dictionary<URN, List<Action<URN>>> _entityFeatureRegisteredCallbacks = new();
    
    /// <summary>
    /// First URN is the entity feature to be replaced.<br/>
    /// The list contains the URNs of the features that can replace it.<br/>
    /// </summary>
    private readonly Dictionary<URN, List<URN>> _entityFeatureReplacementsMapping = new();
    
    /// <summary>
    /// First URN is the replacing entity feature.<br/>
    /// The second URN is the entity feature being replaced.<br/>
    /// </summary>
    private readonly Dictionary<URN, URN> _entityFeatureReplacedByMapping = new();
    
    public FeatureManager()
    {
        EngineServices.OnceServiceReady((IFeaturesManager featureManager) =>
        {
            featureManager.RegisterEntityFeature<SpriteFeature>();
        });
    }
    
    public void RegisterEntityFeature<T>() where T : IEntityFeature, new()
    {
        var type = typeof(T);
        var attr = type.GetCustomAttributes<EntityFeatureAttribute>(true);
        T feature;
        try
        {
            feature = new T();
            feature.OnSetup();
        }
        catch (Exception ex)
        {
            Logger.Error("Failed to setup entity feature {featureUrn}: {exception}", args: [type.FullName ?? "No Type found", ex]);
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
        
        if (!_featureEntityAnimationMetadata.ContainsKey(feature.FeatureUrn))
        {
            var animAttrs = type.GetCustomAttributes<EntityAnimationRequiredAttribute>(true).ToList();
            _featureEntityAnimationMetadata[feature.FeatureUrn] = new List<EntityFeatureAnimationRequirement>();
            
            foreach (var animAttr in animAttrs)
            {
                _featureEntityAnimationMetadata[feature.FeatureUrn].Add(
                    new EntityFeatureAnimationRequirement(animAttr.DisplayName, animAttr.AnimationUrn, animAttr.AnimationDirection));
            }
        
            if (animAttrs.Count > 0)
            {
                Logger.Debug("Entity feature {feature} registered with {count} required animations.", 
                    args: [feature.FeatureUrn, animAttrs.Count]);
            }
        }

        var replacingFeature = attr.FirstOrDefault()?.ReplacingFeatureUrn;

        if (!string.IsNullOrEmpty(replacingFeature))
        {
            if (!_entityFeatureReplacementsMapping.ContainsKey(replacingFeature))
            {
                _entityFeatureReplacementsMapping[replacingFeature] = new List<URN>();
            }
            _entityFeatureReplacementsMapping[replacingFeature].Add(feature.FeatureUrn);
            _entityFeatureReplacedByMapping[feature.FeatureUrn] = replacingFeature;
            Logger.Debug("Entity feature {feature} is registered as a replacer for feature {replacedFeature}.", 
                args: [feature.FeatureUrn, replacingFeature]);
        }
        
        CallbackRegisteredEntityFeature(feature.FeatureUrn);
    }
    
    private void CallbackRegisteredEntityFeature(URN feature)
    {
        if (_entityFeatureRegisteredCallbacks.TryGetValue(feature, out var callbacks))
        {
            foreach (var callback in callbacks)
            {
                callback(feature);
            }
            _entityFeatureRegisteredCallbacks.Remove(feature);
        }
    }

    public void OnceEntityFeaturesRegistered(URN feature, Action<URN> callback)
    {
        if (HasEntityFeature(feature))
        {
            callback(feature);
            return;
        }
        
        if (!_entityFeatureRegisteredCallbacks.ContainsKey(feature))
        {
            _entityFeatureRegisteredCallbacks[feature] = new List<Action<URN>>();
        }
        _entityFeatureRegisteredCallbacks[feature].Add(callback);
    }

    /// <summary>
    /// Checks if an entity feature with the given URN is registered.
    /// </summary>
    /// <param name="featureUrn"></param>
    /// <returns></returns>
    public bool HasEntityFeature(URN featureUrn)
    {
        return _featuresTemplates.ContainsKey(featureUrn);
    }
    /// <summary>
    /// Return what this feature replaces, or <see cref="URN.Empty"/> if it does not replace anything.
    /// </summary>
    public URN GetEntityFeatureReplacedFeature(URN replacingFeature)
    {
        return _entityFeatureReplacedByMapping.TryGetValue(replacingFeature, out var replacedFeature)
            ? replacedFeature
            : URN.Empty;
    }
    /// <summary>
    /// Checks if the given entity feature is replacing another feature.
    /// </summary>
    /// <param name="featureToReplace"></param>
    /// <param name="replacingFeature"></param>
    /// <returns></returns>
    public bool IsEntityFeatureReplacing(URN featureToReplace, URN replacingFeature)
    {
        return _entityFeatureReplacementsMapping.TryGetValue(featureToReplace, out var replacers) &&
               replacers.Contains(replacingFeature) && _entityFeatureReplacedByMapping.ContainsKey(replacingFeature);
    }
    public void UnregisterEntityFeature(URN featureUrn)
    {
        _featuresTemplates.Remove(featureUrn);
        _featuresPools.Remove(featureUrn);
        _featureEntityPropertiesMetadata.Remove(featureUrn);
        _featureEntityAnimationMetadata.Remove(featureUrn);
        _entityFeatureReplacedByMapping.Remove(featureUrn);
    }
    public IEntityFeature GetEntityFeature(URN featureUrn)
    {
        return _featuresTemplates[featureUrn];
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
    public IEnumerable<EntityFeatureAnimationRequirement> GetEntityRequiredAnimations(URN feature)
    {
        return _featureEntityAnimationMetadata.TryGetValue(feature, out var attrs)
            ? attrs
            : Enumerable.Empty<EntityFeatureAnimationRequirement>();
    }
    public bool AddEntityFeatureAnimationRequirement(URN featureUrn, EntityFeatureAnimationRequirement requirement)
    {
        if (!_featureEntityAnimationMetadata.ContainsKey(featureUrn))
        {
            _featureEntityAnimationMetadata[featureUrn] = new List<EntityFeatureAnimationRequirement>();
        }

        var requirements = _featureEntityAnimationMetadata[featureUrn];
        if (requirements.Any(r => r.AnimUrn == requirement.AnimUrn && r.Direction == requirement.Direction))
        {
            return false; // Requirement already exists
        }

        requirements.Add(requirement);
        return true;
    }
    
    /// <summary>
    /// Returns all entity features that are macro features.
    /// </summary>
    /// <returns></returns>
    public List<IEntityFeature> GetAllMacroEntityFeatures()
    {
        return _featuresTemplates.Values
            .Where(f => f is BaseMacroEntityFeature)
            .ToList();
    }
    
    /// <summary>
    /// Returns all entity features that are not macro features.
    /// </summary>
    /// <returns></returns>
    public List<IEntityFeature> GetAllAtomicEntityFeatures()
    {
        return _featuresTemplates.Values
            .Where(f => f is not BaseMacroEntityFeature)
            .ToList();
    }
    
    /// <summary>
    /// Returns all registered entity features.
    /// </summary>
    /// <returns></returns>
    public List<IEntityFeature> GetAllEntityFeatures()
    {
        return _featuresTemplates.Values.ToList();
    }
    public void ClearAllEntityFeatures()
    {
        _featuresTemplates.Clear();
        _featuresPools.Clear();
        _featureEntityPropertiesMetadata.Clear();
    }
    
    #endregion
    
    #region WorldFeature
    private readonly Dictionary<URN, IWorldFeature> _worldFeatures = new();
    private readonly Dictionary<URN, List<WorldFeaturePropertyMetadata>> _featureWorldPropertiesMetadata = new();
    /// <summary>
    /// First URN is the world feature to be replaced.<br/>
    /// The list contains the URNs of the features that can replace it.<br/>
    /// </summary>
    private readonly Dictionary<URN, List<URN>> _worldFeatureReplacementsMapping = new();
    
    /// <summary>
    /// First URN is the replacing world feature.<br/>
    /// The second URN is the world feature being replaced.<br/>
    /// </summary>
    private readonly Dictionary<URN, URN> _worldFeatureReplacedByMapping = new();
    
    public void RegisterWorldFeature<T>() where T : IWorldFeature, new()
    {
        var type = typeof(T);
        var attr = type.GetCustomAttributes<WorldFeatureAttribute>(true);
        T feature;
        try
        {
            feature = new T();
            feature.OnSetup();
        }
        catch (Exception ex)
        {
            Logger.Error("Failed to setup world feature {featureUrn}: {exception}", args: [type.FullName ?? "No Type found", ex]);
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
        
        var replacingFeature = attr.FirstOrDefault()?.ReplacingFeatureUrn;
        
        if (!string.IsNullOrEmpty(replacingFeature))
        {
            if (!_worldFeatureReplacementsMapping.ContainsKey(replacingFeature))
            {
                _worldFeatureReplacementsMapping[replacingFeature] = new List<URN>();
            }
            _worldFeatureReplacementsMapping[replacingFeature].Add(feature.FeatureUrn);
            _worldFeatureReplacedByMapping[feature.FeatureUrn] = replacingFeature;
            Logger.Debug("World feature {feature} is registered as a replacer for feature {replacedFeature}.", 
                args: [feature.FeatureUrn, replacingFeature]);
        }
    }
    /// <summary>
    /// Checks if a world feature with the given URN is registered.
    /// </summary>
    /// <param name="featureUrn"></param>
    /// <returns></returns>
    public bool HasWorldFeature(URN featureUrn)
    {
        return _worldFeatures.ContainsKey(featureUrn);
    }
    /// <summary>
    /// Return what this world feature replaces, or <see cref="URN.Empty"/> if it does not replace anything.
    /// </summary>
    /// <param name="replacingFeature"></param>
    /// <returns></returns>
    public URN GetWorldFeatureReplacedFeature(URN replacingFeature)
    {
        return _worldFeatureReplacedByMapping.TryGetValue(replacingFeature, out var replacedFeature)
            ? replacedFeature
            : URN.Empty;
    }
    /// <summary>
    /// Checks if the given world feature is replacing another feature.
    /// </summary>
    /// <param name="featureToReplace"></param>
    /// <param name="replacingFeature"></param>
    /// <returns></returns>
    public bool IsWorldFeatureReplacing(URN featureToReplace, URN replacingFeature)
    {
        return _worldFeatureReplacementsMapping.TryGetValue(featureToReplace, out var replacers) &&
               replacers.Contains(replacingFeature) && _worldFeatureReplacedByMapping.ContainsKey(replacingFeature);
    }
    public void UnregisterWorldFeature(URN featureUrn)
    {
        _worldFeatures.Remove(featureUrn);
        _featureWorldPropertiesMetadata.Remove(featureUrn);
        _worldFeatureReplacedByMapping.Remove(featureUrn);
    }
    public IWorldFeature GetWorldFeature(URN featureUrn)
    {
        return _worldFeatures[featureUrn];
    }
    public bool TryGetWorldFeature(URN featureUrn, [NotNullWhen(true)] out IWorldFeature? feature)
    {
        return _worldFeatures.TryGetValue(featureUrn, out feature);
    }
    public IEnumerable<WorldFeaturePropertyMetadata> GetWorldProperties(URN feature)
    {
        return _featureWorldPropertiesMetadata.TryGetValue(feature, out var props)
            ? props
            : Enumerable.Empty<WorldFeaturePropertyMetadata>();
    }
    public List<IWorldFeature> GetAllWorldFeatures(bool inPriorityOrder = false)
    {
        return _worldFeatures.Values.ToList();
    }
    public void ClearAllWorldFeatures()
    {
        _worldFeatures.Clear();
        _featureWorldPropertiesMetadata.Clear();
    }
    
    #endregion
    
    #region GameFeature
    private readonly Dictionary<URN, IGameFeature> _gameFeatures = new();
    private readonly Dictionary<URN, List<GameFeaturePropertyMetadata>> _featureGamePropertiesMetadata = new();
    
    /// <summary>
    /// First URN is the game feature to be replaced.<br/>
    /// The list contains the URNs of the features that can replace it.<br/>
    /// </summary>
    private readonly Dictionary<URN, List<URN>> _gameFeatureReplacementsMapping = new();
    
    /// <summary>
    /// First URN is the replacing game feature.<br/>
    /// The second URN is the game feature being replaced.<br/>
    /// </summary>
    private readonly Dictionary<URN, URN> _gameFeatureReplacedByMapping = new();

    public void RegisterGameFeature<T>() where T : IGameFeature, new()
    {
        var type = typeof(T);
        var attr = type.GetCustomAttributes<GameFeatureAttribute>(true);
        T feature;
        try
        {
            feature = new T();
            feature.OnSetup();
        }
        catch (Exception ex)
        {
            Logger.Error("Failed to setup game feature {featureUrn}: {exception}", args: [type.FullName ?? "No Type found", ex]);
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
        
        var replacingFeature = attr.FirstOrDefault()?.ReplacingFeatureUrn;
        
        if (!string.IsNullOrEmpty(replacingFeature))
        {
            if (!_gameFeatureReplacementsMapping.ContainsKey(replacingFeature))
            {
                _gameFeatureReplacementsMapping[replacingFeature] = new List<URN>();
            }
            _gameFeatureReplacementsMapping[replacingFeature].Add(feature.FeatureUrn);
            _gameFeatureReplacedByMapping[feature.FeatureUrn] = replacingFeature;
            Logger.Debug("Game feature {feature} is registered as a replacer for feature {replacedFeature}.", 
                args: [feature.FeatureUrn, replacingFeature]);
        }
    }
    /// <summary>
    /// Checks if a game feature with the given URN is registered.
    /// </summary>
    /// <param name="featureUrn"></param>
    /// <returns></returns>
    public bool HasGameFeature(URN featureUrn)
    {
        return _gameFeatures.ContainsKey(featureUrn);
    }
    /// <summary>
    /// Return what this game feature replaces, or <see cref="URN.Empty"/> if it does not replace anything.
    /// </summary>
    /// <param name="replacingFeature"></param>
    /// <returns></returns>
    public URN GetGameFeatureReplacedFeature(URN replacingFeature)
    {
        return _gameFeatureReplacedByMapping.TryGetValue(replacingFeature, out var replacedFeature)
            ? replacedFeature
            : URN.Empty;
    }
    /// <summary>
    /// Checks if the given game feature is replacing another feature.
    /// </summary>
    /// <param name="featureToReplace"></param>
    /// <param name="replacingFeature"></param>
    /// <returns></returns>
    public bool IsGameFeatureReplacing(URN featureToReplace, URN replacingFeature)
    {
        return _gameFeatureReplacementsMapping.TryGetValue(featureToReplace, out var replacers) &&
               replacers.Contains(replacingFeature) && _gameFeatureReplacedByMapping.ContainsKey(replacingFeature);
    }
    public void UnregisterGameFeature(URN featureUrn)
    {
        _gameFeatures.Remove(featureUrn);
        _featureGamePropertiesMetadata.Remove(featureUrn);
        _gameFeatureReplacedByMapping.Remove(featureUrn);
    }
    public IGameFeature GetGameFeature(URN featureUrn)
    {
        return _gameFeatures[featureUrn];
    }
    public bool TryGetGameFeature(URN featureUrn, [NotNullWhen(true)] out IGameFeature? feature)
    {
        return _gameFeatures.TryGetValue(featureUrn, out feature);
    }
    public IEnumerable<GameFeaturePropertyMetadata> GetGameProperties(URN feature)
    {
        return _featureGamePropertiesMetadata.TryGetValue(feature, out var props)
            ? props
            : Enumerable.Empty<GameFeaturePropertyMetadata>();
    }
    public List<IGameFeature> GetAllGameFeatures(bool inPriorityOrder = false)
    {
        return _gameFeatures.Values.ToList();
    }
    public void ClearAllGameFeatures()
    {
        _gameFeatures.Clear();
        _featureGamePropertiesMetadata.Clear();
    }
    
    #endregion

    public void ClearAllFeatures()
    {
        ClearAllEntityFeatures();
        ClearAllWorldFeatures();
        ClearAllGameFeatures();
    }
}