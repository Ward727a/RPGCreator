using Microsoft.Xna.Framework;
using RPGCreator.Core.Rendering.Batching;
using RPGCreator.Core.Types.Assets;
using RPGCreator.Core.Types.Internal;

namespace RPGCreator.Core.Types.Map;

public interface IMapDef : IHasUniqueId, ISerializable, IDeserializable, IAssetDef
{
    event EventHandler<BaseLayerDef> TileLayerAdded;
    event EventHandler<BaseLayerDef> TileLayerRemoved;
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
}