using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Serializer;
using RPGCreator.SDK.Types;

namespace RPGCreator.SDK.Assets.Definitions.Tilesets;

[SerializingType("Tileset")]
public sealed class TilesetDef : BaseTilesetDef
{
    public TilesetDef()
    {
    }
    
    public TilesetDef(string imagePath, string name, int tileWidth = 32, int tileHeight = 32)
    {
        Unique = Ulid.NewUlid();
        Name = name;
        ImagePath = imagePath;
        TileWidth = tileWidth;
        TileHeight = tileHeight;
    }
    
    public override SerializationInfo GetObjectData()
    {
        var info = new SerializationInfo(typeof(TilesetDef));

        info.AddValue(nameof(Unique), Unique)
            .AddValue(nameof(Name), Name)
            .AddValue(nameof(ImagePath), ImagePath)
            .AddValue(nameof(TileWidth), TileWidth)
            .AddValue(nameof(TileHeight), TileHeight);
        
        return info;
    }

    public override void SetObjectData(DeserializationInfo info)
    {
        if (info == null)
        {
            throw new ArgumentNullException(nameof(info), "SerializationInfo cannot be null.");
        }

        info.TryGetValue(nameof(Unique), out Ulid unique, Ulid.Empty);
        info.TryGetValue(nameof(Name), out string name, string.Empty);
        info.TryGetValue("ImagePath", out string imagePath, string.Empty);
        info.TryGetValue("TileWidth", out int tileWidth, 32);
        info.TryGetValue("TileHeight", out int tileHeight, 32);

        Unique = unique;
        Name = name;
        ImagePath = imagePath;
        TileWidth = tileWidth;
        TileHeight = tileHeight;
        
        Logger.Debug("[TilesetDef] SetObjectData ({0}, {1})", Unique, Name);
    }

    public override UrnSingleModule UrnModule => "tileset".ToUrnSingleModule();
}