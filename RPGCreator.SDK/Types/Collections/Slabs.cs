// RPG Creator - Open-source RPG Engine.
// (c) 2026 Ward
// 
// This file is part of RPG Creator and is distributed under the Apache 2.0 License.
// You are free to use, modify, and distribute this file under the terms of the Apache 2.0 License.
// See LICENSE for details.
// 
// ---
// 
// Ce fichier fait partie de RPG Creator et est distribué sous licence Apache 2.0.
// Vous êtes libre de l'utiliser, de le modifier et de le distribuer sous les termes de la licence Apache 2.0.
// Voir LICENSE pour plus de détails.
// 
// Contact:
// => Mail: Ward727a@gmail.com
//    Please use this object: "RPG Creator [YourObject]"
// => Discord: ward727
// 
// For urgent inquiries, sending both an email and a message on Discord is highly recommended for a quicker response.

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using RPGCreator.SDK.Logging;

namespace RPGCreator.SDK.Types.Collections;

public record struct FreeBlock
{
    public int StartIndex;
    public int Size;
}

public interface ISlabItem
{
    /// <summary>
    /// PLEASE NOTE: This should only be set by the <see cref="Slabs{T}"/> class when allocating or adding an item to a block.<br/>
    /// It is used to keep track of the slab pointer index that the item belongs to, which is necessary for the internal management of the slabs.<br/>
    /// Manually setting this property can lead to unexpected behavior and should be done with caution, if you don't know what you are doing.<br/>
    /// Consider using the appropriate methods in the <see cref="Slabs{T}"/> class to manage the allocation and addition of items to blocks instead of manually setting this property.<br/>
    /// </summary>
    public int? SlabPointerIndex { get; set; }
}

public record struct SlabPointer
{
    public int OccupiedItemsCount;
    /// <summary>
    /// Total block size (empty or occupied) in terms of number of items it can hold.
    /// </summary>
    public int BlockSize;
    
    /// <summary>
    /// The starting index of the block in the main items list. This is where the first item of the block is located.
    /// </summary>
    public int BlockStart;
}

public sealed class Slabs<T> where T : ISlabItem
{
    private int _blockSize;
    private List<T> _items;
    
    private int _nextPointerIndex = 0;
    private ConcurrentDictionary<int, SlabPointer> _pointers;
    private List<FreeBlock> _blocks;
    
    
    public Slabs(int blockSize)
    {
        if (blockSize <= 0)
            throw new ArgumentException("Block size must be greater than zero.", nameof(blockSize));
        
        _blockSize = blockSize;
        _items = new List<T>();
        _blocks = new List<FreeBlock>();
        _pointers = new ConcurrentDictionary<int, SlabPointer>();
    }
    
    public void Clear()
    {
        _items.Clear();
        _blocks.Clear();
        _pointers.Clear();
        _nextPointerIndex = 0;
    }

    private int TryFindFreeBlock(int requiredSize)
    {
        
        Span<FreeBlock> blocks = CollectionsMarshal.AsSpan(_blocks);
        
        for (int i = 0; i < blocks.Length; i++)
        {
            if (blocks[i].Size >= requiredSize)
            {
                return i;
            }
        }
        
        return -1;
    }
    
    /// <summary>
    /// Search for a free block that is adjacent to the target start index.<br/>
    /// So if we have that (where O is occupied and F is free):<br/>
    /// [O O O] [F F] [O O]<br/>
    /// And we want to allocate an item in the second [O O] block,<br/>
    /// this method will return the index of the [F F] block.
    /// </summary>
    /// <param name="targetStartIndex"></param>
    /// <returns></returns>
    private int FindBlockStartNeighbor(int targetStartIndex)
    {
        Span<FreeBlock> blocks = CollectionsMarshal.AsSpan(_blocks);
        
        for (int i = 0; i < blocks.Length; i++)
        {
            if (blocks[i].StartIndex + blocks[i].Size == targetStartIndex)
            {
                return i;
            }
        }
        
        return -1;
    }
    
