using Avalonia.Media.Imaging;
using SkiaSharp;
using Point = RPGCreator.Core.Type.Internal.Point;

namespace RPGCreator.Core.Type.Assets.Tilesets;

/*
 *
 * Tileset.cs
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
 * [Ward727, 26/07/2025 - DONE]
 * 
 */


public class Tileset : ImageAsset, ITileset, ISerializable, IDeserializable
{
    public int TileWidth { get; set; }
    public int TileHeight { get; set; }
 
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
        AddBaseSerialization(info);
        info.AddValue("ImagePath", ImagePath);
        info.AddValue("TileWidth", TileWidth);
        info.AddValue("TileHeight", TileHeight);
        return info;
    }

    public void SetObjectData(DeserializationInfo info)
    {
        if (info == null)
        {
            throw new ArgumentNullException(nameof(info), "SerializationInfo cannot be null.");
        }

        LoadBaseSerialization(info);
        info.TryGetValue("ImagePath", out string imagePath, string.Empty, "Image path not found or invalid (Set to empty string by default).");
        info.TryGetValue("TileWidth", out int tileWidth, 32, "Tile width not found or invalid (Set to 32 by default).");
        info.TryGetValue("TileHeight", out int tileHeight, 32, "Tile height not found or invalid (Set to 32 by default).");

        ImagePath = imagePath;
        TileWidth = tileWidth;
        TileHeight = tileHeight;
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
        return SKBitmap.Decode(ImagePath);
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
    }
}
