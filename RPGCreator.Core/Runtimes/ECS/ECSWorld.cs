using Microsoft.Xna.Framework;
using Serilog;

namespace RPGCreator.Core.Runtimes.ECS;

public class ECSWorld : IECSWorld
{
    public EntityManager _entityManager { get; }
    public ComponentManager _componentManager { get; }
    public SystemManager _systemManager { get; }

    public ECSWorld()
    {
        _componentManager = new ComponentManager();
        _entityManager = new EntityManager(_componentManager);
        _systemManager = new SystemManager();
    }
    
    public IEntity CreateEntity()
    {
        return _entityManager.CreateEntity();
    }

    public void DestroyEntity(IEntity entity)
    {
        _entityManager.DestroyEntity(entity);
    }

    public T AddComponent<T>(IEntity entity) where T : struct, IComponent
    {
        return _componentManager.AddComponent<T>(entity);
    }

    public ref T GetComponent<T>(IEntity entity) where T : struct, IComponent
    {
        return ref _componentManager.GetComponent<T>(entity);
    }
    
    public void AddSystem(ISystem system)
    {
        _systemManager.AddSystem(system);
    }
    
    public void RemoveSystem(ISystem system)
    {
        _systemManager.RemoveSystem(system);
    }

    public void Update(GameTime deltaTime)
    {
        // Update systems here
        // For now, we do nothing
        _systemManager.Update(deltaTime);
    }
    
    public void Draw(GameTime deltaTime)
    {
        // Draw systems here
        // For now, we do nothing
        _systemManager.Draw(deltaTime);
    }

    public WorldQuery<T> Query<T>() where T : struct, IComponent
    {
        Log.Error("ECSWorld.Query<T> is not implemented yet.");
        Log.Error("ECSWorld.Query<T> Asked for: " + typeof(T).FullName);
        return null;
        // return new WorldQuery<T>(_entityManager, _componentManager);
    }
}