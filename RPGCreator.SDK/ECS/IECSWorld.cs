using RPGCreator.SDK.ECS.Entities;
using RPGCreator.SDK.ECS.Services;
using RPGCreator.SDK.ECS.Systems;

namespace RPGCreator.SDK.ECS;

public interface IECSWorld
{
    EntityManager EntityManager { get; }
    ComponentManager ComponentManager { get; }
    SystemManager SystemManager { get; }
    ECSEventBus EventBus { get; }
    IEntityFactory EntityFactory { get; }
    
    Entity CreateEntity();
    void DestroyEntity(Entity entity);
    T AddComponent<T>(Entity entity) where T : struct, IComponent;
    ref T GetComponent<T>(Entity entity) where T : struct, IComponent;
    void Update(TimeSpan deltaTime);
    void Draw(TimeSpan deltaTime);
    
    public WorldQuery<T> Query<T>() where T : struct, IComponent;
}