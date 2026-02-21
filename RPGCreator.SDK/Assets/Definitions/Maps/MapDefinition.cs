using Newtonsoft.Json;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers;
using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.Serializer;
using RPGCreator.SDK.Types;

namespace RPGCreator.SDK.Assets.Definitions.Maps;

[SerializingType("Map")]
public class MapDefinition : BaseAssetDef, IMapDef
{
    private readonly List<IMapDef> _mapDefs = new List<IMapDef>();
    [JsonProperty(ItemTypeNameHandling = TypeNameHandling.All)]
    private readonly List<BaseLayerDef> _tileLayers = new List<BaseLayerDef>();

    public Ulid PackId { get; set; }
    public event Action<BaseLayerDef>? TileLayerAdded;
    public event Action<BaseLayerDef>? TileLayerRemoved;
    
    public override UrnSingleModule UrnModule => "maps".ToUrnSingleModule();
    public string Description { get; set; }
    public IReadOnlyList<IMapDef> MapDefs => _mapDefs;
    
    [JsonProperty(ItemTypeNameHandling = TypeNameHandling.All)]
    public IReadOnlyList<BaseLayerDef> TileLayers => _tileLayers;
    public Size Size { get; set; } = new Size(10, 20); // Default size, can be changed later

    public SGridParameter GridParameter { get; set; } = new()
    {
        CellWidth = 32,
        CellHeight = 32,
        CellBorderColor = Color.Black
    }; // Default grid parameters, can be changed later
    public Color BackgroundColor { get; set; }
    
    public MapDefinition()
    {
        SuspendTracking();
        Unique = Ulid.NewUlid();
        Name = "New Map";
        Description = "";
    }

    public MapDefinition(string mapName, string mapDescription = "")
    {
        Unique = Ulid.NewUlid();
        Name = mapName;
        Description = mapDescription;
    }

    public bool AddMap(IMapDef mapDef)
    {
        if (mapDef == null || _mapDefs.Contains(mapDef))
            return false; // If the map definition is null or already exists, we can't add it

        _mapDefs.Add(mapDef);
        return true;
    }
    public bool RemoveMap(IMapDef mapDef)
    {
        if (mapDef == null || !_mapDefs.Contains(mapDef))
            return false; // If the map definition is null or doesn't exist, we can't remove it

        _mapDefs.Remove(mapDef);
        return true;
    }
    
    public bool AddLayer(BaseLayerDef layer)
    {
        if (layer == null || _tileLayers.Exists(l => l.Unique == layer.Unique))
            return false; // If the layer is null or already exists, we can't add it

        _tileLayers.Add(layer);
        TileLayerAdded?.Invoke(layer); // Notify subscribers that a new layer has been added
        return true;
    }
    
    public bool RemoveLayer(BaseLayerDef layer)
    {
        if (layer == null || !_tileLayers.Exists(l => l.Unique == layer.Unique))
            return false; // If the layer is null or doesn't exist, we can't remove it

        _tileLayers.Remove(layer);
        TileLayerRemoved?.Invoke(layer); // Notify subscribers that a layer has been removed
        return true;
    }
    
    public SerializationInfo GetObjectData()
    {
        return new SerializationInfo(typeof(MapDefinition))
            .AddValue(nameof(Unique), Unique)
            .AddValue(nameof(Name), Name)
            .AddValue(nameof(Description), Description)
            .AddValue(nameof(MapDefs), _mapDefs)
            .AddValue(nameof(TileLayers), _tileLayers)
            .AddValue(nameof(Size), Size)
            .AddValue(nameof(GridParameter), GridParameter)
            .AddValue(nameof(BackgroundColor), BackgroundColor);
    }

    public List<Ulid> GetReferencedAssetIds()
    {
        var referencedIds = new List<Ulid>();
        foreach (var mapDef in _mapDefs)
        {
            referencedIds.AddRange(mapDef.GetReferencedAssetIds());
        }
        foreach (var layer in _tileLayers)
        {
            referencedIds.AddRange(layer.GetReferencedAssetIds());
        }
        return referencedIds.Distinct().ToList();
    }

    // public void AddLayer(TileLayer layer)

    public void SetObjectData(DeserializationInfo info)
    {
        ArgumentNullException.ThrowIfNull(info);

        info.TryGetValue(nameof(Unique), out var unique, Ulid.Empty);
        Unique = unique;
        info.TryGetValue(nameof(Name), out var name, string.Empty);
        Name = name;
        info.TryGetValue(nameof(Description), out var description, string.Empty);
        Description = description;
        info.TryGetValue(nameof(MapDefs), out var mapDefs, new List<IMapDef>());
        _mapDefs.Clear();
        _mapDefs.AddRange(mapDefs);
        info.TryGetValue(nameof(TileLayers), out var tileLayers, new List<BaseLayerDef>());
        _tileLayers.Clear();
        _tileLayers.AddRange(tileLayers);
        info.TryGetValue(nameof(Size), out var size, new Size(10, 20));
        Size = size;
        info.TryGetValue(nameof(GridParameter), out var gridParameter, new SGridParameter());
        GridParameter = gridParameter;
        info.TryGetValue(nameof(BackgroundColor), out var backgroundColor, Color.DeepSkyBlue);
        BackgroundColor = backgroundColor;
    }

    public string SavePath { get; set; }
}