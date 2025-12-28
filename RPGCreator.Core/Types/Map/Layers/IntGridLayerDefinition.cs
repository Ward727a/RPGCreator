using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using RPGCreator.Core.Serializer;
using RPGCreator.Core.Types.Internal;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Maps;
using RPGCreator.SDK.Serializer;
using RPGCreator.SDK.Types;

namespace RPGCreator.Core.Types.Map;

public class IntGridLayerDefinition : IMapLayerDef<int>
{
    public event EventHandler<(Point, int)>? ElementAdded;
    public event EventHandler<(Point, int)>? ElementRemoved;
    public Ulid Unique { get; private set; } = Ulid.NewUlid();
    public URN Urn { get; private set; }
    public string Name { get; set; }
    public int ZIndex { get; set; }
    public bool VisibleByDefault { get; set; } = false;
    private Dictionary<Point, int> _elements = new();
    public ReadOnlyDictionary<Point, int> Elements { get; }
    
    public List<IntGridValueRef> ValueRefs { get; set; }

    public void SetValue(Point location, int value)
    {
        _elements[location] = value;
        ElementAdded?.Invoke(this, (location, value));
    }
    
    public int GetValue(Point location) => 
        _elements.TryGetValue(location, out var value) ? value : int.MinValue;
    
    public void AddElement(int element, Point location)
    {
        _elements[location] = element;
        ElementAdded?.Invoke(this, (location, element));
    }

    public bool TryAddElement(int element, Point location)
    {
        if (_elements.ContainsKey(location))
            return false;
        _elements[location] = element;
        ElementAdded?.Invoke(this, (location, element));
        return true;
    }

    public int RemoveElement(Point location)
    {
        return _elements.Remove(location, out var removedElement) ? removedElement : int.MinValue;
    }

    public bool TryRemoveElement(Point location, out int removedElement)
    {
        if (_elements.Remove(location, out removedElement))
        {
            ElementRemoved?.Invoke(this, (location, removedElement));
            return true;
        }
        return false;
    }

    public bool TryRemoveElement(int element, [NotNullWhen(true)] out Point? removedLocation)
    {
        foreach (var kvp in _elements)
        {
            if (kvp.Value == element)
            {
                _elements.Remove(kvp.Key);
                removedLocation = kvp.Key;
                ElementRemoved?.Invoke(this, (kvp.Key, element));
                return true;
            }
        }
        removedLocation = null;
        return false;
    }

    public int GetElement(Point location)
    {
        return _elements.TryGetValue(location, out var element) ? element : int.MinValue;
    }

    public bool TryGetElement(Point location, out int element)
    {
        return _elements.TryGetValue(location, out element);
    }

    public bool HasElement(Point location)
    {
        return _elements.ContainsKey(location);
    }

    public Dictionary<Point, int> GetSurroundingElements(Point location, int radius = 1, int offset = 1)
    {
        throw new NotImplementedException();
    }

    public void ClearElements()
    {
        _elements.Clear();
    }

    public SerializationInfo GetObjectData()
    {
        return new SerializationInfo(typeof(IntGridLayerDefinition))
            .AddValue(nameof(Unique), Unique)
            .AddValue(nameof(Urn), Urn)
            .AddValue(nameof(Name), Name)
            .AddValue(nameof(ZIndex), ZIndex)
            .AddValue(nameof(VisibleByDefault), VisibleByDefault)
            .AddValue(nameof(_elements), _elements)
            .AddValue(nameof(ValueRefs), ValueRefs);
    }

    public void SetObjectData(DeserializationInfo info)
    {
        return;
    }

    public bool IsDirty { get; set; }
    public bool IsTransient { get; set; }
}