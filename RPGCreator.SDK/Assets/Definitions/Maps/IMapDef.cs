using System.Drawing;
using RPGCreator.SDK.Serializer;
using RPGCreator.SDK.Types.Interfaces;
using RPGCreator.SDK.Types.Internals;
using Size = RPGCreator.SDK.Types.Size;

namespace RPGCreator.SDK.Assets.Definitions.Maps;

public interface IMapDef : IHasUniqueId, ISerializable, IDeserializable, IAssetDef
{
    event Action<BaseLayerDef> TileLayerAdded;
    event Action<BaseLayerDef> TileLayerRemoved;
    /// <summary>
    /// Name of the map.
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// Description of the map.
    /// </summary>
    public string Description { get; set; }
    /// <summary>
    /// Other maps that are part of this map definition, such as levels or sub-maps.
    /// </summary>
    public IReadOnlyList<IMapDef> MapDefs { get; }
    /// <summary>
    /// List of tile layers in the map, which can include background, foreground, and other layers.
    /// </summary>
    public IReadOnlyList<BaseLayerDef> TileLayers { get; }
    /// <summary>
    /// Size of the map in tiles, represented as a width and height.
    /// </summary>
    public Size Size { get; set; }
    /// <summary>
    /// Parameters for the grid layout of the map, including cell size and border color.
    /// </summary>
    public SGridParameter GridParameter { get; set; }
    /// <summary>
    /// Background color of the map, which can be used to set a default background or for visual effects.
    /// </summary>
    public Color BackgroundColor { get; set; }
    
    /// <summary>
    /// Adds a new tile layer to the map definition.
    /// </summary>
    /// <param name="layer">The layer to add.</param>
    /// <returns>True if the layer was added successfully; false if it already exists.</returns>
    bool AddLayer(BaseLayerDef layer);
    
    /// <summary>
    /// Removes a tile layer from the map definition.
    /// </summary>
    /// <param name="layer">The layer to remove.</param>
    /// <returns>True if the layer was removed successfully; false if it was not found.</returns>
    bool RemoveLayer(BaseLayerDef layer);
}