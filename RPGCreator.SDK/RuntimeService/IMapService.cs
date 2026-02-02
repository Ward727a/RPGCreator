using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Numerics;
using RPGCreator.SDK.Assets.Definitions.Maps;
using RPGCreator.SDK.Assets.Definitions.Tilesets;
using RPGCreator.SDK.Types.Records;
using Size = RPGCreator.SDK.Types.Size;

namespace RPGCreator.SDK.RuntimeService;

public record struct MapData(Ulid MapId, string MapName, string MapDescription, Size MapSize, float CellWidth, float CellHeight, Color BackgroundColor);

public interface IMapService : INotifyPropertyChanged, INotifyPropertyChanging, IService
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
    /// A general purpose method to place an object at the given coordinates on the map.<br/>
    /// It should be used with caution as it may not work for all object types.<br/>
    /// The implementation is up to the RTP or game to decide how to handle the object placement.
    /// </summary>
    /// <param name="x">The X coordinate on the map.</param>
    /// <param name="y">The Y coordinate on the map.</param>
    /// <param name="objectToPlace">The object to place.</param>
    /// <exception cref="NotImplementedException">Thrown if the RTP or game does not support this operation.</exception>
    /// <exception cref="ArgumentException">Thrown if the object type is not supported for placement.</exception>
    /// <exception cref="InvalidOperationException">Thrown if there is no map loaded or no layer selected.</exception>
    void PlaceObjectAt(float x, float y, object objectToPlace);
    
    /// <summary>
    /// A general purpose method to place an asset at the given coordinates on the map.<br/>
    /// It should be used with caution as it may not work for all asset types. It is better than the <see cref="PlaceObjectAt"/>.<br/>
    /// The implementation is up to the RTP or game to decide how to handle the asset placement.
    /// </summary>
    /// <param name="x">The X coordinate on the map.</param>
    /// <param name="y">The Y coordinate on the map.</param>
    /// <param name="assetId">The asset ID to place.</param>
    /// <exception cref="NotImplementedException">Thrown if the RTP or game does not support this operation.</exception>
    /// <exception cref="ArgumentException">Thrown if the asset type is not supported for placement.</exception>
    /// <exception cref="ArgumentException">Thrown if the given assetId could not be loaded.</exception>
    /// <exception cref="InvalidOperationException">Thrown if there is no map loaded or no layer selected.</exception>
    void PlaceAssetAt(float x, float y, Ulid assetId);
    
    /// <summary>
    /// This method will place a tile at the given coordinates on the map layer.
    /// </summary>
    /// <param name="x">The X coordinate on the map.</param>
    /// <param name="y">The Y coordinate on the map.</param>
    /// <param name="tileDef">The tile def to place.</param>
    /// <exception cref="NotImplementedException">Thrown if the RTP or game does not support this operation.</exception>
    /// <exception cref="InvalidOperationException">Thrown if there is no map loaded or no layer selected.</exception>
    void PlaceTileAt(int x, int y, ITileDef tileDef);
    
    /// <summary>
    /// A general purpose method to get an object at the given coordinates on the map.<br/>
    /// It should be used with caution as it may not work for all object types.<br/>
    /// The implementation is up to the RTP or game to decide how to handle the object retrieval.
    /// </summary>
    /// <param name="x">The X coordinate on the map.</param>
    /// <param name="y">The Y coordinate on the map.</param>
    /// <param name="foundObject">The found object at the given coordinates, or null if none found.</param>
    /// <returns>
    /// True: An object was found at the given coordinates.<br/>
    /// False: No object was found at the given coordinates.
    /// </returns>
    /// <exception cref="NotImplementedException">Thrown if the RTP or game does not support this operation.</exception>
    /// <exception cref="InvalidOperationException">Thrown if there is no map loaded or no layer selected.</exception>
    bool TryGetObjectAt(float x, float y, [NotNullWhen(true)] out object? foundObject);
    
    /// <summary>
    /// Try to get the tile data at the given coordinates on the map.
    /// </summary>
    /// <param name="x">The X coordinate on the map.</param>
    /// <param name="y">The Y coordinate on the map.</param>
    /// <param name="tileData">The tile data at the given coordinates, or default if none found.</param>
    /// <returns>
    /// True: A tile was found at the given coordinates.<br/>
    /// False: No tile was found at the given coordinates.
    /// </returns>
    /// <exception cref="NotImplementedException">Thrown if the RTP or game does not support this operation.</exception>
    /// <exception cref="InvalidOperationException">Thrown if there is no map loaded or no layer selected.</exception>
    bool TryGetTileAt(int x, int y, out TileData tileData);
    
    /// <summary>
    /// Converts a world position to map coordinates.<br/>
    /// World position is the position in the game world with the camera at (0,0).<br/>
    /// Map coordinates are the coordinates on the map grid.<br/>
    /// This conversion takes into account the map's cell size.
    /// </summary>
    /// <param name="worldPosition">The world position to convert.</param>
    /// <returns>The corresponding map coordinates.</returns>
    Vector2 WorldToMapCoordinates(Vector2 worldPosition);
    
}