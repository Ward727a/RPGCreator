using System.Collections;
using RPGCreator.Core.Types.Internal;
using RPGCreator.SDK.Types.Internals;

namespace RPGCreator.SDK.ECS.Entities;

public class Entity : IEntity, ICleanable
{
    public EntityManager? _entityManager { get; private set; }
    public ComponentManager? _componentManager { get; private set; }

    public void SetManager(EntityManager manager, ComponentManager componentManager)
    {
        if (_entityManager != null)
            throw new InvalidOperationException("Entity manager is already set and cannot be changed.");
        _entityManager = manager ?? throw new ArgumentNullException(nameof(manager));
        _componentManager = componentManager ?? throw new ArgumentNullException(nameof(componentManager));
    }
    
    public int Id { get; set; }
    public bool HasComponent<T>() where T : IComponent
    {
        var bit = ComponentTypeRegistry.GetBit<T>();
        return ComponentBits[bit];
    }

    public ref T GetComponent<T>() where T : IComponent
    {
        return ref _componentManager.GetComponent<T>(this);
    }

    public ref T AddComponent<T>() where T : IComponent, new()
    {
        return ref _componentManager.AddComponent<T>(this);
    }

    public void RemoveComponent<T>() where T :  IComponent
    {
        _componentManager.RemoveComponent<T>(this);
    }

    public BitArray ComponentBits { get; } = new BitArray(64); // Initial size of 256 bits, can grow dynamically if needed.

    public void Clean()
    {
        Id = -1;
        _entityManager = null;
        _componentManager = null;
        ComponentBits.SetAll(false);
    }
}