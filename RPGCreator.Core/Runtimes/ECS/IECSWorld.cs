using Microsoft.Xna.Framework;

namespace RPGCreator.Core.Runtimes.ECS;

public interface IECSWorld
{
    EntityManager _entityManager { get; }
    ComponentManager _componentManager { get; }
    
    IEntity CreateEntity();
    void DestroyEntity(IEntity entity);
    T AddComponent<T>(IEntity entity) where T : struct, IComponent;
    ref T GetComponent<T>(IEntity entity) where T : struct, IComponent;
    void Update(GameTime deltaTime);
    void Draw(GameTime deltaTime);
    
    public WorldQuery<T> Query<T>() where T : struct, IComponent;
}