namespace RPGCreator.Core.Type.Blueprint;

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

    public virtual void SetObjectData(DeserializationInfo info)
    {
        info.TryGetValue(nameof(Id), out var id, string.Empty, "Error while deserializing Port.Id");
        Id = id;
        info.TryGetValue(nameof(Name), out var name, string.Empty, "Error while deserializing Port.Name");
        Name = name;
        info.TryGetValue(nameof(Kind), out PortKind kind, PortKind.Exec, "Error while deserializing Port.Kind");
        Kind = kind;
        info.TryGetValue(nameof(ValueType), out var valueType, string.Empty, "Error while deserializing Port.ValueType");
        ValueType = valueType;
        info.TryGetValue(nameof(Value), out var value, string.Empty, "Error while deserializing Port.Value");
        Value = value;
        info.TryGetValue(nameof(IsInput), out bool isInput, false, "Error while deserializing Port.IsInput");
        IsInput = isInput;
        info.TryGetValue(nameof(AllowManualInput), out bool allowManualInput, true, "Error while deserializing Port.AllowManualInput");
        AllowManualInput = allowManualInput;
    }
}