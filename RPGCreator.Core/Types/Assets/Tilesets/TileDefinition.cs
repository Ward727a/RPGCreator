using System.Numerics;
using RPGCreator.SDK.Assets.Definitions.Tilesets;
using RPGCreator.SDK.Serializer;
using RPGCreator.SDK.Types;

namespace RPGCreator.Core.Types.Assets.Tilesets;

public class TileDefinition : ITileDef
{
    public Vector2 Position { get; set; }
    public Vector2 DefaultPosition { get; set; }
    public Size SizeInTileset { get; private set; }
    public Vector2 PositionInTileset { get; private set; }
    public Rect UV => new (new(PositionInTileset.X, PositionInTileset.Y), new(TilesetDef.TileWidth, TilesetDef.TileHeight));
    public TileFlip Flip { get; set; } = TileFlip.None;
    public ITilesetDef TilesetDef { get; private set; }
    
    public TileDefinition(Vector2 defaultPosition, Size sizeInTileset, Vector2 positionInTileset, ITilesetDef tilesetDef)
    {
        DefaultPosition = defaultPosition;
        SizeInTileset = sizeInTileset;
        PositionInTileset = positionInTileset;
        TilesetDef = tilesetDef;
    }
    public TileDefinition(Size sizeInTileset, Vector2 positionInTileset, ITilesetDef tilesetDef)
    {
        SizeInTileset = sizeInTileset;
        PositionInTileset = positionInTileset;
        TilesetDef = tilesetDef;
        DefaultPosition = Vector2.Zero; // Default position
    }
    
    public void UpdateTileset(ITilesetDef newTilesetDefinition)
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
        SerializationInfo info = new SerializationInfo(typeof(TileInstance));
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

        void OnEditedProjectLoaded()
        {
            var tileset = EngineCore.Instance.Managers.Assets.TryResolveAsset<TilesetDef>(tilesetUnique, out var result) ? result : null;
            
            if (tileset == null)
            {
                throw new Exception($"Tileset with unique ID {tilesetUnique} not found in the project.");
            }

            TilesetDef = tileset;
            
            EngineCore.Instance.Data.EditedProject.OnProjectLoaded -= OnEditedProjectLoaded; // Unsubscribe from the event to avoid memory leaks
        }

        EngineCore.Instance.Data.EditedProject.OnProjectLoaded += OnEditedProjectLoaded;
    }

    public Ulid Unique { get; }
    public URN Urn { get; }
    public bool IsDirty { get; set; }
    public bool IsTransient { get; set; } = false;
}