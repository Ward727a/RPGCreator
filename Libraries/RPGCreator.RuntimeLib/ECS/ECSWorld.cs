using System.Numerics;
using RPGCreator.RuntimeLib.Factories;
using RPGCreator.SDK.ECS;
using RPGCreator.SDK.ECS.Components;
using RPGCreator.SDK.ECS.Entities;
using RPGCreator.SDK.ECS.Factories;
using RPGCreator.SDK.ECS.Systems;

namespace RPGCreator.RuntimeLib.ECS;

public class EcsWorld : IEcsWorld
{
    public EntityManager EntityManager { get; }
    public ComponentManager ComponentManager { get; }
    public SystemManager SystemManager { get; }
    public EcsEventBus EventBus { get; }
    public IEntityFactory EntityFactory { get; }
    
    public IEcsCommandBuffer CommandBuffer { get; }
    public BlobManager WorldBlobManager { get; }

    public EcsWorld()
    {
        EventBus = new EcsEventBus();
        ComponentManager = new ComponentManager(EventBus);
        EntityManager = new EntityManager(ComponentManager);
        ComponentManager.Initialize(EntityManager);
        SystemManager = new SystemManager(this);
        EntityFactory = new EntityFactory(this);
        CommandBuffer = new DefaultCommandBuffer(this);
        WorldBlobManager = new BlobManager();
    }

    public BufferedEntity SpawnEntity(IEntityDefinition entityDefinition)
    {
        return EntityFactory.SpawnEntity(entityDefinition, Vector2.Zero);
    }
    
    public BufferedEntity SpawnEntity(IEntityDefinition entityDefinition, Vector2 position)
    {
        return EntityFactory.SpawnEntity(entityDefinition, position);
    }
    
    public BufferedEntity CreateEntity(Action<int>? onCreated = null)
    {
        return CommandBuffer.CreateEntity(onCreated);
    }

    public int CreateDirectEntity()
    {
        return EntityManager.CreateEntity();
    }

    public void DestroyEntity(int entity)
    {
        CommandBuffer.DestroyEntity(entity);
    }

    public void DestroyDirectEntity(int entity)
    {
        EntityManager.DestroyEntity(entity);
    }

    public void AddComponent<T>(int entity, T component) where T : struct, IComponent
    {
        CommandBuffer.AddComponent(entity, component);
    }

    public void AddDirectComponent<T>(int entity, T component) where T : struct, IComponent
    {
        ComponentManager.AddComponent(entity, component);
    }

    public ref T AddDirectComponent<T>(int entity) where T : struct, IComponent
    {
        return ref ComponentManager.AddComponent<T>(entity);
    }

    public ref T GetComponent<T>(int entity) where T : struct, IComponent
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
        
        // Execute command buffer
        CommandBuffer.Execute();
    }
    
    public void Draw(TimeSpan deltaTime)
    {
        // Draw systems here
        // For now, we do nothing
        SystemManager.Draw(deltaTime);
    }

    /// <summary>
    /// Warning: Not implemented yet.
    /// </summary>
    /// <typeparam name="T">The type of component to query for.</typeparam>
    /// <returns>A WorldQuery for the specified component type.</returns>
    public WorldQuery<T> Query<T>() where T : struct, IComponent
    {
        return null;
    }

    public void Dispose()
    {
        SystemManager.Dispose();
        ComponentManager.Dispose();
        CommandBuffer.Dispose();
        EntityManager.Dispose();
        WorldBlobManager.Dispose();
        
        GC.SuppressFinalize(this);
    }
}