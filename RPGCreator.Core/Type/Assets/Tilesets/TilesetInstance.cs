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


public class TilesetInstance : ITilesetInstance
{
    public int TileWidth => Definition.TileWidth;
    public int TileHeight => Definition.TileHeight;

    public event EventHandler? ImageChanged;
    public ITilesetDef Definition { get; }
 
    public TilesetInstance()
    {
    }
    
    public TilesetInstance(TilesetDef def)
    {
        Definition = def;
    }
    
    public ITileDef? GetTileAt(int row, int column)
    {
        var uv = new Point(row * TileWidth, column * TileHeight);
        var position = new Point(row, column);
        
        return new TileDefinition(
            uv,
            position,
            Definition
        );
    }

    public bool HasTile(int row, int column)
    {
        // In a simple tileset, we can always assume the tile exists if the row and column are within bounds.
        return row >= 0 && column >= 0 && row < (Definition.ImageWidth / TileWidth) && column < (Definition.ImageHeight / TileHeight);
    }
    
    public bool HasTile(Point rowColumn)
    {
        return HasTile(rowColumn.X, rowColumn.Y);
    }
}
