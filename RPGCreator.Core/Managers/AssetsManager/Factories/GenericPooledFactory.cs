using System.Linq.Expressions;
using RPGCreator.Core.Types.Internal;
using RPGCreator.SDK.Assets;
using RPGCreator.SDK.Types.Internals;
using Serilog;

namespace RPGCreator.Core.Managers.AssetsManager.Factories;

/// <summary>
/// A pooled factory is a factory that reuses instances from a pool to minimize allocations.<br/>
/// It creates new instances only when the pool is empty, otherwise it rents instances from the pool.<br/>
/// This is useful for types that are frequently created and destroyed, as it reduces the overhead of garbage collection.
/// </summary>
public class GenericPooledFactory<TInstance, TDef> : IAssetFactory<TInstance, TDef>
    where TInstance : class, IResettable<TDef>, ICleanable
    where TDef : class
{
    protected readonly Func<TDef, TInstance> Constructor;
    protected readonly ObjectPool<TInstance> Pool;

    public GenericPooledFactory(int maxPool = 1024)
    {
        // Check if TInstance has a constructor that accepts TDef and save it.
        var instanceConstructor = typeof(TInstance)
                                      .GetConstructor(new[] { typeof(TDef) })
                                  ?? throw new InvalidOperationException(
                                      $"Type {typeof(TInstance).Name} does not have a constructor that accepts {typeof(TDef).Name}.");
        var param = Expression.Parameter(typeof(TDef), "def");
        var newExpression = Expression.New(instanceConstructor, param);
        Constructor = Expression.Lambda<Func<TDef, TInstance>>(newExpression, param).Compile();
        Pool = new ObjectPool<TInstance>(null, maxPool);
    }
    
    public virtual TInstance Create(TDef def)
    {
        TInstance instance;
        
        if (Pool.Count > 0)
        {
            // Rent an instance from the pool.
            instance = Pool.Rent();
            instance.ResetFrom(def);
        }
        else
        {
            // Create a new instance if the pool is empty.
            instance = Constructor(def);
        }
        return instance;
    }

    public virtual ValueTask<TInstance> CreateAsync(TDef def, CancellationToken ct = default)
    {
        // Create a new instance if the pool is empty.
        return new ValueTask<TInstance>(Create(def));
    }

    public virtual void Release(TInstance instance)
    {
        if (instance == null) throw new ArgumentNullException(nameof(instance));

        // Return the instance to the pool for reuse.
        Pool.Return(instance);
    }
    
    public void Refresh(TDef def)
    {
        Log.Warning("Refresh method can't be used with GenericPooledFactory. This factory does not support refreshing instances.");
        Log.Warning("If you need to refresh instances, consider using a different factory (like GenericCachedFactory) that supports reloading or refreshing instances.");
        // This factory does not support refreshing instances
    }

    public void Release(TDef def)
    {
        Log.Warning("Release(TDef) method can't be used with GenericPooledFactory. This factory does not support releasing instances by definition.");
        Log.Warning("If you need to release instances, use Release(TInstance) instead.");
        // This factory does not support releasing instances by definition.
    }

    public virtual void Clear()
    {
        Pool.Clear();
    }
}