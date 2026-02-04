using System;
using System.Collections.Generic;
using System.Numerics;
using CommunityToolkit.Mvvm.ComponentModel;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Maps;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers;
using RPGCreator.SDK.Assets.Definitions.Tilesets;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.RuntimeService;
using RPGCreator.SDK.Types.Collections;
using RPGCreator.SDK.Types.Records;

namespace RPGCreator.Player.Services;

public partial class MapService : ObservableObject, IMapService
{

    private readonly ScopedLogger _logger = Logger.ForContext<MapService>();
    
    private readonly HashSet<Type> _validAssetTypes = [typeof(ITileDef)];
    private readonly IAssetScope _assetScope;
    
    public event Action<Ulid>? OnMapLoaded;
    public event Action? OnMapUnloaded;
    public event Action<float, float>? OnMapEdited;
    
    private bool _hasLoadedMap;
    public bool HasLoadedMap
    {
        get => _hasLoadedMap;
        private set => SetProperty(ref _hasLoadedMap, value);
    }
    
    private bool _isMapDirty;
    public bool IsMapDirty
    {
        get => _isMapDirty; 
        private set => SetProperty(ref _isMapDirty, value);
    }

    public Ulid CurrentLoadedMapId => CurrentLoadedMapData.MapId;
    
    private MapData _currentLoadedMapData;
    public MapData CurrentLoadedMapData { 
        get => _currentLoadedMapData; 
        private set => SetProperty(ref _currentLoadedMapData, value);
    }
    
