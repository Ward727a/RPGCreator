using RPGCreator.Core.Type.Internal;
using RPGCreator.Core.Type.Map;

namespace RPGCreator.Core.Type.Assets.Tilesets;

public class AutotileGroupDef : IHasUniqueId, ISerializable, IDeserializable
{
    public Ulid Unique { get; private set; }
    public URN Urn => new("autotile_group", $"{Name}@{Unique}");
    
    public ITileDef? BaseTile { get; set; }
    
    public string Name { get; set; } = string.Empty;
    public Ulid TilesetUnique { get; set; }
    public ITilesetDef? TilesetDef => EngineCore.Instance.Managers.Assets.TilesetRegistry.Get(TilesetUnique);
    
    private readonly List<AutotileDef> _tiles = new List<AutotileDef>();
    public IReadOnlyList<AutotileDef> Tiles => _tiles.AsReadOnly();
    
    private readonly List<string> _tags = new List<string>();
    public IReadOnlyList<string> Tags => _tags.AsReadOnly();
    
    private HashSet<Point> _affectedTiles = new HashSet<Point>();
    public IReadOnlySet<Point> AffectedTiles => _affectedTiles;

    public AutotileGroupDef(string name)
    {
        Unique = Ulid.NewUlid();
        Name = name;
    }
    
    public bool AddTile(AutotileDef tile)
    {
        if (tile == null || _affectedTiles.Contains(tile.PositionInTileset))
            return false; // If the tile is null or already in the list, we can't add it
        
        _affectedTiles.Add(tile.PositionInTileset);
        _tiles.Add(tile);
        return true;
    }
    public bool HasTile(Point position)
    {
        return _affectedTiles.Contains(position);
    }
    public AutotileDef? GetTileAt(TileLayerDefinition? layer, Point position)
    {
        return _tiles.FirstOrDefault(autotile => autotile.RespectRules(layer, position) && !autotile.IsEqualTo(BaseTile));
    }
    public AutotileDef? GetDirectTileAt(Point position)
    {
        return _tiles.FirstOrDefault(autotile => autotile.PositionInTileset.IsEqualTo(position) && !autotile.IsEqualTo(BaseTile));
    }
    public bool RemoveTile(AutotileDef tile)
    {
        if (!_tiles.Contains(tile))
        {
            return false;
        }
        
        _affectedTiles.Remove(tile.PositionInTileset);
        _tiles.Remove(tile);
        return true;
    }
    public bool RemoveTile(Point position)
    {
        if (!HasTile(position))
            return false;

        var tileToRemove = GetTileAt(null, position);
        if (tileToRemove == null)
            return false;

        _affectedTiles.Remove(tileToRemove.PositionInTileset);
        _tiles.Remove(tileToRemove);
        return true;
    }
    
    public void SetBaseTile(ITileDef? tile)
    {
        if (tile == null || tile.TilesetDef?.Unique != TilesetUnique)
            throw new ArgumentException("Base tile must belong to the same tileset as this autotile group.");
        
        BaseTile = tile;
    }
    
    public void AddTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag) || _tags.Contains(tag))
            return; // If the tag is null, empty, or already exists, we can't add it
        
        _tags.Add(tag);
    }
    public void RemoveTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag) || !_tags.Contains(tag))
            return; // If the tag is null, empty, or does not exist, we can't remove it
        
        _tags.Remove(tag);
    }
    
    public SerializationInfo GetObjectData()
    {
        var info = new SerializationInfo(typeof(AutotileGroupDef));
        info.AddValue(nameof(Unique), Unique);
        info.AddValue(nameof(Name), Name);
        info.AddValue(nameof(TilesetUnique), TilesetUnique);
        info.AddValue(nameof(BaseTile), BaseTile);
        info.AddValue(nameof(_tiles), _tiles);
        info.AddValue(nameof(_tags), _tags);
        return info;
    }

    public void SetObjectData(DeserializationInfo info)
    {
        if(info == null) 
            throw new ArgumentNullException(nameof(info), "Deserialization info cannot be null.");
        
        info.TryGetValue(nameof(Unique), out var unique, Ulid.Empty, $"{nameof(AutotileGroupDef)}.{nameof(Unique)} not found or invalid (Set to Ulid.Empty by default).");
        info.TryGetValue(nameof(Name), out var name, string.Empty, $"{nameof(AutotileGroupDef)}.{nameof(Name)} not found or invalid (Set to empty string by default).");
        info.TryGetValue(nameof(TilesetUnique), out var tilesetUnique, Ulid.Empty, $"{nameof(AutotileGroupDef)}.{nameof(TilesetUnique)} not found or invalid (Set to Ulid.Empty by default).");
        info.TryGetValue(nameof(BaseTile), out ITileDef? baseTile, null, $"{nameof(AutotileGroupDef)}.{nameof(BaseTile)} not found or invalid (Set to null by default).");
        info.TryGetList(nameof(_tiles), out List<AutotileDef> tiles, [], $"{nameof(AutotileGroupDef)}.{nameof(_tiles)} not found or invalid (Set to empty list by default).");
        info.TryGetList(nameof(_tags), out List<string> tags, [], $"{nameof(AutotileGroupDef)}.{nameof(_tags)} not found or invalid (Set to empty list by default).");
        if (unique == Ulid.Empty)
            throw new InvalidOperationException($"{nameof(AutotileGroupDef)}.{nameof(Unique)} cannot be empty.");
        
        Unique = unique;
        Name = name;
        TilesetUnique = tilesetUnique;
        BaseTile = baseTile;
        _tiles.Clear();
        _tiles.AddRange(tiles);
        _tags.Clear();
        _tags.AddRange(tags);
        _affectedTiles = new HashSet<Point>(_tiles.Select(t => t.PositionInTileset));
    }
}