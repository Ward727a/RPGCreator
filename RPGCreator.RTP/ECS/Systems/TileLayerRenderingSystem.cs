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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Maps.Chunks;
using RPGCreator.SDK.Assets.Definitions.Tilesets;
using RPGCreator.SDK.Assets.Runtime.Tilesets;
using RPGCreator.SDK.ECS;
using RPGCreator.SDK.ECS.Components.Maps;
using RPGCreator.SDK.ECS.Systems;
using RPGCreator.SDK.Types;
using RPGCreator.SDK.Types.Collections;
using Color = RPGCreator.SDK.Types.Color;

namespace RPGCreator.RTP.ECS.Systems;

public class TileLayerRenderingSystem : ISystem
{
    public override int Priority => 30;
    public override bool IsDrawingSystem => true;

    public BlobManager _blobManager;
    public ComponentManager _componentManager;
    
    private Dictionary<Ulid, Texture2D> _tilesetCache = new();
    
    public override void Initialize(IEcsWorld ecsWorld)
    {
        _blobManager = ecsWorld.WorldBlobManager;
        _componentManager = ecsWorld.ComponentManager;
        
        RuntimeServices.MapService.MapLoaded += OnMapLoaded;
    }

    private void OnMapLoaded(Ulid obj)
    {
        // Clearing the cache so that if the map is changed, we don't keep the old tileset data in memory
        _tilesetCache.Clear();
    }

    public override void Update(TimeSpan deltaTime)
    {

        foreach (var layerEntityId in _componentManager.Query<LayerHeaderComponent, LayerChunkStorageComponent, TileLayerTagComponent>())
        {
            ref var layerHeader = ref _componentManager.GetComponent<LayerHeaderComponent>(layerEntityId);
            ref var chunkStorage = ref _componentManager.GetComponent<LayerChunkStorageComponent>(layerEntityId);

            var visibleChunks = RuntimeServices.ChunkService.GetVisibleChunkBounds();

            for (var x = visibleChunks.minX; x <= visibleChunks.maxX; x++)
            {
                for (var y = visibleChunks.minY; y <= visibleChunks.maxY; y++)
                {
                    var chunkId = LayerChunk.GetChunkId(x, y);
                    
                    if (!chunkStorage.ChunkDataPointers.TryGetValue(chunkId, out var chunkBlobPointIndex)) continue;
                    
                    var runtimeChunk = _blobManager.Get<RuntimeChunk<RuntimeTile>>(chunkBlobPointIndex);

                    for (int bitGroupIndex = 0; bitGroupIndex < 16; bitGroupIndex++)
                    {
                        ulong bits = runtimeChunk.PresenceMask[bitGroupIndex];
                        if(bits == 0) continue;
                        
                        for (int bitIndex = 0; bitIndex < 64; bitIndex++)
                        {
                            if ((bits & (1UL << bitIndex)) == 0) continue;
                            
                            int tileIndex = bitGroupIndex * 64 + bitIndex;
                            
                            var tile = runtimeChunk.Data[tileIndex];

                            var tilesetTextureResult = GetTilesetTexture(tile.TilesetId);

                            if (tilesetTextureResult.IsFailure)
                            {
                                // Later we could add a fallback tileset here
                                throw new Exception($"Failed to load tileset with ID: {tile.TilesetId} - {tilesetTextureResult.Error}");
                            }

                            // Need to switch to a buffered drawing system
                            // once it's ready inside the RTP.
                            RuntimeServices.RenderService.DirectDraw(
                                tilesetTextureResult.Value, tile.PositionInMap,
                                tile.SourceRect, Color.White * layerHeader.Opacity,
                                effects: tile.Effects);
                            
                        }
                    }
                }
            }

        }
        
    }

    private Result<Texture2D> GetTilesetTexture(Ulid tilesetId)
    {
        if (_tilesetCache.TryGetValue(tilesetId, out var tileset))
        {
            return tileset;
        }

        return EngineServices.AssetsManager.Load<BaseTilesetDef>(tilesetId).Bind<Texture2D>(def =>
        {
            var texture = EngineServices.Resources.Load<Texture2D>(def.ImagePath);
            if (texture == null)
                return Result.Fail($"Failed to load tileset texture: {def.ImagePath}");
            _tilesetCache.Add(tilesetId, texture);
            return texture;
        });
    }
}