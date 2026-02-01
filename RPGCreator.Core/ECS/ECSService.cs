using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.ECS;
using RPGCreator.SDK.EngineService;
using RPGCreator.SDK.Modules.Features.Entity;
using RPGCreator.SDK.Types;
using Logger = RPGCreator.SDK.Logging.Logger;

namespace RPGCreator.Core.ECS;

public class EcsService : IEcsService
{
    private readonly Dictionary<URN, IEntityFeature> _featuresTemplates = new();
    private readonly Dictionary<URN, Stack<IEntityFeature>> _featuresPools = new();
    private readonly Dictionary<URN, List<EntityFeaturePropertyMetadata>> _featurePropertiesMetadata = new();
    
    
    public IEcsWorld CreateWorld()
    {
        return new EcsWorld();
    }

    public IEntityFeature CreateFeatureInstance(URN featureUrn)
    {
        if (_featuresPools.TryGetValue(featureUrn, out var pool) && pool.Count > 0)
        {
            var instance = pool.Pop();
            instance.Reset();
            return instance;
        }

        if (_featuresTemplates.TryGetValue(featureUrn, out var template))
        {
            return template.Clone();
        }
        
        throw new KeyNotFoundException($"Feature with URN {featureUrn} not found.");
    }
    
    public void ReturnToPool(BaseEntityFeature feature)
    {
        if(!HasFeature(feature.FeatureUrn))
            return;
        
        if (!_featuresPools.TryGetValue(feature.FeatureUrn, out var stack))
        {
            stack = new Stack<IEntityFeature>();
            _featuresPools[feature.FeatureUrn] = stack;
        }

        stack.Push(feature);
    }

    public List<IEntityFeature> GetAllFeatures()
    {
        return [.._featuresTemplates.Values];
    }

    public bool TryGetFeature(URN featureUrn, [NotNullWhen(true)] out IEntityFeature? feature)
    {
        if (!HasFeature(featureUrn))
        {
            feature = null;
            return false;
        }

        feature = CreateFeatureInstance(featureUrn);
        return true;
    }

    public void RegisterFeature(IEntityFeature feature)
    {
        var type = feature.GetType();
        feature.OnSetup();
        
        _featuresTemplates[feature.FeatureUrn] = feature;
        _featuresPools[feature.FeatureUrn] = new Stack<IEntityFeature>();
        
        if (!_featurePropertiesMetadata.ContainsKey(feature.FeatureUrn))
        {
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => new { 
                    Prop = p, 
                    Attr = p.GetCustomAttribute<EntityFeaturePropertyAttribute>() 
                })
                .Where(x => x.Attr != null)
                .Select(x => new EntityFeaturePropertyMetadata(x.Prop, x.Attr!, x.Prop.PropertyType))
                .ToList();

            _featurePropertiesMetadata[feature.FeatureUrn] = props;
            Logger.Debug("Registered feature {feature} with {count} editable properties.", feature.FeatureUrn, props.Count);
        }
    }
    
    public IEnumerable<EntityFeaturePropertyMetadata> GetEditableProperties(URN feature)
    {
        return _featurePropertiesMetadata.TryGetValue(feature, out var props) 
            ? props 
            : Enumerable.Empty<EntityFeaturePropertyMetadata>();
    }

    public bool HasFeature(URN featureUrn)
    {
        return _featuresTemplates.ContainsKey(featureUrn);
    }

    public void UnregisterFeature(URN featureUrn)
    {
        _featuresTemplates.Remove(featureUrn);
        _featuresPools.Remove(featureUrn);
    }

    public EntityStateRegistry StateRegistry { get; } = new();
}