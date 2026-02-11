using System.Numerics;
using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Serializer;
using RPGCreator.SDK.Types;
using RPGCreator.SDK.Types.Collections;
using Size = RPGCreator.SDK.Types.Size;

namespace RPGCreator.SDK.Assets.Definitions.Tilesets;

[SerializingType("TileDefinition")]
public class TileDefinition : BaseAssetDef, ITileDef
{
    public Vector2 Position { get; set; }
    public Size SizeInTileset { get; private set; }
    public Vector2 PositionInTileset { get; private set; }
    public Rect UV => new (new(PositionInTileset.X, PositionInTileset.Y), new(TilesetDef.TileWidth, TilesetDef.TileHeight));
    public TileFlip Flip { get; set; } = TileFlip.None;
    public BaseTilesetDef TilesetDef { get; private set; }
    public RuntimeBag Tags { get; } = new RuntimeBag();

    public TileDefinition()
    {
    }

    public TileDefinition(Size sizeInTileset, Vector2 positionInTileset, BaseTilesetDef tilesetDef)
    {
        SizeInTileset = sizeInTileset;
        PositionInTileset = positionInTileset;
        TilesetDef = tilesetDef;
    }

    public override UrnSingleModule UrnModule => "tile".ToUrnSingleModule();

    public void UpdateTileset(BaseTilesetDef newTilesetDefinition)
    {
        if (newTilesetDefinition == null)
        {
            throw new ArgumentNullException(nameof(newTilesetDefinition), "New tileset definition cannot be null.");
        }

        TilesetDef = newTilesetDefinition;
    }

    public bool IsEqualTo(ITileDef other)
    {
        if (other == null)
        {
            return false;
        }

        return SizeInTileset.Equals(other.SizeInTileset) &&
               PositionInTileset.Equals(other.PositionInTileset) &&
               TilesetDef.Unique == other.TilesetDef.Unique;
    }

    public SerializationInfo GetObjectData()
    {
        SerializationInfo info = new SerializationInfo(typeof(TileDefinition));
        info.AddValue("SizeInTileset", SizeInTileset);
        info.AddValue("PositionInTileset", PositionInTileset);
        info.AddValue("Tileset", TilesetDef.Unique);
        return info;
    }

    public List<Ulid> GetReferencedAssetIds()
    {
        List<Ulid> referencedIds = new List<Ulid>();
        if (TilesetDef != null)
        {
            referencedIds.Add(TilesetDef.Unique);
        }
        return referencedIds;
    }

    public void SetObjectData(DeserializationInfo info)
    {
        if (info == null)
        {
            throw new ArgumentNullException(nameof(info), "SerializationInfo cannot be null.");
        }

        info.TryGetValue("SizeInTileset", out Size sizeInTileset, new Size(32, 32));
        info.TryGetValue("PositionInTileset", out Vector2 positionInTileset, Vector2.Zero);
        info.TryGetValue("Tileset", out Ulid tilesetUnique, Ulid.Empty);

        SizeInTileset = sizeInTileset;
        PositionInTileset = positionInTileset;
        if(EngineServices.AssetsManager.TryResolveAsset(tilesetUnique, out TilesetDef? tileset))
        {
            TilesetDef = tileset;
        }
        else
        {
            Logger.Error("Failed to resolve TilesetDef with Unique ID {TilesetUnique} during TileDefinition deserialization.", tilesetUnique);
        }
    }
}