using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using RPGCreator.Core.Serializer;
using RPGCreator.Core.Types.Assets.Tilesets;
using RPGCreator.Core.Types.Internal;
using Serilog;

namespace RPGCreator.Core.Types.Map;

public abstract class BaseLayerDef<TDef> : IMapLayerDef<TDef> where TDef : class, ILayerElem
{
    protected Dictionary<Point, TDef> _elements = new();
    protected readonly HashSet<Point> _surroundingElementsToIgnore = new();
    
    public event EventHandler<(Point, TDef)>? ElementAdded;
    public event EventHandler<(Point, TDef?)>? ElementRemoved;
    public Ulid Unique { get; private set; }
    public URN Urn { get; private set; }
    public string Name { get; set; }
    public int ZIndex { get; set; }
    public bool VisibleByDefault { get; set; }
    public ReadOnlyDictionary<Point, TDef> Elements => _elements.AsReadOnly();
    public void AddElement(TDef element, Point location)
    {
        if (!_elements.TryAdd(location, element))
            return;
        
        Log.Debug("[Layer: {LayerName}] Added element at {Location}", Name, location);
        
        element.Position = location;
        ElementAdded?.Invoke(this, (location, element));
    }

    public bool TryAddElement(TDef element, Point location)
    {
        if (!_elements.TryAdd(location, element))
            return false;

        element.Position = location;
        ElementAdded?.Invoke(this, (location, element));
        return true;
    }

    public TDef? RemoveElement(Point location)
    {
        if (!_elements.Remove(location, out var removedElement))
            return null;

        ElementRemoved?.Invoke(this, (location, removedElement));
        return removedElement;
    }

    public bool TryRemoveElement(Point location, [NotNullWhen(true)] out TDef? removedElement)
    {
        if (!_elements.Remove(location, out removedElement))
            return false;
        ElementRemoved?.Invoke(this, (location, removedElement));
        return true;
    }

    public bool TryRemoveElement(TDef element, [NotNullWhen(true)] out Point? removedLocation)
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

    public TDef? GetElement(Point location)
    {
        if (_elements.TryGetValue(location, out var element))
        {
            return element;
        }
        return null;
    }

    public bool TryGetElement(Point location, [NotNullWhen(true)] out TDef? element)
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

    public Dictionary<Point, TDef> GetSurroundingElements(Point location, int radius = 1, int offset = 1)
    {
        
        Dictionary<Point,TDef> surroundingElements = new();
        // Check the 8 surrounding positions
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue; // Skip the center position
                int offsetX = x * offset;
                int offsetY = y * offset;
                Point surroundingPosition = new(location.X + offsetX, location.Y + offsetY);
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
        ElementRemoved?.Invoke(this, (Point.Empty, null)); // Notify that all elements have been cleared
    }

    public SerializationInfo GetObjectData()
    {
        SerializationInfo info = new SerializationInfo(typeof(BaseLayerDef<TDef>));
        info.AddValue(nameof(Unique), Unique);
        info.AddValue(nameof(Name), Name);
        info.AddValue(nameof(ZIndex), ZIndex);
        info.AddValue(nameof(VisibleByDefault), VisibleByDefault);
        info.AddValue(nameof(_elements), _elements);
        return info;
    }

    public void SetObjectData(Serializer.DeserializationInfo info)
    {
        if (info == null)
        {
            throw new ArgumentNullException(nameof(info), "SerializationInfo cannot be null.");
        }

        info.TryGetValue(nameof(Unique), out Ulid unique, Ulid.Empty);
        info.TryGetValue(nameof(Name), out string name, string.Empty);
        info.TryGetValue(nameof(ZIndex), out int zIndex, 0);
        info.TryGetValue(nameof(VisibleByDefault), out bool visibleByDefault, true);
        info.TryGetValue(nameof(_elements), out Dictionary<Point, TDef> elements, new Dictionary<Point, TDef>());

        Unique = unique;
        Name = name;
        ZIndex = zIndex;
        VisibleByDefault = visibleByDefault;
        _elements = elements;
    }

    public bool IsDirty { get; set; }
    public bool IsTransient { get; set; }
}