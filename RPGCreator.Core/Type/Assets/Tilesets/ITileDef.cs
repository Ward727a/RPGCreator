using Microsoft.Xna.Framework;
using RPGCreator.Core.Rendering.Batching;
using RPGCreator.Core.Type.Map;
using Point = RPGCreator.Core.Type.Internal.Point;

namespace RPGCreator.Core.Type.Assets.Tilesets;

public interface ITileDef : ILayerElem
{
    public Vector2 DefaultPosition { get; set; }
    public Point SizeInTileset { get; }
    public Point PositionInTileset { get; } // Position in the tileset grid (row by column)
    public ITilesetDef TilesetDef { get; } // The tileset this tile belongs to
    public Rectangle UV { get; }


    public void UpdateTileset(ITilesetDef newTilesetDefinition);

    public bool IsEqualTo(ITileDef other);
}