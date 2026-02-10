using RPGCreator.SDK.Serializer;

namespace RPGCreator.Core.Types.Blueprint;

public sealed class Link : ISerializable, IDeserializable
{
    public string FromNodeId { get; private set; }
    public string ToNodeId { get; private set; }
    public string FromPortId { get; private set; }
    public string ToPortId { get; private set; }

    public Link()
    {
        // Needed for serialization and deserialization
        // This constructor is used when deserializing the Link object
    }

    public Link(string fromNodeId, string fromPortId, string toNodeId, string toPortId)
    {
        FromNodeId = fromNodeId;
        FromPortId = fromPortId;
        ToNodeId = toNodeId;
        ToPortId = toPortId;
    }

    public SerializationInfo GetObjectData()
    {
        var info = new SerializationInfo(GetType())
            .AddValue(nameof(FromNodeId), FromNodeId)
            .AddValue(nameof(ToNodeId), ToNodeId)
            .AddValue(nameof(FromPortId), FromPortId)
            .AddValue(nameof(ToPortId), ToPortId);
        return info;
    }

    public List<Ulid> GetReferencedAssetIds()
    {
        // Links do not reference any assets, so we return an empty list
        return new List<Ulid>();
    }

    public void SetObjectData(DeserializationInfo info)
    {
        info.TryGetValue(nameof(FromNodeId), out var fromNodeId, string.Empty);
        FromNodeId = fromNodeId;
        info.TryGetValue(nameof(ToNodeId), out var toNodeId, string.Empty);
        ToNodeId = toNodeId;
        info.TryGetValue(nameof(FromPortId), out var fromPortId, string.Empty);
        FromPortId = fromPortId;
        info.TryGetValue(nameof(ToPortId), out var toPortId, string.Empty);
        ToPortId = toPortId;
    }
}