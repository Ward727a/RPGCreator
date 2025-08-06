using Avalonia.Media.Imaging;
using SkiaSharp;
using Point = RPGCreator.Core.Type.Internal.Point;

namespace RPGCreator.Core.Type.Assets.Tilesets;

/*
 *
 * NTileset.cs
 * ===========
 * This class represents a tileset asset in the RPG Creator framework.
 *
 * Note: This class is the new version of Tileset.cs, which was previously used.
 *      It is designed to be more efficient and easier to use.
 *      It combines the logic of the old Tileset system, with the new Autotiling system.
 *
 * DevNote:
 * For now, this class is still in development and is not fully implemented.
 * Right now I'm still just trying to get the basic functionality working.
 * [Ward727, 26/07/2025]
 * 
 */


public class Tileset : ImageAsset, ITileset, ISerializable, IDeserializable
{
    public int TileWidth { get; set; }
    public int TileHeight { get; set; }
    public bool IsSimple { get; set; } = true; // Indicates if the tileset is a simple tileset (no autotiling)
 
    public Tileset() : base()
    {
        Type = TYPE.TILESETS;
    }
    
    public Tileset(string name) : base(name)
    {
        Type = TYPE.TILESETS;
    }
    
    public Tileset(string name, int tileWidth, int tileHeight, string imagePath) : base(name)
    {
        Type = TYPE.TILESETS;
        Name = name;
        TileWidth = tileWidth;
        TileHeight = tileHeight;
        ImagePath = imagePath;
    }

    public SerializationInfo GetObjectData()
    {
        SerializationInfo info = new SerializationInfo(typeof(Tileset));
        info.AddValue("Unique", Unique);
        info.AddValue("Name", Name);
        info.AddValue("ImagePath", ImagePath);
        info.AddValue("TileWidth", TileWidth);
        info.AddValue("TileHeight", TileHeight);
        info.AddValue("IsSimple", IsSimple);
        return info;
    }

    public void SetObjectData(SerializationInfo info)
    {
        if (info == null)
        {
            throw new ArgumentNullException(nameof(info), "SerializationInfo cannot be null.");
        }

        info.TryGetValue("Unique", out Ulid unique, Ulid.NewUlid(), "Unique ID not found or invalid (Set to new Ulid by default).");
        info.TryGetValue("Name", out string name, "Unnamed Tileset", "Name not found or invalid (Set to 'Unnamed Tileset' by default).");
        info.TryGetValue("ImagePath", out string imagePath, string.Empty, "Image path not found or invalid (Set to empty string by default).");
        info.TryGetValue("TileWidth", out int tileWidth, 32, "Tile width not found or invalid (Set to 32 by default).");
        info.TryGetValue("TileHeight", out int tileHeight, 32, "Tile height not found or invalid (Set to 32 by default).");
        info.TryGetValue("IsSimple", out bool isSimple, true, "Is simple tileset not found or invalid (Set to true by default).");

        Unique = unique;
        Name = name;
        ImagePath = imagePath;
        TileWidth = tileWidth;
        TileHeight = tileHeight;
        IsSimple = isSimple;
    }
    
    public ITileable? GetTileAt(int row, int column)
    {
        var uv = new Point(row * TileWidth, column * TileHeight);
        var position = new Point(row, column);
        
        return new Tile(
            uv,
            position,
            this
        );
    }
    
    public bool HasTile(int row, int column)
    {
        // In a simple tileset, we can always assume the tile exists if the row and column are within bounds.
        return row >= 0 && column >= 0 && row < (ImageWidth / TileWidth) && column < (ImageHeight / TileHeight);
    }
    
    public bool HasTile(Point rowColumn)
    {
        return HasTile(rowColumn.X, rowColumn.Y);
    }

    public SKBitmap GetSKBitmap()
    {
        return SKBitmap.Decode(ImagePath); // If the tileset is simple, return the bitmap of the tileset image.
    }

    public Bitmap GetSimpleBitmap()
    {
        return new Bitmap(ImagePath);
    }
    
    public override Bitmap GetBitmap(bool forceReload = false)
    {
        if(_BitmapCache != null)
            return _BitmapCache;
        
        return GetSimpleBitmap();
        
        // If the tileset is not single, we need to combine the images of all tiles inside the autotiling and return a single bitmap.
    }
}
