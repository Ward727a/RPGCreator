// RPG Creator - Open-source RPG Engine.
// (c) 2026 Ward
// 
// This file is part of RPG Creator and is distributed under the Apache 2.0 License.
// You are free to use, modify, and distribute this file under the terms of the Apache 2.0 License.
// See LICENSE for details.
// 
// ---
// 
// Ce fichier fait partie de RPG Creator et est distribué sous licence Apache 2.0.
// Vous êtes libre de l'utiliser, de le modifier et de le distribuer sous les termes de la licence Apache 2.0.
// Voir LICENSE pour plus de détails.
// 
// Contact:
// => Mail: Ward727a@gmail.com
//    Please use this object: "RPG Creator [YourObject]"
// => Discord: ward727
// 
// For urgent inquiries, sending both an email and a message on Discord is highly recommended for a quicker response.

using System;
using System.Collections.Generic;
using System.Numerics;
using CommunityToolkit.Mvvm.ComponentModel;
using RPGCreator.Core.Types.Map.Chunks;
using RPGCreator.SDK;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.RuntimeService;
using RPGCreator.SDK.Types;
using Size = RPGCreator.SDK.Types.Size;

namespace RPGCreator.RTP.Services;

public partial class ChunkService : ObservableObject, IChunkService
{
    private static readonly ScopedLogger Logger = SDK.Logging.Logger.ForContext<ChunkService>();
    
    public event Action<long>? OnChunkLoaded;
    public event Action<long>? OnChunkUnloaded;

    private int ChunkSize => LayerChunk.ChunkSize;
    private Size _chunkTileSize;

    private Size ChunkSizeInPixels { get; set; }
    
    private readonly HashSet<long> _activeChunkIds = [];
    public IReadOnlyCollection<long> ActiveChunks => _activeChunkIds;

    private bool _hasMapLoaded;
    private (long minX, long maxX, long minY, long maxY) _lastLoadedRange;
    
    /// <summary>
    /// True if chunk loading is internally frozen (e.g., during map transitions).<br/>
    /// This is mainly used to allow the user to freeze loading, change maps, and keep it frozen until they unfreeze it again.
    /// </summary>
    private bool _internalIsLoadFrozen;
    public bool IsLoadFrozen { get; private set; }
    
    private bool CanUpdateLoadedChunks => !_internalIsLoadFrozen && !IsLoadFrozen && RuntimeServices.CameraService.CameraEntityId != null && _hasMapLoaded;
    
    public ChunkService()
    {
        RuntimeServices.MapService.OnMapLoaded += OnMapLoaded;
        RuntimeServices.MapService.OnMapUnloaded += OnMapUnloaded;
    }
    
    public void UpdateLoadedChunk()
    {
        if(!CanUpdateLoadedChunks)
        {
            return;
        }

        var loadedChunks = GetVisibleChunkBounds(IChunkService.ChunkLoadDistance);
        var unloadRange = GetVisibleChunkBounds(IChunkService.ChunkUnloadDistance);
        
        if (loadedChunks.minX == _lastLoadedRange.minX && 
            loadedChunks.maxX == _lastLoadedRange.maxX &&
            loadedChunks.minY == _lastLoadedRange.minY && 
            loadedChunks.maxY == _lastLoadedRange.maxY)
        {
            return;
        }
        _lastLoadedRange = loadedChunks;
        
        var changed = false;
        
        // Load new chunks
        for (var x = loadedChunks.minX; x <= loadedChunks.maxX; x++)
        {
            for (var y = loadedChunks.minY; y <= loadedChunks.maxY; y++)
            {
                var chunkId = LayerChunk.GetChunkId(x, y);

                if (_activeChunkIds.Add(chunkId))
                {
                    OnChunkLoaded?.Invoke(chunkId);
                    changed = true;
                }
            }
        }
        
        // Unload distant chunks
        _activeChunkIds.RemoveWhere(id =>
        {
            var chunkLocalPosition = LayerChunk.GetChunkCoordinate(id);

            var isOutsideUnloadRange = chunkLocalPosition.X < unloadRange.minX || chunkLocalPosition.X > unloadRange.maxX ||
                                       chunkLocalPosition.Y < unloadRange.minY || chunkLocalPosition.Y > unloadRange.maxY;
            if (isOutsideUnloadRange)
            {
                OnChunkUnloaded?.Invoke(id);
                changed = true;
            }

            return isOutsideUnloadRange;
        });

        if (!changed) return;
        
        Logger.Debug("Active chunks updated. Total loaded chunks: {Count}", args: _activeChunkIds.Count);
        OnPropertyChanged(nameof(ActiveChunks));

    }

