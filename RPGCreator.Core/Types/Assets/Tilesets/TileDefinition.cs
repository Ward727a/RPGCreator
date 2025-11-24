using Microsoft.Xna.Framework;
using RPGCreator.Core.Types.Internal;
using Internal_Point = RPGCreator.Core.Types.Internal.Point;
using Point = RPGCreator.Core.Types.Internal.Point;

namespace RPGCreator.Core.Types.Assets.Tilesets;

public class TileDefinition : ITileDef
{
    public Vector2 DefaultPosition { get; set; }
    public Internal_Point SizeInTileset { get; private set; }
    public Internal_Point PositionInTileset { get; private set; }
    public Rectangle UV => new (new(PositionInTileset.X, PositionInTileset.Y), new(TilesetDef.TileWidth));
    public ITilesetDef TilesetDef { get; private set; }
    
    public TileDefinition(Vector2 defaultPosition, Internal_Point sizeInTileset, Internal_Point positionInTileset, ITilesetDef tilesetDef)
    {
        DefaultPosition = defaultPosition;
        SizeInTileset = sizeInTileset;
        PositionInTileset = positionInTileset;
        TilesetDef = tilesetDef;
    }
    public TileDefinition(Internal_Point sizeInTileset, Internal_Point positionInTileset, ITilesetDef tilesetDef)
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
               SizeInTileset.IsEqualTo(other.SizeInTileset) &&
               PositionInTileset.IsEqualTo(other.PositionInTileset) &&
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

    public void SetObjectData(Serializer.DeserializationInfo info)
    {
        if (info == null)
        {
            throw new ArgumentNullException(nameof(info), "SerializationInfo cannot be null.");
        }

        info.TryGetValue("DefaultPosition", out Vector2 defaultPosition, Vector2.Zero);
        info.TryGetValue("SizeInTileset", out Internal_Point sizeInTileset, new Internal_Point(32, 32));
        info.TryGetValue("PositionInTileset", out Internal_Point positionInTileset, new Internal_Point(0, 0));
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