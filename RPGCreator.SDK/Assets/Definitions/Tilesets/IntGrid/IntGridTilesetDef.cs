using RPGCreator.SDK.Assets.Definitions.Maps.AutoLayer;
using RPGCreator.SDK.Serializer;
using RPGCreator.SDK.Types;

namespace RPGCreator.SDK.Assets.Definitions.Tilesets.IntGrid;

public class IntGridTilesetDef : ITilesetDef, IAutotileDef
{
    public override Ulid Unique { get; protected set; } = Ulid.NewUlid();
    public override URN Urn => new URN("tileset", $"{Name}@{Unique}");

    public List<IntGridValueRef> IntRefs { get; set; } = new();
    public List<AutoLayerRule> Rules { get; set; } = new();
    
    public override SerializationInfo GetObjectData()
    {
        var info = base.GetObjectData();
        info.AddValue("Rules", Rules);
        info.AddValue("IntRefs", IntRefs);
        return info;
    }

    public override void SetObjectData(DeserializationInfo info)
    {
        base.SetObjectData(info);
        info.TryGetValue("Rules", out List<AutoLayerRule> rules);
        Rules = rules ?? new();
        info.TryGetValue("IntRefs", out List<IntGridValueRef> intRefs);
        IntRefs = intRefs ?? new();
    }
}