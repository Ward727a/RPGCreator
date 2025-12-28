using RPGCreator.Core.Runtimes.Factories;
using RPGCreator.SDK.ECS;
using RPGCreator.SDK.ECS.Entities;
using RPGCreator.SDK.ECS.Services;
using RPGCreator.SDK.ECS.Systems;
using Serilog;
using Vector2 = System.Numerics.Vector2;

namespace RPGCreator.Core.ECS;

public class ECSWorld : IECSWorld
{
    public EntityManager EntityManager { get; }
    public ComponentManager ComponentManager { get; }
    public SystemManager SystemManager { get; }
    public ECSEventBus EventBus { get; }
    public IEntityFactory EntityFactory { get; }

    public ECSWorld()
    {
        EventBus = new ECSEventBus();
        ComponentManager = new ComponentManager(EventBus);
        EntityManager = new EntityManager(ComponentManager);
        SystemManager = new SystemManager(this);
        EntityFactory = new EntityFactory(EntityManager);
    }

    public Entity SpawnEntity(BaseEntity entity)
    {
        return EntityFactory.SpawnEntity(entity, Vector2.Zero);
    }
    
    public Entity SpawnEntity(BaseEntity entity, Vector2 position)
    {
        return EntityFactory.SpawnEntity(entity, position);
    }
    
    public Entity CreateEntity()
    {
        return EntityManager.CreateEntity();
    }

    public void DestroyEntity(Entity entity)
    {
        EntityManager.DestroyEntity(entity);
    }

    public T AddComponent<T>(Entity entity) where T : struct, IComponent
    {
        return ComponentManager.AddComponent<T>(entity);
    }

    public ref T GetComponent<T>(Entity entity) where T : struct, IComponent
    {
        return ref ComponentManager.GetComponent<T>(entity);
    }
    
    public void AddSystem(ISystem system)
    {
        SystemManager.AddSystem(system);
    }
    
    public void RemoveSystem(ISystem system)
    {
        SystemManager.RemoveSystem(system);
    }

    public void Update(TimeSpan deltaTime)
    {
        // Update systems here
        // For now, we do nothing
        SystemManager.Update(deltaTime);
    }
    
    public void Draw(TimeSpan deltaTime)
    {
        // Draw systems here
        // For now, we do nothing
        SystemManager.Draw(deltaTime);
    }

    public WorldQuery<T> Query<T>() where T : struct, IComponent
    {
        Log.Error("ECSWorld.Query<T> is not implemented yet.");
        Log.Error("ECSWorld.Query<T> Asked for: " + typeof(T).FullName);
        return null;
        // return new WorldQuery<T>(_entityManager, _componentManager);
    }
}