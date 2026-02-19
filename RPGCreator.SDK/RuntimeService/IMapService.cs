using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using RPGCreator.SDK.Assets.Definitions.Maps;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers;
using RPGCreator.SDK.Assets.Definitions.Tilesets;
using RPGCreator.SDK.GlobalState;
using RPGCreator.SDK.Types;
using RPGCreator.SDK.Types.Records;
using Size = RPGCreator.SDK.Types.Size;

namespace RPGCreator.SDK.RuntimeService;

public record struct MapData(Ulid MapId, string MapName, string MapDescription, Size MapSize, float CellWidth, float CellHeight, Color BackgroundColor);

public interface IMapService : IService
{
    /// <summary>
    /// Event called when a map is loaded.<br/>
    /// The Ulid parameter is the ID of the loaded map.
    /// </summary>
    event Action<Ulid>? OnMapLoaded;
    
    /// <summary>
    /// Event called when a map is unloaded.
    /// </summary>
    event Action? OnMapUnloaded;
    
    /// <summary>
    /// Event called when the currently loaded map is edited.<br/>
    /// The float parameters are the X and Y coordinates of the edit.
    /// </summary>
    event Action<float, float>? OnMapEdited;
    
    IMapState MapState { get; }
    
    /// <summary>
    /// Is there any map loaded?
    /// </summary>
    bool HasLoadedMap { get; }
    /// <summary>
    /// Is the currently loaded map dirty (has unsaved changes)?
    /// </summary>
    bool IsMapDirty { get; }
    
    /// <summary>
    /// The currently loaded map ID.<br/>
    /// If no map is loaded, this will be <see cref="Ulid.Empty"/>.
    /// </summary>
    public Ulid CurrentLoadedMapId { get; }
    
    /// <summary>
    /// The currently loaded map data.<br/>
    /// If no map is loaded, this will be default.<br/>
    /// This is a lightweight property to get basic info about the loaded map without having to load the full definition in memory.
    /// </summary>
    MapData CurrentLoadedMapData { get; }
    
    /// <summary>
    /// The currently loaded map definition.<br/>
    /// If no map is loaded, this will be null.<br/>
    /// This is a convenience property to avoid having to resolve the map definition from the assets manager using the <see cref="CurrentLoadedMapId"/>.<br/>
    /// If you just need the name or other basic info, use <see cref="CurrentLoadedMapData"/> instead to avoid loading the full definition in memory.<br/>
    /// </summary>
    IMapDef? CurrentLoadedMapDefinition { get; }

    /// <summary>
    /// Load a map in the game / RTP.
    /// </summary>
    /// <returns>
    /// True: It worked, the map is now loaded.<br/>
    /// False: It didn't work, either the mapId doesn't exist, or there is already a map loaded see <see cref="HasLoadedMap"/> to check that.
    /// </returns>
    /// <exception cref="NotImplementedException">Thrown if the RTP or game does not support this operation.</exception>
    bool LoadMap(Ulid mapId);

    /// <summary>
    /// Save the current loaded map.
    /// </summary>
    /// <returns>
    /// True: It worked, the map is now saved.<br/>
    /// False: It didn't work, either there is no map loaded, see <see cref="HasLoadedMap"/> to check that, or there was an error during the save process.
    /// </returns>
    /// <exception cref="NotImplementedException">Thrown if the RTP or game does not support this operation.</exception>
    bool SaveMap();
    
    /// <summary>
    /// Unload the current loaded map.<br/>
    /// It can be used even if there is no map loaded, in this case, it will do nothing at all, but will not throw any exception.
    /// </summary>
    /// <exception cref="NotImplementedException">Thrown if the RTP or game does not support this operation.</exception>
    void UnloadMap();
    
