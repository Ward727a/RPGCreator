using System;
using System.Numerics;
using Fluxor;
using MediatR;
using RPGCreator.EditorLib.Mediator.Services;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Maps;
using RPGCreator.SDK.Assets.Definitions.Maps.Chunks;
using RPGCreator.SDK.Assets.MapLayers;
using RPGCreator.SDK.Assets.Maps;
using RPGCreator.SDK.Assets.Maps.Chunks;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Services.EngineService;
using RPGCreator.SDK.Services.RuntimeService;
using RPGCreator.SDK.States;
using RPGCreator.SDK.States.Actions;
using RPGCreator.Shared.Types;

namespace RPGCreator.RTP.Services;

public class MapService : IMapService
{
    private readonly Dispatcher _dispatcher;
    private readonly MapMdrService _mapMdrService;
    private readonly IAssetsManager _assetsManager;
    private readonly ScopedLogger _logger = Logger.ForContext<MapService>();
    
    public event Action<Ulid>? MapLoaded;
    public event Action? OnMapUnloaded;
    public event Action<float, float>? OnMapEdited;

    public IState<MapState> MapState { get; }

    public bool HasLoadedMap => MapState.Value.IsMapOpen;

    public bool IsMapDirty => MapState.Value.IsMapDirty;

    public Ulid CurrentLoadedMapId => MapState.Value.MapId;
    
    public MapDefinition? CurrentLoadedMapDefinition => MapState.Value.MapDef;

    public bool HasSelectedLayer => MapState.Value.HasSelectedLayer;
    public int CurrentLayerIndex => MapState.Value.SelectedLayerIndex;
    public int LayerCount => MapState.Value.MapDef?.Layers.Count ?? 0;

    public MapService(IState<MapState> mapState, Dispatcher dispatcher, MapMdrService mapMdrService, IAssetsManager assetsManager)
    {
        _dispatcher = dispatcher;
        _mapMdrService = mapMdrService;
        _assetsManager = assetsManager;
        MapState = mapState;
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
        var mapDef = _assetsManager.Load<MapDefinition>(mapId).OnSuccess((map) =>
        {
            _dispatcher.Dispatch(new MapActions.OpenMapAction(map));
            ClearDirtyFlag();
            MapLoaded?.Invoke(mapId); 
        });
        return mapDef.IsSuccess;
    }
    
    public bool SaveMap()
    {
        if (!HasLoadedMap || CurrentLoadedMapDefinition == null)
            return false;
        
        _assetsManager.Save(CurrentLoadedMapDefinition);
        
        ClearDirtyFlag();
        _logger.Info("Map '{mapName}' (ID: {mapId}) has been saved.", 
            args: [CurrentLoadedMapDefinition.Name, CurrentLoadedMapDefinition.Unique]);
        return true;
    }