    public void ClearLoadedChunks()
    {
        foreach (var id in _activeChunkIds)
        {
            OnChunkUnloaded?.Invoke(id);
        }
        _activeChunkIds.Clear();
        OnPropertyChanged(nameof(ActiveChunks));
    }

    public void ReloadAllChunks()
    {
        ClearLoadedChunks();
        UpdateLoadedChunk();
    }

    public Vector2 GetChunkWorldPosition(long chunkId)
    {
        var (x, y) = LayerChunk.DeconstructChunkId(chunkId);

        float pxX = x * (LayerChunk.ChunkSize * _chunkTileSize.Width);
        float pxY = y * (LayerChunk.ChunkSize * _chunkTileSize.Height);

        return new Vector2(pxX, pxY);
    }

    public void FreezeLoading()
    {
        IsLoadFrozen = true;
    }

    public void UnfreezeLoading()
    {
        IsLoadFrozen = false;
    }
    
    
    public (long minX, long maxX, long minY, long maxY) GetVisibleChunkBounds(int padding = 1)
    {
        var camera = RuntimeServices.CameraService;
    
        // Taille d'un chunk en pixels (ex: 32 * 32 = 1024)
        float chunkPx = LayerChunk.ChunkSize * _chunkTileSize.Width;

        // On calcule les bords réels du monde vus par la caméra
        // (L'utilisation du zoom est correcte ici)
        float viewW = (camera.ViewportSize.Width / camera.ZoomLevel);
        float viewH = (camera.ViewportSize.Height / camera.ZoomLevel);

        float left = camera.Position.X - (viewW / 2f);
        float right = camera.Position.X + (viewW / 2f);
        float top = camera.Position.Y - (viewH / 2f);
        float bottom = camera.Position.Y + (viewH / 2f);

        // Conversion en index de Chunks avec Math.Floor impératif
        long minX = (long)Math.Floor(left / chunkPx) - padding;
        long maxX = (long)Math.Floor(right / chunkPx) + padding;
        long minY = (long)Math.Floor(top / chunkPx) - padding;
        long maxY = (long)Math.Floor(bottom / chunkPx) + padding;

        return (minX, maxX, minY, maxY);
    }

    public void DebugDrawLoadedChunks()
    {
        foreach (var chunkId in ActiveChunks)
        {
            var chunkWorldPosition = GetChunkWorldPosition(chunkId);
            
            RuntimeServices.RenderService.DrawDebugRect(
                chunkWorldPosition,
                ChunkSizeInPixels,
                Color.Red
            );
            
        }
    }

    public void DebugDrawChunkItemsGrid()
    {
        var gridColor = Color.Green * 0.5f;
        var render = RuntimeServices.RenderService;

        foreach (var chunkId in ActiveChunks)
        {
            var startPos = GetChunkWorldPosition(chunkId);
        
            var totalChunkWidth = ChunkSize * _chunkTileSize.Width;
            var totalChunkHeight = ChunkSize * _chunkTileSize.Height;
        
            var endPos = new Vector2(startPos.X + totalChunkWidth, startPos.Y + totalChunkHeight);

            // Vertical lines
            for (var x = 0; x <= ChunkSize; x++)
            {
                var xOffset = startPos.X + (x * _chunkTileSize.Width);
                render.DrawDebugLine(
                    startPos with { X = xOffset },
                    endPos with { X = xOffset },
                    color: gridColor
                );
            }

            // Horizontal lines
            for (var y = 0; y <= ChunkSize; y++)
            {
                var yOffset = startPos.Y + (y * _chunkTileSize.Height);
                render.DrawDebugLine(
                    startPos with { Y = yOffset },
                    endPos with { Y = yOffset },
                    color: gridColor
                );
            }
        }
    }

    public void DebugDrawChunkActualItems()
    {
        throw new NotImplementedException();
    }
    
    public void Dispose()
    {
        RuntimeServices.MapService.OnMapLoaded -= OnMapLoaded;
        RuntimeServices.MapService.OnMapUnloaded -= OnMapUnloaded;
    }

    #region EventHandlers
    
    void OnMapLoaded(Ulid mapId)
    {
        _chunkTileSize = new(
            RuntimeServices.MapService.CurrentLoadedMapData.CellWidth, 
            RuntimeServices.MapService.CurrentLoadedMapData.CellHeight);
        
        ChunkSizeInPixels = new Size(ChunkSize * _chunkTileSize.Width, ChunkSize * _chunkTileSize.Height);
        _hasMapLoaded = true;
        _internalIsLoadFrozen = false;
    }
    
    void OnMapUnloaded()
    {
        _internalIsLoadFrozen = true;
        _hasMapLoaded = false;
        ClearLoadedChunks();
    }

    #endregion
}