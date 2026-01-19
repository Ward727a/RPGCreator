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
using System.Drawing;
using System.Numerics;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.Core.Types.Map.Chunks;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Maps;
using RPGCreator.SDK.Assets.Definitions.Tilesets;
using RPGCreator.SDK.ECS;
using RPGCreator.SDK.ECS.Systems;
using RPGCreator.SDK.Logging;
using Size = RPGCreator.SDK.Types.Size;

namespace RPGCreator.RTP.ECS.Systems;

public class MapDrawingSystem(GraphicsDevice graphicsDevice) : BaseMapDrawingSystem
{

    private GraphicsDevice _graphicsDevice = graphicsDevice;

    public override void Initialize(IECSWorld iecsWorld)
    {
    }

    public override void Update(TimeSpan deltaTime)
    {
        if(CameraService.CameraEntity == null)
            return;
        
        if(MapService.CurrentLoadedMapDefinition == null)
            return;
        
        var range = CameraService.GetVisibleChunkBounds();

        foreach (var layer in MapService.CurrentLoadedMapDefinition.TileLayers)
        {
            if(layer is not LayerWithElements<ITileDef> tileLayer)
                continue;
            
            for(var x = range.minX; x <= range.maxX; x++)
            {
                for(var y = range.minY; y <= range.maxY; y++)
                {
                    var chunk = LayerChunk.GetChunkId(x, y);

                    DrawChunkTiles(chunk, tileLayer);
                }
            }
        }
        DrawDebugChunkBounds();
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
    
    private void DrawDebugChunkBounds()
    {
        var range = CameraService.GetVisibleChunkBounds();
        
        // Get numbers of chunks in view
        var chunksInViewX = range.maxX - range.minX + 1;
        var chunksInViewY = range.maxY - range.minY + 1;
        
        var totalChunks = chunksInViewX * chunksInViewY;
        Logger.Debug($"Drawing debug for {totalChunks} chunks in view ({chunksInViewX} x {chunksInViewY})");

        for(var x = range.minX; x <= range.maxX; x++)
        {
            for(var y = range.minY; y <= range.maxY; y++)
            {
                float tileSize = RuntimeServices.MapService.CurrentLoadedMapData.CellWidth;
                int chunkSizeInTiles = LayerChunk.ChunkSize;
                float visualSize = chunkSizeInTiles * tileSize;
                RuntimeServices.RenderService.DrawDebugRect(
                    new Vector2(
                        x,
                        y),
                    new Size(
                        visualSize,
                        visualSize
                    ),
                    Color.White
                );
            }
        }
    }
}