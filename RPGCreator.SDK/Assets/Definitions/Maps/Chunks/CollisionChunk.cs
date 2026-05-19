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

using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Common.MethodExtensions;
using RPGCreator.SDK.Serializer;
using RPGCreator.SDK.Types;
using RPGCreator.Shared.Types;

namespace RPGCreator.SDK.Assets.Definitions.Maps.Chunks;

public record struct RuntimeCollisionChunkData
{
    public Rect[] Collisions { get; init; }
}

public class CollisionChunk : LayerChunk
{
    static CollisionChunk()
    {
        if (!(ChunkSize > 0) && ((ChunkSize & (ChunkSize - 1)) == 0))
            throw new NotSupportedException(
            $"[LayerChunk] Critical Error: ChunkSize ({ChunkSize}) must be a power of 2 " +
            "to support bitwise sanitization. Please use 16, 32, 64, etc. " +
            "or reimplement SanitizeLocalCoord using modulo or your own system!");
    }
    
    private static readonly ScopedLogger Logger = Common.Logging.Logger.ForContext<CollisionChunk>();
    
    public bool IsEmpty => _localElements.All(e => e == null);
    
    /// <summary>
    /// Elements stored in this chunk.<br/>
    /// The array is 32x32 elements.<br/>
    /// Each element can be null.<br/>
    /// <br/>
    /// 32*32 = 1024 elements total. (32 tiles wide, 32 tiles high)
    /// </summary>
    private readonly RuntimeCollisionChunkData?[] _localElements = new RuntimeCollisionChunkData?[LocalElementsLength];
    
    /// <summary>
    /// Sets the element at the given local coordinates.<br/>
    /// Local coordinates are between 0 and 31 inclusive.<br/>
    /// If the coordinates are out of bounds, an <see cref="ArgumentOutOfRangeException"/> is thrown.<br/>
    /// <br/>
    /// Note: If you provide out of bounds coordinates, they will be sanitized using modulo 32.<br/>
    /// For example, a localX of 33 will be sanitized to 1, and a localY of -1 will be sanitized to 31.<br/>
    /// But if the sanitized coordinates are still out of bounds, an <see cref="ArgumentOutOfRangeException"/> will be thrown.
    /// </summary>
    /// <param name="localX">The local X coordinate (0-31).</param>
    /// <param name="localY">The local Y coordinate (0-31).</param>
    /// <param name="element">The element to set at the given coordinates. Can be null.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the local coordinates are out of bounds even after sanitization.</exception>
    public void SetElement(int localX, int localY, RuntimeCollisionChunkData? element)
    {
        if (SanitizeLocalCoord(ref localX))
        {
            // Logger.Debug("Sanitized local X coordinate to {localX}.", args: localX);
        }

        if (SanitizeLocalCoord(ref localY))
        {
            // Logger.Debug("Sanitized local Y coordinate to {localY}.", args: localY);
        }

        if (localX is < 0 or >= ChunkSize)
            throw new ArgumentOutOfRangeException(nameof(localX), "Local coordinates must be between 0 and 31.");
        
        if (localY is < 0 or >= ChunkSize)
            throw new ArgumentOutOfRangeException(nameof(localY), "Local coordinates must be between 0 and 31.");

        _localElements[localY * ChunkSize + localX] = element;
    }
    
    /// <summary>
    /// Sets the element at the given world coordinates.<br/>
    /// The world coordinates will be converted to local coordinates within the chunk.<br/>
    /// <br/>
    /// Note: This method floors the world coordinates to get the local coordinates.<br/>
    /// Note2: For more information on local coordinates, see <see cref="SetElement(int, int, TDef?)"/>.
    /// </summary>
    /// <param name="location">The world location to set the element at.</param>
    /// <param name="element">The element to set at the given location. Can be null.</param>
    public void SetElement(Vector2 location, RuntimeCollisionChunkData? element)
    {
        var intLocation = location.ToIntFloored();
        SetElement(intLocation.Item1, intLocation.Item2, element);
    }

