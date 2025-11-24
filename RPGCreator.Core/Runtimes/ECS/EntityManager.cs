using System.ComponentModel;
using RPGCreator.Core.Types.Internal;

namespace RPGCreator.Core.Runtimes.ECS;

public class EntityManager(ComponentManager componentManager)
{
    ObjectPool<Entity> _entityPool = new(() => new Entity());
    private ComponentManager _componentManager { get; } = componentManager;

    public IEntity CreateEntity()
    {
        var entity = _entityPool.Rent();
        entity.Id = _entityPool.RentedCount - 1;
        entity.SetManager(this, _componentManager);
        return entity;
    }
    
    public void DestroyEntity(IEntity entity)
    {
        _componentManager.RemoveAllComponents(entity.Id, entity.ComponentBits);
        _entityPool.Return((Entity)entity);
    }
    
}