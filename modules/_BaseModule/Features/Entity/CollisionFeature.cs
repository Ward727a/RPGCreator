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
using _BaseModule.Enums;
using RPGCreator.SDK;
using RPGCreator.SDK.ECS;
using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.ECS.Components;
using RPGCreator.SDK.ECS.Features;
using RPGCreator.SDK.ECS.Systems;
using RPGCreator.SDK.Modules.Features.Entity;
using RPGCreator.SDK.RuntimeService;
using RPGCreator.SDK.Types;
using RPGCreator.SDK.Types.Collections;
using Logger = RPGCreator.SDK.Logging.Logger;

namespace _BaseModule.Features.Entity;

[EntityFeature(MaxInstancesPerCharacter = 1)]
public class CollisionFeature : BaseEntityFeature
{
    public static URN Urn = FeatureUrnModule.ToUrnModule("rpgc").ToUrn("collision_feature");
    public override string FeatureName => "Collision Feature";
    public override string FeatureDescription => "Adds collision detection to the entity.";
    public override URN FeatureUrn => Urn;

    public override URN[] DependentFeatures { get; } = [BoundsFeature.Urn];

    [EntityFeatureProperty("Collider type", "Define the type of collider this entity has")]
    public EColliderType ColliderType
    {
        get => GetConfig(EColliderType.Square);
        set => SetConfig(value);
    }

    [EntityFeatureProperty("Collision origin", "Define the origin of the collision box")]
    public Vector2 CollisionOrigin {
        get => GetConfig(Vector2.Zero);
        set => SetConfig(value);
    }

    [EntityFeatureProperty("Collision size", "Define the size of the collision box")]
    public Vector2 CollisionSize
    {
        get => GetConfig(Vector2.Zero);
        set => SetConfig(value);
    }

    public override void OnSetup()
    {
        
    }

    public override void OnWorldSetup(IEcsWorld world)
    {
        world.SystemManager.AddSystem(new CollisionSystem());
    }

    public override void OnInject(BufferedEntity entity, IEntityDefinition entityDefinition)
    {
        entity.AddComponent(new ColliderComponent
        {
            ColliderType = ColliderType,
        });
        
        entity.ExecuteOnceCreated((entityId) =>
        {
            // Later we need to check the type, but for now we will simply use a square collider by default
            var componentManager = GlobalStates.GameSession.ActiveEcsWorld?.ComponentManager;
            if (componentManager != null && componentManager.HasComponent<BoundsComponent, ColliderComponent>(entityId))
            {
                ref var boundsComponent = ref componentManager.GetComponent<BoundsComponent>(entityId);
                ref var colliderComponent = ref componentManager.GetComponent<ColliderComponent>(entityId);
                
                var colliderBlobIndex = GlobalStates.GameSession.BlobManager.Register(
                    new SquareCollider(
                        new(CollisionOrigin,
                            (CollisionSize == Vector2.Zero ? boundsComponent.Size : CollisionSize))
                    )
                );

                colliderComponent.ColliderDataPointer = colliderBlobIndex;
            }
        });
    }
}

// A square collision box.
// This class has ISlabItem so that we can use it as a slab item in the collision system.
// This is a HARD NEED for it to be a valid collider shape!
public record struct SquareCollider(Rect Rect) : ISlabItem
{
    public int? BlockPointerIndex { get; set; }
}

public struct ColliderComponent : IComponent
{
    public Vector2? OffsetToApply;
    public EColliderType ColliderType;
    public SlabItemPointer ColliderDataPointer;
}

public class CollisionSystem : ISystem
{
    public override int Priority => 200;
    public override bool IsDrawingSystem => false;
    
    private bool _test_IsColliding = false;
    
    private ComponentManager _componentManager = null!;
    private IMapService _mapService = null!;
    
    public override void Initialize(IEcsWorld ecsWorld)
    {
        _componentManager = ecsWorld.ComponentManager;
        _mapService = RuntimeServices.MapService;
    }

    public override void Update(TimeSpan deltaTime)
    {
        foreach (var entityId in _componentManager.Query<ColliderComponent, MovementComponent, TransformComponent>())
        {
            ref var boundsComponent = ref _componentManager.GetComponent<BoundsComponent>(entityId);
            ref var colliderComponent = ref _componentManager.GetComponent<ColliderComponent>(entityId);
            ref var movementComponent = ref _componentManager.GetComponent<MovementComponent>(entityId);
            ref var transformComponent = ref _componentManager.GetComponent<TransformComponent>(entityId);

            if (movementComponent.DesiredVelocity == Vector2.Zero) continue;
            
            var colliderDataPointer = colliderComponent.ColliderDataPointer;
            
            var colliderData = GlobalStates.GameSession.BlobManager.Get<SquareCollider>(colliderDataPointer);
            
            var colliderRect = colliderData.Rect; // Local entity collision rectangle.
            var worldColliderRect = new Rect(
                transformComponent.Position + colliderRect.Position - (boundsComponent.OffsetOrigin ?? Vector2.Zero),
                colliderRect.Size
            );
            
            var velocityX = movementComponent.DesiredVelocity with { Y = 0 };
            var velocityY = movementComponent.DesiredVelocity with { X = 0 };
            
            var isCollidingX = CheckVelocity(worldColliderRect, velocityX);
            var isCollidingY = CheckVelocity(worldColliderRect, velocityY);
            
            movementComponent.DesiredVelocity = new Vector2(
                isCollidingX ? 0 : movementComponent.DesiredVelocity.X,
                isCollidingY ? 0 : movementComponent.DesiredVelocity.Y
            );
        }
    }

    /// <summary>
    /// Check if the entity is colliding with the map at a given velocity.
    /// </summary>
    /// <param name="worldColliderRect">The rectangle of the entity's collider in the world position.</param>
    /// <param name="velocity">The velocity to check.</param>
    /// <returns>
    /// True if the entity is colliding with the map at the given velocity, false otherwise.
    /// </returns>
    private bool CheckVelocity(Rect worldColliderRect, Vector2 velocity)
    {
        var futureColliderPosition = worldColliderRect with {Position = worldColliderRect.Position + velocity};
        return _mapService.IsAreaBlocked(futureColliderPosition);
    }
}