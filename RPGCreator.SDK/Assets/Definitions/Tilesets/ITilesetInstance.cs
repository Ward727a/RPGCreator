using System.Numerics;

namespace RPGCreator.SDK.Assets.Definitions.Tilesets;

public interface ITilesetInstance 
{
    public BaseTilesetDef Definition { get; }
    
    public bool HasTile(int row, int column);
    public bool HasTile(Vector2 rowColumn);
    public ITileDef? GetTileAt(int row, int column);
}