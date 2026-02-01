using System.Numerics;
using RPGCreator.SDK.Assets.Definitions.Tilesets;

namespace RPGCreator.SDK.Types.Records;

public record struct TileData()
{
    public Vector2 TileSize;
    public Vector2 TilePosition;
    public Ulid TilesetId;
    
    public static TileData FromTileDef(ITileDef tileDef)
    {
        return new TileData
        {
            TileSize = new Vector2(tileDef.SizeInTileset.Width, tileDef.SizeInTileset.Height),
            TilePosition = new Vector2(tileDef.PositionInTileset.X, tileDef.PositionInTileset.Y),
            TilesetId = tileDef.TilesetDef.Unique,
        };
    }

    public ITileDef ToTileDef()
    {
        if(EngineServices.AssetsManager.TryResolveAsset(TilesetId, out BaseTilesetDef? tilesetDef))
        {
            return new TileDefinition(
                new Size((int)TileSize.X, (int)TileSize.Y),
                TilePosition,
                tilesetDef);
        }
        throw new InvalidOperationException($"Tileset with ID {TilesetId} could not be resolved.");
    }
}