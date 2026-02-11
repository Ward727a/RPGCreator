using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using RPGCreator.SDK.Assets.Definitions;
using RPGCreator.SDK.Assets.Definitions.Maps;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers;
using RPGCreator.SDK.Assets.Definitions.Tilesets.IntGrid;
using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.Serializer;
using RPGCreator.SDK.Types;

namespace RPGCreator.Core.Types.Map;

[SerializingType("IntGridLayerDefinition")]
public class IntGridLayerDefinition : BaseAssetDef, IMapLayerDef<int>
{
    public event EventHandler<(Vector2, int)>? ElementAdded;
    public event EventHandler<(Vector2, int)>? ElementRemoved;
    public int ZIndex { get; set; }
    public bool VisibleByDefault { get; set; } = false;
    private Dictionary<Vector2, int> _elements = new();
    public ReadOnlyDictionary<Vector2, int> Elements { get; }
    
    public List<IntGridValueRef> ValueRefs { get; set; }
    
    public void SetValue(Vector2 location, int value)
    {
        _elements[location] = value;
        ElementAdded?.Invoke(this, (location, value));
    }
    
    public int GetValue(Vector2 location) => 
        _elements.TryGetValue(location, out var value) ? value : int.MinValue;
    
    public void AddElement(int element, Vector2 location)
    {
        _elements[location] = element;
        ElementAdded?.Invoke(this, (location, element));
    }

    public bool TryAddElement(int element, Vector2 location)
    {
        if (_elements.ContainsKey(location))
            return false;
        _elements[location] = element;
        ElementAdded?.Invoke(this, (location, element));
        return true;
    }

    public int RemoveElement(Vector2 location)
    {
        return _elements.Remove(location, out var removedElement) ? removedElement : int.MinValue;
    }

    public bool TryRemoveElement(Vector2 location, out int removedElement)
    {
        if (_elements.Remove(location, out removedElement))
        {
            ElementRemoved?.Invoke(this, (location, removedElement));
            return true;
        }
        return false;
    }

    public bool TryRemoveElement(int element, [NotNullWhen(true)] out Vector2? removedLocation)
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

    public int GetElement(Vector2 location)
    {
        return _elements.TryGetValue(location, out var element) ? element : int.MinValue;
    }

    public bool TryGetElement(Vector2 location, out int element)
    {
        return _elements.TryGetValue(location, out element);
    }

    public bool HasElement(Vector2 location)
    {
        return _elements.ContainsKey(location);
    }

    public void ClearElements()
    {
        _elements.Clear();
    }

    public SerializationInfo GetObjectData()
    {
        return new SerializationInfo(typeof(IntGridLayerDefinition))
            .AddValue(nameof(Unique), Unique)
            .AddValue(nameof(Name), Name)
            .AddValue(nameof(ZIndex), ZIndex)
            .AddValue(nameof(VisibleByDefault), VisibleByDefault)
            .AddValue(nameof(_elements), _elements)
            .AddValue(nameof(ValueRefs), ValueRefs);
    }

    public List<Ulid> GetReferencedAssetIds()
    {
        return [];
    }

    public void SetObjectData(DeserializationInfo info)
    {
        return;
    }

    public override UrnSingleModule UrnModule => "intgrid_layer".ToUrnSingleModule();
}