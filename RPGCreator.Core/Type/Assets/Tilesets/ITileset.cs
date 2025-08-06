using Avalonia.Media.Imaging;
using RPGCreator.Core.Type.Internal;
using SkiaSharp;

namespace RPGCreator.Core.Type.Assets.Tilesets;

public interface ITileset
{
    
    public event EventHandler? ImageChanged;
    public string ImagePath { get; }
    public Ulid Unique { get; }
    public string Name { get; set; }
    public int TileWidth { get; set; }
    public int TileHeight { get; set; }

    /// <summary>
    /// Define if the tileset is a simple tileset or not.<br/>
    /// A simple tileset is a tileset that does not use autotiling.<br/>
    /// A "complex" tileset is a tileset that uses autotiling.
    /// </summary>
    public bool IsSimple { get; }
    
    public bool HasTile(int row, int column);
    public bool HasTile(Point rowColumn);
    public ITileable? GetTileAt(int row, int column);
    public SKBitmap GetSKBitmap();
    public Bitmap GetBitmap(bool forceReload = false);
}