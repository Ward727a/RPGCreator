namespace RPGCreator.Core.Types.Internal;

public sealed class SparseSet<T>
{
    private List<T> dense = new();
    private List<int> sparse = new();

    public int Count => dense.Count;

    public SparseSet(int capacity = 16)
    {
        dense.Capacity = capacity;
        sparse.Capacity = capacity;
    }

    public int Add(T item)
    {
        int id = sparse.Count;
        sparse.Add(dense.Count);
        dense.Add(item);
        return id;
    }

    public void Remove(int id)
    {
        if (id >= sparse.Count) return;

        int index = sparse[id];
        int lastIndex = dense.Count - 1;

        dense[index] = dense[lastIndex];
        dense.RemoveAt(lastIndex);

        for (int i = 0; i < sparse.Count; i++)
        {
            if (sparse[i] == lastIndex)
            {
                sparse[i] = index;
                break;
            }
        }
    }

    public T Get(int id)
    {
        int index = sparse[id];
        return dense[index];
    }

    public bool Contains(int id)
    {
        return id < sparse.Count && sparse[id] < dense.Count;
    }

    public IEnumerable<T> ActiveElements()
    {
        foreach (var item in dense)
            yield return item;
    }
}