using System.Runtime.CompilerServices;

namespace RPGCreator.SDK.Types.Collections;

public sealed class SparseSet<T>
{
    private T[] dense;        // Dense are the actual data
    private int[] sparse;     // index => dense index
    private int[] reverseIndex;   // dense index => index
    private int count;
    
    public Span<T> DatasSpan => new Span<T>(dense, 0, count);
    public ReadOnlySpan<int> ReverseIndexes => new ReadOnlySpan<int>(reverseIndex, 0, count);
    public bool Contains(int reverseId) => Has(reverseId);


    public int Count => count;

    public SparseSet(int capacity = 16)
    {
        dense = new T[capacity];
        sparse = new int[capacity];
        reverseIndex = new int[capacity];

        Array.Fill(sparse, -1);
        count = 0;
    }

    public ref T Add(int index, T component)
    {
        EnsureCapacity(index);

        if (Has(index))
        {
            dense[sparse[index]] = component;
            return ref dense[sparse[index]];
        }

        if (count == dense.Length)
            Grow();

        sparse[index] = count;
        dense[count] = component;
        reverseIndex[count] = index;
        count++;
        
        return ref dense[count - 1];
    }

    public void Remove(int index)
    {
        if (!Has(index))
            return;

        int denseIndex = sparse[index];
        int lastIndex = count - 1;
        int lastEntity = reverseIndex[lastIndex];

        // swap
        dense[denseIndex] = dense[lastIndex];
        reverseIndex[denseIndex] = lastEntity;
        sparse[lastEntity] = denseIndex;

        // nettoie
        sparse[index] = -1;
        count--;
        dense[count] = default!;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T Get(int index)
    {
        return ref dense[sparse[index]];
    }

    public bool Has(int index)
    {
        return index < sparse.Length && sparse[index] != -1;
    }
    
    public IEnumerable<(int index, T data)> ActiveElements()
    {
        for (int i = 0; i < count; i++)
            yield return (reverseIndex[i], dense[i]);
    }

    private void EnsureCapacity(int index)
    {
        int oldSize = sparse.Length;
        if (index < oldSize) return;

        int newSize = oldSize;
        while (newSize <= index) newSize *= 2;

        Array.Resize(ref sparse, newSize);
        Array.Fill(sparse, -1, oldSize, newSize - oldSize);
    }

    private void Grow()
    {
        int newSize = dense.Length * 2;
        Array.Resize(ref dense, newSize);
        Array.Resize(ref reverseIndex, newSize);
    }
}
