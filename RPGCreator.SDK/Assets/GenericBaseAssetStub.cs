using RPGCreator.SDK.Assets.Definitions;
using RPGCreator.SDK.Serializer;

namespace RPGCreator.SDK.Assets;

public class GenericBaseAssetStub : BaseAssetDef, IDeserializable
{
    public Ulid Unique { get; set; }

    public Dictionary<string, object> RawData { get; } = new();
    public void SetObjectData(DeserializationInfo info)
    {
        ArgumentNullException.ThrowIfNull(info);

        info.TryGetValue(nameof(Unique), out Ulid unique, Ulid.Empty);
        Unique = unique;

        foreach (var key in info.GetAvailableKeys())
        {
            if (key == nameof(Unique)) continue;
            info.TryGetValue(key, out object? value);
            RawData[key] = value!;
        }
    }
}