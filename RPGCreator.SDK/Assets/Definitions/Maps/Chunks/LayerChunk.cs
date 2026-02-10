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

using System.Numerics;
using System.Runtime.CompilerServices;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Maps;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers.EntityLayer;
using RPGCreator.SDK.Assets.Definitions.Tilesets;
using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.Extensions;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Serializer;

namespace RPGCreator.Core.Types.Map.Chunks;

[SerializingType("TileLayerChunk")]
public class TileLayerChunk() : LayerChunk<ITileDef>
{
}

[SerializingType("EntityLayerChunk")]
public class EntityLayerChunk() : LayerChunk<EntitySpawner>
{
}

/// <summary>
/// A chunk of a layer in a map.<br/>
/// Each chunk is 32x32 elements.<br/>
/// <br/>
/// This class is used to store elements of a layer in a map in a more efficient way.<br/>
/// Instead of storing all elements in a single large array, the layer is divided into chunks.<br/>
/// This allows for more efficient memory usage and faster access to elements.<br/>
/// <br/>
/// Note: This class is generic and can be used with any type that implements <see cref="ILayerElem"/>.
/// </summary>
/// <typeparam name="TDef">The type of elements stored in the chunk. Must implement <see cref="ILayerElem"/>.</typeparam>
public class LayerChunk<TDef> : LayerChunk, ISerializable, IDeserializable where TDef : class, ILayerElem
{

    static LayerChunk()
    {
        if (!(ChunkSize > 0) && ((ChunkSize & (ChunkSize - 1)) == 0))
            throw new NotSupportedException(
            $"[LayerChunk] Critical Error: ChunkSize ({ChunkSize}) must be a power of 2 " +
            "to support bitwise sanitization. Please use 16, 32, 64, etc. " +
            "or reimplement SanitizeLocalCoord using modulo or your own system!");
    }
    
    private static readonly ScopedLogger Logger = SDK.Logging.Logger.ForContext<LayerChunk<TDef>>();
    
    public bool IsEmpty => _localElements.All(e => e == null);
    
    /// <summary>
    /// Elements stored in this chunk.<br/>
    /// The array is 32x32 elements.<br/>
    /// Each element can be null.<br/>
    /// <br/>
    /// 32*32 = 1024 elements total. (32 tiles wide, 32 tiles high)
    /// </summary>
    private readonly TDef?[] _localElements = new TDef?[LocalElementsLength];
    
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
    public void SetElement(int localX, int localY, TDef? element)
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
    public void SetElement(Vector2 location, TDef? element)
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
    public TDef? GetElement(int localX, int localY)
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
    public TDef? GetElement(Vector2 location)
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
    public TDef? RemoveElement(int localX, int localY)
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
    public TDef? RemoveElement(Vector2 location)
    {
        var intLocation = location.ToIntFloored();
        return RemoveElement(intLocation.Item1, intLocation.Item2);
    }
    
    public ReadOnlySpan<TDef?> GetAllElementsSpan()
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
        var info = new SerializationInfo(typeof(LayerChunk<TDef>));
    
        ReadOnlySpan<TDef?> span = _localElements.AsSpan();
    
        var dataToSave = new Dictionary<int, TDef>();

        for (int i = 0; i < span.Length; i++)
        {
            TDef? element = span[i];
            if (element != null)
            {
                dataToSave.Add(i, element);
            }
        }

