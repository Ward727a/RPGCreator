using RPGCreator.SDK.Assets.Definitions;
using RPGCreator.SDK.Serializer;
using RPGCreator.SDK.Types;

namespace RPGCreator.SDK.Assets;

public class GenericAssetStub : IAssetDef, IDeserializable
{
    public Ulid Unique { get; set; }
    public URN Urn { get; set; }
    public void Init(Ulid id)
    {
        if (Unique != Ulid.Empty) return;
        Unique = id;
    }

    public bool IsDirty { get; set; }
    public bool IsTransient { get; set; }
    public Dictionary<string, object> RawData { get; } = new Dictionary<string, object>();
    public void SetObjectData(DeserializationInfo info)
    {
        ArgumentNullException.ThrowIfNull(info);

        info.TryGetValue(nameof(Unique), out Ulid unique, Ulid.Empty);
        Unique = unique;

        info.TryGetValue(nameof(Urn), out URN urn, URN.Empty);
        Urn = urn;

        foreach (var key in info.GetAvailableKeys())
        {
            if (key == nameof(Unique) || key == nameof(Urn)) continue;
            info.TryGetValue(key, out object? value);
            RawData[key] = value!;
        }
    }
}