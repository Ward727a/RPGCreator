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

using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Animations;
using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.ECS;
using RPGCreator.SDK.ECS.Components;
using RPGCreator.SDK.ECS.Systems;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Modules.Features.Entity;
using RPGCreator.SDK.Types;

namespace _BaseModule.Features.Entity;

[EntityFeature(MaxInstancesPerCharacter = 1)]
public class AnimationFeature : BaseEntityFeature
{
    public static URN Urn = FeatureUrnModule.ToUrnModule("rpgc").ToUrn("animation_feature");
    public override string FeatureName => "Animation Feature";
    public override string FeatureDescription => "Adds animation capabilities to the entity.";
    public override URN FeatureUrn => Urn;
    
    /// <summary>
    /// Define the animation state index inside the state component.<br/>
    /// This is used to know which state to use for animations.<br/>
    /// Note: This is assigned inside the <see cref="OnSetup"/> with the help of the <see cref="EntityStateRegistry.Register"/> method.
    /// </summary>
    private int _animationStateIdx = 0;
    private int _animationDirStateIdx = 0;

    public override void OnSetup()
    {
        var stateRegistry = EngineServices.ECS.StateRegistry;

        _animationStateIdx =
            stateRegistry.Register(new URN("rpgc", "entity_states", "animation_state"), StateStorageType.Int).Index;
        _animationDirStateIdx =
            stateRegistry.Register(new URN("rpgc", "entity_states", "animation_direction"), StateStorageType.Int).Index;
    }

    public override void OnWorldSetup(IEcsWorld world)
    {
        world.SystemManager.AddSystem(new AnimationSystem(_animationStateIdx, _animationDirStateIdx));
    }

    public override void OnInject(BufferedEntity entity, IEntityDefinition entityDefinition)
    {
        entity.AddComponent(new AnimationComponent
        {
            CurrentAnimation = 0,
            CurrentDirection = EntityDirection.Center.ToInt(),
            SpeedMultiplier = 1.0f,
            IsPlaying = false,
            LastWorkingAnimation = -1,
            LastWorkingDirection = -1
        });
    }

    public override void OnDestroy(BufferedEntity entity)
    {
        entity.RemoveComponent<AnimationComponent>();
    }

    public override void Dispose()
    {
        // Nothing to dispose
    }
}

public struct AnimationComponent : IComponent
{
    
    public Ulid CurrentAnimationId { get; set; }
    public AnimationDef? CurrentAnimationDef;
    
    public int CurrentAnimation;
    public int CurrentDirection;
    
    public double ElapsedTime;
    public int CurrentFrame;
    
    public bool IsPlaying;
    public float SpeedMultiplier;
    
    public int LastWorkingAnimation;
    public int LastWorkingDirection;
}

public class AnimationSystem(int animationStateIdx, int animationDirStateIdx) : ISystem
{
    public override int Priority => 80;
    public override bool IsDrawingSystem => false;
    
    private int _animationStateIdx = animationStateIdx;
    private int _animationDirStateIdx = animationDirStateIdx;
    
    private ComponentManager _componentManager;
    
    public override void Initialize(IEcsWorld ecsWorld)
    {
        _componentManager = ecsWorld.ComponentManager;
    }

