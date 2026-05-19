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

using RPGCreator.SDK.Common.Attributes;
using RPGCreator.SDK.ECS.Components;
using RPGCreator.SDK.ECS.Entities;
using RPGCreator.SDK.ECS.Systems;
using RPGCreator.SDK.Modules.Features.Entity;
using RPGCreator.SDK.Services.EngineService;
using RPGCreator.SDK.Types;
using RPGCreator.Shared.Types;

namespace RPGCreator.SDK.ECS.Features;

[EntityFeature]
public class SpriteFeature : BaseEntityFeature
{
    public static URN Urn = FeatureUrnModule.ToUrnModule("rpgc").ToUrn("sprite_feature");
    public override string FeatureName { get; } = "Sprite Feature";
    public override string FeatureDescription { get; } = "Adds a sprite to the entity for rendering.";
    public override URN FeatureUrn => Urn;
    
    public override URN[] DependentFeatures { get; } = [BoundsFeature.Urn];
    
    [EntityFeatureProperty("Size mode", "Define how the sprite should be scaled to fit the entity bounds.")]
    public ESizeMode SizeMode { get; set; } = ESizeMode.Fit;

    private int _shouldRecalculateSpriteSizeIdx = -1;
    
    public override void OnSetup()
    {
        // As internal feature are loaded before even service like ECS are loaded, we need to get the service Once it's ready.
        // In a normal module, we don't need to do this.
        EngineServices.OnceServiceReady<IEcsService>(ecsService =>
        {
           var stateRegistry = ecsService.StateRegistry;
           
            // We use a byte registry for those following reason:
            // - We need to know when the TransformComponent or BoundsComponent has changed
            // - But we also need to do it "one" time, so to skip
            _shouldRecalculateSpriteSizeIdx = stateRegistry.Register(EntityStateRegistry.ModuleUrn.ToUrnModule("rpgc").ToUrn("should_recalculate_sprite_size"), StateStorageType.Byte).Index;
        });
        
    }

    public override void OnWorldSetup(IEcsWorld world)
    {
        world.SystemManager.AddSystem(new SpriteRenderSystem(_shouldRecalculateSpriteSizeIdx));
    }

    public override void OnInject(BufferedEntity entity, IEntityDefinition entityDefinition)
    {
        entity.AddComponent(new SpriteComponent()
        {
            Color = Color.White,
            LayerDepth = .1f
        });
    }

    public override void OnDestroy(BufferedEntity entity)
    {
        entity.RemoveComponent<SpriteComponent>();
    }
}