    /// <summary>
    /// Search for a free block that is adjacent to the target start index.<br/>
    /// So if we have that (where O is occupied and F is free):<br/>
    /// [O O O O O] [O O] [F F F]<br/>
    /// And we want to allocate an item in the second [O O] block,<br/>
    /// this method will return the index of the [F F F] block.
    /// </summary>
    /// <param name="targetStartIndex"></param>
    /// <returns></returns>
    private int FindBlockEndNeighbor(int targetEndIndex)
    {
        Span<FreeBlock> blocks = CollectionsMarshal.AsSpan(_blocks);
        
        for (int i = 0; i < blocks.Length; i++)
        {
            if (blocks[i].StartIndex == targetEndIndex)
            {
                return i;
            }
        }
        
        return -1;
    }
    
    private void MergeBlocks(int index1, int index2)
    {
        if (index1 < 0 || index1 >= _blocks.Count || index2 < 0 || index2 >= _blocks.Count)
            throw new ArgumentOutOfRangeException("Invalid block indices for merging.");
        
        var block1 = _blocks[index1];
        var block2 = _blocks[index2];
        
        if (block1.StartIndex + block1.Size != block2.StartIndex)
            throw new InvalidOperationException("Blocks are not adjacent and cannot be merged.");
        
        _blocks[index1] = block1 with { Size = block1.Size + block2.Size };
        
        _blocks.RemoveAt(index2);
    }
    
    private void MergeWithNeighbors(int targetStartIndex)
    {
        int currentIdx = -1;
        for (int i = 0; i < _blocks.Count; i++)
        {
            if (_blocks[i].StartIndex == targetStartIndex)
            {
                currentIdx = i;
                break;
            }
        }

        if (currentIdx == -1) return;

        if (currentIdx + 1 < _blocks.Count)
        {
            if (_blocks[currentIdx].StartIndex + _blocks[currentIdx].Size == _blocks[currentIdx + 1].StartIndex)
            {
                MergeBlocks(currentIdx, currentIdx + 1);
            }
        }

        if (currentIdx > 0)
        {
            if (_blocks[currentIdx - 1].StartIndex + _blocks[currentIdx - 1].Size == _blocks[currentIdx].StartIndex)
            {
                MergeBlocks(currentIdx - 1, currentIdx);
            }
        }
    }
    
    private void AddFreeBlock(int startIndex, int size)
    {
        if (startIndex < 0 || size <= 0)
            throw new ArgumentException("Invalid start index or size for free block.");

        int insertIdx = 0;
        while (insertIdx < _blocks.Count && _blocks[insertIdx].StartIndex < startIndex)
        {
            insertIdx++;
        }

        _blocks.Insert(insertIdx, new FreeBlock { StartIndex = startIndex, Size = size });
    
        MergeWithNeighbors(startIndex);
    }
    
    private void RemoveFreeBlock(int index)
    {
        if (index < 0 || index >= _blocks.Count)
            throw new ArgumentOutOfRangeException(nameof(index), "Invalid block index for removal.");
        
        _blocks.RemoveAt(index);
    }
    
    private void ReduceFreeBlock(int index, int size, bool fromStart = true)
    {
        if (index < 0 || index >= _blocks.Count)
            throw new ArgumentOutOfRangeException(nameof(index), "Invalid block index for reduction.");
        
        var block = _blocks[index];
        
        if(size == block.Size)
        {
            RemoveFreeBlock(index);
            return;
        }
        
        if (size <= 0 || size > block.Size)
            throw new ArgumentException("Invalid size for reducing free block.", nameof(size));
        
        if (fromStart)
        {
            _blocks[index] = block with { StartIndex = block.StartIndex + size, Size = block.Size - size };
        }
        else
        {
            _blocks[index] = block with { Size = block.Size - size };
        }
    }
    
    private enum AllocationDirection
    {
        NoNeed,
        FromStart,
        FromEnd,
        NoSpace
    }
    
    /// <summary>
    /// Determine if the allocation for the next item in the block.<br>
    /// should be done from the start of the block (if there are any FreeBlocks there).<br/>
    /// or from the end of the block (if there are any FreeBlocks there OR if the item list can be extended to accommodate it).<br/>
    /// <br/>
    /// So: <br/>
    /// if FreeBlock at the start of the block => allocate from start.<br/>
    /// else if FreeBlock at the end of the block OR the size of item list is equal to the end of the block => allocate from end.<br/>
    /// else => no space for allocation (So we need to find another block, or extend the item list by the block size + the new item, then move all items inside the new space, and add the old on to the FreeBlocks).<br/>
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    private AllocationDirection GetAllocationDirection(int index)
    {
        var totalItemSize = _items.Count;
        var item = _items[index];
        
        var slabPointer = _pointers[item.SlabPointerIndex.Value];
        
        var startFreeBlockIndex = FindBlockStartNeighbor(slabPointer.BlockStart);
        
        if (startFreeBlockIndex != -1)
        {
            return AllocationDirection.FromStart;
        }
        
        var endOfBlock = slabPointer.BlockStart + slabPointer.BlockSize;
        var endFreeBlockIndex = FindBlockEndNeighbor(endOfBlock);
        if (endFreeBlockIndex != -1 || totalItemSize == endOfBlock)
        {
            return AllocationDirection.FromEnd;
        }
        
        return AllocationDirection.NoSpace;
    }