    public override void Update(TimeSpan deltaTime)
    {
        foreach (var entityId in _componentManager.Query<AnimationComponent, StateComponent, CharStateComponent, SpriteComponent>())
        {
            ref var animationComponent = ref _componentManager.GetComponent<AnimationComponent>(entityId);
            ref var stateComponent = ref _componentManager.GetComponent<StateComponent>(entityId);
            ref var charStateComponent = ref _componentManager.GetComponent<CharStateComponent>(entityId);
            ref var spriteComponent = ref _componentManager.GetComponent<SpriteComponent>(entityId);
            
            var desiredAnimation = stateComponent.GetInt(_animationStateIdx);
            var desiredDirection = stateComponent.GetInt(_animationDirStateIdx);

            if (desiredAnimation < 0)
                desiredAnimation = 0;

            if (!charStateComponent.AnimationsMapping.TryGetValue(desiredAnimation, out var animSet))
            {
                if(animationComponent.LastWorkingAnimation == -1)
                    continue;
                Logger.Error("[AnimationSystem] Failed to find animation set for animation ID: {animId} - Trying fallback with last working animationId.", desiredAnimation);
                desiredAnimation = animationComponent.LastWorkingAnimation;
                if (!charStateComponent.AnimationsMapping.TryGetValue(desiredAnimation, out animSet))
                {
                    animationComponent.LastWorkingAnimation = -1;
                    Logger.Error("[AnimationSystem] Failed to find fallback animation set for animation ID: {animId} - Stopping animation.", desiredAnimation);
                    continue;
                }
                
                Logger.Warning("[AnimationSystem] Successfully found fallback animation set for animation ID: {animId}.", desiredAnimation);
            }
            
            if (!animSet.Animations.TryGetValue(desiredDirection, out var animId))
            {
                if(animationComponent.LastWorkingDirection == -1)
                    continue;
                Logger.Error("[AnimationSystem] Failed to find animation for direction ID: {dirId} in animation set for animation ID: {animId} - Trying fallback with last working direction.", desiredDirection, desiredAnimation);
                desiredDirection = animationComponent.LastWorkingDirection;
                if (!animSet.Animations.TryGetValue(desiredDirection, out animId))
                {
                    Logger.Error("[AnimationSystem] Failed to find animation for direction ID: {dirId} in animation set for animation ID: {animId} - Stopping animation.", desiredDirection, desiredAnimation);
                    animationComponent.LastWorkingDirection = -1;
                    continue;
                }
                Logger.Warning("[AnimationSystem] Successfully found fallback animation for direction ID: {dirId} in animation set for animation ID: {animId}.", desiredDirection, desiredAnimation);
            }
            
            var anim = animId;
            
            if(animationComponent.CurrentAnimationId != anim)
            {
                animationComponent.CurrentAnimationId = anim;
                animationComponent.CurrentAnimationDef = null;
                animationComponent.ElapsedTime = 0;
                animationComponent.CurrentFrame = 0;
                animationComponent.IsPlaying = true;
            }

            if (animationComponent.IsPlaying && animationComponent.CurrentAnimationId != Ulid.Empty)
            {
                if (animationComponent.CurrentAnimationDef == null)
                {
                    if (EngineServices.AssetsManager.TryResolveAsset(animationComponent.CurrentAnimationId,
                            out AnimationDef? animDef))
                    {
                        animationComponent.CurrentAnimationDef = animDef;
                        spriteComponent.SpritesheetId = animDef.SpritesheetId;
                        spriteComponent.CurrentFrameIndex = animDef.FrameIndexes[animationComponent.CurrentFrame];
                        animationComponent.LastWorkingAnimation = desiredAnimation;
                        animationComponent.LastWorkingDirection = desiredDirection;
                    }
                    else
                    {
                        Logger.Error("[AnimationSystem] Failed to load animation with ID: {animId}", animationComponent.CurrentAnimationId);
                        animationComponent.CurrentAnimationId = Ulid.Empty; // Stop trying to load it
                        animationComponent.IsPlaying = false; // Really, stop it
                        continue;
                    }
                }
                
                var animDefinition = animationComponent.CurrentAnimationDef!;
                animationComponent.ElapsedTime += deltaTime.TotalMilliseconds * animationComponent.SpeedMultiplier;
                
                double frameDuration = animDefinition.FrameDuration;
                
                while (animationComponent.ElapsedTime >= frameDuration && animationComponent.IsPlaying)
                {
                    animationComponent.ElapsedTime -= frameDuration;
                    animationComponent.CurrentFrame++;

                    if (animationComponent.CurrentFrame >= animDefinition.TotalFrames)
                    {
                        if (animDefinition.Loop)
                        {
                            animationComponent.CurrentFrame = 0;
                        }
                        else
                        {
                            animationComponent.CurrentFrame = Math.Max(0, animDefinition.TotalFrames - 1);
                            animationComponent.IsPlaying = false;
                            animationComponent.ElapsedTime = 0;
                        }
                    }
                    
                    spriteComponent.CurrentFrameIndex = animDefinition.FrameIndexes[animationComponent.CurrentFrame];
                }
            }
        }
    }
}