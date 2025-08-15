using Microsoft.Xna.Framework;
using Point = RPGCreator.Core.Type.Internal.Point;

namespace RPGCreator.Core.Type.Assets.Tilesets;

public class TileDefinition : ITileDef
{
    public Vector2 DefaultPosition { get; set; }
    public Point SizeInTileset { get; private set; }
    public Point PositionInTileset { get; private set; }
    public Rectangle UV => new (new(PositionInTileset.X, PositionInTileset.Y), new(TilesetDef.TileWidth));
    public ITilesetDef TilesetDef { get; private set; }
    
    public TileDefinition(Vector2 defaultPosition, Point sizeInTileset, Point positionInTileset, ITilesetDef tilesetDef)
    {
        DefaultPosition = defaultPosition;
        SizeInTileset = sizeInTileset;
        PositionInTileset = positionInTileset;
        TilesetDef = tilesetDef;
    }
    public TileDefinition(Point sizeInTileset, Point positionInTileset, ITilesetDef tilesetDef)
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

    public void SetObjectData(DeserializationInfo info)
    {
        if (info == null)
        {
            throw new ArgumentNullException(nameof(info), "SerializationInfo cannot be null.");
        }

        info.TryGetValue("DefaultPosition", out Vector2 defaultPosition, Vector2.Zero, "Default position not found or invalid (Set to Vector2.Zero by default).");
        info.TryGetValue("SizeInTileset", out Point sizeInTileset, new Point(32, 32), "Size in tileset not found or invalid (Set to 32x32 by default).");
        info.TryGetValue("PositionInTileset", out Point positionInTileset, new Point(0, 0), "Position in tileset not found or invalid (Set to 0,0 by default).");
        info.TryGetValue("Tileset", out Ulid tilesetUnique, Ulid.Empty, "Tileset not found or invalid.");

        DefaultPosition = defaultPosition;
        SizeInTileset = sizeInTileset;
        PositionInTileset = positionInTileset;

        void OnEditedProjectLoaded()
        {
            var tileset = EngineCore.Instance.Managers.Assets.TilesetRegistry.Get(tilesetUnique);
            
            if (tileset == null)
            {
                throw new Exception($"Tileset with unique ID {tilesetUnique} not found in the project.");
            }

            TilesetDef = tileset;
            
            EngineCore.Instance.Data.EditedProject.OnProjectLoaded -= OnEditedProjectLoaded; // Unsubscribe from the event to avoid memory leaks
        }

        EngineCore.Instance.Data.EditedProject.OnProjectLoaded += OnEditedProjectLoaded;
    }
}