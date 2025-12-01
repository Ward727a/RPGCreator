using Avalonia.Media.Imaging;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.Core.Serializer;
using RPGCreator.Core.Types.Internal;
using RPGCreator.Core.Types.Map;
using RPGCreator.Core.Types.Map.AutoLayer;
using Serilog;
using SkiaSharp;

namespace RPGCreator.Core.Types.Assets.Tilesets.IntGridTileset;

public class IntGridTileset : ITilesetDef
{
    public override Ulid Unique { get; protected set; } = Ulid.NewUlid();
    public override URN Urn => new URN("tileset", $"{Name}@{Unique}");

    public List<IntGridValueRef> IntRefs { get; set; } = new();
    public List<AutoLayerRule> Rules { get; set; } = new();
    
    public override SerializationInfo GetObjectData()
    {
        var info = new SerializationInfo(GetType());
        info.AddValue("Unique", Unique.ToString());
        info.AddValue("Name", Name);
        info.AddValue("Rules", Rules);
        info.AddValue("IntRefs", IntRefs);
        return info;
    }

    public override void SetObjectData(DeserializationInfo info)
    {
        info.TryGetValue("Unique", out string uniqueStr);
        Unique = Ulid.Parse(uniqueStr);
        info.TryGetValue("Name", out string name);
        Name = name ?? "";
        info.TryGetValue("Rules", out List<AutoLayerRule> rules);
        Rules = rules ?? new();
        info.TryGetValue("IntRefs", out List<IntGridValueRef> intRefs);
        IntRefs = intRefs ?? new();
    }
}