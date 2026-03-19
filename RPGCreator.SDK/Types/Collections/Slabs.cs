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
    public int? BlockPointerIndex { get; set; }
}

public record struct BlockPointer
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

internal interface ISlab
{
    void Clear();
}

public sealed class Slabs<T> : ISlab where T : ISlabItem
{
    private int _blockSize;
    private List<T> _items;
    
    private int _nextPointerIndex = 0;
    private ConcurrentDictionary<int, BlockPointer> _blockPointers;
    private List<FreeBlock> _blocks;
    
    
    public Slabs(int blockSize)
    {
        if (blockSize <= 0)
            throw new ArgumentException("Block size must be greater than zero.", nameof(blockSize));
        
        _blockSize = blockSize;
        _items = new List<T>();
        _blocks = new List<FreeBlock>();
        _blockPointers = new ConcurrentDictionary<int, BlockPointer>();
    }
    
    public void Clear()
    {
        _items.Clear();
        _blocks.Clear();
        _blockPointers.Clear();
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
    /// Should be done from the start of the block (if there are any FreeBlocks there).<br/>
    /// Or from the end of the block (if there are any FreeBlocks there OR if the item list can be extended to accommodate it).<br/>
    /// <br/>
    /// So: <br/>
    /// if FreeBlock at the start of the block => allocate from start.<br/>
    /// Else if FreeBlock at the end of the block OR the size of item list is equal to the end of the block => allocate from end.<br/>
    /// Else => no space for allocation. (So we need to find another block, or extend the item list by the block size plus the new item, then move all items inside the new space, and add the old on to the FreeBlocks.)<br/>
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    private AllocationDirection GetAllocationDirection(int index)
    {
        var totalItemSize = _items.Count;
        var item = _items[index];
        
        var slabPointer = _blockPointers[item.BlockPointerIndex.Value];
        
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
    /// Allocate an empty block and return its pointer index.<br/>
    /// </summary>
    /// <returns></returns>
    public int AllocateEmpty()
    {
        var freeBlockIndex = TryFindFreeBlock(_blockSize);
        if (freeBlockIndex != -1)
        {
            var blockPointer = new BlockPointer
            {
                BlockSize = _blockSize,
                BlockStart = _blocks[freeBlockIndex].StartIndex,
                OccupiedItemsCount = 0
            };
            _blockPointers.TryAdd(_nextPointerIndex, blockPointer);
            
            ReduceFreeBlock(freeBlockIndex, _blockSize);
            
            _nextPointerIndex++;
            
            return _nextPointerIndex - 1;
        }
        else
        {
            var slabPointer = new BlockPointer
            {
                BlockSize = _blockSize,
                BlockStart = _items.Count,
                OccupiedItemsCount = 0
            };
            
            _blockPointers.TryAdd(_nextPointerIndex, slabPointer);
            
            EnsureCapacity(_items.Count + _blockSize);
            
            _nextPointerIndex++;
            
            return _nextPointerIndex - 1;
        }
    }
    public SlabItemPointer Allocate(T? item = default)
    {
        var freeBlockIndex = TryFindFreeBlock(_blockSize);
        if (freeBlockIndex != -1)
        {
            var blockPointer = new BlockPointer
            {
                BlockSize = _blockSize,
                BlockStart = _blocks[freeBlockIndex].StartIndex,
                OccupiedItemsCount = 1
            };
            _blockPointers.TryAdd(_nextPointerIndex, blockPointer);
            if(item != null)
                item.BlockPointerIndex = _nextPointerIndex;
            
            ReduceFreeBlock(freeBlockIndex, _blockSize);
            
            _items[blockPointer.BlockStart] = item;
            _nextPointerIndex++;
        }
        else
        {
            var blockPointer = new BlockPointer
            {
                BlockSize = _blockSize,
                BlockStart = _items.Count,
                OccupiedItemsCount = 1
            };
            
            item.BlockPointerIndex = _nextPointerIndex;
            if(item.Equals(default(T)))
            {
                blockPointer.OccupiedItemsCount = 0;
            }
            _blockPointers.TryAdd(_nextPointerIndex, blockPointer);
            
            EnsureCapacity(_items.Count + _blockSize);
            
            _items[blockPointer.BlockStart] = item;
            _nextPointerIndex++;
        }
        
        return new SlabItemPointer(item.BlockPointerIndex.Value, 0);
    }

    /// <summary>
    /// This should only be used to add items to an already allocated block!<br/>
    /// If you don't know what you are doing, consider using an already allocated item as a reference for the block you want to add to, and use the overload that takes an already inserted item as a reference.<br/>
    /// </summary>
    /// <param name="blockPointIndex"></param>
    /// <param name="item"></param>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public int AddItem(int blockPointIndex, T item)
    {
        if (item.BlockPointerIndex != null)
            throw new ArgumentException("The item already has a block pointer index, it may already be allocated.", nameof(item));
        
        if(!_blockPointers.TryGetValue(blockPointIndex, out var ptr))
        {
            throw new ArgumentException("Invalid block pointer index for expansion.", nameof(blockPointIndex));
        }
        
        if (ptr.BlockSize <= 0)
            throw new InvalidOperationException("Invalid block pointer block size.");
        
        // Check if we need to expand it or if there is space for it in the block.
        if (ptr.BlockSize > 0)
        {
            var blockStart = ptr.BlockStart;
            for (int i = blockStart; i < blockStart + ptr.BlockSize; i++)
            {
                if (!_items[i].BlockPointerIndex.HasValue || _items[i].BlockPointerIndex!.Value != blockPointIndex || _items[i].Equals(default(T)))
                {
                    _items[i] = item;
                    item.BlockPointerIndex = blockPointIndex;
                    ptr.OccupiedItemsCount++;
                    return ptr.OccupiedItemsCount - 1;
                }
            }
        }
        
        var allocationDirection = GetAllocationDirection(ptr.BlockStart);
        switch (allocationDirection)
        {
            case AllocationDirection.FromStart:
                ExpandFromStart(blockPointIndex, item);
                break;
            case AllocationDirection.FromEnd:
                ExpandFromEnd(blockPointIndex, item);
                break;
            case AllocationDirection.NoSpace:
                ReallocateAndAdd(blockPointIndex, item);
                break;
            default:
                throw new InvalidOperationException("Unexpected allocation direction.");
        }
        return ptr.OccupiedItemsCount - 1;
    }
    
    public int AddItem(SlabItemPointer itemPointer, T item)
    {
        return AddItem(itemPointer.BlockPtrIdx, item);
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
        if(alreadyInsertedItem.BlockPointerIndex == null)
            throw new ArgumentException("The provided item does not have a valid slab pointer index.", nameof(alreadyInsertedItem));
        
        if(!_blockPointers.TryGetValue(alreadyInsertedItem.BlockPointerIndex.Value, out var ptr))
        {
            throw new ArgumentException("Invalid slab pointer index for expansion.", nameof(alreadyInsertedItem.BlockPointerIndex.Value));
        }
        
        if (ptr.BlockSize <= 0)
            throw new InvalidOperationException("Invalid slab pointer block size.");
        if (itemToAdd.BlockPointerIndex != null)
            throw new ArgumentException("The item to add already has a slab pointer index, it may already be allocated.", nameof(itemToAdd));
        
        if (ptr.BlockSize > 0)
        {
            var blockStart = ptr.BlockStart;
            for (int i = blockStart; i < blockStart + ptr.BlockSize; i++)
            {
                if (_items[i] == null)
                {
                    _items[i] = itemToAdd;
                    itemToAdd.BlockPointerIndex = alreadyInsertedItem.BlockPointerIndex;
                    ptr.OccupiedItemsCount++;
                    return i - blockStart;
                }
            }
        }
        
        var allocationDirection = GetAllocationDirection(ptr.BlockStart);
        switch (allocationDirection)
        {
            case AllocationDirection.FromStart:
                ExpandFromStart(alreadyInsertedItem.BlockPointerIndex.Value, itemToAdd);
                break;
            case AllocationDirection.FromEnd:
                ExpandFromEnd(alreadyInsertedItem.BlockPointerIndex.Value, itemToAdd);
                break;
            case AllocationDirection.NoSpace:
                ReallocateAndAdd(alreadyInsertedItem.BlockPointerIndex.Value, itemToAdd);
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

        foreach (var pair in _blockPointers)
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
    
    private void ExpandFromEnd(int blockPointerIndex, T itemToAdd)
    {
        
        if(!_blockPointers.TryGetValue(blockPointerIndex, out var ptr))
        {
            throw new ArgumentException("Invalid slab pointer index for expansion.", nameof(blockPointerIndex));
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
        itemToAdd.BlockPointerIndex = blockPointerIndex;
    }

    private void ExpandFromStart(int blockPointerIndex, T itemToAdd)
    {
        if(!_blockPointers.TryGetValue(blockPointerIndex, out var ptr))
        {
            throw new ArgumentException("Invalid slab pointer index for expansion.", nameof(blockPointerIndex));
        }
    
        int freeIdx = FindBlockStartNeighbor(ptr.BlockStart);
    
        ReduceFreeBlock(freeIdx, 1, fromStart: false);

        int newStart = ptr.BlockStart - 1;
    
        EnsureCapacity(newStart);
        _items[newStart] = itemToAdd;

        ptr.BlockStart = newStart;
        ptr.BlockSize++;
        ptr.OccupiedItemsCount++;
    
        itemToAdd.BlockPointerIndex = blockPointerIndex;
    }

    private void EnsureCapacity(int index)
    {
        while (_items.Count < index)
        {
            _items.Add(default!);
        }
    }
    
    private void ReallocateAndAdd(int blockPointerIndex, T itemToAdd)
    {
        if(!_blockPointers.TryGetValue(blockPointerIndex, out var ptr))
        {
            throw new ArgumentException("Invalid slab pointer index for expansion.", nameof(blockPointerIndex));
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
        itemToAdd.BlockPointerIndex = blockPointerIndex;
    }
    
    public void Deallocate(T item)
    {
        if (item.BlockPointerIndex == null)
            throw new ArgumentException("The item does not have a valid slab pointer index.", nameof(item));
        
        if(!_blockPointers.TryGetValue(item.BlockPointerIndex.Value, out var ptr))
        {
            throw new ArgumentException("Invalid slab pointer index for expansion.", nameof(item.BlockPointerIndex.Value));
        }
        
        int blockStart = ptr.BlockStart;
        int blockSize = ptr.BlockSize;

        for (int i = blockStart; i < blockStart + blockSize; i++)
        {
            _items[i] = default!;
        }

        AddFreeBlock(blockStart, blockSize);
        
        _blockPointers.TryRemove(item.BlockPointerIndex.Value, out _);
        item.BlockPointerIndex = null;
    }
    
    public void DeallocateBlock(int blockPointerIndex)
    {
        
        if(!_blockPointers.TryGetValue(blockPointerIndex, out var ptr))
        {
            throw new ArgumentException("Invalid slab pointer index for expansion.", nameof(blockPointerIndex));
        }
        
        int blockStart = ptr.BlockStart;
        int blockSize = ptr.BlockSize;

        for (int i = blockStart; i < blockStart + blockSize; i++)
        {
            _items[i] = default!;
        }

        AddFreeBlock(blockStart, blockSize);
        
        _blockPointers.Remove(blockPointerIndex, out _);
    }
    
    public void DeallocateBlock(SlabItemPointer itemPointer)
    {
        DeallocateBlock(itemPointer.BlockPtrIdx);
    }
    
    public void RemoveItem(int blockPointerIndex, int itemIndex)
    {
        if(!_blockPointers.TryGetValue(blockPointerIndex, out var ptr))
        {
            throw new ArgumentException("Invalid slab pointer index for expansion.", nameof(blockPointerIndex));
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
        item.BlockPointerIndex = null;
    
        if (ptr.BlockSize == 0)
        {
            _blockPointers.Remove(blockPointerIndex, out _);
        }
        
    }
    
    public void RemoveItem(SlabItemPointer itemPointer)
    {
        RemoveItem(itemPointer.BlockPtrIdx, itemPointer.ItemIndex);
    }

    public bool TryRemoveItem(int blockPointerIndex, int itemIndex, out T? item)
    {
        item = default;
        
        if(!_blockPointers.TryGetValue(blockPointerIndex, out var ptr))
        {
            return false;
        }
        
        int globalIndex = ptr.BlockStart + itemIndex;
        if (globalIndex < ptr.BlockStart || globalIndex >= ptr.BlockStart + ptr.BlockSize)
            return false;
        
        item = _items[globalIndex];
        if (item == null)
            return false;
        
        int lastIndexInBlock = ptr.BlockStart + ptr.BlockSize - 1;

        if (globalIndex < lastIndexInBlock)
        {
            _items[globalIndex] = _items[lastIndexInBlock];
        }

        _items[lastIndexInBlock] = default!;
    
        AddFreeBlock(lastIndexInBlock, 1);
    
        ptr.BlockSize--;
        ptr.OccupiedItemsCount--;
        item.BlockPointerIndex = null;
    
        if (ptr.BlockSize == 0)
        {
            _blockPointers.Remove(blockPointerIndex, out _);
        }
        
        return true;
    }
    
    public bool TryRemoveItem(SlabItemPointer itemPointer, out T? item)
    {
        return TryRemoveItem(itemPointer.BlockPtrIdx, itemPointer.ItemIndex, out item);
    }
    
    public void RemoveItem(T item)
    {
        if (item.BlockPointerIndex == null) return;

        int ptrIdx = item.BlockPointerIndex.Value;
        if(!_blockPointers.TryGetValue(ptrIdx, out var ptr))
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
        item.BlockPointerIndex = null;
    
        if (ptr.BlockSize == 0)
        {
            _blockPointers.Remove(ptrIdx, out _);
        }
    }

    public bool ContainsItem(int blockPointerIndex, int itemIndex)
    {
        if(!_blockPointers.TryGetValue(blockPointerIndex, out var ptr))
        {
            return false;
        }
        
        int globalIndex = ptr.BlockStart + itemIndex;
        if (globalIndex < ptr.BlockStart || globalIndex >= ptr.BlockStart + ptr.BlockSize)
            return false;
        
        return _items[globalIndex] != null;
    }
    
    public bool ContainsItem(SlabItemPointer itemPointer)
    {
        return ContainsItem(itemPointer.BlockPtrIdx, itemPointer.ItemIndex);
    }
    
    public bool ContainsItem(T item)
    {
        return item.BlockPointerIndex != null && _blockPointers.TryGetValue(item.BlockPointerIndex.Value, out var ptr) && ptr.BlockStart <= item.BlockPointerIndex.Value && item.BlockPointerIndex.Value < ptr.BlockStart + ptr.BlockSize;
    }
    
    public Span<T> GetSpan(int blockPointerIndex)
    {
        if(!_blockPointers.TryGetValue(blockPointerIndex, out var ptr))
        {
            throw new ArgumentException("Invalid slab pointer index for expansion.", nameof(blockPointerIndex));
        }


        return CollectionsMarshal.AsSpan(_items).Slice(ptr.BlockStart, ptr.BlockSize);
    }
    
    public Span<T> GetSpan(SlabItemPointer itemPointer)
    {
        return GetSpan(itemPointer.BlockPtrIdx);
    }
    
    public int GetOccupiedCount(int blockPointerIndex)
    {
        if(!_blockPointers.TryGetValue(blockPointerIndex, out var ptr))
        {
            throw new ArgumentException("Invalid slab pointer index for expansion.", nameof(blockPointerIndex));
        }
        
        return ptr.OccupiedItemsCount;
    }

    public int GetOccupiedCount(SlabItemPointer itemPointer)
    {
        return GetOccupiedCount(itemPointer.BlockPtrIdx);
    }

    public T GetItem(int blockPtrIdx, int itemIdx)
    {
        if(!_blockPointers.TryGetValue(blockPtrIdx, out var ptr))
        {
            throw new ArgumentException("Invalid slab pointer index for expansion.", nameof(blockPtrIdx));
        }
        
        if (itemIdx < 0 || itemIdx >= ptr.BlockSize)
            throw new ArgumentOutOfRangeException(nameof(itemIdx), "Invalid local item index for the specified slab pointer.");
        
        return _items[ptr.BlockStart + itemIdx];
    }

    public T GetItem(SlabItemPointer itemPointer)
    {
        return GetItem(itemPointer.BlockPtrIdx, itemPointer.ItemIndex);
    }

    public ref T GetItemRef(SlabItemPointer itemPointer)
    {
        if(!_blockPointers.TryGetValue(itemPointer.BlockPtrIdx, out var ptr))
        {
            throw new ArgumentException("Invalid slab pointer index for expansion.", nameof(itemPointer.BlockPtrIdx));
        }
        
        if (itemPointer.ItemIndex < 0 || itemPointer.ItemIndex >= ptr.BlockSize)
            throw new ArgumentOutOfRangeException(nameof(itemPointer.ItemIndex), "Invalid local item index for the specified slab pointer.");

        return ref GetSpan(itemPointer)[itemPointer.ItemIndex];
    }
    
    public bool TryGetItem(int blockPtrIdx, int itemIdx, out T item)
    {
        if(!_blockPointers.TryGetValue(blockPtrIdx, out var ptr))
        {
            item = default!;
            return false;
        }

        if (itemIdx < 0 || itemIdx >= ptr.BlockSize)
        {
            item = default!;
            return false;
        }
        
        item = _items[ptr.BlockStart + itemIdx];
        return true;
    }
    
    public bool TryGetItem(SlabItemPointer itemPointer, out T item)
    {
        return TryGetItem(itemPointer.BlockPtrIdx, itemPointer.ItemIndex, out item);
    }
}

public record struct SlabItemPointer(int BlockPtrIdx, int ItemIndex)
{
    public int BlockPtrIdx { get; internal set; } = BlockPtrIdx;
    public int ItemIndex { get; internal set; } = ItemIndex;
}