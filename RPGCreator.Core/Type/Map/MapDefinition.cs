using Microsoft.Xna.Framework;
using RPGCreator.Core.Type.Internal;

namespace RPGCreator.Core.Type.Map;

public class MapDefinition : IMapDef
{
    private readonly List<IMapDef> _mapDefs = new List<IMapDef>();
    private readonly List<TileLayerDefinition> _tileLayers = new List<TileLayerDefinition>();
    
    public event EventHandler<TileLayerDefinition>? TileLayerAdded;
    public event EventHandler<TileLayerDefinition>? TileLayerRemoved;
    
    public Ulid Unique { get; private set; }
    public URN Urn => new URN("maps", $"{Name}@{Unique}");
    public string Name { get; set; }
    public string Description { get; set; }
    public IReadOnlyList<IMapDef> MapDefs => _mapDefs;
    public IReadOnlyList<TileLayerDefinition> TileLayers => _tileLayers;
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
    
    public bool AddLayer(TileLayerDefinition layer)
    {
        if (layer == null || _tileLayers.Contains(layer))
            return false; // If the layer is null or already exists, we can't add it

        _tileLayers.Add(layer);
        TileLayerAdded?.Invoke(this, layer); // Notify subscribers that a new layer has been added
        return true;
    }
    public bool RemoveLayer(TileLayerDefinition layer)
    {
        if (layer == null || !_tileLayers.Contains(layer))
            return false; // If the layer is null or doesn't exist, we can't remove it

        _tileLayers.Remove(layer);
        TileLayerRemoved?.Invoke(this, layer); // Notify subscribers that a layer has been removed
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
    
    // public void AddLayer(TileLayer layer)

    public void SetObjectData(DeserializationInfo info)
    {
        ArgumentNullException.ThrowIfNull(info);

        info.TryGetValue(nameof(Unique), out var unique, Ulid.Empty, $"{nameof(MapDefinition)}.{nameof(Unique)} not found, using default value Ulid.Empty.");
        Unique = unique;
        info.TryGetValue(nameof(Name), out var name, string.Empty, $"{nameof(MapDefinition)}.{nameof(Name)} not found, using default value empty string.");
        Name = name;
        info.TryGetValue(nameof(Description), out var description, string.Empty, $"{nameof(MapDefinition)}.{nameof(Description)} not found, using default value empty string.");
        Description = description;
        info.TryGetValue(nameof(MapDefs), out var mapDefs, new List<IMapDef>(), $"{nameof(MapDefinition)}.{nameof(MapDefs)} not found, using default value empty list.");
        _mapDefs.Clear();
        _mapDefs.AddRange(mapDefs);
        info.TryGetValue(nameof(TileLayers), out var tileLayers, new List<TileLayerDefinition>(), $"{nameof(MapDefinition)}.{nameof(TileLayers)} not found, using default value empty list.");
        _tileLayers.Clear();
        _tileLayers.AddRange(tileLayers);
        info.TryGetValue(nameof(Size), out var size, new Size(10, 20), $"{nameof(MapDefinition)}.{nameof(Size)} not found, using default value Size(10, 20).");
        Size = size;
        info.TryGetValue(nameof(GridParameter), out var gridParameter, new SGridParameter(), $"{nameof(MapDefinition)}.{nameof(GridParameter)} not found, using default value SGridParameter().");
        GridParameter = gridParameter;
        info.TryGetValue(nameof(BackgroundColor), out var backgroundColor, Color.DeepSkyBlue, $"{nameof(MapDefinition)}.{nameof(BackgroundColor)} not found, using default value Color.DeepSkyBlue.");
        BackgroundColor = backgroundColor;
    }

    public bool IsDirty { get; set; }
    public bool IsTransient { get; set; } = false;
}