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
using RPGCreator.SDK;
using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.ECS;
using RPGCreator.SDK.ECS.Components;
using RPGCreator.SDK.ECS.Systems;
using RPGCreator.SDK.Modules.Features.Entity;
using RPGCreator.SDK.Types;

namespace _BaseModule.Features.Entity;

public enum MovementType
{
    [Description("Four Directions", "Allows movement in up, down, left, and right directions only.")]
    FourDir,
    [Description("Eight Directions", "Allows movement in up, down, left, right, and diagonal directions.")]
    EightDir,
    [Description("Free Direction", "Allows movement in any direction without restrictions.")]
    FreeDir
}

[EntityAnimationRequired("rpgc://entity_animations/walk", "walk", AnimationDirection.EightDirections)]
[EntityAnimationRequired("rpgc://entity_animations/idle", "idle", AnimationDirection.EightDirections)]
[EntityAnimationRequired("rpgc://entity_animations/test", "test", AnimationDirection.FourDirections)]
[EntityAnimationRequired("rpgc://entity_animations/test_single_dir", "test single dir", AnimationDirection.SingleDirection)]
[EntityFeature(MaxInstancesPerCharacter = 1)]
public class MovementFeature : BaseEntityFeature
{

    public static URN Urn = FeatureUrnModule.ToUrnModule("rpgc").ToUrn("movement_feature");
    
    private static readonly URN WalkUrn = new("rpgc://entity_animations/walk");
    private static readonly URN IdleUrn = new("rpgc://entity_animations/idle");
    
    public override string FeatureName => "Movement Feature";
    public override string FeatureDescription => "Provides basic movement capabilities to entities.";
    public override URN FeatureUrn => Urn;

    /// <summary>
    /// Define the animation state index inside the state component.<br/>
    /// This is used to know which state to use for animations.<br/>
    /// Note: This is assigned inside the <see cref="OnSetup"/> with the help of the <see cref="EntityStateRegistry.Register"/> method.
    /// </summary>
    private int _animationStateIdx = 0;
    private int _animationDirStateIdx = 0;
    
    private int _walkActionId = 0;
    private int _idleActionId = 0;
    
    [EntityFeatureProperty("Movement Type", "Defines the type of movement allowed for the entity.\n" +
                                            "Property is global with all other same feature.", IsShared = true)]
    public MovementType MovementType
    {
        get => GetShared(MovementType.FourDir);
        set => SetShared(value);
    }
    
    [EntityFeatureProperty("Speed", "Defines the movement speed of the entity.", MinValue = 0)]
    public int Speed
    {
        get => GetConfig(16);
        set => SetConfig(value);
    }

    public override void OnSetup()
    {
        // Initialization logic for the movement feature can be added here.
        var stateRegistry = EngineServices.ECS.StateRegistry;

        _animationStateIdx =
            stateRegistry.Register(new URN("rpgc", "entity_states", "animation_state"), StateStorageType.Int).Index;
        _animationDirStateIdx =
            stateRegistry.Register(new URN("rpgc", "entity_states", "animation_direction"), StateStorageType.Int).Index;

        _walkActionId = stateRegistry.RegisterAction(WalkUrn);
        _idleActionId = stateRegistry.RegisterAction(IdleUrn);
    }

    public override void OnWorldSetup(IEcsWorld world)
    {
        // Logic to execute when the world is set up can be added here.
        world.SystemManager.AddSystem(new MovementSystem(_animationStateIdx, _animationDirStateIdx, _walkActionId, _idleActionId));
    }

    public override void OnInject(BufferedEntity entity, IEntityDefinition entityDefinition)
    {
        entity.AddComponent(new MovementComponent
        {
            MovementType = MovementType,
            Speed = Speed
        });
    }

    public override void OnDestroy(BufferedEntity entity)
    {
        entity.RemoveComponent<MovementComponent>();
    }
}

public struct MovementComponent : IComponent
{
    public MovementType MovementType;
    public int Speed;
    public Vector2 Direction;
}

public class MovementSystem(int animationStateIdx, int animationDirStateIdx, int walkId, int idleId) : ISystem
{
    public override int Priority => 100;
    public override bool IsDrawingSystem => false;

    private ComponentManager _componentManager = null!;
    
    public override void Initialize(IEcsWorld ecsWorld)
    {
        _componentManager = ecsWorld.ComponentManager;
    }

    public override void Update(TimeSpan deltaTime)
    {
        
        foreach (var entityId in _componentManager.Query<MovementComponent, TransformComponent, StateComponent>())
        {
            ref var moveComponent = ref _componentManager.GetComponent<MovementComponent>(entityId);
            ref var transformComponent = ref _componentManager.GetComponent<TransformComponent>(entityId);
            ref var stateComponent = ref _componentManager.GetComponent<StateComponent>(entityId);
            
            if (!RuntimeServices.GameSession.IsPaused)
            {
                switch (moveComponent.MovementType)
                {
                    case MovementType.FourDir:
                        HandleMovement8(ref moveComponent, ref transformComponent, deltaTime);
                        break;
                    case MovementType.EightDir:
                        HandleMovement8(ref moveComponent, ref transformComponent, deltaTime);
                        break;
                    case MovementType.FreeDir:
                        HandleMovementFree(ref moveComponent, ref transformComponent, deltaTime);
                        break;
                }
            }

            if (moveComponent.Direction != Vector2.Zero && !RuntimeServices.GameSession.IsPaused)
                stateComponent.GetInt(animationStateIdx) = walkId;
            else
                stateComponent.GetInt(animationStateIdx) = idleId;
            
            stateComponent.GetInt(animationDirStateIdx) = moveComponent.Direction.GetDirectionFromVector();
            
            // Reset direction
            moveComponent.Direction = new Vector2();
        }

    }

    private void HandleMovement4(ref MovementComponent movement, ref TransformComponent transform, TimeSpan deltaTime)
    {
        if (movement.Direction.LengthSquared() > 0)
        {
            Vector2 dir = movement.Direction;

            if (MathF.Abs(dir.X) > MathF.Abs(dir.Y))
                dir = new Vector2(MathF.Sign(dir.X), 0);
            else
                dir = new Vector2(0, MathF.Sign(dir.Y));

            float dt = (float)deltaTime.TotalSeconds;
            transform.Position += dir * movement.Speed * dt;
        
            movement.Direction = dir;
        }
    }
    private void HandleMovement8(ref MovementComponent movement, ref TransformComponent transform, TimeSpan deltaTime)
    {
        if (movement.Direction.LengthSquared() > 0)
        {
            var normalizedDir = Vector2.Normalize(movement.Direction);
            float dt = (float)deltaTime.TotalSeconds;
        
            transform.Position += normalizedDir * movement.Speed * dt;
        
            movement.Direction = normalizedDir;
        }
    }
    private void HandleMovementFree(ref MovementComponent movement, ref TransformComponent transform, TimeSpan deltaTime)
    {
        if (movement.Direction.LengthSquared() > 0)
        {
            var normalizedDir = Vector2.Normalize(movement.Direction);
            float dt = (float)deltaTime.TotalSeconds;
        
            transform.Position += normalizedDir * movement.Speed * dt;
        }
    }
}