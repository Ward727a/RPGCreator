using System.Diagnostics.CodeAnalysis;
using RPGCreator.Core.Runtimes.ECS;
using RPGCreator.SDK.ECS;

namespace RPGCreator.Core.Types.Internal;
public interface ISparseSet
{
    int Count { get; }
    ReadOnlySpan<int> EntitiesSpan { get; }
    bool Contains(int entityId);
}
public sealed class ECSSparseSet<T> : ISparseSet where T : IComponent
{
    private T[] dense;
    private int[] sparse;     // entityId => dense index
    private int[] entities;   // dense index => entityId
    private int count;
    
    public Span<T> ComponentsSpan => new Span<T>(dense, 0, count);
    public ReadOnlySpan<int> EntitiesSpan => new ReadOnlySpan<int>(entities, 0, count);
    public bool Contains(int entityId) => Has(entityId);


    public int Count => count;

    public ECSSparseSet(int capacity = 16)
    {
        dense = new T[capacity];
        sparse = new int[capacity];
        entities = new int[capacity];

        Array.Fill(sparse, -1);
        count = 0;
    }

    public ref T Add(int entityId, T component)
    {
        EnsureCapacity(entityId);

        if (Has(entityId))
        {
            dense[sparse[entityId]] = component;
            return ref dense[sparse[entityId]];
        }

        if (count == dense.Length)
            Grow();

        sparse[entityId] = count;
        dense[count] = component;
        entities[count] = entityId;
        count++;
        
        return ref dense[count - 1];
    }

    public void Remove(int entityId)
    {
        if (!Has(entityId))
            return;

        int index = sparse[entityId];
        int lastIndex = count - 1;
        int lastEntity = entities[lastIndex];

        // swap
        dense[index] = dense[lastIndex];
        entities[index] = lastEntity;
        sparse[lastEntity] = index;

        // nettoie
        sparse[entityId] = -1;
        count--;
    }
    
    public ref T Get(int entityId)
    {
        if (!Has(entityId))
            throw new Exception($"Entity has no component of type {typeof(T)}");
        return ref dense[sparse[entityId]];
    }

    public bool Has(int entityId)
    {
        return entityId < sparse.Length && sparse[entityId] != -1;
    }
    
    public IEnumerable<(int entityId, T component)> ActiveElements()
    {
        for (int i = 0; i < count; i++)
            yield return (entities[i], dense[i]);
    }

    private void EnsureCapacity(int entityId)
    {
        if (entityId < sparse.Length)
            return;

        int newSize = sparse.Length;
        while (newSize <= entityId)
            newSize *= 2;

        Array.Resize(ref sparse, newSize);
        Array.Fill(sparse, -1, count, newSize - count);
    }

    private void Grow()
    {
        int newSize = dense.Length * 2;
        Array.Resize(ref dense, newSize);
        Array.Resize(ref entities, newSize);
    }
}