    /// <summary>
    /// Gets the element at the given local coordinates.<br/>
    /// Local coordinates are between 0 and 31 inclusive.<br/>
    /// If the coordinates are out of bounds, an <see cref="ArgumentOutOfRangeException"/> is thrown.<br/>
    /// <br/>
    /// Note: If you provide out of bounds coordinates, they will be sanitized using modulo 32.<br/>
    /// For example, a localX of 33 will be sanitized to 1, and a localY of -1 will be sanitized to 31.<br/>
    /// But if the sanitized coordinates are still out of bounds, an <see cref="ArgumentOutOfRangeException"/> will be thrown.
    /// </summary>
    /// <param name="localX">The local X coordinate (0-31).</param>
    /// <param name="localY">The local Y coordinate (0-31).</param>
    /// <returns>The element at the given coordinates, or null if none is set.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the local coordinates are out of bounds even after sanitization.</exception>
    public RuntimeCollisionChunkData? GetElement(int localX, int localY)
    {
        if (SanitizeLocalCoord(ref localX))
        {
            // Logger.Debug("Sanitized local X coordinate to {localX}.", args: localX);
        }

        if (SanitizeLocalCoord(ref localY))
        {
            // Logger.Debug("Sanitized local Y coordinate to {localY}.", args: localY);
        }
        
        if (localX is < 0 or >= ChunkSize)
            throw new ArgumentOutOfRangeException(nameof(localX), "Local coordinates must be between 0 and 31.");
        
        if (localY is < 0 or >= ChunkSize)
            throw new ArgumentOutOfRangeException(nameof(localY), "Local coordinates must be between 0 and 31.");
        
        return _localElements[localY * ChunkSize + localX];
    }
    
    /// <summary>
    /// Gets the element at the given world coordinates.<br/>
    /// The world coordinates will be converted to local coordinates within the chunk.<br/>
    /// <br/>
    /// Note: This method floors the world coordinates to get the local coordinates.<br/>
    /// Note2: For more information on local coordinates, see <see cref="GetElement(int, int)"/>.
    /// </summary>
    /// <param name="location">The world location to get the element from.</param>
    /// <returns>The element at the given location, or null if none is set.</returns>
    public RuntimeCollisionChunkData? GetElement(Vector2 location)
    {
        var intLocation = location.ToIntFloored();
        return GetElement(intLocation.Item1, intLocation.Item2);
    }

    /// <summary>
    /// Verifies if an element exists at the given local coordinates.<br/>
    /// Local coordinates are between 0 and 31 inclusive.<br/>
    /// If the coordinates are out of bounds, an <see cref="ArgumentOutOfRangeException"/> is thrown.<br/>
    /// <br/>
    /// Note: If you provide out of bounds coordinates, they will be sanitized using modulo 32.<br/>
    /// For example, a localX of 33 will be sanitized to 1, and a localY of -1 will be sanitized to 31.<br/>
    /// But if the sanitized coordinates are still out of bounds, an <see cref="ArgumentOutOfRangeException"/> will be thrown.
    /// </summary>
    /// <param name="localX">The local X coordinate (0-31).</param>
    /// <param name="localY">The local Y coordinate (0-31).</param>
    /// <returns>
    /// True if an element exists at the given coordinates, false otherwise.
    /// </returns>
    public bool HasElement(int localX, int localY)
    {
        return GetElement(localX, localY) != null;
    }
    
    /// <summary>
    /// Verifies if an element exists at the given world coordinates.<br/>
    /// The world coordinates will be converted to local coordinates within the chunk.<br/>
    /// <br/>
    /// Note: This method floors the world coordinates to get the local coordinates.<br/>
    /// Note2: For more information on local coordinates, see <see cref="HasElement(int, int)"/>.
    /// </summary>
    /// <param name="location">The world location to check for an element.</param>
    /// <returns>
    /// True if an element exists at the given location, false otherwise.
    /// </returns>
    public bool HasElement(Vector2 location)
    {
        return GetElement(location) != null;
    }
    
    /// <summary>
    /// Removes the element at the given local coordinates and returns it.<br/>
    /// Local coordinates are between 0 and 31 inclusive.<br/>
    /// If the coordinates are out of bounds, an <see cref="ArgumentOutOfRangeException"/> is thrown.<br/>
    /// <br/>
    /// Note: If you provide out of bounds coordinates, they will be sanitized using modulo 32.<br/>
    /// For example, a localX of 33 will be sanitized to 1, and a localY of -1 will be sanitized to 31.<br/>
    /// But if the sanitized coordinates are still out of bounds, an <see cref="ArgumentOutOfRangeException"/> will be thrown.
    /// </summary>
    /// <param name="localX">The local X coordinate (0-31).</param>
    /// <param name="localY">The local Y coordinate (0-31).</param>
    /// <returns>
    /// The removed element, or null if none was set at the given coordinates.
    /// </returns>
    public RuntimeCollisionChunkData? RemoveElement(int localX, int localY)
    {
        if(!HasElement(localX, localY))
            return null;
        var element = GetElement(localX, localY);
        SetElement(localX, localY, null);
        return element;
    }
    
