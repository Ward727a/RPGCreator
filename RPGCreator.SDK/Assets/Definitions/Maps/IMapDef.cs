using System.Collections.ObjectModel;
using RPGCreator.SDK.Assets.Definitions.Maps.Chunks;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers;
using RPGCreator.SDK.Types;
using RPGCreator.Shared.Types;

namespace RPGCreator.SDK.Assets.Definitions.Maps;

public interface IMapDef : IBaseAssetDef
{
    
    public CollisionLayer CollisionChunk { get; set; }
    
    /// <summary>
    /// Description of the map.
    /// </summary>
    public string Description { get; set; }
    /// <summary>
    /// Other maps that are part of this map definition, such as levels or sub-maps.
    /// </summary>
    public List<IMapDef> ChildMaps { get; }
    /// <summary>
    /// List of tile layers in the map, which can include background, foreground, and other layers.
    /// </summary>
    public ObservableCollection<BaseLayerDef> Layers { get; }
    /// <summary>
    /// Size of the map in tiles, represented as a width and height.
    /// </summary>
    public Size Size { get; set; }
    /// <summary>
    /// Parameters for the grid layout of the map, including cell size and border color.
    /// </summary>
    public GridParameter GridParameter { get; set; }
    /// <summary>
    /// Background color of the map, which can be used to set a default background or for visual effects.
    /// </summary>
    public Color BackgroundColor { get; set; }
}

public static class IMapDefExtension
{
    extension(IMapDef map)
    {
        public bool AddLayer(BaseLayerDef layer)
        {
            if(map.Layers.Contains(layer))
                return false;
            map.Layers.Add(layer);
            return true;
        }

        public bool RemoveLayer(BaseLayerDef layer)
        {
            return map.Layers.Remove(layer);
        }
    }
}