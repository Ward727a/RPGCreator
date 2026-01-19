using RPGCreator.Core.Runtimes.ECS;
using RPGCreator.Core.Runtimes.ECS.Components.Display;
using RPGCreator.Core.Types.Internal;
using RPGCreator.SDK.ECS.Components;
using RPGCreator.SDK.ECS.Entities;

namespace RPGCreator.SDK.ECS;

public class EntityManager(ComponentManager componentManager)
{
    ObjectPool<Entity> _entityPool = new(() => new Entity());
    private ComponentManager _componentManager { get; } = componentManager;
    
    private int _nextEntityId = 0;

    public Entity CreateEntity()
    {
        var entity = _entityPool.Rent();
        entity.Id = _nextEntityId;
        _nextEntityId++;
        entity.SetManager(this, _componentManager);
        return entity;
    }

    /// <summary>
    /// Create a basic camera entity with Transform and Camera components.<br/>
    /// You need to add a CameraSystem to the ECSWorld, then set the active camera to this entity.
    /// </summary>
    /// <returns>
    /// The created camera entity.
    /// </returns>
    public Entity CreateCameraEntity()
    {
        var entity = CreateEntity();
        entity.AddComponent<TransformComponent>();
        ref var cameraComponent = ref entity.AddComponent<CameraComponent>();
        
        cameraComponent.Zoom = 1.0f;
        cameraComponent.ViewportSize = new(800, 600);
        cameraComponent.IsFollowingEntity = false;
        cameraComponent.Position = new(0, 0);
        cameraComponent.Rotation = 0.0f;
        
        return entity;
    }
    
    public void DestroyEntity(Entity entity)
    {
        _componentManager.RemoveAllComponents(entity.Id, entity.ComponentBits);
        _entityPool.Return(entity);
    }
    
}