    /// <summary>
    /// Removes the element at the given world coordinates and returns it.<br/>
    /// The world coordinates will be converted to local coordinates within the chunk.<br/>
    /// <br/>
    /// Note: This method floors the world coordinates to get the local coordinates.<br/>
    /// Note2: For more information on local coordinates, see <see cref="RemoveElement(int, int)"/>.
    /// </summary>
    /// <param name="location">The world location to remove the element from.</param>
    /// <returns>
    /// The removed element, or null if none was set at the given location.
    /// </returns>
    public RuntimeCollisionChunkData? RemoveElement(Vector2 location)
    {
        var intLocation = location.ToIntFloored();
        return RemoveElement(intLocation.Item1, intLocation.Item2);
    }
    
    public ReadOnlySpan<RuntimeCollisionChunkData?> GetAllElementsSpan()
    {
        return _localElements.AsSpan();
    }
    
    
    #region Helpers
    
    private bool SanitizeLocalCoord(ref int coord)
    {
        var originalCoord = coord;
        // Bitwise AND with ChunkSize - 1 (31) to wrap around
        // Very efficient way to do coord % 32 when ChunkSize is a power of two
        coord &= (ChunkSize - 1);
        return coord != originalCoord;
    }
    
    #endregion

    public override SerializationInfo GetObjectData()
    {
        var info = new SerializationInfo(typeof(CollisionChunk));
    
        ReadOnlySpan<RuntimeCollisionChunkData?> span = _localElements.AsSpan();
    
        var dataToSave = new Dictionary<int, RuntimeCollisionChunkData>();

        for (int i = 0; i < span.Length; i++)
        {
            RuntimeCollisionChunkData? element = span[i];
            if (element.HasValue)
            {
                dataToSave.Add(i, element.Value);
            }
        }

        info.AddValue("Data", dataToSave);
        return info;
    }

    public override void SetObjectData(DeserializationInfo info)
    {
        Array.Clear(_localElements, 0, _localElements.Length);

        if (info.TryGetValue("Data", out Dictionary<int, RuntimeCollisionChunkData>? savedData))
        {
            Span<RuntimeCollisionChunkData?> span = _localElements.AsSpan();

            foreach (var (index, element) in savedData)
            {
                if ((uint)index < (uint)span.Length)
                {
                    span[index] = element;
                }
                else
                {
                    Logger.Warning("Ignoring tile with out-of-bounds index {index} during loading.", args: index);
                }
            }
        }
    }
}

public class CollisionLayer : BaseAssetDef
{
    public SerializationInfo GetObjectData()
    {
        return new SerializationInfo(typeof(CollisionLayer)).AddValue(nameof(Elements), _elements);
    }

    public List<Ulid> GetReferencedAssetIds()
    {
        return [];
    }

    public void SetObjectData(DeserializationInfo info)
    {
        info.TryGetValue(nameof(Elements), out _elements, new Dictionary<long, CollisionChunk>());
    }

    public int ZIndex => 999999;
    public bool VisibleByDefault { get; set; } = true;
    public event EventHandler<(long, CollisionChunk)>? ElementAdded;
    public event EventHandler<(long, CollisionChunk)>? ElementRemoved;
    private Dictionary<long, CollisionChunk> _elements = new();
    public ReadOnlyDictionary<long, CollisionChunk> Elements => _elements.AsReadOnly();
    public void AddElement(CollisionChunk element, long location)
    {
        _elements.Add(location, element);
    }

    public bool TryAddElement(CollisionChunk element, long location)
    {
        return _elements.TryAdd(location, element);
    }

    public CollisionChunk RemoveElement(long location)
    {
        return _elements.Remove(location) ? _elements[location] : throw new KeyNotFoundException();
    }

    public bool TryRemoveElement(long location, out CollisionChunk removedElement)
    {
        return _elements.Remove(location, out removedElement);
    }

    public bool TryRemoveElement(CollisionChunk element, [NotNullWhen(true)] out long? removedLocation)
    {
        foreach (var kvp in _elements)
        {
            if (EqualityComparer<CollisionChunk>.Default.Equals(kvp.Value, element))
            {
                removedLocation = kvp.Key;
                _elements.Remove(kvp.Key);
                return true;
            }
        }
        removedLocation = null;
        return false;
    }

    public CollisionChunk GetElement(long location)
    {
        return _elements[location];
    }

    public bool TryGetElement(long location, out CollisionChunk element)
    {
        return _elements.TryGetValue(location, out element);
    }

    public bool HasElement(long location)
    {
        return _elements.ContainsKey(location);
    }

    public void ClearElements()
    {
        _elements.Clear();
    }
}