    private IMapDef? _currentLoadedMapDefinition;
    public IMapDef? CurrentLoadedMapDefinition
    {
        get => _currentLoadedMapDefinition;
        private set
        {
            OnPropertyChanging();
            OnPropertyChanging(nameof(CurrentLoadedMapId));
            _currentLoadedMapDefinition = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CurrentLoadedMapId));
        }
    }

    public MapService()
    {
        _assetScope = EngineServices.AssetsManager.CreateAssetScope("MapServiceScope");
        // When the map is edited, set the dirty flag
        OnMapEdited += (_, _) =>
        {
            SetDirtyFlag();
        };
        _logger.Info("MapService initialized at {time}.", args: DateTime.Now);
    }
    
    public bool LoadMap(Ulid mapId)
    {
        if (HasLoadedMap)
            return false;
        var mapDef = _assetScope.Load<IMapDef>(mapId);
        HasLoadedMap = true;
        CurrentLoadedMapDefinition = mapDef;
        CurrentLoadedMapData = CreateMapData(mapDef);
        ClearDirtyFlag();
        OnMapLoaded?.Invoke(mapId);
        return true;
    }
    
    public bool SaveMap()
    {
        if (!HasLoadedMap || CurrentLoadedMapDefinition == null)
            return false;
        
        // Here we would implement the logic to save the CurrentLoadedMapData back to the asset system.
        // This is a placeholder for demonstration purposes.
        
        // Getting the first assetpack (it should be the default project assetpack)
        var assetPacks = EngineServices.AssetsManager.GetLoadedPacks()[0];
        assetPacks.AddOrUpdateAsset(CurrentLoadedMapDefinition);
        
        ClearDirtyFlag();
        _logger.Info("Map '{mapName}' (ID: {mapId}) has been saved.", 
            args: [CurrentLoadedMapDefinition.Name, CurrentLoadedMapDefinition.Unique]);
        return true;
    }

    public void UnloadMap()
    {
        HasLoadedMap = false;
        ClearDirtyFlag();
        CurrentLoadedMapDefinition = null;
        CurrentLoadedMapData = default;
        OnMapUnloaded?.Invoke();
    }

    public void PlaceObjectAt(float x, float y, object objectToPlace)
    {
        ValidatePlacementChecks();
        ValidatePlacementAssetType(objectToPlace.GetType());
        
        OnMapEdited?.Invoke(x, y);
        throw new NotImplementedException();
    }

    public void PlaceAssetAt(float x, float y, Ulid assetId)
    {
        ValidatePlacementChecks();
        
        var asset = _assetScope.Load(assetId, out Type assetType);
        
        ValidatePlacementAssetType(assetType);
        
        if(asset == null)
            throw new ArgumentException($"Asset with ID {assetId} could not be loaded.");

        if (assetType == typeof(ITileDef) && asset is ITileDef tileDef)
        {
            PlaceTileAt((int)x, (int)y, tileDef);
            return;
        }
        
        OnMapEdited?.Invoke(x, y);
    }

    public void PlaceTileAt(int x, int y, ITileDef tileDef)
    {
        ValidatePlacementChecks();
        
        var layer = RuntimeServices.LayerService.GetSelectedLayer();
        
        // Should probably handle this more gracefully
        if (layer is not LayerWithElements<ITileDef> tileLayer)
            throw new InvalidOperationException("Selected layer is not a tile layer. Cannot place tile.");
        
        tileLayer.AddElement(tileDef, new Vector2(x, y));
        
        OnMapEdited?.Invoke(x, y);
        SetDirtyFlag();
        _logger.Debug("Placed tile '{tilePos}' at ({x}, {y}) on layer '{layerName}'.", 
            args: [tileDef.PositionInTileset, x, y, tileLayer.Name]);
    }

    public bool TryGetObjectAt(float x, float y, out object foundObject)
    {
        throw new NotImplementedException();
    }

    public bool TryGetTileAt(int x, int y, out TileData tileData)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// This should convert world coordinates to map grid coordinates.<br/>
    /// Like: (1,0) should return: (0,0)
    /// >>> (34, 0) should return: (32, 0) if cell width is 32
    /// </summary>
    /// <param name="worldPosition"></param>
    /// <returns></returns>
    public Vector2 WorldToMapCoordinates(Vector2 worldPosition)
    {
        if (CurrentLoadedMapDefinition == null)
            return Vector2.Zero;
        var cellWidth = CurrentLoadedMapDefinition.GridParameter.CellWidth;
        var cellHeight = CurrentLoadedMapDefinition.GridParameter.CellHeight;
        
        var mapPosition = new Vector2(
            (float)Math.Floor(worldPosition.X / cellWidth) * cellWidth,
            (float)Math.Floor(worldPosition.Y / cellHeight) * cellHeight
        );
        return mapPosition;
    }

    #region Helpers

    /// <summary>
    /// Creates a MapData instance from the given IMapDef.
    /// </summary>
    /// <param name="mapDef">The map definition to create the data from.</param>
    /// <returns>The created MapData instance.</returns>
    private MapData CreateMapData(IMapDef mapDef)
    {
        return new MapData(
            mapDef.Unique,
            mapDef.Name,
            mapDef.Description,
            mapDef.Size,
            mapDef.GridParameter.CellWidth,
            mapDef.GridParameter.CellHeight,
            mapDef.BackgroundColor
        );
    }

    /// <summary>
    /// Sets the dirty flag to indicate the map has been modified but not saved.
    /// </summary>
    private void SetDirtyFlag()
    {
        IsMapDirty = true;
    }
    
    /// <summary>
    /// Clears the dirty flag to indicate the map is not modified.
    /// </summary>
    private void ClearDirtyFlag()
    {
        IsMapDirty = false;
    }

    /// <summary>
    /// Checks if a map and layer are loaded before placing an asset.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if no map or layer is loaded.</exception>
    private void ValidatePlacementChecks()
    {
        if (!HasLoadedMap)
        {
            throw new InvalidOperationException("No map is currently loaded. Cannot place asset.");
        }

        if (!RuntimeServices.LayerService.HasSelectedLayer)
        {
            throw new InvalidOperationException("No layer is currently loaded. Cannot place asset.");
        }
    }
    
    /// <summary>
    /// Checks if the asset type is valid for placement on the map.
    /// </summary>
    /// <param name="assetType">The type of the asset to validate.</param>
    /// <exception cref="ArgumentException">Thrown if the asset type is not valid for placement.</exception>
    private void ValidatePlacementAssetType(Type assetType)
    {
        if(!_validAssetTypes.Contains(assetType))
            throw new ArgumentException($"Asset of type {assetType} is not valid for placement on the map.");
    }
    
    #endregion
}