using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using RPGCreator.Core.Types.Internal;
using RPGCreator.SDK.ECS.Entities;
using RPGCreator.SDK.Types.Collections;

namespace RPGCreator.SDK.ECS;

public readonly ref struct DirtyQueryView
{
    private readonly ReadOnlySpan<int> _entities;
    private readonly Bitmask256 _queryMask;
    private readonly ComponentManager _manager;

    public DirtyQueryView(ReadOnlySpan<int> entities, Bitmask256 queryMask, ComponentManager manager)
    {
        _entities = entities;
        _queryMask = queryMask;
        _manager = manager;
    }

    public DirtyQueryEnumerator GetEnumerator() => new DirtyQueryEnumerator(_entities, _queryMask, _manager);
}
public ref struct DirtyQueryEnumerator
{
    private readonly ReadOnlySpan<int> _dirtyEntities;
    private readonly Bitmask256 _queryMask;
    private readonly ComponentManager _manager;
    private int _index;

    public DirtyQueryEnumerator(ReadOnlySpan<int> entities, Bitmask256 queryMask, ComponentManager manager)
    {
        _dirtyEntities = entities;
        _queryMask = queryMask;
        _manager = manager;
        _index = -1;
    }

    public int Current => _dirtyEntities[_index];

    public bool MoveNext()
    {
        while (++_index < _dirtyEntities.Length)
        {
            // On vérifie si l'entité dirty possède bien les composants demandés par la query
            if (_manager.IsMatch(_dirtyEntities[_index], _queryMask))
                return true;
        }
        return false;
    }
}

public readonly ref struct QueryView
{
    private readonly ReadOnlySpan<int> _entities;
    private readonly Bitmask256 _queryMask;
    private readonly ComponentManager _manager;

    public QueryView(ReadOnlySpan<int> entities, Bitmask256 queryMask, ComponentManager manager)
    {
        _entities = entities;
        _queryMask = queryMask;
        _manager = manager;
    }

    public QueryEnumerator GetEnumerator() => new QueryEnumerator(_entities, _queryMask, _manager);
}

public ref struct QueryEnumerator
{
    private readonly ReadOnlySpan<int> _entities;
    private readonly Bitmask256 _queryMask;
    private readonly ComponentManager _manager;
    private int _index;

    public QueryEnumerator(ReadOnlySpan<int> entities, Bitmask256 queryMask, ComponentManager manager)
    {
        _entities = entities;
        _queryMask = queryMask;
        _manager = manager;
        _index = -1;
    }

    public int Current => _entities[_index];

    public bool MoveNext()
    {
        while (++_index < _entities.Length)
        {
            if (_manager.IsMatch(_entities[_index], _queryMask))
                return true;
        }
        return false;
    }
}

/// <summary>
/// The componentsMask is a struct that holds a bitmask representing which components an entity has.<br/>
/// The max number of components is 256, so we can use 4 ulong (64 bits each) to store the mask. Each bit represents whether the entity has a specific component or not.
/// </summary>
public struct Bitmask256
{
    private ulong _b0, _b1, _b2, _b3;

