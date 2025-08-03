using RPGCreator.Core.Type.Map;

namespace RPGCreator.Core.Type.Interfaces;

public interface ITileset
{
    public int TileWidth { get; protected set; }
    public int TileHeight { get; protected set; }
    
    public Tile? GetTile(int row, int column);
    public Tile? GetTileAt(RPGCreator.Core.Type.Internal.Point at);
    public bool HasTile(int row, int column);
}