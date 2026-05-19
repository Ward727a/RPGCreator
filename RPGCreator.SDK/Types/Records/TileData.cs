using System.Numerics;
using RPGCreator.SDK.Assets.Definitions.Tilesets;
using RPGCreator.Shared.Types;

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
        var tileset = EngineServices.AssetsManager.Load<BaseTilesetDef>(TilesetId);
        
        if(tileset.IsFailure)
            throw new InvalidOperationException($"Tileset with ID {TilesetId} could not be resolved. Error: {tileset.Error}");
        
        return new TileDefinition(
            new Size((int)TileSize.X, (int)TileSize.Y),
            TilePosition,
            tileset.Value);
    }
}