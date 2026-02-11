using RPGCreator.SDK.ECS.Entities;
using RPGCreator.SDK.ECS.Factories;
using RPGCreator.SDK.ECS.Systems;

namespace RPGCreator.SDK.ECS;


/// <summary>
/// This represents an entity created in a command buffer.<br/>
/// It allows adding/removing components and destroying the entity in a buffered manner.
/// </summary>
/// <param name="temporaryId">The temporary entity ID assigned by the command buffer.</param>
/// <param name="buffer">The command buffer where this entity is created.</param>
public struct BufferedEntity(int temporaryId, IEcsCommandBuffer buffer)
{
    /// <summary>
    /// The temporary entity ID assigned by the command buffer.<br/>
    /// This ID is used to reference the entity in buffered commands!<br/>
    /// As such, it <b>should NOT be used</b> as an actual entity ID in the world!
    /// </summary>
    public readonly int Id = temporaryId;
    
    /// <summary>
    /// Add a component of type T to this buffered entity.
    /// </summary>
    /// <typeparam name="T">The type of component to add.</typeparam>
    /// <param name="component">The component to add.</param>
    /// <remarks>
    /// DO NOT USE THIS METHOD IN ASYNC CONTEXTS OR MULTITHREADED SYSTEMS WITHOUT PROPER SYNCHRONIZATION.<br/>
    /// Using this method in such contexts can lead to race conditions and undefined behavior.<br/>
    /// Make sure to only call this method from the main thread or within a properly synchronized context.
    /// </remarks>
    public void AddComponent<T>(T component) where T : struct, IComponent
    {
        buffer.AddComponent(Id, component);
    }
    
    /// <summary>
    /// Remove a component of type T from this buffered entity.
    /// </summary>
    /// <typeparam name="T">The type of component to remove.</typeparam>
    /// <remarks>
    /// DO NOT USE THIS METHOD IN ASYNC CONTEXTS OR MULTITHREADED SYSTEMS WITHOUT PROPER SYNCHRONIZATION.<br/>
    /// Using this method in such contexts can lead to race conditions and undefined behavior.<br/>
    /// Make sure to only call this method from the main thread or within a properly synchronized context.
    /// </remarks>
    public void RemoveComponent<T>() where T : struct, IComponent
    {
        buffer.RemoveComponent<T>(Id);
    }
    
    /// <summary>
    /// Destroy this buffered entity.<br/>
    /// This will queue a destroy command in the command buffer.<br/>
    /// Note: All other operations on this entity after calling Destroy() will still be queued, but not executed.
    /// </summary>
    /// <remarks>
    /// DO NOT USE THIS METHOD IN ASYNC CONTEXTS OR MULTITHREADED SYSTEMS WITHOUT PROPER SYNCHRONIZATION.<br/>
    /// Using this method in such contexts can lead to race conditions and undefined behavior.<br/>
    /// Make sure to only call this method from the main thread or within a properly synchronized context.
    /// </remarks>
    public void Destroy()
    {
        buffer.DestroyEntity(Id);
    }
    
    /// <summary>
    /// Allow executing an action once the entity is created in the world and has a valid ID.<br/>
    /// This is useful for cases where you need to perform additional setup on the entity immediately after creation, such as adding components that require the actual entity ID or registering it in some way.
    /// </summary>
    /// <param name="onCreated">The action to execute once the entity is created, receiving the new entity ID.</param>
    public void ExecuteOnceCreated(Action<int> onCreated)
    {
        buffer.ExecuteOnceCreated(Id, onCreated);
    }
}

/// <summary>
/// The command buffer interface, allowing deferred execution of ECS commands.<br/>
/// This is useful for batching entity/component operations to improve performance and avoid issues during iteration, or multithreading.
/// </summary>
public interface IEcsCommandBuffer
{
    /// <summary>
    /// Create a new entity.
    /// </summary>
    /// <exception cref="NotImplementedException">
    /// Thrown if the command buffer does not support entity creation.<br/>
    /// If you didn't modify the engine, this should never happen.
    /// </exception>
    BufferedEntity CreateEntity(Action<int>? onCreated = null);
    
    /// <summary>
    /// Destroy the given entity.
    /// </summary>
    /// <param name="entity">The entity to destroy.</param>
    /// <exception cref="NotImplementedException">
    /// Thrown if the command buffer does not support entity creation.<br/>
    /// If you didn't modify the engine, this should never happen.
    /// </exception>
    void DestroyEntity(int entity);
    
    /// <summary>
    /// Add a component of type T to the given entity.
    /// </summary>
    /// <param name="entity">The entity to add the component to.</param>
    /// <param name="component">The component to add.</param>
    /// <typeparam name="T">The type of component to add.</typeparam>
    /// <exception cref="NotImplementedException">
    /// Thrown if the command buffer does not support entity creation.<br/>
    /// If you didn't modify the engine, this should never happen.
    /// </exception>
    void AddComponent<T>(int entity, T component) where T : struct, IComponent;
    
    /// <summary>
    /// Remove a component of type T from the given entity.
    /// </summary>
    /// <param name="entity">The entity to remove the component from.</param>
    /// <typeparam name="T">The type of component to remove.</typeparam>
    /// <exception cref="NotImplementedException">
    /// Thrown if the command buffer does not support entity creation.<br/>
    /// If you didn't modify the engine, this should never happen.
    /// </exception>
    void RemoveComponent<T>(int entity) where T : struct, IComponent;

    public void ExecuteOnceCreated(int entityId, Action<int> onCreated);
    
