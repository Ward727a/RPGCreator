using System.Drawing;

namespace RPGCreator.SDK.Assets.Definitions.Tilesets;

public interface ITilesetInstance 
{
    public ITilesetDef Definition { get; }
    
    public bool HasTile(int row, int column);
    public bool HasTile(Point rowColumn);
    public ITileDef? GetTileAt(int row, int column);
}