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

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using RPGCreator.SDK.Types.Collections;

namespace RPGCreator.SDK.ECS;

public class BlobManager : IDisposable
{
    const int BlobSize = 16;
    private Dictionary<Type, ISlab> Blobs { get; } = new();

    private ISlab? _lastSlab;

    /// <summary>
    /// Create a new slab of the given type. If a slab of the same type already exists, it will be returned instead.
    /// </summary>
    /// <param name="blobBlockSize">The size of the slab to create. If a slab of the same type already exists, this parameter will be ignored. Default is 16.</param>
    /// <typeparam name="T">The type of the slab. Must be a struct and implement ISlabItem.</typeparam>
    /// <returns>
    /// The slab of the given type.
    /// </returns>
    public Slabs<T> CreateSlab<T>(int blobBlockSize = BlobSize) where T : struct, ISlabItem
    {
        if(_lastSlab is Slabs<T> slab)
            return slab;
        
        if(Blobs.TryGetValue(typeof(T), out var existingSlab))
        {
            if(existingSlab is Slabs<T> existingTypedSlab)
            {
                _lastSlab = existingTypedSlab;
                return existingTypedSlab;
            }
            throw new InvalidOperationException($"A slab of type {typeof(T)} already exists but is of a different type.\nWeird.. This should NEVER happen?");
        }
        
        Blobs[typeof(T)] = new Slabs<T>(blobBlockSize);
        _lastSlab = Blobs[typeof(T)];
        return (Slabs<T>)_lastSlab;
    }
    
    /// <summary>
    /// Returns the slab of the given type. If the slab does not exist, it will be created with the default size (16).<br/>
    /// If you want to create one with a custom size, use <see cref="CreateSlab{T}(int)"/> instead.
    /// </summary>
    /// <typeparam name="T">The type of the slab. Must be a struct and implement ISlabItem.</typeparam>
    /// <returns>
    /// The slab of the given type.
    /// </returns>
    public Slabs<T> GetSlab<T>() where T : struct, ISlabItem
    {
        if(_lastSlab is Slabs<T> slabs)
            return slabs;

        var type = typeof(T);
        
        if(!Blobs.TryGetValue(type, out var slab))
        {
            slab = new Slabs<T>(BlobSize);
            Blobs[type] = slab;
        }
        _lastSlab = slab;
        return (Slabs<T>)slab;
    }
    
    /// <summary>
    /// Register an item in the blob.
    /// </summary>
    /// <param name="item">The item to register.</param>
    /// <typeparam name="T">The type of the item. Must be a struct and implement ISlabItem.</typeparam>
    /// <returns>
    /// A pointer to the registered item.
    /// </returns>
    public SlabItemPointer Register<T>(T item) where T : struct, ISlabItem
    {
        return GetSlab<T>().Allocate(item);
    }

    /// <summary>
    /// Retrieves a reference to the item of the specified type within the slab, based on the provided slab item pointer.
    /// </summary>
    /// <param name="index">The slab item pointer indicating which item to retrieve. Contains the block pointer index and local item index.</param>
    /// <typeparam name="T">The type of the item within the slab. Must be a struct and implement ISlabItem.</typeparam>
    /// <returns>
    /// A reference to the item of the specified type within the slab at the location specified by the slab item pointer.
    /// </returns>
    public ref T GetRef<T>(SlabItemPointer index) where T : struct, ISlabItem
    {
        return ref GetSlab<T>().GetItemRef(index);
    }

    /// <summary>
    /// Attempts to retrieve a reference to an item of the specified type from the slab at the given index.
    /// </summary>
    /// <param name="index">The slab item pointer that specifies the location of the item within the slab to be retrieved.</param>
    /// <param name="item">A reference to the item that will be populated if the operation is successful.</param>
    /// <typeparam name="T">The type of the item. Must be a struct and implement ISlabItem.</typeparam>
    /// <returns>
    /// True if the reference to the item was successfully retrieved; false if the reference is null or the item does not exist.
    /// </returns>
    public bool TryGetRef<T>(SlabItemPointer index, out Span<T> item) where T : struct, ISlabItem
    {
        ref var refItem = ref GetSlab<T>().GetItemRef(index);

        if (Unsafe.IsNullRef(ref refItem))
        {
            item = default;
            return false;
        }
        
        item = MemoryMarshal.CreateSpan(ref refItem, 1);
        return true;
    }

    /// <summary>
    /// Get an item from the blob.
    /// </summary>
    /// <param name="index">The pointer to the item to get.</param>
    /// <typeparam name="T">The type of the item. Must be a struct and implement ISlabItem.</typeparam>
    /// <returns>
    /// The item at the given pointer.
    /// </returns>
    public T Get<T>(SlabItemPointer index) where T : struct, ISlabItem
    {
        return GetSlab<T>().GetItem(index);
    }
    