    /// <summary>
    /// Execute all buffered commands on the given ECS world.<br/>
    /// If no world is provided, it will automatically use the world associated with this command buffer.
    /// </summary>
    /// <param name="world">The ECS world to execute the commands on.</param>
    /// <exception cref="NotImplementedException">
    /// Thrown if the command buffer does not support entity creation.<br/>
    /// If you didn't modify the engine, this should never happen.
    /// </exception>
    void Execute(IEcsWorld? world = null);
}

public interface IEcsWorld
{
    /// <summary>
    /// The entity manager for this world.
    /// </summary>
    EntityManager EntityManager { get; }
    
    /// <summary>
    /// The component manager for this world.
    /// </summary>
    ComponentManager ComponentManager { get; }
    
    /// <summary>
    /// The system manager for this world.
    /// </summary>
    SystemManager SystemManager { get; }
    
    /// <summary>
    /// The event bus for this world.
    /// </summary>
    EcsEventBus EventBus { get; }
    
    /// <summary>
    /// The entity factory for this world.
    /// </summary>
    IEntityFactory EntityFactory { get; }
    
    /// <summary>
    /// The command buffer for this world.
    /// </summary>
    IEcsCommandBuffer CommandBuffer { get; }
    
    /// <summary>
    /// Create a new entity.
    /// <param name="onCreated">Optional callback invoked when the entity is created, receiving the new entity ID.</param>
    /// </summary>
    BufferedEntity CreateEntity(Action<int>? onCreated = null);
    
    /// <summary>
    /// Create a new entity directly in the world, bypassing command buffers.<br/>
    /// This is generally discouraged in favor of using command buffers for better performance and safety during iteration.<br/>
    /// Only use this method when you absolutely need immediate entity creation (Note: You probably don't).
    /// </summary>
    /// <returns>
    /// The ID of the newly created entity.
    /// </returns>
    int CreateDirectEntity();
    
    /// <summary>
    /// Destroy the given entity.
    /// </summary>
    /// <param name="entity">The entity to destroy.</param>
    void DestroyEntity(int entity);
    
    /// <summary>
    /// Destroy the given entity directly in the world, bypassing command buffers.<br/>
    /// This is generally discouraged in favor of using command buffers for better performance and safety during iteration.<br/>
    /// Only use this method when you absolutely need immediate entity destruction (Note: You probably don't).<br/>
    /// <br/>
    /// WARNING: Using this method can, and likely will, lead to undefined behavior (bugs, crashes, data corruption) if the entity is being used elsewhere (e.g. during system updates).<br/>
    /// Make sure you know what you are doing when using this method.
    /// </summary>
    /// <param name="entity">The entity to destroy.</param>
    void DestroyDirectEntity(int entity);
    
    /// <summary>
    /// Add a component of type T to the given entity.
    /// </summary>
    /// <param name="entity">The entity to add the component to.</param>
    /// <param name="component">The component to add.</param>
    /// <typeparam name="T">The type of component to add.</typeparam>
    void AddComponent<T>(int entity, T component) where T : struct, IComponent;
    
    /// <summary>
    /// Add a component of type T to the given entity directly in the world, bypassing command buffers.<br/>
    /// This is generally discouraged in favor of using command buffers for better performance and safety during iteration.<br/>
    /// Only use this method when you absolutely need immediate component addition (Note: You probably don't).
    /// </summary>
    /// <param name="entity">The entity to add the component to.</param>
    /// <param name="component">The component to add.</param>
    /// <typeparam name="T">The type of component to add.</typeparam>
    void AddDirectComponent<T>(int entity, T component) where T : struct, IComponent;
    
    /// <summary>
    /// Add a component of type T to the given entity directly in the world, bypassing command buffers, and returns a reference to it.<br/>
    /// This is generally discouraged in favor of using command buffers for better performance and safety during iteration.<br/>
    /// Only use this method when you absolutely need immediate component addition (Note: You probably don't).
    /// </summary>
    /// <param name="entity">The entity to add the component to.</param>
    /// <typeparam name="T">The type of component to add.</typeparam>
    /// <returns>
    /// A reference to the newly added component.
    /// </returns>
    ref T AddDirectComponent<T>(int entity) where T : struct, IComponent;
    
    /// <summary>
    /// Get a reference to the component of type T attached to the given entity.
    /// </summary>
    /// <param name="entity">The entity to get the component from.</param>
    /// <typeparam name="T">The type of component to get.</typeparam>
    /// <returns>
    /// A reference to the requested component.
    /// </returns>
    ref T GetComponent<T>(int entity) where T : struct, IComponent;
    
    /// <summary>
    /// Update the world by updating all systems.<br/>
    /// This should be called once per frame or tick to advance the simulation.
    /// </summary>
    /// <param name="deltaTime">The time elapsed since the last update.</param>
    void Update(TimeSpan deltaTime);
    
    /// <summary>
    /// Draw the world by invoking draw methods on all systems.<br/>
    /// This should be called once per frame to render the current state.
    /// </summary>
    /// <param name="deltaTime">The time elapsed since the last draw.</param>
    void Draw(TimeSpan deltaTime);
    
    /// <summary>
    /// Warning: Not implemented yet.<br/>
    /// World queries allow efficient iteration over entities that match specific component criteria.
    /// </summary>
    /// <typeparam name="T">The type of component to query for.</typeparam>
    /// <returns>A WorldQuery for the specified component type.</returns>
    public WorldQuery<T> Query<T>() where T : struct, IComponent;
}