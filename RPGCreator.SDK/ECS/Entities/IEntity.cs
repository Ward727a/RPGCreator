using System.Collections;

namespace RPGCreator.SDK.ECS.Entities;

public interface IEntity
{
    EntityManager? _entityManager { get; }
    ComponentManager? _componentManager { get; }
    /// <summary>
    /// Here we do not use ULID for performance reasons.
    /// The ID is assigned by the EntityManager when the entity is created.
    /// </summary>
    public int Id { get; set; }
    
    public bool HasComponent<T>() where T : struct, IComponent;
    public ref T GetComponent<T>() where T : struct, IComponent;
    public ref T AddComponent<T>() where T  : struct, IComponent;
    public void RemoveComponent<T>() where T : struct, IComponent;

    /// <summary>
    /// A bit vector representing the components attached to this entity.
    /// Explanation:
    /// Each bit in the ComponentMask corresponds to a specific component type.
    /// If a bit is set to 1, it indicates that the entity has that component attached.
    /// If a bit is set to 0, it indicates that the entity does not have that component.
    /// This allows for efficient storage and quick checks of component presence using bitwise operations.
    /// </summary>
    public Bitmask256 ComponentMask { get; }
}