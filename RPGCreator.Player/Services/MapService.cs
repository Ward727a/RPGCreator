using System;
using System.Numerics;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Maps;
using RPGCreator.SDK.Assets.Definitions.Maps.Chunks;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers;
using RPGCreator.SDK.Debug;
using RPGCreator.SDK.GlobalState;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.RuntimeService;
using RPGCreator.SDK.Types;
using RPGCreator.SDK.Types.Collections;

namespace RPGCreator.Player.Services;

public class MapService : IMapService
{
    private readonly ScopedLogger _logger = Logger.ForContext<MapService>();
    
    private readonly IAssetScope _assetScope;
    
    public event Action<Ulid>? OnMapLoaded;
    public event Action? OnMapUnloaded;
    public event Action<float, float>? OnMapEdited;

    public IMapState MapState => GlobalStates.MapState;

    public bool HasLoadedMap => MapState.HasCurrentMap;

    public bool IsMapDirty => MapState.IsMapDirty;

    public Ulid CurrentLoadedMapId => MapState.CurrentMapId;

    public MapData CurrentLoadedMapData => MapState.CurrentMapData ?? default;

    public IMapDef? CurrentLoadedMapDefinition => MapState.CurrentMapDef;

    public bool HasSelectedLayer => MapState.HasSelectedLayer;
    public bool CanSelectLayer => MapState.CanSelectLayer;
    public int CurrentLayerIndex => MapState.CurrentLayerIndex;
    public int LayerCount => MapState.LayerCount;

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
        MapState.HasCurrentMap = true;
        MapState.CurrentMapDef = mapDef;
        MapState.CurrentMapData = CreateMapData(mapDef);
        if (MapState.CurrentMapDef is MapDefinition mapDefinition)
        {
            mapDefinition.BakeCollisionChunk();
        }
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
        MapState.HasCurrentMap = false;
        ClearDirtyFlag();
        MapState.CurrentMapDef = null;
        MapState.CurrentMapData = null;
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

    private bool IsTileBlocked(Rect collisionRectangle)
    {
       if (CurrentLoadedMapDefinition == null) return true;

        var min = WorldToMapCoordinates(collisionRectangle.Left, collisionRectangle.Top);
        var max = WorldToMapCoordinates(collisionRectangle.Right, collisionRectangle.Bottom);
        
        long? lastChunkId = null;
        ReadOnlySpan<RuntimeCollisionChunkData?> currentChunkSpan = default;
        Vector2 currentChunkPosition = Vector2.Zero;

        // Need to move that inside the chunk class, and not there
        int chunkSize = LayerChunk.ChunkSize; 

        int chunkMask = chunkSize - 1; 

        int chunkShift = (int)Math.Log2(chunkSize);
        
        for (int y = (int)Math.Floor(min.Y); y <= (int)Math.Floor(max.Y); y++)
        {
            for (int x = (int)Math.Floor(min.X); x <= (int)Math.Floor(max.X); x++)
            {
                long chunkId = LayerChunk.GetChunkId(new Vector2(x >> chunkShift, y >> chunkShift));
                var chunkPosition = LayerChunk.GetChunkPosition(new Vector2(x, y));
                if (!lastChunkId.HasValue || chunkId != lastChunkId)
                {
                    if (CurrentLoadedMapDefinition.CollisionChunk.TryGetElement(chunkId, out var chunk))
                    {
                        currentChunkSpan = chunk.GetAllElementsSpan();
                        lastChunkId = chunkId;
                        currentChunkPosition = chunkPosition;
                    }
                    else
                    {
                        lastChunkId = chunkId;
                        currentChunkSpan = default;
                        currentChunkPosition = Vector2.Zero;
                    }
                }

                if (currentChunkSpan == default) continue;

                var localPosition = new Vector2(x / LayerChunk.ChunkSize, y / LayerChunk.ChunkSize);
                int localIndex = (int)localPosition.X & chunkMask | ((int)localPosition.Y & chunkMask) << chunkShift;
                var tileData = currentChunkSpan[localIndex];
                Vector2 worldPos = LayerChunk.GetWorldPosition(chunkId, localIndex);

                if (tileData.HasValue)
                {

                    foreach (var localRect in tileData.Value.Collisions)
                    {
                        var worldCollisionRect = new Rect(
                            worldPos.X + localRect.X,
                            worldPos.Y + localRect.Y,
                            localRect.Width,
                            localRect.Height
                        );

                        if (worldCollisionRect.Intersects(collisionRectangle))
                        {
                            DebugMemory.Set("map.collision_rectangle", collisionRectangle);
                            DebugMemory.Set("map.world_collision_rectangle", worldCollisionRect);
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    public void SelectLayer(int layerIndex)
    {
        MapState.CurrentLayerIndex = layerIndex;
    }
    
    public bool TryAddLayer(BaseLayerDef layerDef)
    {
        if (!HasLoadedMap || CurrentLoadedMapDefinition == null)
            return false;
        return CurrentLoadedMapDefinition.AddLayer(layerDef);
    }

    public bool TryRemoveLayer(int layerIndex)
    {
        if (!HasLoadedMap || CurrentLoadedMapDefinition == null)
            return false;
        if (layerIndex < 0 || layerIndex >= CurrentLoadedMapDefinition.TileLayers.Count)
            return false;
        var layerDef = CurrentLoadedMapDefinition.TileLayers[layerIndex];
        return CurrentLoadedMapDefinition.RemoveLayer(layerDef);
    }

    public BaseLayerDef GetSelectedLayer()
    {
        if (!HasLoadedMap || CurrentLoadedMapDefinition == null)
            throw new InvalidOperationException("No map loaded.");
        if (CurrentLayerIndex < 0 || CurrentLayerIndex >= CurrentLoadedMapDefinition.TileLayers.Count)
            throw new InvalidOperationException("Selected layer index is out of bounds.");
        return CurrentLoadedMapDefinition.TileLayers[CurrentLayerIndex];
    }

    public BaseLayerDef GetLayerAt(int layerIndex)
    {
        if (!HasLoadedMap || CurrentLoadedMapDefinition == null)
            throw new InvalidOperationException("No map loaded.");
        if (layerIndex < 0 || layerIndex >= CurrentLoadedMapDefinition.TileLayers.Count)
            throw new InvalidOperationException("Layer index is out of bounds.");
        return CurrentLoadedMapDefinition.TileLayers[layerIndex];
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
        MapState.IsMapDirty = true;
    }
    
    /// <summary>
    /// Clears the dirty flag to indicate the map is not modified.
    /// </summary>
    private void ClearDirtyFlag()
    {
        MapState.IsMapDirty = false;
    }
    
    #endregion
}