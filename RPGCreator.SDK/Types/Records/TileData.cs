using System.Drawing;
using RPGCreator.SDK.Assets.Definitions.Tilesets;

namespace RPGCreator.SDK.Types.Records;

public record struct TileData()
{
    public Ulid UniqueId = Ulid.NewUlid();
    public Point TileSize;
    public Point TilePosition;
    public Ulid TilesetId;
    
    public static TileData FromTileDef(ITileDef tileDef)
    {
        return new TileData
        {
            TileSize = new Point(tileDef.SizeInTileset.Width, tileDef.SizeInTileset.Height),
            TilePosition = new Point(tileDef.PositionInTileset.X, tileDef.PositionInTileset.Y),
            TilesetId = tileDef.TilesetDef.Unique,
        };
    }
}