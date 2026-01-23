using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using RPGCreator.Core.Types.Map.Chunks;
using RPGCreator.SDK.Assets.Definitions.Tilesets;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Serializer;

namespace RPGCreator.SDK.Assets.Definitions.Maps;

public abstract class LayerWithElements<TDef> : BaseLayerDef
    where TDef : class, ILayerElem
{
    protected static readonly ScopedLogger Logger = Logging.Logger.ForContext<LayerWithElements<TDef>>();
    
    protected Dictionary<long, LayerChunk<TDef>> _chunks = new();
    protected readonly HashSet<Vector2> _surroundingElementsToIgnore = new();
    
    public record struct LayerElementEventArgs(long ChunkId, Vector2 Location, TDef? Element);
    
    public event Action<LayerElementEventArgs>? ElementAdded;
    public event Action<LayerElementEventArgs>? ElementRemoved;
    public ReadOnlyDictionary<long, LayerChunk<TDef>> Chunks => _chunks.AsReadOnly();
    
    /// <summary>
    /// Add a new element to the layer at the specified location.<br/>
    /// If an element already exists at that location, it will be replaced.<br/>
    /// <br/>
    /// If you want to avoid replacing existing elements, use <see cref="TryAddElement"/> instead.
    /// </summary>
    /// <param name="element">Element to add.</param>
    /// <param name="location">Location to add the element at.</param>
    public void AddElement(TDef element, Vector2 location)
    {
        var chunkPosition = LayerChunk.GetChunkPosition(location);
        var chunkId = LayerChunk.GetChunkId(location);

        var chunk = GetChunk(chunkId);
        chunk.SetElement(chunkPosition, element);
        
        Logger.Debug("Placed element of type {ElementType} at location {Location} in chunk {ChunkId}.", args: [typeof(TDef).Name, location, chunkId]);
        
        ElementAdded?.Invoke(new LayerElementEventArgs(chunkId, location, element));
    }

    /// <summary>
    /// Try to add a new element to the layer at the specified location.<br/>
    /// If an element already exists at that location, the addition will fail and return false.<br/>
    /// <br/>
    /// If you want to replace existing elements, use <see cref="AddElement"/> instead.
    /// </summary>
    /// <param name="element">The element to add.</param>
    /// <param name="location">The location to add the element at.</param>
    /// <returns></returns>
    public bool TryAddElement(TDef element, Vector2 location)
    {
        var chunkId = LayerChunk.GetChunkId(location);
        var chunk = GetChunk(chunkId);
        if (chunk.HasElement(location))
            return false;
        
        chunk.SetElement(location, element);

        ElementAdded?.Invoke(new LayerElementEventArgs(chunkId, location, element));
        return true;
    }

    /// <summary>
    /// Remove the element at the specified location.<br/>
    /// Returns the removed element, or null if no element was found at that location.<br/>
    /// <br/>
    /// If you want to check if an element was removed, use <see cref="TryRemoveElement"/> instead.
    /// </summary>
    /// <param name="location">Location of the element to remove.</param>
    /// <returns></returns>
    public TDef? RemoveElement(Vector2 location)
    {
        var chunkId = LayerChunk.GetChunkId(location);
        var chunk = GetChunk(chunkId);
        var removedElement = chunk.RemoveElement(location);

        ElementRemoved?.Invoke(new LayerElementEventArgs(chunkId, location, removedElement));
        return removedElement;
    }

    /// <summary>
    /// Try to remove the element at the specified location.<br/>
    /// Returns true if an element was removed, false otherwise.<br/>
    /// The removed element is returned via the out parameter.<br/>
    /// <br/>
    /// If you don't need the bool, use <see cref="RemoveElement"/> instead.
    /// </summary>
    /// <param name="location">Location of the element to remove.</param>
    /// <param name="removedElement">The removed element, or null if no element was found at that location.</param>
    /// <returns>
    /// True if an element was removed, false otherwise.
    /// </returns>
    public bool TryRemoveElement(Vector2 location, [NotNullWhen(true)] out TDef? removedElement)
    {
        var chunkId = LayerChunk.GetChunkId(location);
        var chunk = GetChunk(chunkId);
        
        removedElement = chunk.RemoveElement(location);
        
        if(removedElement == null)
        {
            removedElement = null;
            return false;
        }
        
        ElementRemoved?.Invoke(new LayerElementEventArgs(chunkId, location, removedElement));
        return true;
    }

    /// <summary>
    /// Return the element at the specified location, or null if no element exists there.
    /// </summary>
    /// <param name="location">Location of the element to get.</param>
    /// <returns>The element at the specified location, or null if no element exists there.</returns>
    public TDef? GetElement(Vector2 location)
    {
        var chunkId = LayerChunk.GetChunkId(location);
        var chunk = GetChunk(chunkId);
        return chunk.GetElement(location);
    }

    /// <summary>
    /// Try to get the element at the specified location.<br/>
    /// Returns true if an element was found, false otherwise.<br/>
    /// The found element is returned via the out parameter.
    /// </summary>
    /// <param name="location">Location of the element to get.</param>
    /// <param name="element">The found element, or null if no element was found at that location.</param>
    /// <returns>
    /// True if an element was found, false otherwise.
    /// </returns>
    public bool TryGetElement(Vector2 location, [NotNullWhen(true)] out TDef? element)
    {
        var chunkId = LayerChunk.GetChunkId(location);
        var chunk = GetChunk(chunkId);
        
        element = chunk.GetElement(location);

        if (element != null) return true;
        
        element = null;
        return false;

    }
    
    /// <summary>
    /// Return a span of all elements in the specified chunk.<br/>
    /// If the chunk does not exist, an empty span is returned.
    /// </summary>
    /// <param name="chunkId">ID of the chunk to get elements from.</param>
    /// <returns>A span of all elements in the specified chunk.</returns>
    public ReadOnlySpan<TDef?> GetElements(long chunkId)
    {
        if (TryGetChunk(chunkId, out var chunk))
        {
            return chunk.GetAllElementsSpan();
        }

        return ReadOnlySpan<TDef?>.Empty;
    }
    
    /// <summary>
    /// Return a span of all elements at the specified location's chunk.<br/>
    /// If the chunk does not exist, an empty span is returned.
    /// </summary>
    /// <param name="location">Location to get the chunk ID from.</param>
    /// <returns>A span of all elements in the specified chunk.</returns>
    public ReadOnlySpan<TDef?> GetElements(Vector2 location)
    {
        var chunkId = LayerChunk.GetChunkId(location);
        return GetElements(chunkId);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2 GetElementLocalPosition(int elementIndex) 
        => LayerChunk.GetChunkCoordinate(elementIndex);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2 GetElementWorldPosition(long chunkId, int elementIndex) 
        => LayerChunk.GetWorldPosition(chunkId, elementIndex);

    /// <summary>
    /// Check if an element exists at the specified location.
    /// </summary>
    /// <param name="location">Location to check for an element.</param>
    /// <returns>
    /// True if an element exists at the specified location, false otherwise.
    /// </returns>
    public bool HasElement(Vector2 location)
    {
        var chunkId = LayerChunk.GetChunkId(location);
        var chunk = GetChunk(chunkId);
        return chunk.HasElement(location);
    }

    /// <summary>
    /// Return a dictionary of surrounding elements around the specified location.<br/>
    /// The radius parameter defines how far from the location to check (in tiles).<br/>
    /// The offset parameter defines the distance between each surrounding tile (in tiles).<br/>
    /// <br/>
    /// For example, with radius = 1 and offset = 1, it will check the 8 surrounding tiles.<br/>
    /// With radius = 2 and offset = 1, it will check the 16 surrounding tiles in a 5x5 area (excluding the center).<br/>
    /// With radius = 1 and offset = 2, it will check the 8 surrounding tiles in a 3x3 area, but spaced out by 2 tiles.<br/>
    /// <br/>
    /// Tiles that are in the <see cref="_surroundingElementsToIgnore"/> list will be ignored and not included in the result.
    /// </summary>
    /// <param name="location">The center location to check around.</param>
    /// <param name="results">An array to store the results. Must be pre-allocated with a size of at least (radius * 2 + 1)² - 1. For a radius 1, the size need to be 8.</param>
    /// <param name="radius">The radius around the location to check (in tiles).</param>
    /// <returns>
    /// A dictionary of surrounding elements, with their locations as keys.
    /// </returns>
    public int GetSurroundingElements(Vector2 location, TDef?[] results, int radius = 1)
    {
        int expectedSize = (radius * 2 + 1) * (radius * 2 + 1) - 1;
        
        if (results.Length < expectedSize)
            throw new ArgumentException($"Results array must be at least of size {expectedSize} for radius {radius}.", nameof(results));
        
        var foundCount = 0;
        long lastChunkId = -1;
        LayerChunk<TDef>? lastChunk = null;

        var index = 0;
        for (var y = -radius; y <= radius; y++)
        {
            for (var x = -radius; x <= radius; x++)
            {
                if (x == 0 && y == 0) continue;

                Vector2 surroundingPosition = new(location.X + x, location.Y + y);
            
                var currentId = LayerChunk.GetChunkId(surroundingPosition);
            
                if (currentId != lastChunkId)
                {
                    lastChunkId = currentId;
                    if (!TryGetChunk(currentId, out lastChunk))
                    {
                        lastChunk = null;
                    }
                }

                if (lastChunk != null)
                {
                    results[index] = lastChunk.GetElement(surroundingPosition);
                    if (results[index] != null) foundCount++;
                }
                else
                {
                    results[index] = null;
                }
                index++;
            }
        }
        return foundCount;
    }

    /// <summary>
    /// Remove all elements from the layer.<br/>
    /// This will trigger the <see cref="ElementRemoved"/> event with null element to indicate all elements have been cleared.
    /// </summary>
    public void ClearElements()
    {
        _chunks.Clear();
        ElementRemoved?.Invoke(new LayerElementEventArgs(0, default, null)); // Notify that all elements have been cleared
    }
    
    #region Helpers

    private LayerChunk<TDef> GetChunk(Vector2 location)
    {
        var chunkId = LayerChunk.GetChunkId(location);
        return GetChunk(chunkId);
    }
    
    private LayerChunk<TDef> GetChunk(long chunkId)
    {
        if (_chunks.TryGetValue(chunkId, out var chunk))
            return chunk;
        
        var newChunk = new LayerChunk<TDef>();
        _chunks.Add(chunkId, newChunk);
        return newChunk;
    }
    
    private bool TryGetChunk(long chunkId, [NotNullWhen(true)] out LayerChunk<TDef>? chunk)
    {
        return _chunks.TryGetValue(chunkId, out chunk);
    }
    
    private bool TryGetChunk(Vector2 location, [NotNullWhen(true)] out LayerChunk<TDef>? chunk)
    {
        var chunkId = LayerChunk.GetChunkId(location);
        return TryGetChunk(chunkId, out chunk);
    }
    
    #endregion

    public override SerializationInfo GetObjectData()
    {
        SerializationInfo info = base.GetObjectData();
        info.AddValue(nameof(_chunks), _chunks);
        return info;
    }

    public void SetObjectData(DeserializationInfo info)
    {
        base.SetObjectData(info);
        info.TryGetValue(nameof(_chunks), out Dictionary<long, LayerChunk<TDef>> elements, new Dictionary<long, LayerChunk<TDef>>());
        _chunks = elements;
    }

    public bool IsDirty { get; set; }
    public bool IsTransient { get; set; }
}


public class TileLayerDefinition : LayerWithElements<ITileDef>
{
}