    /// <summary>
    /// Allocate an empty block and return its pointer index and the item index of the first item in the block (which should be 0 since it's empty).<br/>
    /// </summary>
    /// <returns></returns>
    public int AllocateEmpty()
    {
        var freeBlockIndex = TryFindFreeBlock(_blockSize);
        if (freeBlockIndex != -1)
        {
            var slabPointer = new SlabPointer
            {
                BlockSize = _blockSize,
                BlockStart = _blocks[freeBlockIndex].StartIndex,
                OccupiedItemsCount = 0
            };
            _pointers.TryAdd(_nextPointerIndex, slabPointer);
            
            ReduceFreeBlock(freeBlockIndex, _blockSize);
            
            _nextPointerIndex++;
            
            return _nextPointerIndex - 1;
        }
        else
        {
            var slabPointer = new SlabPointer
            {
                BlockSize = _blockSize,
                BlockStart = _items.Count,
                OccupiedItemsCount = 0
            };
            
            _pointers.TryAdd(_nextPointerIndex, slabPointer);
            
            EnsureCapacity(_items.Count + _blockSize);
            
            _nextPointerIndex++;
            
            return _nextPointerIndex - 1;
        }
    }
    public (int slabPointerIndex, int itemIndex) Allocate(T? item = default)
    {
        var freeBlockIndex = TryFindFreeBlock(_blockSize);
        if (freeBlockIndex != -1)
        {
            var slabPointer = new SlabPointer
            {
                BlockSize = _blockSize,
                BlockStart = _blocks[freeBlockIndex].StartIndex,
                OccupiedItemsCount = 1
            };
            _pointers.TryAdd(_nextPointerIndex, slabPointer);
            if(item != null)
                item.SlabPointerIndex = _nextPointerIndex;
            
            ReduceFreeBlock(freeBlockIndex, _blockSize);
            
            _items[slabPointer.BlockStart] = item;
            _nextPointerIndex++;
        }
        else
        {
            var slabPointer = new SlabPointer
            {
                BlockSize = _blockSize,
                BlockStart = _items.Count,
                OccupiedItemsCount = 1
            };
            
            item.SlabPointerIndex = _nextPointerIndex;
            if(item.Equals(default(T)))
            {
                slabPointer.OccupiedItemsCount = 0;
            }
            _pointers.TryAdd(_nextPointerIndex, slabPointer);
            
            EnsureCapacity(_items.Count + _blockSize);
            
            _items[slabPointer.BlockStart] = item;
            _nextPointerIndex++;
        }
        
        return (item.SlabPointerIndex.Value, 0);
    }

    /// <summary>
    /// This should only be used to add items to an already allocated block!<br/>
    /// If you don't know what you are doing, consider using an already allocated item as a reference for the block you want to add to, and use the overload that takes an already inserted item as a reference.<br/>
    /// </summary>
    /// <param name="slabPointIndex"></param>
    /// <param name="item"></param>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public int AddItem(int slabPointIndex, T item)
    {
        if (item.SlabPointerIndex != null)
            throw new ArgumentException("The item already has a slab pointer index, it may already be allocated.", nameof(item));
        
        if(!_pointers.TryGetValue(slabPointIndex, out var ptr))
        {
            throw new ArgumentException("Invalid slab pointer index for expansion.", nameof(slabPointIndex));
        }
        
        if (ptr.BlockSize <= 0)
            throw new InvalidOperationException("Invalid slab pointer block size.");
        
        // Check if we need to expand it or if there is space for it in the block.
        if (ptr.BlockSize > 0)
        {
            var blockStart = ptr.BlockStart;
            for (int i = blockStart; i < blockStart + ptr.BlockSize; i++)
            {
                if (!_items[i].SlabPointerIndex.HasValue || _items[i].SlabPointerIndex!.Value != slabPointIndex || _items[i].Equals(default(T)))
                {
                    _items[i] = item;
                    item.SlabPointerIndex = slabPointIndex;
                    ptr.OccupiedItemsCount++;
                    return ptr.OccupiedItemsCount - 1;
                }
            }
        }
        
        var allocationDirection = GetAllocationDirection(ptr.BlockStart);
        switch (allocationDirection)
        {
            case AllocationDirection.FromStart:
                ExpandFromStart(slabPointIndex, item);
                break;
            case AllocationDirection.FromEnd:
                ExpandFromEnd(slabPointIndex, item);
                break;
            case AllocationDirection.NoSpace:
                ReallocateAndAdd(slabPointIndex, item);
                break;
            default:
                throw new InvalidOperationException("Unexpected allocation direction.");
        }
        return ptr.OccupiedItemsCount - 1;
    }
    
