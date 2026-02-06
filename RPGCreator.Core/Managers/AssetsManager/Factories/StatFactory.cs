using RPGCreator.Core.Types.Assets.Characters.Stats;
using RPGCreator.Core.Types.Assets.Entities.Characters.Stats;
using RPGCreator.SDK.Assets;
using RPGCreator.SDK.Assets.Definitions.Stats;

namespace RPGCreator.Core.Managers.AssetsManager.Factories;

public class StatFactory : IAssetFactory<StatInstance, IStatDef>
{
    private readonly Dictionary<Ulid, StatInstance> _instances = [];
    
    public StatInstance Create(IStatDef def)
    {
        if (_instances.TryGetValue(def.Unique, out var foundedInstance))
        {
            return foundedInstance;
        }

        var instance = new StatInstance(def);
        _instances[def.Unique] = instance;
        return instance;
    }

    public ValueTask<StatInstance> CreateAsync(IStatDef def, CancellationToken ct = default)
    {
        return new ValueTask<StatInstance>(Create(def));
    }

    public void Refresh(IStatDef def)
    {
        if (_instances.TryGetValue(def.Unique, out var instance))
        {
            instance.Reload(def);
        }
    }

    public void Release(StatInstance instance)
    {
        if (_instances.ContainsValue(instance))
        {
            _instances.Remove(instance.StatDefinitionId);
        }
        else
        {
            throw new KeyNotFoundException($"Stat instance with unique ID {instance.RuntimeId} not found.");
        }
    }

    public void Release(IStatDef def)
    {
        if (_instances.ContainsKey(def.Unique))
        {
            _instances.Remove(def.Unique);
        }
        else
        {
            throw new KeyNotFoundException($"Tileset instance with unique ID {def.Unique} not found.");
        }
    }

    public void Clear()
    {
        _instances.Clear();
    }
}