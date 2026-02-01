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
using RPGCreator.Core.Runtimes.ECS.Components.Display;
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
    [Description("Four Directions")]
    FourDir,
    [Description("Eight Directions")]
    EightDir,
    [Description("Free Direction")]
    FreeDir
}

[EntityFeature(MaxInstancesPerCharacter = 1)]
public class MovementFeature : BaseEntityFeature
{
    public override string FeatureName => "Movement Feature";
    public override string FeatureDescription => "Provides basic movement capabilities to entities.";
    public override URN FeatureUrn => new("rpgc", FeatureUrnModule, "MovementFeature");

    /// <summary>
    /// Define the movement state index inside the state component.<br/>
    /// This is used to know which state to use for movement animations.<br/>
    /// Note: This is assigned inside the <see cref="OnSetup"/> with the help of the <see cref="EntityStateRegistry.Register"/> method.
    /// </summary>
    private int _movementStateIdx = 0;
    private int _movementDirStateIdx = 0;
    
    [EntityFeatureProperty("Movement Type", "Defines the type of movement allowed for the entity." +
                                            "Property is global with all other same feature.", IsShared = true)]
    public MovementType MovementType
    {
        get => GetShared(MovementType.FourDir);
        set => SetShared(value);
    }
    
    [EntityFeatureProperty("Speed", "Defines the movement speed of the entity.", MinValue = 0)]
    public int Speed
    {
        get => GetConfig(5);
        set => SetConfig(value);
    }
    
    public override void OnSetup()
    {
        // Initialization logic for the movement feature can be added here.
        var stateRegistry = EngineServices.ECS.StateRegistry;

        _movementStateIdx =
            stateRegistry.Register(new URN("rpgc", "entity_states", "move_state"), StateStorageType.String).Index;
        _movementDirStateIdx =
            stateRegistry.Register(new URN("rpgc", "entity_states", "move_direction"), StateStorageType.Int).Index;
        
    }

    public override void OnWorldSetup(IEcsWorld world)
    {
        // Logic to execute when the world is set up can be added here.
        world.SystemManager.AddSystem(new MovementSystem(_movementStateIdx, _movementDirStateIdx));
    }

    public override void OnInject(BufferedEntity entity)
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

    public override void Dispose()
    {
        // Cleanup logic for the movement feature can be added here.
    }
}

public struct MovementComponent : IComponent
{
    public MovementType MovementType;
    public int Speed;
    public Vector2 Direction;
}

public class MovementSystem(int movementStateIdx, int movementDirStateIdx) : ISystem
{
    
    private int _movementStateIdx = movementStateIdx;
    private int _movementDirStateIdx = movementDirStateIdx;
    
    public override int Priority => 100;
    public override bool IsDrawingSystem => false;

    private ComponentManager _componentManager;
    
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
            
            switch (moveComponent.MovementType)
            {
                case MovementType.FourDir:
                    HandleMovement4(entityId, ref moveComponent, ref transformComponent, deltaTime);
                    break;
                case MovementType.EightDir:
                    HandleMovement8(entityId, ref moveComponent, ref transformComponent, deltaTime);
                    break;
                case MovementType.FreeDir:
                    HandleMovementFree(entityId, ref moveComponent, ref transformComponent, deltaTime);
                    break;
            }
            
            if(moveComponent.Direction != Vector2.Zero)
                stateComponent.GetString(_movementStateIdx) = "move";
            else
                stateComponent.GetString(_movementStateIdx) = "idle";
            
            stateComponent.GetInt(_movementDirStateIdx) = GetDirectionFromVector(moveComponent.Direction);
            
            // Reset direction
            moveComponent.Direction = new Vector2();
        }

    }
    
    /// <summary>
    /// A simple map to convert a vector2 direction to an entity direction int.<br/>
    /// The index is calculated as: (signX + 1) + (signY + 1) * 3<br/>
    /// Where signX and signY are the signs of the X and Y components of the vector2 direction (-1, 0, 1).
    /// </summary>
    public static readonly int[] DirectionMap =
    {
        (int)EntityDirection.UpLeft,    // (-1,-1)
        (int)EntityDirection.Up,        // (0,-1)
        (int)EntityDirection.UpRight,   // (1,-1)
        (int)EntityDirection.Left,      // (-1,0)
        (int)EntityDirection.Center,    // (0,0)
        (int)EntityDirection.Right,     // (1,0)
        (int)EntityDirection.DownLeft,  // (-1,1)
        (int)EntityDirection.Down,      // (0,1)
        (int)EntityDirection.DownRight  // (1,1)
    };
    
    /// <summary>
    /// Return a direction int from a vector2 direction.<br/>
    /// Values are defined in <see cref="DirectionMap"/>, see for more info.
    /// </summary>
    /// <param name="direction">The direction vector.</param>
    /// <returns>The direction as an int.</returns>
    private int GetDirectionFromVector(Vector2 direction)
    {
        if (direction.LengthSquared() < 0.01f) // Plus robuste qu'un simple == Zero
            return (int)EntityDirection.Center;
        int ix = (int)Math.Sign(direction.X) + 1; // -1,0,1 -> 0,1,2
        int iy = (int)Math.Sign(direction.Y) + 1; // -1,0,1 -> 0,1,2
        return DirectionMap[ix + iy * 3];
    }

    private void HandleMovement4(int entityId, ref MovementComponent movement, ref TransformComponent transform, TimeSpan deltaTime)
    {
        
    }
    private void HandleMovement8(int entityId, ref MovementComponent movement, ref TransformComponent transform, TimeSpan deltaTime)
    {
    }
    private void HandleMovementFree(int entityId, ref MovementComponent movement, ref TransformComponent transform, TimeSpan deltaTime)
    {
        if (movement.Direction.LengthSquared() > 0)
        {
            // On normalise pour éviter d'aller plus vite en diagonale
            var normalizedDir = Vector2.Normalize(movement.Direction);
            float dt = (float)deltaTime.TotalSeconds;
        
            transform.Position += normalizedDir * movement.Speed * dt;
        }
    }
}