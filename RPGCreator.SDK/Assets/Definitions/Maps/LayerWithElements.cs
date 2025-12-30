using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Numerics;
using RPGCreator.SDK.Assets.Definitions.Tilesets;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Serializer;

namespace RPGCreator.SDK.Assets.Definitions.Maps;

public abstract class LayerWithElements<TDef> : BaseLayerDef
    where TDef : class, ILayerElem
{
    protected Dictionary<Vector2, TDef> _elements = new();
    protected readonly HashSet<Vector2> _surroundingElementsToIgnore = new();
    
    public event EventHandler<(Vector2, TDef)>? ElementAdded;
    public event EventHandler<(Vector2, TDef?)>? ElementRemoved;
    public ReadOnlyDictionary<Vector2, TDef> Elements => _elements.AsReadOnly();
    public void AddElement(TDef element, Vector2 location)
    {
        if (!_elements.TryAdd(location, element))
        {
            RemoveElement(location);
            _elements.Add(location, element);
        }
        
        Logger.Debug("[Layer: {LayerName}] Added element at {Location}", Name, location);
        
        element.Position = location;
        ElementAdded?.Invoke(this, (location, element));
    }

    public bool TryAddElement(TDef element, Vector2 location)
    {
        if (!_elements.TryAdd(location, element))
            return false;

        element.Position = location;
        ElementAdded?.Invoke(this, (location, element));
        return true;
    }

    public TDef? RemoveElement(Vector2 location)
    {
        if (!_elements.Remove(location, out var removedElement))
            return null;

        ElementRemoved?.Invoke(this, (location, removedElement));
        return removedElement;
    }

    public bool TryRemoveElement(Vector2 location, [NotNullWhen(true)] out TDef? removedElement)
    {
        if (!_elements.Remove(location, out removedElement))
            return false;
        ElementRemoved?.Invoke(this, (location, removedElement));
        return true;
    }

    public bool TryRemoveElement(TDef element, [NotNullWhen(true)] out Vector2? removedLocation)
    {
        foreach (var kvp in _elements)
        {
            if (kvp.Value.Equals(element))
            {
                removedLocation = kvp.Key;
                _elements.Remove(kvp.Key);
                ElementRemoved?.Invoke(this, (kvp.Key, element));
                return true;
            }
        }
        removedLocation = null;
        return false;
    }

    public TDef? GetElement(Vector2 location)
    {
        if (_elements.TryGetValue(location, out var element))
        {
            return element;
        }
        return null;
    }

    public bool TryGetElement(Vector2 location, [NotNullWhen(true)] out TDef? element)
    {
        if (_elements.TryGetValue(location, out element))
        {
            return true;
        }
        element = null;
        return false;
    }

    public bool HasElement(Vector2 location)
    {
        return _elements.ContainsKey(location);
    }

    public Dictionary<Vector2, TDef> GetSurroundingElements(Vector2 location, int radius = 1, int offset = 1)
    {
        
        Dictionary<Vector2,TDef> surroundingElements = new();
        // Check the 8 surrounding positions
        for (float x = -1; x <= 1; x++)
        {
            for (float y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue; // Skip the center position
                float offsetX = x * offset;
                float offsetY = y * offset;
                Vector2 surroundingPosition = new(location.X + offsetX, location.Y + offsetY);
                if (Elements.TryGetValue(surroundingPosition, out TDef? element))
                {
                    if(_surroundingElementsToIgnore.Contains(surroundingPosition))
                    {
                        // If the element is in the list of surrounding tiles to ignore, skip it
                        continue;
                    }
                    surroundingElements[surroundingPosition] = element;
                }
            }
        }
        return surroundingElements;
    }

    public void ClearElements()
    {
        _elements.Clear();
        ElementRemoved?.Invoke(this, (default, null)); // Notify that all elements have been cleared
    }

    public override SerializationInfo GetObjectData()
    {
        SerializationInfo info = base.GetObjectData();
        info.AddValue(nameof(_elements), _elements);
        return info;
    }

    public void SetObjectData(DeserializationInfo info)
    {
        base.SetObjectData(info);
        info.TryGetValue(nameof(_elements), out Dictionary<Vector2, TDef> elements, new Dictionary<Vector2, TDef>());

        _elements = elements;
    }

    public bool IsDirty { get; set; }
    public bool IsTransient { get; set; }
}


public class TileLayerDefinition : LayerWithElements<ITileDef>
{
}