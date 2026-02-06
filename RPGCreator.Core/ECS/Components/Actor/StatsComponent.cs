using RPGCreator.Core.Managers.AssetsManager.Registries;
using RPGCreator.Core.Types.Assets.Characters.Stats;
using RPGCreator.Core.Types.Assets.Entities.Characters.Stats;
using RPGCreator.SDK.ECS;

namespace RPGCreator.Core.Runtimes.ECS.Components.Actor;

public struct StatsComponent : IComponent
{
    
    private List<StatInstance> _stats = new();
    public IReadOnlyList<StatInstance> Stats => _stats;
    private Dictionary<string, int> _statNameToIndex { get; set; } = new();
    public IReadOnlyDictionary<string, int> StatNameToIndex => _statNameToIndex;
    public StatsComponent()
    {
        _stats = new List<StatInstance>();

        EngineCore.Instance.Managers.Assets.TryResolveRegistry("stats", out var assetRegistry);

        if (assetRegistry is not StatsRegistry statsRegistry) return;

        var statDefs = statsRegistry.All();
        
        foreach (var statDef in statDefs)
        {
            // Create a new StatInstance for each IStatDef and add it to the Stats list
            _stats.Add(new StatInstance(statDef));
            _statNameToIndex[statDef.Name] = Stats.Count - 1;
        }
    }
    
    public StatInstance? GetStatByName(string name)
    {
        if (_statNameToIndex.TryGetValue(name, out var index))
        {
            return _stats[index];
        }
        return null;
    }
    
    public StatInstance? GetStatByUnique(Ulid unique)
    {
        return _stats.FirstOrDefault(stat => stat.StatDefinitionId == unique);
    }
}