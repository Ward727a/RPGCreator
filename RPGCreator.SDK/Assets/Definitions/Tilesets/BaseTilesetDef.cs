using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Serializer;
using RPGCreator.SDK.Types;
using RPGCreator.SDK.Types.Collections;
using RPGCreator.SDK.Types.Internals;

namespace RPGCreator.SDK.Assets.Definitions.Tilesets;

[SerializingType("BaseTilesetDef")]
public abstract class BaseTilesetDef : BaseAssetDef, ISerializable, IDeserializable, IHasSavePath
{
    
    public event Action? ImageChanged;
    public string SavePath { get; set; } = null!;

    public string PackName { get; set; } = "";
    public virtual IAssetsPack Pack { get; set; } = null!;
    public virtual string ImagePath { get; set; } = null!;
    public virtual int ImageWidth { get; set; }
    public virtual int ImageHeight { get; set; }
    public virtual int TileWidth { get; set; }
    public virtual int TileHeight { get; set; }
    
    public RuntimeBag Tags { get; } = new RuntimeBag();
    
    public virtual SerializationInfo GetObjectData()
    {
        var info = new SerializationInfo(GetType());
        info.AddValue("Unique", Unique);
        info.AddValue("PackName", string.IsNullOrWhiteSpace(PackName) ? Pack?.Name ?? string.Empty : PackName);
        info.AddValue("Name", Name);
        info.AddValue("ImagePath", ImagePath);
        info.AddValue("ImageWidth", ImageWidth);
        info.AddValue("ImageHeight", ImageHeight);
        info.AddValue("TileWidth", TileWidth);
        info.AddValue("TileHeight", TileHeight);
        return info;
    }

    public List<Ulid> GetReferencedAssetIds()
    {
        var referencedIds = new List<Ulid>();
        if (Pack != null)
            referencedIds.Add(Pack.Id);
        return referencedIds;
    }

    public virtual void SetObjectData(DeserializationInfo info)
    {
        info.TryGetValue("Unique", out Ulid unique, Ulid.Empty);
        Unique = unique;
        info.TryGetValue("PackName", out string packName, string.Empty);
        PackName = packName;
        info.TryGetValue("Name", out string name, string.Empty);
        Name = name;
        info.TryGetValue("ImagePath", out string imagePath, string.Empty);
        ImagePath = imagePath;
        info.TryGetValue("ImageWidth", out int imageWidth, 0);
        ImageWidth = imageWidth;
        info.TryGetValue("ImageHeight", out int imageHeight, 0);
        ImageHeight = imageHeight;
        info.TryGetValue("TileWidth", out int tileWidth, 0);
        TileWidth = tileWidth;
        info.TryGetValue("TileHeight", out int tileHeight, 0);
        TileHeight = tileHeight;
        
        //Get the assets pack
        if (EngineServices.AssetsManager.TryGetPack(PackName, out var pack))
        {
            Pack = pack;
        }
        else
        {
            Logger.Warning("Tileset {TilesetName} ({Unique}) references missing pack {PackName}", Name, Unique, PackName);
        }
    }
}