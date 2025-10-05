using RPGCreator.Core.Runtimes.ECS;

namespace RPGCreator.Core.Type.Internal;

public sealed class ECSSparseSet<T> where T : struct, IComponent
{
    private List<T> dense = new();
    private List<int> sparse = new(); // entityId => dense index mapping
    private List<int> entities = new(); // dense => entityId mapping

    public int Count => dense.Count;

    public ECSSparseSet(int capacity = 16)
    {
        dense.Capacity = capacity;
        sparse.Capacity = capacity;
        entities.Capacity = capacity;
    }
    
    public void Add(int entityId, T component)
    {
        EnsureCapacity(entityId);

        if (Has(entityId))
        {
            dense[sparse[entityId]] = component; // écrase si déjà existant
            return;
        }

        sparse[entityId] = dense.Count;
        dense.Add(component);
        entities.Add(entityId);
    }
    
    public void Remove(int entityId)
    {
        if (!Has(entityId)) return;

        int index = sparse[entityId];
        int lastIndex = dense.Count - 1;

        dense[index] = dense[lastIndex];
        entities[index] = entities[lastIndex];

        sparse[entities[index]] = index;

        dense.RemoveAt(lastIndex);
        entities.RemoveAt(lastIndex);

        sparse[entityId] = -1;
    }
    public T Get(int entityId)
    {
        if (!Has(entityId)) throw new Exception("Entity has no component.");
        return dense[sparse[entityId]];
    }
    public bool Has(int entityId)
    {
        return entityId < sparse.Count && sparse[entityId] != -1;
    }
    public IEnumerable<(int entityId, T component)> ActiveElements()
    {
        for (int i = 0; i < dense.Count; i++)
            yield return (entities[i], dense[i]);
    }
    
    private void EnsureCapacity(int entityId)
    {
        while (sparse.Count <= entityId)
            sparse.Add(-1);
    }
}