    /// <summary>
    /// Add an item to the block of an already inserted item.<br/>
    /// This is a safer overload that ensures you are adding to an already allocated block by using an already inserted item as a reference for the block you want to add to.<br/>
    /// </summary>
    /// <param name="alreadyInsertedItem"></param>
    /// <param name="itemToAdd"></param>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public int AddItem(T alreadyInsertedItem, T itemToAdd)
    {
        if(alreadyInsertedItem.SlabPointerIndex == null)
            throw new ArgumentException("The provided item does not have a valid slab pointer index.", nameof(alreadyInsertedItem));
        
        if(!_pointers.TryGetValue(alreadyInsertedItem.SlabPointerIndex.Value, out var ptr))
        {
            throw new ArgumentException("Invalid slab pointer index for expansion.", nameof(alreadyInsertedItem.SlabPointerIndex.Value));
        }
        
        if (ptr.BlockSize <= 0)
            throw new InvalidOperationException("Invalid slab pointer block size.");
        if (itemToAdd.SlabPointerIndex != null)
            throw new ArgumentException("The item to add already has a slab pointer index, it may already be allocated.", nameof(itemToAdd));
        
        if (ptr.BlockSize > 0)
        {
            var blockStart = ptr.BlockStart;
            for (int i = blockStart; i < blockStart + ptr.BlockSize; i++)
            {
                if (_items[i] == null)
                {
                    _items[i] = itemToAdd;
                    itemToAdd.SlabPointerIndex = alreadyInsertedItem.SlabPointerIndex;
                    ptr.OccupiedItemsCount++;
                    return i - blockStart;
                }
            }
        }
        
        var allocationDirection = GetAllocationDirection(ptr.BlockStart);
        switch (allocationDirection)
        {
            case AllocationDirection.FromStart:
                ExpandFromStart(alreadyInsertedItem.SlabPointerIndex.Value, itemToAdd);
                break;
            case AllocationDirection.FromEnd:
                ExpandFromEnd(alreadyInsertedItem.SlabPointerIndex.Value, itemToAdd);
                break;
            case AllocationDirection.NoSpace:
                ReallocateAndAdd(alreadyInsertedItem.SlabPointerIndex.Value, itemToAdd);
                break;
            default:
                throw new InvalidOperationException("Unexpected allocation direction.");
        }
        
        return ptr.OccupiedItemsCount - 1;
    }

    #if DEBUG
    /// <summary>
    /// This is a debug method that prints the current state of the slabs.<br/>
    /// N = Null (not allocated) - Should not happen!!!<br/>
    /// F = Free (allocated but not occupied)<br/>
    /// [PointerIndex] = Occupied by an item with the given pointer index.<br/>
    /// ([PointerIndex]?) = Allocated for an item with the given pointer index, but currently free (not occupied).<br/>
    /// </summary>
    public void DEBUG_PRINT_SCHEMA_BLOCKS()
    {
        Logger.Debug("--- Slabs Memory Schema ---");
        var schema = new string[_items.Count];
    
        Array.Fill(schema, "N");

        foreach (var block in _blocks)
        {
            for (int i = block.StartIndex; i < block.StartIndex + block.Size; i++)
            {
                if (i < schema.Length) schema[i] = "F";
            }
        }

        foreach (var pair in _pointers)
        {
            var ptr = pair.Value;
            for (int i = ptr.BlockStart; i < ptr.BlockStart + ptr.BlockSize; i++)
            {
                if (i >= schema.Length) continue;
            
                if (_items[i] != null)
                    schema[i] = $"{pair.Key}";
                else
                    schema[i] = $"({pair.Key}?)";
            }
        }

        Logger.Debug(string.Join(" ", schema));
    }
    #endif
    