        info.AddValue("Data", dataToSave);
        return info;
    }

    public override void SetObjectData(DeserializationInfo info)
    {
        Array.Clear(_localElements, 0, _localElements.Length);

        if (info.TryGetValue("Data", out Dictionary<int, TDef> savedData))
        {
            Span<TDef?> span = _localElements.AsSpan();

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

public abstract class LayerChunk : ISerializable, IDeserializable
{
    /// <summary>
    /// The size of the chunk in both width and height.<br/>
    /// Each chunk is 32x32 elements.<br/>
    /// <br/>
    /// Be aware that this is a constant value, and SHOULD NOT be changed.<br/>
    /// Changing this value will break the map system and cause unexpected behavior.
    /// </summary>
    public const int ChunkSize = 32;
    
    protected const int LocalElementsLength = ChunkSize * ChunkSize;
    
    /// <summary>
    /// Returns the unique chunk ID for the given world location.<br/>
    /// The chunk ID is calculated by flooring the location coordinates, dividing by chunk size (32), and combining them into a single long value.<br/>
    /// <br/>
    /// This allows for efficient storage and retrieval of chunks based on world coordinates.<br/>
    /// <br/>
    /// Note: This method is efficient, but limit the number of chunks to: 2^32 in each direction (X and Y) to avoid overflow issues.
    /// </summary>
    /// <param name="location"></param>
    /// <returns></returns>
    public static long GetChunkId(Vector2 location)
    {
        int cx = (int)Math.Floor(location.X / 1024f);
        int cy = (int)Math.Floor(location.Y / 1024f);

        long xBits = (long)(uint)cx & 0xFFFFFFFFL;
        long yBits = (long)(uint)cy & 0xFFFFFFFFL;

        return (yBits << 32) | xBits;
    }
    
    /// <summary>
    /// Returns the unique chunk ID for the given chunk coordinates.<br/>
    /// The chunk ID is calculated by combining the chunkX and chunkY into a single long value.<br/>
    /// <br/>
    /// This allows for efficient storage and retrieval of chunks based on chunk coordinates.<br/>
    /// <br/>
    /// Note: This method is efficient, but limit the number of chunks to: 2^32 in each direction (X and Y) to avoid overflow issues.
    /// </summary>
    /// <param name="chunkX"></param>
    /// <param name="chunkY"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long GetChunkId(long chunkX, long chunkY)
    {
        return (long)(((ulong)(uint)chunkY << 32) | (uint)chunkX);
    }
    
    /// <summary>
    /// Deconstructs the given chunk ID into its chunk coordinates (chunkX, chunkY).<br/>
    /// <br/>
    /// This allows for efficient retrieval of chunk coordinates from a unique chunk ID.<br/>
    /// <br/>
    /// Note: This is strictly like <see cref="GetChunkCoordinate"/> but returns a tuple instead of a Vector2.<br/>
    /// In fact, <see cref="GetChunkCoordinate"/> uses this method internally.
    /// </summary>
    /// <param name="chunkId"></param>
    /// <returns>
    /// A tuple containing the chunkX and chunkY coordinates.
    /// </returns>
    public static (long chunkX, long chunkY) DeconstructChunkId(long chunkId)
    {
        long x = (int)(chunkId & 0xFFFFFFFFL);

        long y = chunkId >> 32;

        return (x, y);
    }
    
    /// <summary>
    /// Returns the chunk coordinates (chunkX, chunkY) for the given chunk ID as a Vector2.<br/>
    /// <br/>
    /// This allows for efficient retrieval of chunk coordinates from a unique chunk ID.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public static Vector2 GetChunkCoordinate(long index)
    {
        var localPos = DeconstructChunkId(index);
        return new Vector2(localPos.chunkX, localPos.chunkY);
    }
    
    public static Vector2 GetWorldPosition(long chunkId, int index)
    {
        if (!RuntimeServices.MapService.HasLoadedMap)
            return Vector2.Zero;
        
        var gridParam = RuntimeServices.MapService.CurrentLoadedMapDefinition?.GridParameter;
        if (gridParam == null)
            return Vector2.Zero;
        
        if (index is < 0 or >= LocalElementsLength)
            throw new ArgumentOutOfRangeException(nameof(index), $"Index must be between 0 and {LocalElementsLength}.");
        
        long chunkX = (int)(chunkId & 0xFFFFFFFFL);
        long chunkY = (int)(chunkId >> 32);
        
        int localX = index & 31;
        int localY = index >> 5;
        
        float worldX = ((chunkX * ChunkSize) + localX) * gridParam.Value.CellWidth;
        float worldY = ((chunkY * ChunkSize) + localY) * gridParam.Value.CellHeight;
        
        return new Vector2(worldX, worldY);
    }

    public static Vector2 GetChunkPosition(Vector2 worldPosition)
    {
        if (!RuntimeServices.MapService.HasLoadedMap)
            return Vector2.Zero;
        
        var gridParam = RuntimeServices.MapService.CurrentLoadedMapDefinition?.GridParameter;
        if (gridParam == null)
            return Vector2.Zero;
        
        float cellWidth = gridParam.Value.CellWidth;
        float cellHeight = gridParam.Value.CellHeight;
        
        return new Vector2(worldPosition.X/cellWidth, worldPosition.Y/cellHeight);
    }

    public abstract SerializationInfo GetObjectData();
    public List<Ulid> GetReferencedAssetIds()
    {
        return new List<Ulid>();
    }

    public abstract void SetObjectData(DeserializationInfo info);
}