    /// <summary>
    /// Converts a world position to map coordinates.<br/>
    /// World position is the position in the game world with the camera at (0,0).<br/>
    /// Map coordinates are the coordinates on the map grid.<br/>
    /// This conversion takes into account the map's cell size.
    /// </summary>
    /// <param name="worldPosition">The world position to convert.</param>
    /// <returns>The corresponding map coordinates.</returns>
    Vector2 WorldToMapCoordinates(Vector2 worldPosition);

    #region Layers

    /// <summary>
    /// Selects the layer at the given index, or last layer if the index is out of range.<br/>
    /// In the case where the index is out of range, it will also log a warning message.
    /// </summary>
    /// <param name="layerIndex">The layer index to select.</param>
    /// <exception cref="NotImplementedException">Thrown if the RTP or game does not support this operation.</exception>
    /// <exception cref="InvalidOperationException">Thrown if no layers can be loaded/selected (ex: No Map Loaded).</exception>
    void SelectLayer(int layerIndex);
    
    /// <summary>
    /// Is there a selected layer?
    /// </summary>
    bool HasSelectedLayer { get; }

    /// <summary>
    /// Can a layer be selected?<br/>
    /// If false, no layer selecting actions should be allowed (in the case where no layers exist, or <see cref="IMapService.HasLoadedMap"/> is false).
    /// </summary>
    bool CanSelectLayer { get; }
    
    /// <summary>
    /// The index of the currently selected layer.
    /// </summary>
    int CurrentLayerIndex { get; }
    /// <summary>
    /// The total number of layers available. (Starts at 1 => So if there is 0 layers, this will return 0, if there is 1 layer, this will return 1, etc...)
    /// </summary>
    int LayerCount { get; }
    
    /// <summary>
    /// Returns the index of the previous layer.<br/>
    /// If the current layer is the first one, it will return the index of the first layer.
    /// </summary>
    public int PreviousLayerIndex => Math.Max(CurrentLayerIndex - 1, GetFirstLayerIndex());
    
    /// <summary>
    /// Returns the index of the next layer.<br/>
    /// If the current layer is the last one, it will return the index of the last layer.
    /// </summary>
    public int NextLayerIndex => Math.Min(CurrentLayerIndex + 1, GetLastLayerIndex());
    
    /// <summary>
    /// Selects the next layer (higher index) if possible.<br/>
    /// If the current layer is the last one, it will do nothing.
    /// </summary>
    public void SelectNextLayer() => SelectLayer(NextLayerIndex);
    
    /// <summary>
    /// Selects the previous layer (lower index) if possible.<br/>
    /// If the current layer is the first one, it will do nothing.
    /// </summary>
    public void SelectPreviousLayer() => SelectLayer(PreviousLayerIndex);
    
    /// <summary>
    /// Returns the index of the first layer.
    /// </summary>
    /// <returns>The index of the first layer.</returns>
    public int GetFirstLayerIndex() => 0;
    
    /// <summary>
    /// Returns the index of the last layer.
    /// </summary>
    /// <returns>The index of the last layer.</returns>
    public int GetLastLayerIndex() => LayerCount - 1;
    
    /// <summary>
    /// Tries to add a new layer with the given definition.<br/>
    /// Returns true if the layer was added successfully, false otherwise.
    /// </summary>
    /// <param name="layerDef">The definition of the layer to add.</param>
    /// <returns>
    /// True if the layer was added successfully, false otherwise.
    /// </returns>
    bool TryAddLayer(BaseLayerDef layerDef);
    
    /// <summary>
    /// Tries to remove the layer at the given index.<br/>
    /// Returns true if the layer was removed successfully, false otherwise.
    /// </summary>
    /// <param name="layerIndex">The index of the layer to remove.</param>
    /// <returns>
    /// True if the layer was removed successfully, false otherwise.
    /// </returns>
    bool TryRemoveLayer(int layerIndex);

    /// <summary>
    /// Returns the definition of the currently selected layer.
    /// </summary>
    /// <returns>The definition of the currently selected layer.</returns>
    BaseLayerDef GetSelectedLayer();

    #endregion
    
}