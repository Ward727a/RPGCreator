using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using RPGCreator.SDK.Assets.Definitions.Maps.Chunks;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers;
using RPGCreator.SDK.Assets.Runtime;
using RPGCreator.SDK.Assets.Runtime.Maps;
using RPGCreator.SDK.Common.Attributes;
using RPGCreator.SDK.Types;
using RPGCreator.Shared.Types;

namespace RPGCreator.SDK.Assets.Definitions.Maps;

[EngineClass("rpgc", "assets", "definitions", "maps", "map", DisplayName = "Map Definition")]
public partial class MapDefinition : BaseAssetDef, IMapDef, IDefinition<RuntimeMap>
{
    public string Description { get; set; }
    
    [JsonPropertyName("child_maps")]
    public List<IMapDef> ChildMaps { get; set; } = new();
    
    [JsonPropertyName("layers")]
    public ObservableCollection<BaseLayerDef> Layers { get; set; } = [];
    
    [JsonPropertyName("collision_chunk")]
    public CollisionLayer CollisionChunk { get; set; } = new();
    
    public Size Size { get; set; } = new(10, 20); // Default size, can be changed later
    
    [JsonPropertyName("grid_parameter")]
    public GridParameter GridParameter { get; set; } = new()
    {
        CellWidth = 32,
        CellHeight = 32,
        CellBorderColor = Color.Black
    }; // Default grid parameters, can be changed later
    
    [JsonPropertyName("background_color")]
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

}