    private void ExpandFromEnd(int pointerIndex, T itemToAdd)
    {
        
        if(!_pointers.TryGetValue(pointerIndex, out var ptr))
        {
            throw new ArgumentException("Invalid slab pointer index for expansion.", nameof(pointerIndex));
        }
        
        int endOfBlock = ptr.BlockStart + ptr.BlockSize;

        if (endOfBlock == _items.Count)
        {
            _items.Add(itemToAdd);
        }
        else
        {
            int freeIdx = FindBlockEndNeighbor(endOfBlock);
            ReduceFreeBlock(freeIdx, 1, fromStart: true);
        
            EnsureCapacity(endOfBlock);
            _items[endOfBlock] = itemToAdd;
        }

        ptr.BlockSize++;
        ptr.OccupiedItemsCount++;
        itemToAdd.SlabPointerIndex = pointerIndex;
    }

    private void ExpandFromStart(int pointerIndex, T itemToAdd)
    {
        if(!_pointers.TryGetValue(pointerIndex, out var ptr))
        {
            throw new ArgumentException("Invalid slab pointer index for expansion.", nameof(pointerIndex));
        }
    
        int freeIdx = FindBlockStartNeighbor(ptr.BlockStart);
    
        ReduceFreeBlock(freeIdx, 1, fromStart: false);

        int newStart = ptr.BlockStart - 1;
    
        EnsureCapacity(newStart);
        _items[newStart] = itemToAdd;

        ptr.BlockStart = newStart;
        ptr.BlockSize++;
        ptr.OccupiedItemsCount++;
    
        itemToAdd.SlabPointerIndex = pointerIndex;
    }

    private void EnsureCapacity(int index)
    {
        while (_items.Count < index)
        {
            _items.Add(default!);
        }
    }
    
    private void ReallocateAndAdd(int pointerIndex, T itemToAdd)
    {
        if(!_pointers.TryGetValue(pointerIndex, out var ptr))
        {
            throw new ArgumentException("Invalid slab pointer index for expansion.", nameof(pointerIndex));
        }

        int oldStart = ptr.BlockStart;
        int oldSize = ptr.BlockSize;
        int newSize = oldSize + _blockSize;

        int freeBlockIndex = TryFindFreeBlock(newSize);
        int newStart;

        if (freeBlockIndex == -1)
        {
            newStart = _items.Count;
            EnsureCapacity(newStart + newSize);
        }
        else
        {
            newStart = _blocks[freeBlockIndex].StartIndex;
            ReduceFreeBlock(freeBlockIndex, newSize);
        }

        Span<T> itemsSpan = CollectionsMarshal.AsSpan(_items);
        itemsSpan.Slice(oldStart, oldSize).CopyTo(itemsSpan.Slice(newStart, oldSize));

        itemsSpan.Slice(oldStart, oldSize).Clear();
    
        AddFreeBlock(oldStart, oldSize);

        ptr.BlockStart = newStart;
        ptr.BlockSize = newSize;
        ptr.OccupiedItemsCount++;

        int itemIndex = newStart + oldSize;
        _items[itemIndex] = itemToAdd;
        itemToAdd.SlabPointerIndex = pointerIndex;
    }
    
    public void Deallocate(T item)
    {
        if (item.SlabPointerIndex == null)
            throw new ArgumentException("The item does not have a valid slab pointer index.", nameof(item));
        
        if(!_pointers.TryGetValue(item.SlabPointerIndex.Value, out var ptr))
        {
            throw new ArgumentException("Invalid slab pointer index for expansion.", nameof(item.SlabPointerIndex.Value));
        }
        
        int blockStart = ptr.BlockStart;
        int blockSize = ptr.BlockSize;

        for (int i = blockStart; i < blockStart + blockSize; i++)
        {
            _items[i] = default!;
        }

        AddFreeBlock(blockStart, blockSize);
        
        _pointers.TryRemove(item.SlabPointerIndex.Value, out _);
        item.SlabPointerIndex = null;
    }
    
