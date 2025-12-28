using RPGCreator.Core.Runtimes.ECS;
using RPGCreator.Core.Types.Internal;
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
    
    public void DestroyEntity(Entity entity)
    {
        _componentManager.RemoveAllComponents(entity.Id, entity.ComponentBits);
        _entityPool.Return(entity);
    }
    
}