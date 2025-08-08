using RPGCreator.Core.Type.Internal;
using RPGCreator.Core.Type.Map;

namespace RPGCreator.Core.Type.Assets.Tilesets;

public class AutotileGroup : ISerializable, IDeserializable
{
    public string Name { get; set; } = "Unnamed Autotile Group"; // Name of the autotile group
    public Ulid Unique { get; private set; } = Ulid.NewUlid();
    public List<Point> AffectedTiles = []; // List of affected tiles by this autotiling
    public List<Autotile> Autotiles = [];
    public ITileable? BaseTile; // The base tile for this autotile
    public List<string> GroupTags = []; // Tags for this autotile group, can be used for filtering or categorization
    public ITileset? Tileset => BaseTile?.Tileset; // The tileset this autotile group belongs to

    public AutotileGroup()
    {
    }

    public bool AddTile(ITileable tile)
    {
        if (tile == null || AffectedTiles.Contains(tile.PositionInTileset))
            return false; // If the tile is null or already in the list, we can't add it
        
        AffectedTiles.Add(tile.PositionInTileset);
        Autotiles.Add(new Autotile(tile.SizeInTileset, tile.PositionInTileset, tile.Tileset, this));
        return true; // Successfully added the tile
    }

    public ITileable? GetTileAt(TileLayer? layer, Point position)
    {
        var tile = Autotiles.FirstOrDefault(autotile => autotile.RespectRules(layer, position) && autotile != BaseTile);
        return tile ??
               BaseTile;
    }

    public ITileable? GetDirectTileAt(Point position)
    {
        return Autotiles.FirstOrDefault(autotile => autotile.PositionInTileset.X == position.X && autotile.PositionInTileset.Y == position.Y);
    }

    public bool RemoveTile(Autotile tile)
    {
        if (!Autotiles.Contains(tile))
        {
            return false;
        }
        
        AffectedTiles.Remove(tile.PositionInTileset);
        Autotiles.Remove(tile);
        return true;
    }

    public bool RemoveTile(Point position)
    {
        if (!HasTile(position))
            return false;

        var tileToRemove = GetDirectTileAt(position);
        if (tileToRemove == null)
            return false;
        
        return RemoveTile((Autotile)tileToRemove);
    }

    public bool SetBaseTile(ITileable tile)
    {
        if (!Autotiles.Contains(tile))
            return false;
        BaseTile = tile;
        return true;
    }
    
    public bool HasTile(Point position)
    {
        return AffectedTiles.Contains(position);
    }

    public SerializationInfo GetObjectData()
    {
        SerializationInfo info = new SerializationInfo(typeof(AutotileGroup));
        info.AddValue("Unique", Unique);
        info.AddValue("AffectedTiles", AffectedTiles);
        info.AddValue("Autotiles", Autotiles);
        info.AddValue("BaseTile", (ISerializable)BaseTile); // Force class to use the AddValue(string name, ISerializable? obj) method, and not the generic one.
        info.AddValue("Name", Name);
        info.AddValue("GroupTags", GroupTags);
        return info;
    }

    public void SetObjectData(DeserializationInfo info)
    {
        if (info == null)
        {
            throw new ArgumentNullException(nameof(info), "SerializationInfo cannot be null.");
        }

        info.TryGetValue("Unique", out Ulid unique, Ulid.NewUlid(), "Unique ID not found or invalid (Set to new Ulid by default).");
        info.TryGetList("AffectedTiles", out List<Point> affectedTiles, [], "Affected tiles not found or invalid (Set to empty list by default).");
        info.TryGetList("Autotiles", out List<Autotile> autotiles, [], "Autotiles not found or invalid (Set to empty list by default).");
        info.TryGetValue("BaseTile", out ITileable baseTile, null, "Base tile not found or invalid (Set to null by default).");
        info.TryGetValue("Name", out string name, "Unnamed Autotile Group", "Name not found or invalid (Set to 'Unnamed Autotile Group' by default).");
        info.TryGetList("GroupTags", out List<string> groupTags, [], "Group tags not found or invalid (Set to empty list by default).");

        Unique = unique;
        AffectedTiles = affectedTiles;
        Autotiles = autotiles;
        BaseTile = baseTile;
        Name = name;
        GroupTags = groupTags ?? [];
    }
}