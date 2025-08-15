using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using RPGCreator.Core.Type.Assets.Tilesets;
using RPGCreator.Core.Type.Internal;

namespace RPGCreator.Core.Type.Map;

public class TileLayerDefinition : IMapLayerDef<ITileDef>
{
    #region Events
    public event EventHandler<(Point, ITileDef)>? ElementAdded;
    public event EventHandler<(Point, ITileDef?)>? ElementRemoved;
    #endregion
    
    #region Properties
    public Ulid Unique { get; private set; } = Ulid.NewUlid();
    public URN Urn => new URN("map layer", $"{Name}@{Unique}");
    public string Name { get; set; }
    public int ZIndex { get; set; }
    public bool VisibleByDefault { get; set; } = true;
    /// <summary>
    /// A private dictionary that holds the elements of the layer, where the key is the position of the element and the value is the element itself.<br/>
    /// If you need to access the elements, use the <see cref="Elements"/> property instead, which returns a read-only dictionary.<br/>
    /// If you need to add or remove elements, use the methods:<br/>
    /// - <see cref="AddElement"/>,<br/>
    /// - <see cref="TryAddElement"/>,<br/>
    /// - <see cref="RemoveElement"/>,<br/>
    /// - <see cref="TryRemoveElement(Point, out ITileDef?)"/>,<br/>
    /// - <see cref="TryRemoveElement(ITileDef, out Point?)"/><br/>
    /// This dictionary is serialized, and will be saved between sessions.
    /// </summary>
    private Dictionary<Point, ITileDef> _elements = new();
    public ReadOnlyDictionary<Point, ITileDef> Elements => _elements.AsReadOnly();
    
    private readonly HashSet<Point> _surroundingElementsToIgnore = new();
    #endregion

    public TileLayerDefinition()
    {
    }
    
    public void AddElement(ITileDef element, Point location)
    {
        if (!_elements.TryAdd(location, element))
            return;
        
        element.DefaultPosition = location;
        ElementAdded?.Invoke(this, (location, element));
    }

    public bool TryAddElement(ITileDef element, Point location)
    {
        if (!_elements.TryAdd(location, element))
            return false;

        element.DefaultPosition = location;
        ElementAdded?.Invoke(this, (location, element));
        return true;
    }

    public ITileDef? RemoveElement(Point location)
    {
        if (!_elements.Remove(location, out var removedElement))
            return null;

        ElementRemoved?.Invoke(this, (location, removedElement));
        return removedElement;
    }

    public bool TryRemoveElement(Point location, [NotNullWhen(true)] out ITileDef? removedElement)
    {
        if (!_elements.Remove(location, out removedElement))
        {
            removedElement = null;
            return false;
        }
        ElementRemoved?.Invoke(this, (location, removedElement));
        return true;
    }

    public bool TryRemoveElement(ITileDef element, [NotNullWhen(true)] out Point? removedLocation)
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

    public ITileDef? GetElement(Point location)
    {
        if (_elements.TryGetValue(location, out var element))
        {
            return element;
        }
        return null;
    }

    public bool TryGetElement(Point location, [NotNullWhen(true)] out ITileDef? element)
    {
        if (_elements.TryGetValue(location, out element))
        {
            return true;
        }
        element = null;
        return false;
    }

    public bool HasElement(Point location)
    {
        return _elements.ContainsKey(location);
    }

    public Dictionary<Point, ITileDef> GetSurroundingElements(Point location, int radius = 1, int offset = 1)
    {
        Dictionary<Point,ITileDef> surroundingElements = new();
        // Check the 8 surrounding positions
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue; // Skip the center position
                int offsetX = x * offset;
                int offsetY = y * offset;
                Point surroundingPosition = new(location.X + offsetX, location.Y + offsetY);
                if (Elements.TryGetValue(surroundingPosition, out ITileDef? element))
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
        ElementRemoved?.Invoke(this, (Point.Empty, null)); // Notify that all elements have been cleared
        
    }

    public SerializationInfo GetObjectData()
    {
        SerializationInfo info = new SerializationInfo(typeof(TileLayerDefinition));
        info.AddValue(nameof(Unique), Unique);
        info.AddValue(nameof(Name), Name);
        info.AddValue(nameof(ZIndex), ZIndex);
        info.AddValue(nameof(VisibleByDefault), VisibleByDefault);
        info.AddValue(nameof(_elements), _elements);
        return info;
    }

    public void SetObjectData(DeserializationInfo info)
    {
        if (info == null)
        {
            throw new ArgumentNullException(nameof(info), "SerializationInfo cannot be null.");
        }

        info.TryGetValue(nameof(Unique), out Ulid unique, Ulid.Empty, $"{nameof(TileLayerDefinition)}.{nameof(Unique)} not found or invalid (Set to Ulid.Empty by default).");
        info.TryGetValue(nameof(Name), out string name, string.Empty, $"{nameof(TileLayerDefinition)}.{nameof(Name)} not found or invalid (Set to empty string by default).");
        info.TryGetValue(nameof(ZIndex), out int zIndex, 0, $"{nameof(TileLayerDefinition)}.{nameof(ZIndex)} not found or invalid (Set to 0 by default).");
        info.TryGetValue(nameof(VisibleByDefault), out bool visibleByDefault, true, $"{nameof(TileLayerDefinition)}.{nameof(VisibleByDefault)} not found or invalid (Set to true by default).");
        info.TryGetValue(nameof(_elements), out Dictionary<Point, ITileDef> elements, new Dictionary<Point, ITileDef>(), $"{nameof(TileLayerDefinition)}.{nameof(_elements)} not found or invalid (Set to empty dictionary by default).");

        Unique = unique;
        Name = name;
        ZIndex = zIndex;
        VisibleByDefault = visibleByDefault;
        _elements = elements;
    }
}