    public void DeallocateBlock(int pointerIndex)
    {
        
        if(!_pointers.TryGetValue(pointerIndex, out var ptr))
        {
            throw new ArgumentException("Invalid slab pointer index for expansion.", nameof(pointerIndex));
        }
        
        int blockStart = ptr.BlockStart;
        int blockSize = ptr.BlockSize;

        for (int i = blockStart; i < blockStart + blockSize; i++)
        {
            _items[i] = default!;
        }

        AddFreeBlock(blockStart, blockSize);
        
        _pointers.Remove(pointerIndex, out _);
    }

    
    public void RemoveItem(int pointerIndex, int itemIndex)
    {
        if(!_pointers.TryGetValue(pointerIndex, out var ptr))
        {
            throw new ArgumentException("Invalid slab pointer index for expansion.", nameof(pointerIndex));
        }
        
        int globalIndex = ptr.BlockStart + itemIndex;
        if (globalIndex < ptr.BlockStart || globalIndex >= ptr.BlockStart + ptr.BlockSize)
            throw new ArgumentOutOfRangeException(nameof(itemIndex), "Invalid item index for the specified slab pointer.");
        
        var item = _items[globalIndex];
        if (item == null)
            throw new InvalidOperationException("The specified item index is already free.");
        
        if (globalIndex == -1) return;

        int lastIndexInBlock = ptr.BlockStart + ptr.BlockSize - 1;

        if (globalIndex < lastIndexInBlock)
        {
            _items[globalIndex] = _items[lastIndexInBlock];
        }

        _items[lastIndexInBlock] = default!;
    
        AddFreeBlock(lastIndexInBlock, 1);
    
        ptr.BlockSize--;
        ptr.OccupiedItemsCount--;
        item.SlabPointerIndex = null;
    
        if (ptr.BlockSize == 0)
        {
            _pointers.Remove(pointerIndex, out _);
        }
        
    }
    
    public void RemoveItem(T item)
    {
        if (item.SlabPointerIndex == null) return;

        int ptrIdx = item.SlabPointerIndex.Value;
        if(!_pointers.TryGetValue(ptrIdx, out var ptr))
        {
            throw new ArgumentException("Invalid slab pointer index for expansion.", nameof(ptrIdx));
        }

        int globalIndex = -1;
        for (int i = ptr.BlockStart; i < ptr.BlockStart + ptr.BlockSize; i++)
        {
            if (ReferenceEquals(_items[i], item))
            {
                globalIndex = i;
                break;
            }
        }

        if (globalIndex == -1) return;

        int lastIndexInBlock = ptr.BlockStart + ptr.BlockSize - 1;

        if (globalIndex < lastIndexInBlock)
        {
            _items[globalIndex] = _items[lastIndexInBlock];
        }

        _items[lastIndexInBlock] = default!;
    
        AddFreeBlock(lastIndexInBlock, 1);
    
        ptr.BlockSize--;
        ptr.OccupiedItemsCount--;
        item.SlabPointerIndex = null;
    
        if (ptr.BlockSize == 0)
        {
            _pointers.Remove(ptrIdx, out _);
        }
    }
    
    public Span<T> GetSpan(int pointerIndex)
    {
        if(!_pointers.TryGetValue(pointerIndex, out var ptr))
        {
            throw new ArgumentException("Invalid slab pointer index for expansion.", nameof(pointerIndex));
        }


        return CollectionsMarshal.AsSpan(_items).Slice(ptr.BlockStart, ptr.BlockSize);
    }
    
    public int GetOccupiedCount(int pointerIndex)
    {
        if(!_pointers.TryGetValue(pointerIndex, out var ptr))
        {
            throw new ArgumentException("Invalid slab pointer index for expansion.", nameof(pointerIndex));
        }
        
        return ptr.OccupiedItemsCount;
    }

    public T GetItem(int slabIdx, int localIdx)
    {
        if(!_pointers.TryGetValue(slabIdx, out var ptr))
        {
            throw new ArgumentException("Invalid slab pointer index for expansion.", nameof(slabIdx));
        }
        
        if (localIdx < 0 || localIdx >= ptr.BlockSize)
            throw new ArgumentOutOfRangeException(nameof(localIdx), "Invalid local item index for the specified slab pointer.");
        
        return _items[ptr.BlockStart + localIdx];
    }
}