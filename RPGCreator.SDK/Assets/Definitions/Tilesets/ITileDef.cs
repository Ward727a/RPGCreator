using System.Drawing;
using System.Numerics;
using RPGCreator.SDK.Assets.Definitions.Maps;
using RPGCreator.SDK.Types.Interfaces;

namespace RPGCreator.SDK.Assets.Definitions.Tilesets;

[Flags]
public enum TileFlip
{
    None = 0,
    Horizontal = 1,
    Vertical = 2,
    Both = Horizontal | Vertical
}

public interface ITileDef : ILayerElem, IAssetDef
{
    public Vector2 DefaultPosition { get; set; }
    public Size SizeInTileset { get; }
    public Point PositionInTileset { get; } // Position in the tileset grid (row by column)
    public ITilesetDef TilesetDef { get; } // The tileset this tile belongs to
    public Rectangle UV { get; }
    public TileFlip Flip { get; set; }

    public void UpdateTileset(ITilesetDef newTilesetDefinition);

    public bool IsEqualTo(ITileDef other);
}