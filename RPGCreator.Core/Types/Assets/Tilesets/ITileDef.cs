using Microsoft.Xna.Framework;
using RPGCreator.Core.Rendering.Batching;
using RPGCreator.Core.Types.Map;
using Internal_Point = RPGCreator.Core.Types.Internal.Point;
using Point = RPGCreator.Core.Types.Internal.Point;

namespace RPGCreator.Core.Types.Assets.Tilesets;

public interface ITileDef : ILayerElem, IAssetDef
{
    public Vector2 DefaultPosition { get; set; }
    public Point SizeInTileset { get; }
    public Point PositionInTileset { get; } // Position in the tileset grid (row by column)
    public ITilesetDef TilesetDef { get; } // The tileset this tile belongs to
    public Rectangle UV { get; }

    public void UpdateTileset(ITilesetDef newTilesetDefinition);

    public bool IsEqualTo(ITileDef other);
}