using System.Numerics;
using RPGCreator.SDK.Assets.Definitions.Maps;
using RPGCreator.SDK.Serializer;
using RPGCreator.SDK.Types;
using RPGCreator.SDK.Types.Collections;
using Size = RPGCreator.SDK.Types.Size;

namespace RPGCreator.SDK.Assets.Definitions.Tilesets;

[Flags]
public enum TileFlip
{
    None = 0,
    Horizontal = 1,
    Vertical = 2,
    Both = Horizontal | Vertical
}

public interface ITileDef : ILayerElem, ISerializable, IDeserializable
{
    public Size SizeInTileset { get; }
    public Vector2 PositionInTileset { get; } // Position in the tileset grid (row by column)
    public BaseTilesetDef TilesetDef { get; } // The tileset this tile belongs to
    public Rect UV { get; }
    public TileFlip Flip { get; set; }
    public RuntimeBag Tags { get; } 

    public void UpdateTileset(BaseTilesetDef newTilesetDefinition);
    public bool IsEqualTo(ITileDef other);
}