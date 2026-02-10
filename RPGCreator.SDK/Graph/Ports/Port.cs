using RPGCreator.SDK.Graph;
using RPGCreator.SDK.Serializer;

namespace RPGCreator.Core.Types.Blueprint;

public class Port : ISerializable, IDeserializable
{
    public Port()
    {
        // Needed for serialization and deserialization
        // This constructor is used when deserializing the Port object
    }

    public virtual void SetData(GraphDocument.PortData data)
    {
        Id = data.Id;
        Name = data.Name;
        Value = data.Value;
    }


    public string Id { get; private set; } = Ulid.NewUlid().ToString();
    public string Name { get; set; } = "";
    public PortKind Kind { get; set; }
    public EPortType Type { get; set; } = EPortType.Single;
    public System.Type ObjectInternalType { get; set; } = typeof(object); // The actual type of the object, e.g. float, bool, etc.
    public string ValueType { get; set; } = ""; // "float", "bool", etc.
    public object? Value { get; set; } // String representation of the value, e.g. "42" for a float or "true" for a bool.
    public bool IsInput { get; set; } = false;
    public bool AllowManualInput { get; set; } = true; // Whether the user can manually input a value in the UI
    public virtual SerializationInfo GetObjectData()
    {
        var info = new SerializationInfo(GetType())
            .AddValue(nameof(Id), Id)
            .AddValue(nameof(Name), Name)
            .AddValue(nameof(Kind), Kind)
            .AddValue(nameof(ValueType), ValueType)
            .AddValue(nameof(Value), Value)
            .AddValue(nameof(IsInput), IsInput)
            .AddValue(nameof(AllowManualInput), AllowManualInput);
        return info;
    }

    public List<Ulid> GetReferencedAssetIds()
    {
        return new List<Ulid>();
    }

    public virtual void SetObjectData(DeserializationInfo info)
    {
        info.TryGetValue(nameof(Id), out var id, string.Empty);
        Id = id;
        info.TryGetValue(nameof(Name), out var name, string.Empty);
        Name = name;
        info.TryGetValue(nameof(Kind), out PortKind kind, PortKind.Exec);
        Kind = kind;
        info.TryGetValue(nameof(ValueType), out var valueType, string.Empty);
        ValueType = valueType;
        info.TryGetValue(nameof(Value), out var value, string.Empty);
        Value = value;
        info.TryGetValue(nameof(IsInput), out bool isInput, false);
        IsInput = isInput;
        info.TryGetValue(nameof(AllowManualInput), out bool allowManualInput, true);
        AllowManualInput = allowManualInput;
    }
}