using System.Collections;
using RPGCreator.SDK.Exceptions;
using RPGCreator.SDK.Types.Internals;

namespace RPGCreator.SDK.ECS.Entities;

public class Entity : IEntity, ICleanable
{
    public EntityManager _entityManager { get; private set; }
    public ComponentManager _componentManager { get; private set; }

    public void SetManager(EntityManager manager, ComponentManager componentManager)
    {
        if (_entityManager != null)
            throw new InvalidOperationException("Entity manager is already set and cannot be changed.");
        _entityManager = manager ?? throw new ArgumentNullException(nameof(manager));
        _componentManager = componentManager ?? throw new ArgumentNullException(nameof(componentManager));
    }
    
    public int Id { get; set; }
    public bool HasComponent<T>() where T : struct, IComponent
    {
        return _componentManager.HasComponent<T>(Id);
    }

    public ref T GetComponent<T>() where T : struct, IComponent
    {
        return ref _componentManager.GetComponent<T>(Id);
    }

    public ref T AddComponent<T>() where T : struct, IComponent
    {
        return ref _componentManager.AddComponent<T>(Id);
    }

    public void RemoveComponent<T>() where T : struct, IComponent
    {
        _componentManager.RemoveComponent<T>(Id);
    }

    public Bitmask256 ComponentMask => _componentManager?.GetEntityComponentMask(Id) ??
                                     throw new CriticalEngineException("Entity's ComponentManager is null.", this);

    public void Clean()
    {
        Id = -1;
        _entityManager = null;
        _componentManager = null;
        ComponentMask.Clear();
    }
}