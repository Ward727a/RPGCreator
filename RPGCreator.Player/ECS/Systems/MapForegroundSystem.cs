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
using System.Diagnostics;
using System.Linq;
using RPGCreator.Core.Types.Map.Chunks;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Maps.AutoLayer;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers.AutoLayer;
using RPGCreator.SDK.Assets.Definitions.Tilesets;
using RPGCreator.SDK.ECS;
using RPGCreator.SDK.ECS.Systems;
using RPGCreator.SDK.RuntimeService;

namespace RPGCreator.Player.ECS.Systems;

public class MapForegroundSystem : ISystem
{
    public override int Priority => 6000; // More than RenderBatchSystem to ensure it runs after it
    public override bool IsDrawingSystem => true;
    
    protected IMapService MapService = RuntimeServices.MapService;
    public override void Initialize(IEcsWorld ecsWorld)
    {
    }

    public override void Update(TimeSpan deltaTime)
    {
        if (!MapService.HasLoadedMap) return;
        var sortedLayersZIndex = MapService.CurrentLoadedMapDefinition!.TileLayers;
        
        var range = RuntimeServices.ChunkService.GetVisibleChunkBounds(IChunkService.ChunkLoadDistance);
        
        foreach (var layer in sortedLayersZIndex.Where(l => l.RenderingMode == RenderingMode.StaticOver))
        {
            DrawSimpleLayer(range, layer);
        }
        RuntimeServices.RenderService.FinishDrawing();
        RuntimeServices.RenderService.ResumeDrawing();
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
}