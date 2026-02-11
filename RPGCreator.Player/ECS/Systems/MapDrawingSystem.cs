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
using System.Linq;
using Microsoft.Xna.Framework;
using RPGCreator.Core.Types.Map.Chunks;
using RPGCreator.Player.Extensions;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Maps;
using RPGCreator.SDK.Assets.Definitions.Maps.AutoLayer;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers.AutoLayer;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers.EntityLayer;
using RPGCreator.SDK.Assets.Definitions.Tilesets;
using RPGCreator.SDK.ECS;
using RPGCreator.SDK.ECS.Components;
using RPGCreator.SDK.ECS.Systems;
using RPGCreator.SDK.RuntimeService;
using ToolsUtilitiesStandard.Helpers;
using Vector2 = System.Numerics.Vector2;

namespace RPGCreator.Player.ECS.Systems;

public class MapDrawingSystem() : BaseMapDrawingSystem
{
    public override int Priority => 100;
    
    public override void Initialize(IEcsWorld ecsWorld)
    {
        RuntimeServices.OnceServiceReady((IMapService MapService) =>
        {
            MapService.OnMapLoaded += map =>
            {
                if (!RuntimeServices.MapService.HasLoadedMap) return;
                PrecalculateLayerRenderingMode(RuntimeServices.MapService.CurrentLoadedMapDefinition);
            };
            if (!RuntimeServices.MapService.HasLoadedMap) return;
            PrecalculateLayerRenderingMode(RuntimeServices.MapService.CurrentLoadedMapDefinition);
        });
    }

    private void PrecalculateLayerRenderingMode(IMapDef map)
    {
        var entityLayer = map.TileLayers.FirstOrDefault(l => l is EntityLayerDefinition);
        int spawnLayerZIndex = entityLayer?.LayerIndex ?? 0;

        foreach (var layer in map.TileLayers)
        {
            if (layer is EntityLayerDefinition) continue;
            if(layer.IsForeground && layer is not EntityLayerDefinition)
            {
                layer.RenderingMode = RenderingMode.StaticOver;
                continue;
            }

            if (layer.LayerIndex < spawnLayerZIndex)
            {
                layer.RenderingMode = RenderingMode.StaticUnder;
            }
            else
            {
                layer.RenderingMode = RenderingMode.Dynamic;
            }
        }
    }

    public override void Update(TimeSpan deltaTime)
    {
        if(CameraService.CameraEntityId == null)
            return;
        
        if(MapService.CurrentLoadedMapDefinition == null)
            return;
        
        var range = RuntimeServices.ChunkService.GetVisibleChunkBounds(IChunkService.ChunkLoadDistance);

        List<(long X, long Y, long ID)> visibleChunks = new();

        var sortedLayersZIndex = MapService.CurrentLoadedMapDefinition.TileLayers
            .OrderBy(layer => layer.ZIndex)
            .ToList();

        RuntimeServices.RenderService.PauseDrawing();
        RuntimeServices.RenderService.PrepareDrawing(IRenderService.SpriteSortMode.Deferred);
        foreach (var layer in sortedLayersZIndex.Where(l => l.RenderingMode == RenderingMode.StaticUnder))
        {
            DrawSimpleLayer(range, layer);
        }
        
        foreach (var layer in sortedLayersZIndex.Where(l => l.RenderingMode == RenderingMode.Dynamic))
        {
            CollectTilesInQueue(range, layer);
        }
    }
    private void DrawSimpleLayer((long minX, long maxX, long minY, long maxY) range, BaseLayerDef layer)
    {
        var actualLayer = (layer is AutoLayerDefinition auto) ? auto.InternalTileLayer : layer;
        if (actualLayer is not LayerWithElements<ITileDef> tileLayer) return;

        for (var x = range.minX; x <= range.maxX; x++)
        {
            for (var y = range.minY; y <= range.maxY; y++)
            {
                DrawChunkTiles(LayerChunk.GetChunkId(x, y), tileLayer);
            }
        }
    }
    /// <summary>
    /// Draw all tiles in the given chunk for the given layer.
    /// </summary>
    /// <param name="chunkId">The chunk ID.</param>
    /// <param name="layer">The layer containing the tiles.</param>
    private void DrawChunkTiles(long chunkId, LayerWithElements<ITileDef> layer)
    {
        var chunkElements = layer.GetElements(chunkId);
        if (chunkElements == null)
            return;
        
        if(chunkElements.IsEmpty)
            return;
        
        for (int i = 0; i < chunkElements.Length; i++)
        {
            var tileDefinition  = chunkElements[i]; 
            if(tileDefinition == null)
                continue;
            
            var position = layer.GetElementWorldPosition(chunkId, i);
            
            RuntimeServices.RenderService.DrawTile(tileDefinition, position);
        }
    }
    
    private void CollectTilesInQueue((long minX, long maxX, long minY, long maxY) range, BaseLayerDef layer)
    {
        var actualLayer = (layer is AutoLayerDefinition auto) ? auto.InternalTileLayer : layer;
        if (actualLayer is not LayerWithElements<ITileDef> tileLayer) return;

        for (var x = range.minX; x <= range.maxX; x++)
        {
            for (var y = range.minY; y <= range.maxY; y++)
            {
                var chunkId = LayerChunk.GetChunkId(x, y);
                var elements = tileLayer.GetElements(chunkId);
                if (elements == null || elements.IsEmpty) continue;

                for (int i = 0; i < elements.Length; i++)
                {
                    var tile = elements[i];
                    if (tile == null) continue;

                    var worldPos = tileLayer.GetElementWorldPosition(chunkId, i);
                
                    // UX : On trie par le BAS de la tuile pour que le perso passe derrière
                    float sortY = worldPos.Y + tile.SizeInTileset.Height;

                    RuntimeServices.RenderService.SubmitToQueue(new RenderCommand
                    {
                        TexturePath = tile.TilesetDef.ImagePath, // Supposant que ITileDef a accès au tileset
                        Position = worldPos,
                        SourceRect = RuntimeServices.RenderService.GetTileSourceRect(tile),
                        Color = (Color.White * layer.Opacity).ToSystemFast(),
                        SortY = sortY
                    });
                }
            }
        }
    }
    
}