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
using System.Numerics;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Maps.Chunks;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers;
using RPGCreator.SDK.Common.Helpers;
using RPGCreator.SDK.Services.RuntimeService;
using RPGCreator.SDK.Types;
using RPGCreator.Shared.Types;

namespace RPGCreator.Player.Services;

public class PhysicsService : IPhysicsService
{
    // Need to rework that, to use a pre-baked collision layer.
    // a pre-baked collision layer should be generated when either the Map is saved or the game is exported.
    //
    // Still need to think how to manage the dynamic collision, because pre-baked collision works for "fixed" collision, but not moveable collision.
    // As we use ECS, and each "moveable" object (characters, doors, other...) are Entity inside the system, we can make a custom
    // collision component with a collision system.
    
    /// <summary>
    /// Check if the position collides with a collision element.
    /// </summary>
    /// <param name="position">Position to check</param>
    /// <param name="size"></param>
    /// <returns>True if there is a collision, false otherwise</returns>
    public bool CheckCollision(Vector2 position, Size size)
    {
        return false;
        var mapService = RuntimeServices.MapService;
        
        var gridParam = mapService.CurrentLoadedMapDefinition?.GridParameter;

        if (gridParam == null)
            return false;
        
        float cellW = gridParam.Value.CellWidth;
        float cellH = gridParam.Value.CellHeight;
        
        var objectRect = new Rect(position, size);     
        
        float chunkSizePxX = LayerChunk.ChunkSize * cellW;
        float chunkSizePxY = LayerChunk.ChunkSize * cellH;
        
        int minX = (int)Math.Floor(position.X / chunkSizePxX);
        int minY = (int)Math.Floor(position.Y / chunkSizePxY);
        int maxX = (int)Math.Ceiling((position.X + size.Width) / chunkSizePxX);
        int maxY = (int)Math.Ceiling((position.Y + size.Height) / chunkSizePxY);
        
        for (int layerIndex = 0; layerIndex < mapService.LayerCount; layerIndex++)
        {
            var layer = mapService.GetLayerAt(layerIndex);

            if (layer is not TileLayerDefinition tileLayer) continue;

            for (int cx = minX; cx <= maxX; cx++)
            {
                for (int cy = minY; cy <= maxY; cy++)
                {
                    long chunkId = LayerChunk.GetChunkId(cx, cy);
                    var chunk = tileLayer.GetElements(chunkId);
                    
                    if(chunk.IsEmpty)
                        continue;
                    
                    int startTileX = (int)Math.Floor(position.X / cellW) % LayerChunk.ChunkSize;
                    int startTileY = (int)Math.Floor(position.Y / cellH) % LayerChunk.ChunkSize;

                    if (startTileX < 0) startTileX += LayerChunk.ChunkSize;
                    if (startTileY < 0) startTileY += LayerChunk.ChunkSize;

                    for (int ty = 0; ty <= 3; ty++)
                    {
                        for (int tx = 0; tx <= 3; tx++)
                        {
                            var tile = tileLayer.GetElement(new(startTileX + tx, startTileY + ty));
                            if (tile == null) continue;
                            
                            var tileWorldPos = LayerChunk.GetWorldPosition(chunkId, (startTileY + ty)*32+(startTileX+tx));

                            if (tile.TilesetDef.RuntimeCollisionCache.TryGetValue(tile.PositionInTileset.ToKey(),
                                    out var collisionRect))
                            {
                                foreach (var rect in collisionRect)
                                {
                                    var worldRect = new Rect(
                                        tileWorldPos.X + rect.X,
                                        tileWorldPos.Y + rect.Y,
                                        rect.Width,
                                        rect.Height);
                                    
                                    if (objectRect.Intersects(worldRect))
                                        return true;
                                }
                            }
                        }
                    }

                }
            }
            
        }

        return false;
    }
}