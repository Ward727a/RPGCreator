using RPGCreator.Core.Runtimes.ECS;
using RPGCreator.Core.Type.Assets.Characters.Stats;

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

        var statDefs = EngineCore.Instance.Managers.Assets.StatsRegistry.All();
        
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