    public void UnloadMap()
    {
        _dispatcher.Dispatch(new MapActions.CloseMapAction());
        ClearDirtyFlag();
        OnMapUnloaded?.Invoke();
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
    
    public Vector2 WorldToMapCoordinates(float worldX, float worldY)
    {
        if (CurrentLoadedMapDefinition == null)
            return Vector2.Zero;
        var cellWidth = CurrentLoadedMapDefinition.GridParameter.CellWidth;
        var cellHeight = CurrentLoadedMapDefinition.GridParameter.CellHeight;
        
        var mapPosition = new Vector2(
            (float)Math.Floor(worldX / cellWidth) * cellWidth,
            (float)Math.Floor(worldY / cellHeight) * cellHeight
        );
        return mapPosition;
    }

    public bool IsAreaBlocked(Rect collisionRectangle, int excludeEntityId = -1)
    {
        if (CurrentLoadedMapDefinition == null) return true; // If no map definition, we block it by default.
        return IsTileBlocked(collisionRectangle);
    }

    public event Action<IMapLayerDefinition>? AddedLayer;
    public event Action<IMapLayerDefinition>? RemovedLayer;
    public event Action<IMapLayerDefinition>? SelectedLayer;

    private bool IsTileBlocked(Rect collisionRectangle)
    {
       if (CurrentLoadedMapDefinition == null) return true;

        var min = WorldToMapCoordinates(collisionRectangle.Left, collisionRectangle.Left);
        var max = WorldToMapCoordinates(collisionRectangle.Right, collisionRectangle.Bottom);
        
        long lastChunkId = -1;
        ReadOnlySpan<RuntimeCollisionChunkData?> currentChunkSpan = default;

        for (int y = (int)Math.Floor(min.Y); y <= (int)Math.Floor(max.Y); y++)
        {
            for (int x = (int)Math.Floor(min.X); x <= (int)Math.Floor(max.X); x++)
            {
                long chunkId = LayerChunk.GetChunkId(x, y);

                if (chunkId != lastChunkId)
                {
                    if (CurrentLoadedMapDefinition.CollisionChunk.TryGetElement(chunkId, out var chunk))
                    {
                        currentChunkSpan = chunk.GetAllElementsSpan();
                        lastChunkId = chunkId;
                    }
                    else
                    {
                        lastChunkId = chunkId;
                        currentChunkSpan = default;
                    }
                }

                if (currentChunkSpan == default) continue;

                int localIndex = (y & 31) << 5 | (x & 31);
                var tileData = currentChunkSpan[localIndex];

                if (tileData.HasValue)
                {
                    float tileWorldX = x * 32;
                    float tileWorldY = y * 32;

                    foreach (var localRect in tileData.Value.Collisions)
                    {
                        var worldCollisionRect = new Rect(
                            tileWorldX + localRect.X,
                            tileWorldY + localRect.Y,
                            localRect.Width,
                            localRect.Height
                        );

                        if (worldCollisionRect.Intersects(collisionRectangle))
                            return true;
                    }
                }
            }
        }
        return false;
    }

    public void SelectLayer(int layerIndex)
    {
        _dispatcher.Dispatch(new MapActions.SelectLayerAction(layerIndex));
        SelectedLayer?.Invoke(GetLayerAt(layerIndex));
    }
    public bool TryAddLayer(IMapLayerDefinition layerDef)
    {
        if (!HasLoadedMap || CurrentLoadedMapDefinition == null)
            return false;
        _mapMdrService.AddLayerAsync(CurrentLoadedMapDefinition, layerDef).Wait();
        AddedLayer?.Invoke(layerDef);
        return true;
    }

    public bool TryRemoveLayer(int layerIndex)
    {
        if (!HasLoadedMap || CurrentLoadedMapDefinition == null)
            return false;
        if (layerIndex < 0 || layerIndex >= CurrentLoadedMapDefinition.Layers.Count)
            return false;
        var layerDef = CurrentLoadedMapDefinition.Layers[layerIndex];
        _mapMdrService.RemoveLayerAsync(CurrentLoadedMapDefinition, layerDef).Wait();
        RemovedLayer?.Invoke(layerDef);
        return true;
    }

    public IMapLayerDefinition GetSelectedLayer()
    {
        if (!HasLoadedMap || CurrentLoadedMapDefinition == null)
            throw new InvalidOperationException("No map loaded.");
        if (CurrentLayerIndex < 0 || CurrentLayerIndex >= CurrentLoadedMapDefinition.Layers.Count)
            throw new InvalidOperationException("Selected layer index is out of bounds.");
        return CurrentLoadedMapDefinition.Layers[CurrentLayerIndex];
    }

    public IMapLayerDefinition GetLayerAt(int layerIndex)
    {
        if (!HasLoadedMap || CurrentLoadedMapDefinition == null)
            throw new InvalidOperationException("No map loaded.");
        if (layerIndex < 0 || layerIndex >= CurrentLoadedMapDefinition.Layers.Count)
            throw new InvalidOperationException("Layer index is out of bounds.");
        return CurrentLoadedMapDefinition.Layers[layerIndex];
    }

    #region Helpers

    /// <summary>
    /// Sets the dirty flag to indicate the map has been modified but not saved.
    /// </summary>
    private void SetDirtyFlag()
    {
        _dispatcher.Dispatch(new MapActions.SetMapDirtyAction());
    }
    
    /// <summary>
    /// Clears the dirty flag to indicate the map is not modified.
    /// </summary>
    private void ClearDirtyFlag()
    {
        _dispatcher.Dispatch(new MapActions.ClearMapDirtyAction());
    }
    
    #endregion
}