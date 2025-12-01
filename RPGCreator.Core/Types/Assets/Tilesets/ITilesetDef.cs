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
        Log.Error("GetObjectData not implemented for tileset {TilesetName}({type})", Name, GetType().Name);
        return default;
    }

    public virtual void SetObjectData(DeserializationInfo info)
    {
        Log.Error("SetObjectData not implemented for tileset {TilesetName}({type})", Name, GetType().Name);
    }

    public virtual bool IsDirty { get; set; }
    public virtual bool IsTransient { get; set; }
    public string SavePath { get; set; }
}