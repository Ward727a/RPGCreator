using Avalonia.Media.Imaging;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.Core.Serializer;
using RPGCreator.Core.Types.Internal;
using Serilog;
using SkiaSharp;

namespace RPGCreator.Core.Types.Assets.Tilesets;

public abstract class ITilesetDef : IHasUniqueId, ISerializable, IDeserializable, IAssetDef, IHasSavePath
{
    
    public event Action? ImageChanged;

    public string PackName { get; set; } = "";
    public virtual BaseAssetsPack.BaseAssetsPack Pack { get; set; }
    public virtual string ImagePath { get; set; }
    public virtual string Name { get; set; }
    public virtual int ImageWidth { get; set; }
    public virtual int ImageHeight { get; set; }
    public virtual int TileWidth { get; set; }
    public virtual int TileHeight { get; set; }
    protected virtual Bitmap? _BitmapCache { get; set; }
    public virtual Bitmap? BitmapCache { get; }

    public virtual ITileDef GetTileAt(int col, int row)
    {
        throw new NotImplementedException();
    }

    public virtual Bitmap GetBitmap(bool forceReload = true)
    {
        Log.Error("GetBitmap not implemented for tileset {TilesetName}({type})", Name, GetType().Name);
        return default;
    }

    public virtual Texture2D GetTexture(GraphicsDevice graphicsDevice)
    {
        Log.Error("GetTexture not implemented for tileset {TilesetName}({type})", Name, GetType().Name);
        return default;
    }

    public virtual Bitmap GetSimpleBitmap()
    {
        Log.Error("GetSimpleBitmap not implemented for tileset {TilesetName}({type})", Name, GetType().Name);
        return default;
    }

    public virtual SKBitmap GetSKBitmap()
    {
        Log.Error("GetSKBitmap not implemented for tileset {TilesetName}({type})", Name, GetType().Name);
        return default;
    }

    public virtual Ulid Unique { get; protected set; }
    public virtual URN Urn { get; protected set; }
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
        if (EngineCore.Instance.Managers.Assets.TryGetPack(PackName, out var pack))
        {
            Pack = pack;
        }
        else
        {
            Log.Warning("Tileset {TilesetName} ({Unique}) references missing pack {PackName}", Name, Unique, PackName);
        }
    }

    public virtual bool IsDirty { get; set; }
    public virtual bool IsTransient { get; set; }
    public string SavePath { get; set; }
}