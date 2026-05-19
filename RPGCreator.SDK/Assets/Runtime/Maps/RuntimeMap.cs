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

using RPGCreator.SDK.Assets.Definitions.Maps;
using RPGCreator.SDK.Assets.Definitions.Maps.Chunks;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers;
using RPGCreator.SDK.Common.Helpers;
using RPGCreator.SDK.Types;
using RPGCreator.Shared.Types;

namespace RPGCreator.SDK.Assets.Runtime.Maps;

public class RuntimeMap : IRuntime<MapDefinition>
{
    public event Action<BaseLayerDef>? TileLayerAdded;
    public event Action<BaseLayerDef>? TileLayerRemoved;
    
    public MapDefinition Definition { get; protected set; }

    public RuntimeMap(MapDefinition definition)
    {
        Definition = definition;
    }

    #region Layers
    
    public bool AddLayer(BaseLayerDef layer)
    {
        if (layer == null || Definition.Layers.Any(l => l.Unique == layer.Unique))
            return false; // If the layer is null or already exists, we can't add it

        Definition.Layers.Add(layer);
        TileLayerAdded?.Invoke(layer); // Notify subscribers that a new layer has been added
        return true;
    }
    
    public bool RemoveLayer(BaseLayerDef layer)
    {
        if (layer == null || Definition.Layers.All(l => l.Unique != layer.Unique))
            return false; // If the layer is null or doesn't exist, we can't remove it 

        Definition.Layers.Remove(layer);
        TileLayerRemoved?.Invoke(layer); // Notify subscribers that a layer has been removed
        return true;
    }

    #endregion

    #region Collision

    public void BakeCollisionChunk()
    {
        Definition.CollisionChunk.ClearElements();

        foreach (var layer in Definition.Layers)
        {
            if (layer is not TileLayerDefinition tileLayer) continue;
        
            foreach (var (chunkId, tileChunk) in tileLayer.Chunks)
            {
                var tiles = tileChunk.GetAllElementsSpan();
                
                for (int i = 0; i < tiles.Length; i++)
                {
                    var tile = tiles[i];
                    if (tile == null) continue;

                    tile.TilesetDef.BuildRuntimeCollisionCache();
                    if (tile.TilesetDef.RuntimeCollisionCache.TryGetValue(tile.PositionInTileset.ToKey(), out var rects))
                    {
                        if (!Definition.CollisionChunk.TryGetElement(chunkId, out var colChunk))
                        {
                            colChunk = new CollisionChunk();
                            Definition.CollisionChunk.AddElement(colChunk, chunkId);
                        }

                        int localIdX = i & 31;
                        int localIdY = i >> 5;

                        var finalData = new RuntimeCollisionChunkData {
                            Collisions = rects.Select(r => new Rect(r.X - tile.PositionInTileset.X, r.Y - tile.PositionInTileset.Y, r.Width, r.Height)).ToArray()
                        };

                        colChunk.SetElement(localIdX, localIdY, finalData);
                    }
                }
            }
        }
    }

    #endregion
}