    public void Set(int bitIndex, bool value)
    {
        int word = bitIndex >> 6;
        int bit = bitIndex & 63;
        ulong mask = 1UL << bit;

        if (word == 0) { if (value) _b0 |= mask; else _b0 &= ~mask; }
        else if (word == 1) { if (value) _b1 |= mask; else _b1 &= ~mask; }
        else if (word == 2) { if (value) _b2 |= mask; else _b2 &= ~mask; }
        else if (word == 3) { if (value) _b3 |= mask; else _b3 &= ~mask; }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Matches(Bitmask256 queryMask)
    {
        return (_b0 & queryMask._b0) == queryMask._b0 &&
               (_b1 & queryMask._b1) == queryMask._b1 &&
               (_b2 & queryMask._b2) == queryMask._b2 &&
               (_b3 & queryMask._b3) == queryMask._b3;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasAny(Bitmask256 interestMask)
    {
        return (_b0 & interestMask._b0) != 0 ||
               (_b1 & interestMask._b1) != 0 ||
               (_b2 & interestMask._b2) != 0 ||
               (_b3 & interestMask._b3) != 0;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsSet(int bitIndex)
    {
        int word = bitIndex >> 6;
        int bit = bitIndex & 63;
        ulong mask = 1UL << bit;

        return word switch
        {
            0 => (_b0 & mask) != 0,
            1 => (_b1 & mask) != 0,
            2 => (_b2 & mask) != 0,
            3 => (_b3 & mask) != 0,
            _ => false
        };
    }
    
    public void Clear() => _b0 = _b1 = _b2 = _b3 = 0;
}

public class ComponentManager(EcsEventBus eventBus)
{
    
    public const int MaxComponents = 256;
    
    private EcsEventBus _eventBus { get; } = eventBus;
    private EntityManager _entityManager = null!;
    private Dictionary<System.Type, object> _sparseSets = new();
    private Dictionary<System.Type, Action<int>> _removeActions = new();
    private Dictionary<Type, List<int>> _dirtyEntities = new();
    private Dictionary<System.Type, Action<int, object>> _cleanupActions = new();
    
    private Bitmask256[] _entityMasks = new Bitmask256[1024];
    
    public void Initialize(EntityManager entityManager)
    {
        _entityManager = entityManager;
    }
    
    public void RegisterComponentCleanup<T>(Action<int, T> cleanupAction) where T : struct, IComponent
    {
        var type = typeof(T);
        
        _cleanupActions[type] = (entityId, component) => cleanupAction(entityId, (T)component);
    }
    
    private void CallCleanupActions<T>(int entityId, T component) where T : IComponent
    {
        var type = typeof(T);
        if (_cleanupActions.TryGetValue(type, out var action))
        {
            action(entityId, component);
        }
    }
    
    public ref T AddComponent<T>(int entityId) where T : struct, IComponent
    {
        var set = GetOrCreateSparseSet<T>();
    
        ref var component = ref Unsafe.NullRef<T>();
        if (set.IsTag)
        {
            ((ECSTagsSet)set).Add(entityId);
        }
        else
        {
            component = ref ((ECSSparseSet<T>)set).Add(entityId, new T());
        }

        var bit = ComponentTypeIdRegistry.GetBit<T>();
        GetEntityComponentMask(entityId).Set(bit, true);
    
        MarkDirty<T>(entityId);
        return ref component;
    }
    
    public void AddComponent<T>(int entityId, T component) where T : struct, IComponent
    {
        var set = GetOrCreateSparseSet<T>();
    
        if (set.IsTag)
        {
            ((ECSTagsSet)set).Add(entityId);
        }
        else
        {
            ((ECSSparseSet<T>)set).Add(entityId, component);
        }

        var bit = ComponentTypeIdRegistry.GetBit<T>();
        GetEntityComponentMask(entityId).Set(bit, true);
    
        MarkDirty<T>(entityId);
    }
    
    public void RegisterEntityComponentBits(int entityId)
    {
        EnsureMaskCapacity(entityId);
        _entityMasks[entityId].Clear();
    }
    
    private void EnsureMaskCapacity(int entityId)
    {
        if (entityId >= _entityMasks.Length)
        {
            int newSize = _entityMasks.Length;
            while (newSize <= entityId) newSize *= 2;
            Array.Resize(ref _entityMasks, newSize);
        }
    }
    
    public ref Bitmask256 GetEntityComponentMask(int entityId)
    {
        if (entityId >= _entityMasks.Length) return ref Unsafe.NullRef<Bitmask256>();
        return ref _entityMasks[entityId];
    }

    private bool HasComponent(int entityId, params Type[] componentTypes)
    {
        if(componentTypes.Length == 0 || componentTypes.Length > MaxComponents) return false;
        if (entityId >= _entityMasks.Length || entityId < 0) return false;

        var checkMask = new Bitmask256();
        foreach (var type in componentTypes)
        {
            var bit = ComponentTypeIdRegistry.GetBit(type);
            checkMask.Set(bit, true);
        }
        
        return _entityMasks[entityId].Matches(checkMask);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasComponent<T>(int entityId) where T : IComponent
    {
        return HasComponent(entityId, typeof(T));
    }
    
    public bool HasComponent<T1, T2>(int entityId) where T1 : IComponent where T2 : IComponent
    {
        return HasComponent(entityId, typeof(T1), typeof(T2));
    }

    public bool HasComponent<T1, T2, T3>(int entityId) where T1 : IComponent where T2 : IComponent where T3 : IComponent
    {
        return HasComponent(entityId, typeof(T1), typeof(T2), typeof(T3));
    }
    
    public bool HasComponent<T1, T2, T3, T4>(int entityId) where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent
    {
        return HasComponent(entityId, typeof(T1), typeof(T2), typeof(T3), typeof(T4));
    }
    
    public bool HasComponent<T1, T2, T3, T4, T5>(int entityId) where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent where T5 : IComponent
    {
        return HasComponent(entityId, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
    }
    
    public bool HasComponent<T1, T2, T3, T4, T5, T6>(int entityId) where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent where T5 : IComponent where T6 : IComponent
    {
        return HasComponent(entityId, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
    }
    
    public bool HasComponent<T1, T2, T3, T4, T5, T6, T7>(int entityId) where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent where T5 : IComponent where T6 : IComponent where T7 : IComponent
    {
        return HasComponent(entityId, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7));
    }
    
    public bool HasComponent<T1, T2, T3, T4, T5, T6, T7, T8>(int entityId) where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent where T5 : IComponent where T6 : IComponent where T7 : IComponent where T8 : IComponent
    {
        return HasComponent(entityId, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8));
    }
    
    public bool HasComponent<T1, T2, T3, T4, T5, T6, T7, T8, T9>(int entityId) where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent where T5 : IComponent where T6 : IComponent where T7 : IComponent where T8 : IComponent where T9 : IComponent
    {
        return HasComponent(entityId, typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9));
    }

    public ref T GetComponent<T>(int entityId) where T : struct, IComponent
    {
        var set = GetOrCreateSparseSet<T>();
    
        if (!set.IsTag)
        {
            return ref ((ECSSparseSet<T>)set).Get(entityId);
        }
    
        return ref Unsafe.NullRef<T>();
    }

    public ISparseSet GetSet<T>() where T : struct, IComponent
    {
        return GetOrCreateSparseSet<T>();
    }
    
    public ECSSparseSet<T> GetCompSet<T>() where T : struct, IComponent
    {
        var set = GetOrCreateSparseSet<T>();
        if (set.IsTag)
            throw new InvalidOperationException($"Component type {typeof(T)} is a tag and does not have a data set.");
        return (ECSSparseSet<T>)set;
    }

    public void RemoveComponent<T>(int entityId) where T : struct, IComponent
    {
        var set = GetOrCreateSparseSet<T>();
    
        if (set.IsTag)
        {
            ((ECSTagsSet)set).Remove(entityId);
        }
        else
        {
            CallCleanupActions(entityId, ((ECSSparseSet<T>)set).Get(entityId));
            ((ECSSparseSet<T>)set).Remove(entityId);
        }

        var bit = ComponentTypeIdRegistry.GetBit<T>();
        GetEntityComponentMask(entityId).Set(bit, false);
        MarkDirty<T>(entityId);
    }

    public IEnumerable<(int entityId, T component)> GetAll<T>() where T : struct, IComponent
    {
        var set = GetOrCreateSparseSet<T>();
    
        if (set.IsTag)
        {
            return ((ECSTagsSet)set).ActiveElements().Select(id => (id, Unsafe.NullRef<T>()));
        }
        return ((ECSSparseSet<T>)set).ActiveElements();
    }
    
    public IEnumerable<int> GetDirtyEntities<T>() where T : IComponent
    {
        if (_dirtyEntities.TryGetValue(typeof(T), out var list))
            return list;
        return Array.Empty<int>();
    }
    
    public DirtyQueryView QueryDirty<T>() where T : IComponent
    {
        return QueryDirty(typeof(T));
    }
    
    public DirtyQueryView QueryDirty<T1, T2>()
        where T1 : IComponent
        where T2 : IComponent
    {
        return QueryDirty(typeof(T1), typeof(T2));
    }
    
    public DirtyQueryView QueryDirty<T1, T2, T3>()
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
    {
        return QueryDirty(typeof(T1), typeof(T2), typeof(T3));
    }
    
    public DirtyQueryView QueryDirty<T1, T2, T3, T4>()
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
        where T4 : IComponent
    {
        return QueryDirty(typeof(T1), typeof(T2), typeof(T3), typeof(T4));
    }
    
    public DirtyQueryView QueryDirty<T1, T2, T3, T4, T5>()
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
        where T4 : IComponent
        where T5 : IComponent
    {
        return QueryDirty(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
    }
    
    public DirtyQueryView QueryDirty<T1, T2, T3, T4, T5, T6>()
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
        where T4 : IComponent
        where T5 : IComponent
        where T6 : IComponent
    {
        return QueryDirty(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
    }
    
    public DirtyQueryView QueryDirty<T1, T2, T3, T4, T5, T6, T7>()
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
        where T4 : IComponent
        where T5 : IComponent
        where T6 : IComponent
        where T7 : IComponent
    {
        return QueryDirty(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7));
    }
    
    public DirtyQueryView QueryDirty<T1, T2, T3, T4, T5, T6, T7, T8>()
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
        where T4 : IComponent
        where T5 : IComponent
        where T6 : IComponent
        where T7 : IComponent
        where T8 : IComponent
    {
        return QueryDirty(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8));
    }
    
    public DirtyQueryView QueryDirty<T1, T2, T3, T4, T5, T6, T7, T8, T9>()
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
        where T4 : IComponent
        where T5 : IComponent
        where T6 : IComponent
        where T7 : IComponent
        where T8 : IComponent
        where T9 : IComponent
    {
        return QueryDirty(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9));
    }
    
    public DirtyQueryView QueryDirty(params System.Type[] componentTypes)
    {
        if (componentTypes == null || componentTypes.Length == 0)
            return new DirtyQueryView(ReadOnlySpan<int>.Empty, new Bitmask256(), this);

        var queryMask = GenerateQueryMask(componentTypes);

        if (_dirtyEntities.TryGetValue(componentTypes[0], out var dirtyList))
        {
            return new DirtyQueryView(CollectionsMarshal.AsSpan(dirtyList), queryMask, this);
        }

        return new DirtyQueryView(ReadOnlySpan<int>.Empty, queryMask, this);
    }
    
    public void ClearDirty<T>() where T : IComponent
    {
        var type = typeof(T);
        if (_dirtyEntities.TryGetValue(type, out var list))
        {
            list.Clear();
        }
    }
    
    private ISparseSet GetOrCreateSparseSet<T>(T _ = default) where T : struct, IComponent
    {
        var type = typeof(T);
        if (!_sparseSets.TryGetValue(type, out var set))
        {
            
            if (Unsafe.SizeOf<T>() <= 1 && type.GetFields().Length == 0)
            {
                set = new ECSTagsSet();
            }
            else
            {
                set = new ECSSparseSet<T>();
            }
            _sparseSets[type] = set;
        }
        return (ISparseSet)set;
    }
    
    public QueryView Query<T>() where T : IComponent
    {
        return Query(typeof(T));
    }
    
    public QueryView Query<T1, T2>()
        where T1 : IComponent
        where T2 : IComponent
    {
        return Query(typeof(T1), typeof(T2));
    }
    
    public QueryView Query<T1, T2, T3>()
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
    {
        return Query(typeof(T1), typeof(T2), typeof(T3));
    }
    
    public QueryView Query<T1, T2, T3, T4>()
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
        where T4 : IComponent
    {
        return Query(typeof(T1), typeof(T2), typeof(T3), typeof(T4));
    }
    
    public QueryView Query<T1, T2, T3, T4, T5>()
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
        where T4 : IComponent
        where T5 : IComponent
    {
        return Query(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
    }
    
    public QueryView Query<T1, T2, T3, T4, T5, T6>()
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
        where T4 : IComponent
        where T5 : IComponent
        where T6 : IComponent
    {
        return Query(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
    }
    
    public QueryView Query<T1, T2, T3, T4, T5, T6, T7>()
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
        where T4 : IComponent
        where T5 : IComponent
        where T6 : IComponent
        where T7 : IComponent
    {
        return Query(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7));
    }
    
    public QueryView Query<T1, T2, T3, T4, T5, T6, T7, T8>()
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
        where T4 : IComponent
        where T5 : IComponent
        where T6 : IComponent
        where T7 : IComponent
        where T8 : IComponent
    {
        return Query(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8));
    }
    
    public QueryView Query<T1, T2, T3, T4, T5, T6, T7, T8, T9>()
        where T1 : IComponent
        where T2 : IComponent
        where T3 : IComponent
        where T4 : IComponent
        where T5 : IComponent
        where T6 : IComponent
        where T7 : IComponent
        where T8 : IComponent
        where T9 : IComponent
    {
        return Query(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9));
    }
    
    private Bitmask256 GenerateQueryMask(params Type[] types)
    {
        var mask = new Bitmask256();
        foreach (var type in types)
        {
            var bit = ComponentTypeIdRegistry.GetBit(type);
            mask.Set(bit, true);
        }
        return mask;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsMatch(int entityId, Bitmask256 queryMask)
    {
        return _entityMasks[entityId].Matches(queryMask);
    }
    
    public QueryView Query(params System.Type[] componentTypes)
    {
        var queryMask = GenerateQueryMask(componentTypes);
        var smallestSet = GetSmallestSet(componentTypes);

        if (smallestSet == null)
            return new QueryView(ReadOnlySpan<int>.Empty, queryMask, this);
        
        return new QueryView(smallestSet.EntitiesSpan, queryMask, this);
    }
    
    private ISparseSet? GetSmallestSet(Type[] componentTypes)
    {
        ISparseSet? smallest = null;
        int minCount = int.MaxValue;

        foreach (var type in componentTypes)
        {
            if (_sparseSets.TryGetValue(type, out var obj) && obj is ISparseSet set)
            {
                if (set.Count < minCount)
                {
                    minCount = set.Count;
                    smallest = set;
                }
            }
            else return null;
        }
        return smallest;
    }

    public void MarkDirty<T>(int entityId) where T : IComponent
    {
        var type = typeof(T);
        if (!_dirtyEntities.TryGetValue(type, out var list))
        {
            list = new List<int>();
            _dirtyEntities[type] = list;
        }
        list.Add(entityId);
    }
    
    public void ClearDirty(int entityId)
    {
        if (entityId >= _entityMasks.Length || entityId < 0) return;
        
        var mask = GetEntityComponentMask(entityId);
        for (int i = 0; i < MaxComponents; i++)
        {
            if (mask.IsSet(i))
            {
                var type = ComponentTypeIdRegistry.GetType(i);
                if (type != null && _dirtyEntities.TryGetValue(type, out var list))
                {
                    list.Remove(entityId);
                }
            }
        }
    }
    
    // Called by EntityManager to remove all components of an entity
    public void RemoveAllComponents(int entityId)
    {
        var mask = GetEntityComponentMask(entityId);
        for (int i = 0; i < MaxComponents; i++)
        {
            if (mask.IsSet(i))
            {
                var type = ComponentTypeIdRegistry.GetType(i);
                if (type != null && _removeActions.TryGetValue(type, out var remove))
                    remove(entityId);
            }
        }
        
        if (entityId < _entityMasks.Length)
        {
            _entityMasks[entityId].Clear();
        }
    }
    
}