using Microsoft.Extensions.ObjectPool;
using RPGCreator.SDK.Common.Exceptions;
using RPGCreator.SDK.ECS.Components;

namespace RPGCreator.SDK.ECS.Entities;

public class EntityManager(ComponentManager componentManager) : IDisposable
{
    private readonly ObjectPool<Entity> _entityPool = new DefaultObjectPool<Entity>(new DefaultPooledObjectPolicy<Entity>(), 1000);
    private Entity?[] _entitiesById = new Entity?[1024];
    
    private int _nextEntityId;
    
    private readonly Queue<int> _freeIds = new();

    private int GetNextEntityId()
    {
        return _freeIds.Count > 0 ? _freeIds.Dequeue() : _nextEntityId++;
    }
    
    private void ReleaseEntityId(int entityId)
    {
        _freeIds.Enqueue(entityId);
    }

    public int CreateEntity()
    {
        return CreateEntityInternal().Id;
    }
    
    private Entity CreateEntityInternal()
    {
        var entity = _entityPool.Get();
        entity.Id = GetNextEntityId();
        entity.SetManager(this, componentManager);
        
        componentManager.RegisterEntityComponentBits(entity.Id);
        EnsureCapacity(entity.Id);
        _entitiesById[entity.Id] = entity;
        
        componentManager.AddComponent(entity.Id, StateComponentFactory.CreateState());
        
        return entity;
    }


    /// <summary>
    /// Create a basic camera entity with Transform and Camera components.<br/>
    /// You need to add a CameraSystem to the ECSWorld, then set the active camera to this entity.
    /// </summary>
    /// <returns>
    /// The created camera entity.
    /// </returns>
    public IEntity CreateCameraEntity()
    {
        var entity = CreateEntityInternal();
        ref var transformComponent = ref entity.AddComponent<TransformComponent>();
        ref var cameraComponent = ref entity.AddComponent<CameraComponent>();
        
        cameraComponent.Zoom = 1.0f;
        cameraComponent.ViewportSize = new(800, 600);
        cameraComponent.IsFollowingEntity = false;
        transformComponent.Position = new(0, 0);
        cameraComponent.Rotation = 0.0f;
        
        return entity;
    }
    
    public void DestroyEntity(Entity entity)
    {
        componentManager.RemoveAllComponents(entity.Id);
        _entityPool.Return(entity);
        ReleaseEntityId(entity.Id);
    }
    
    public void DestroyEntity(int entityId)
    {
        DestroyEntity(GetEntityById(entityId));
    }
    
    private Entity GetEntityById(int id)
    {
        EnsureCapacity(id);
        var entity = _entitiesById[id];
        if (entity == null)
        {
            throw new CriticalEngineException($"Entity with ID {id} does not exist.",
                new KeyNotFoundException($"_entitiesById length is {_entitiesById.Length} asking for ID {id}"), this);
        }

        return entity;
    }
    
    private void EnsureCapacity(int id)
    {
        if (id >= _entitiesById.Length)
        {
            Array.Resize(ref _entitiesById, _entitiesById.Length * 2);
        }
    }

    public void Dispose()
    {
        foreach (var entity in _entitiesById)
        {
            if (entity != null)
            {
                componentManager.RemoveAllComponents(entity.Id);
                _entityPool.Return(entity);
            }
        }

        _entitiesById = Array.Empty<Entity?>();
        _freeIds.Clear();
        
        GC.SuppressFinalize(this);
    }
}