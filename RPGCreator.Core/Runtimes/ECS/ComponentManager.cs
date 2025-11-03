using System.Collections;
using RPGCreator.Core.Type.Internal;

namespace RPGCreator.Core.Runtimes.ECS;

public class ComponentManager(ECSEventBus eventBus)
{
    private ECSEventBus _eventBus { get; } = eventBus;
    private Dictionary<System.Type, object> _sparseSets = new();
    private Dictionary<System.Type, Action<int>> _removeActions = new();
    private readonly Dictionary<System.Type, HashSet<int>> _dirtyEntities = new();
    
    public ref T AddComponent<T>(IEntity entity) where T : IComponent, new()
    {
        var sparseSet = GetOrCreateSparseSet<T>();
        
        ref var component = ref sparseSet.Add(entity.Id, new T());
        
        MarkDirty<T>(entity.Id);
        _eventBus.Publish(new ComponentChangedEvent<T>(entity.Id, ChangeType.Added, default, component));
        
        return ref component;
    }
    
    public ref T GetComponent<T>(IEntity entity) where T : IComponent
    {
        var sparseSet = GetOrCreateSparseSet<T>();
        return ref sparseSet.Get(entity.Id);
    }
    
    public ref T GetComponent<T>(int entityId) where T : IComponent
    {
        var sparseSet = GetOrCreateSparseSet<T>();
        return ref sparseSet.Get(entityId);
    }

    public void RemoveComponent<T>(IEntity entity) where T : IComponent
    {
        var sparseSet = GetOrCreateSparseSet<T>();
        var bit = ComponentTypeRegistry.GetBit<T>();
        entity.ComponentBits[bit] = false;
        sparseSet.Remove(entity.Id);
        MarkDirty<T>(entity.Id);
        _eventBus.Publish(new ComponentChangedEvent<T>(entity.Id, ChangeType.Removed));
    }

    public IEnumerable<(int entityId, T component)> GetAll<T>() where T : IComponent
    {
        var set = GetOrCreateSparseSet<T>();
        return set.ActiveElements();
    }
    
    public IEnumerable<int> GetDirtyEntities<T>() where T : IComponent
    {
        if (_dirtyEntities.TryGetValue(typeof(T), out var list))
            return list;
        return Array.Empty<int>();
    }
    
    public IEnumerable<int> QueryDirty<T>() where T : IComponent
    {
        return QueryDirty(typeof(T));
    }
    
    public IEnumerable<int> QueryDirty<T1, T2>()
        where T1 : IComponent
        where T2 : IComponent
    {
        return QueryDirty(typeof(T1), typeof(T2));
    }
    
    public IEnumerable<int> QueryDirty<T1, T2, T3>()
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
    {
        return QueryDirty(typeof(T1), typeof(T2), typeof(T3));
    }
    
    public IEnumerable<int> QueryDirty(params System.Type[] componentTypes)
    {
        if (componentTypes == null || componentTypes.Length == 0)
            yield break;

        HashSet<int>? result = null;

        foreach (var type in componentTypes)
        {
            if (!_dirtyEntities.TryGetValue(type, out var currentDirty) || currentDirty.Count == 0)
            {
                yield break;
            }

            if (result == null)
            {
                result = new HashSet<int>(currentDirty);
            }
            else
            {
                result.IntersectWith(currentDirty);
                if (result.Count == 0)
                    yield break;
            }
        }

        if (result == null)
            yield break;

        foreach (var entityId in result.ToArray())
            yield return entityId;
    }
    
    public void ClearDirty<T>() where T : IComponent
    {
        var type = typeof(T);
        if (_dirtyEntities.TryGetValue(type, out var list))
        {
            list.Clear();
        }
    }
    
    private ECSSparseSet<T> GetOrCreateSparseSet<T>(T _ = default) where T : IComponent
    {
        var type = typeof(T);
        if (!_sparseSets.TryGetValue(type, out var set))
        {
            set = new ECSSparseSet<T>();
            _sparseSets[type] = set;
            _removeActions.TryAdd(type, (entityId) =>
            {
                var sparseSet = (ECSSparseSet<T>)set;
                sparseSet.Remove(entityId);
            });
        }
        
        return (ECSSparseSet<T>)set;
    }
    
    public IEnumerable<int> Query<T>() where T : IComponent
    {
        return Query(typeof(T));
    }
    
    public IEnumerable<int> Query<T1, T2>()
        where T1 : IComponent
        where T2 : IComponent
    {
        return Query(typeof(T1), typeof(T2));
    }
    
    public IEnumerable<int> Query<T1, T2, T3>()
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
    {
        return Query(typeof(T1), typeof(T2), typeof(T3));
    }
    
    public IEnumerable<int> Query(params System.Type[] componentTypes)
    {
        if (componentTypes == null || componentTypes.Length == 0)
            yield break;

        var sets = new List<ISparseSet>();
        foreach (var type in componentTypes)
        {
            if (_sparseSets.TryGetValue(type, out var obj) && obj is ISparseSet set)
                sets.Add(set);
            else
                yield break;
        }

        if (sets.Count == 0)
            yield break;

        var smallest = sets.OrderBy(s => s.Count).First();

        for (var index = 0; index < smallest.EntitiesSpan.Length; index++)
        {
            var entityId = smallest.EntitiesSpan[index];
            bool hasAll = true;
            for (int i = 0; i < sets.Count; i++)
            {
                if (!sets[i].Contains(entityId))
                {
                    hasAll = false;
                    break;
                }
            }

            if (hasAll)
                yield return entityId;
        }
    }

    public void MarkDirty<T>(int entityId) where T : IComponent
    {
        var type = typeof(T);
        if (!_dirtyEntities.TryGetValue(type, out var list))
        {
            list = new HashSet<int>();
            _dirtyEntities[type] = list;
        }
        list.Add(entityId);
    }
    
    // Called by EntityManager to remove all components of an entity
    public void RemoveAllComponents(int entityId, BitArray componentBits)
    {
        for (int i = 0; i < componentBits.Length; i++)
        {
            if (componentBits[i])
            {
                var type = ComponentTypeRegistry.GetType(i);
                if (type != null && _removeActions.TryGetValue(type, out var remove))
                    remove(entityId);
            }
        }
    }
}