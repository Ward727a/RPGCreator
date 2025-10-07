using System.Collections;
using RPGCreator.Core.Type.Internal;

namespace RPGCreator.Core.Runtimes.ECS;

public class ComponentManager
{
    private Dictionary<System.Type, object> _sparseSets = new();
    private Dictionary<System.Type, Action<int>> _removeActions = new();
    
    public ref T AddComponent<T>(IEntity entity) where T : struct, IComponent
    {
        var sparseSet = GetOrCreateSparseSet<T>();
        return ref sparseSet.Add(entity.Id, new T());
    }
    
    public ref T GetComponent<T>(IEntity entity) where T : struct, IComponent
    {
        var sparseSet = GetOrCreateSparseSet<T>();
        return ref sparseSet.Get(entity.Id);
    }
    
    public ref T GetComponent<T>(int entityId) where T : struct, IComponent
    {
        var sparseSet = GetOrCreateSparseSet<T>();
        return ref sparseSet.Get(entityId);
    }

    public void RemoveComponent<T>(IEntity entity) where T : struct, IComponent
    {
        var sparseSet = GetOrCreateSparseSet<T>();
        var bit = ComponentTypeRegistry.GetBit<T>();
        entity.ComponentBits[bit] = false;
        sparseSet.Remove(entity.Id);
    }

    public IEnumerable<(int entityId, T component)> GetAll<T>() where T : struct, IComponent
    {
        var set = GetOrCreateSparseSet<T>();
        return set.ActiveElements();
    }
    
    private ECSSparseSet<T> GetOrCreateSparseSet<T>() where T : struct, IComponent
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