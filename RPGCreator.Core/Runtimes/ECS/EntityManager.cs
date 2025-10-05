using RPGCreator.Core.Type.Internal;

namespace RPGCreator.Core.Runtimes.ECS;

public class EntityManager
{
    ObjectPool<Entity> _entityPool = new(() => new Entity());
    Dictionary<System.Type, object> _componentsSparseSets = new();
    
    public IEntity CreateEntity()
    {
        var entity = _entityPool.Rent();
        return entity;
    }
}