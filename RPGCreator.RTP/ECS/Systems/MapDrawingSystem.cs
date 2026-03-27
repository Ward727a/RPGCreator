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
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Maps.Chunks;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers.EntityLayer;
using RPGCreator.SDK.Assets.Definitions.Tilesets;
using RPGCreator.SDK.ECS;
using RPGCreator.SDK.ECS.Systems;
using RPGCreator.SDK.RuntimeService;

namespace RPGCreator.RTP.ECS.Systems;

public class MapDrawingSystem : BaseMapDrawingSystem
{
    public override void Initialize(IEcsWorld ecsWorld)
    {
        
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
        foreach (var layer in sortedLayersZIndex)
        {
            for(var x = range.minX; x <= range.maxX; x++)
            {
                for(var y = range.minY; y <= range.maxY; y++)
                {
                    var chunk = LayerChunk.GetChunkId(x, y);
                    
                    visibleChunks.Add((x, y, chunk));
                    
                    layer.GetRenderer().Render(layer, chunk);
                }
            }
        }

        if (RuntimeServices.RenderService.CurrentPreviewTarget != null)
        {

            var previewTarget = RuntimeServices.RenderService.CurrentPreviewTarget;
            if (previewTarget.PreviewObject is ITileDef tileDef)
            {
                RuntimeServices.RenderService.SetGlobalOpacity(.5f);
                foreach (var pos in previewTarget.PreviewPosition)
                {
                    RuntimeServices.RenderService.DrawTile(tileDef, pos);
                }
                RuntimeServices.RenderService.SetGlobalOpacity(1f);
            }
        }

        
        DrawDebugChunkBounds();
        RuntimeServices.RenderService.FinishDrawing();
        RuntimeServices.RenderService.ResumeDrawing();
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

    private void DrawEntity(long chunkId, LayerWithElements<EntitySpawner> layerEntity)
    {
        var chunkElements = layerEntity.GetElements(chunkId);
        if (chunkElements == null)
            return;
        
        if(chunkElements.IsEmpty)
            return;
        
        for (int i = 0; i < chunkElements.Length; i++)
        {
            var entitySpawner  = chunkElements[i]; 
            if(entitySpawner == null)
                continue;

            var position = layerEntity.GetElementWorldPosition(chunkId, i);
            
            RuntimeServices.RenderService.DrawEntitySpawner(entitySpawner, position);
        }
    }
    
    private void DrawDebugChunkBounds()
    {
        RuntimeServices.ChunkService.DebugDrawChunkItemsGrid();
        RuntimeServices.ChunkService.DebugDrawLoadedChunks();
    }
}