using RPGCreator.SDK.Serializer;
using RPGCreator.SDK.Types;
using RPGCreator.SDK.Types.Interfaces;

namespace RPGCreator.SDK.Assets.Definitions.Maps;

public abstract class BaseLayerDef : IAssetDef
{
    public Ulid Unique { get; protected set; }
    public URN Urn => new URN("layer", $"{Name}@{Unique}");
    public bool IsDirty { get; set; }
    public bool IsTransient { get; set; }

    public string Name { get; set; } = "Layer";
    public int ZIndex { get; set; } = 0;
    public bool VisibleByDefault { get; set; } = true;
    public float Opacity { get; set; } = 1.0f;

    public BaseLayerDef()
    {
    }

    public void Init(Ulid id)
    {
        if (id != Ulid.Empty) return;
        Unique = id;
    }
    // Pour la sérialisation polymorphique
    public virtual SerializationInfo GetObjectData()
    {
        var info = new SerializationInfo(GetType());
        info.AddValue(nameof(Unique), Unique);
        info.AddValue(nameof(Name), Name);
        info.AddValue(nameof(ZIndex), ZIndex);
        info.AddValue(nameof(VisibleByDefault), VisibleByDefault);
        info.AddValue(nameof(Opacity), Opacity);
        return info;
    }

    public virtual void SetObjectData(DeserializationInfo info)
    {
        info.TryGetValue(nameof(Unique), out Ulid unique);
        info.TryGetValue(nameof(Name), out string name);
        info.TryGetValue(nameof(ZIndex), out int zIndex);
        info.TryGetValue(nameof(VisibleByDefault), out bool visible);
        info.TryGetValue(nameof(Opacity), out float opacity, 1.0f);

        Unique = unique;
        Name = name ?? "Layer";
        ZIndex = zIndex;
        VisibleByDefault = visible;
        Opacity = opacity;
    }
}