    /// <summary>
    /// Try to get an item from the blob.
    /// </summary>
    /// <param name="index">The pointer to the item to get.</param>
    /// <param name="item">The output parameter that will hold the item if it was found.</param>
    /// <typeparam name="T">The type of the item. Must be a struct and implement ISlabItem.</typeparam>
    /// <returns>
    /// True if the item was found, false otherwise.
    /// </returns>
    public bool TryGet<T>(SlabItemPointer index, out T item) where T : struct, ISlabItem
    {
        return GetSlab<T>().TryGetItem(index, out item);
    }

    /// <summary>
    /// Optimized version of <see cref="Get{T}(SlabItemPointer)"/> for <see cref="Slabs{T}"/>.<br/>
    /// This allows getting multiple slabs at once, then get items from them without having to call <see cref="GetSlab"/> multiple times (And skip dictionary lookup).
    /// </summary>
    /// <param name="slab">The slab to get the item from.</param>
    /// <param name="index">The pointer to the item to get.</param>
    /// <typeparam name="T">The type of the item. Must be a struct and implement ISlabItem.</typeparam>
    /// <returns>
    /// The item at the given pointer.
    /// </returns>
    public T Get<T>(Slabs<T> slab, SlabItemPointer index) where T : struct, ISlabItem
    {
        return slab.GetItem(index);
    }
    
    /// <summary>
    /// Optimized version of <see cref="TryGet{T}(SlabItemPointer, out T)"/> for <see cref="Slabs{T}"/>.<br/>
    /// This allows getting multiple slabs at once, then get items from them without having to call <see cref="GetSlab"/> multiple times (And skip dictionary lookup).
    /// </summary>
    /// <param name="slab">The slab to get the item from.</param>
    /// <param name="index">The pointer to the item to get.</param>
    /// <param name="item">The output parameter that will hold the item if it was found.</param>
    /// <typeparam name="T">The type of the item. Must be a struct and implement ISlabItem.</typeparam>
    /// <returns>
    /// True if the item was found, false otherwise.
    /// </returns>
    public bool TryGet<T>(Slabs<T> slab, SlabItemPointer index, out T item) where T : struct, ISlabItem
    {
        return slab.TryGetItem(index, out item);
    }
    
    /// <summary>
    /// Remove an item from the blob.
    /// </summary>
    /// <param name="index">The pointer to the item to remove.</param>
    /// <typeparam name="T">The type of the item. Must be a struct and implement ISlabItem.</typeparam>
    public void Remove<T>(SlabItemPointer index) where T : struct, ISlabItem
    {
        GetSlab<T>().RemoveItem(index);
    }
    
    /// <summary>
    /// Try to remove an item from the blob.
    /// </summary>
    /// <param name="index">The pointer to the item to remove.</param>
    /// <typeparam name="T">The type of the item. Must be a struct and implement ISlabItem.</typeparam>
    /// <returns>
    /// True if the item was successfully removed; false if the item does not exist.
    /// </returns>   
    public bool TryRemove<T>(SlabItemPointer index) where T : struct, ISlabItem
    {
        return GetSlab<T>().TryRemoveItem(index, out _);
    }
    
    /// <summary>
    /// Try to remove an item from the blob.
    /// </summary>
    /// <param name="index">The pointer to the item to remove.</param>
    /// <param name="item">The output parameter that will hold the removed item if it was found and removed.</param>
    /// <typeparam name="T">The type of the item. Must be a struct and implement ISlabItem.</typeparam>
    /// <returns>
    /// True if the item was successfully removed; false if the item does not exist.
    /// </returns>  
    public bool TryRemove<T>(SlabItemPointer index, out T item) where T : struct, ISlabItem
    {
        return GetSlab<T>().TryRemoveItem(index, out item);
    }
    
    /// <summary>
    /// Remove an item from the blob.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    /// <typeparam name="T">The type of the item. Must be a struct and implement ISlabItem.</typeparam>
    public void Remove<T>(T item) where T : struct, ISlabItem
    {
        GetSlab<T>().RemoveItem(item);
    }
    
    /// <summary>
    /// Check if the blob contains the given item.
    /// </summary>
    /// <param name="item">The item to check.</param>
    /// <typeparam name="T">The type of the item. Must be a struct and implement ISlabItem.</typeparam>
    /// <returns>
    /// True if the blob contains the item; false otherwise.
    /// </returns> 
    public bool Contains<T>(T item) where T : struct, ISlabItem
    {
        return GetSlab<T>().ContainsItem(item);
    }

    /// <summary>
    /// Check if the blob contains the given item.
    /// </summary>
    /// <param name="index">The pointer to the item to check.</param>
    /// <typeparam name="T">The type of the item. Must be a struct and implement ISlabItem.</typeparam>
    /// <returns>
    /// True if the blob contains the item; false otherwise.
    /// </returns>
    public bool Contains<T>(SlabItemPointer index) where T : struct, ISlabItem
    {
        return GetSlab<T>().ContainsItem(index);
    }

    public void Dispose()
    {
        foreach (var slab in Blobs.Values)
        {
            slab.Clear();
        }
        Blobs.Clear();
        GC.SuppressFinalize(this);
    }
}