using Avalonia.Media.Imaging;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.Core.Type.Internal;
using SkiaSharp;

namespace RPGCreator.Core.Type.Assets.Tilesets;

public interface ITilesetDef : IHasUniqueId, ISerializable, IDeserializable, IAssetDef
{
    
    event Action? ImageChanged;
    
    public string ImagePath { get; }
    public string Name { get; set; }
    public int ImageWidth { get; set; }
    public int ImageHeight { get; set; }
    public int TileWidth { get; set; }
    public int TileHeight { get; set; }
    public Bitmap? BitmapCache { get; }

    public ITileDef GetTileAt(int col, int row);
    public Bitmap GetBitmap(bool forceReload = true);
    public Texture2D GetTexture(GraphicsDevice graphicsDevice);
    public Bitmap GetSimpleBitmap();
    public SKBitmap GetSKBitmap();
}