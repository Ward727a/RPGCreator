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

using System.Numerics;
using _BaseModule.AssetDefinitions.SpawnPoint;
using _BaseModule.Features.Entity;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Maps.Chunks;
using RPGCreator.SDK.ECS;
using RPGCreator.SDK.ECS.Components;
using RPGCreator.SDK.Modules.NativeAction;
using RPGCreator.SDK.Types;

namespace _BaseModule.NativeAction;

public class SpawnPointNativeAction : BaseNativeAction
{
    public override URN ExpectedSignal => SignalItemModule.ToUrnModule("rpgc").ToUrn("entity_spawned");
    public override URN ClassUrn => NativeActionModule.ToUrnModule("rpgc").ToUrn("spawn_point_native_action");

    private Ulid _cachedMapId = Ulid.Empty;
    private Vector2 _cachedPosition = Vector2.Zero;
    
    public override void Execute(int entityId, IEcsWorld world)
    {
        var componentManager = world.ComponentManager;

        if (componentManager.HasComponent<TransformComponent, PlayerTagComponent>(entityId))
        {
            ref var transformComponent = ref componentManager.GetComponent<TransformComponent>(entityId);
            
            var mapService = RuntimeServices.MapService;
            var currentMapDef = mapService.CurrentLoadedMapDefinition;
            if (currentMapDef != null)
            {
                if (currentMapDef.Unique == _cachedMapId)
                {
                    transformComponent.Position = _cachedPosition;
                    return;
                }
                
                _cachedMapId = currentMapDef.Unique;
                
                var layers = currentMapDef.TileLayers;
                foreach (var baseLayerDef in layers)
                {
                    if (baseLayerDef is SpawnPointLayer spawnPointLayer)
                    {
                        var chunks = spawnPointLayer.Chunks;
                        foreach (var chunk in chunks)
                        {
                            var span = chunk.Value.GetAllElementsSpan();
                            for (int spanId = 0; spanId < span.Length; spanId++)
                            {
                                var spawnData = span[spanId];
                                if (spawnData == null)
                                    continue;

                                var position = LayerChunk.GetWorldPosition(chunk.Key, spanId);
                                _cachedPosition = position + new Vector2(16, 16); // Need to see if there is not a better solution
                                // This is added so the player is placed at the center of the spawn point.

                                transformComponent.Position = _cachedPosition;
                                return;
                            }
                        }
                    }
                }
            }
        }
    }
}