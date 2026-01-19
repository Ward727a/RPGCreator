using System.Drawing;
using System.Numerics;
using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.Serializer;
using RPGCreator.SDK.Types;
using RPGCreator.SDK.Types.Collections;
using Size = RPGCreator.SDK.Types.Size;

namespace RPGCreator.SDK.Assets.Definitions.Tilesets;

[SerializingType("TileDefinition")]
public class TileDefinition : ITileDef
{

    public Ulid Unique { get; private set; }
    public URN Urn { get; }
    public bool IsDirty { get; set; }
    public bool IsTransient { get; set; } = false;
    
    public Vector2 Position { get; set; }
    public Vector2 DefaultPosition { get; set; }
    public Size SizeInTileset { get; private set; }
    public Vector2 PositionInTileset { get; private set; }
    public Rect UV => new (new(PositionInTileset.X, PositionInTileset.Y), new(TilesetDef.TileWidth, TilesetDef.TileHeight));
    public TileFlip Flip { get; set; } = TileFlip.None;
    public BaseTilesetDef TilesetDef { get; private set; }
    public RuntimeBag Tags { get; }
    
    public TileDefinition(Vector2 defaultPosition, Size sizeInTileset, Vector2 positionInTileset, BaseTilesetDef tilesetDef)
    {
        DefaultPosition = defaultPosition;
        SizeInTileset = sizeInTileset;
        PositionInTileset = positionInTileset;
        TilesetDef = tilesetDef;
    }
    public TileDefinition(Size sizeInTileset, Vector2 positionInTileset, BaseTilesetDef tilesetDef)
    {
        SizeInTileset = sizeInTileset;
        PositionInTileset = positionInTileset;
        TilesetDef = tilesetDef;
        DefaultPosition = Vector2.Zero; // Default position
    }

    public void Init(Ulid id)
    {
        if (id != Ulid.Empty) return;
        Unique = id;
    }

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

        return DefaultPosition == other.DefaultPosition &&
               SizeInTileset.Equals(other.SizeInTileset) &&
               PositionInTileset.Equals(other.PositionInTileset) &&
               TilesetDef.Unique == other.TilesetDef.Unique;
    }

    public SerializationInfo GetObjectData()
    {
        SerializationInfo info = new SerializationInfo(typeof(TileDefinition));
        info.AddValue("DefaultPosition", DefaultPosition);
        info.AddValue("SizeInTileset", SizeInTileset);
        info.AddValue("PositionInTileset", PositionInTileset);
        info.AddValue("Tileset", TilesetDef.Unique);
        return info;
    }

    public void SetObjectData(DeserializationInfo info)
    {
        if (info == null)
        {
            throw new ArgumentNullException(nameof(info), "SerializationInfo cannot be null.");
        }

        info.TryGetValue("DefaultPosition", out Vector2 defaultPosition, Vector2.Zero);
        info.TryGetValue("SizeInTileset", out Size sizeInTileset, new Size(32, 32));
        info.TryGetValue("PositionInTileset", out Vector2 positionInTileset, Vector2.Zero);
        info.TryGetValue("Tileset", out Ulid tilesetUnique, Ulid.Empty);

        DefaultPosition = defaultPosition;
        SizeInTileset = sizeInTileset;
        PositionInTileset = positionInTileset;
    }
}