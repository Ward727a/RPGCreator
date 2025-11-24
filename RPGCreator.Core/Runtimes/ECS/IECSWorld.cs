using Microsoft.Xna.Framework;
using RPGCreator.Core.Runtimes.Factories;

namespace RPGCreator.Core.Runtimes.ECS;

public interface IECSWorld
{
    EntityManager _entityManager { get; }
    ComponentManager _componentManager { get; }
    SystemManager _systemManager { get; }
    ECSEventBus _eventBus { get; }
    EntityFactory _entityFactory { get; }
    
    Entity CreateEntity();
    void DestroyEntity(Entity entity);
    T AddComponent<T>(Entity entity) where T : struct, IComponent;
    ref T GetComponent<T>(Entity entity) where T : struct, IComponent;
    void Update(GameTime deltaTime);
    void Draw(GameTime deltaTime);
    
    public WorldQuery<T> Query<T>() where T : struct, IComponent;
}