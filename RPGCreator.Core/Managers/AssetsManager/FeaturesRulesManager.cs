using RPGCreator.SDK.Modules.Features.Entity;

namespace RPGCreator.Core.Managers.AssetsManager;

public class FeaturesRulesManager
{
    private readonly Dictionary<string, Func<IEntityFeature>> _featureFactories = new();

    public bool RegisterAutoFeature(string featureId, Func<IEntityFeature> featureFactory) => _featureFactories.TryAdd(featureId, featureFactory);
    
    public IEnumerable<IEntityFeature> GetAllAutoFeatures(List<string> tags)
    {
        
        foreach (var tag in tags)
        {
            if (_featureFactories.TryGetValue(tag, out var factory))
            {
                yield return factory();
            }
        }
        
    }
}