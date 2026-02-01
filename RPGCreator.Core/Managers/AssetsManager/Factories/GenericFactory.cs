using System.Linq.Expressions;
using RPGCreator.SDK.Assets;
using RPGCreator.SDK.Types.Internals;
using Serilog;

namespace RPGCreator.Core.Managers.AssetsManager.Factories;

public sealed class GenericFactory<TInstance, TDef> : IAssetFactory<TInstance, TDef>
    where TDef : IHasUniqueId
    where TInstance : class
{
    private readonly Func<TDef, TInstance> _constructor;
    
    public GenericFactory()
    {
        // Check if TInstance has a constructor that accepts TDef and save it.
        var instanceConstructor = typeof(TInstance)
                                   .GetConstructor(new[] { typeof(TDef) })
                               ?? throw new InvalidOperationException(
                                   $"Type {typeof(TInstance).Name} does not have a constructor that accepts {typeof(TDef).Name}.");
        
        var param = Expression.Parameter(typeof(TDef), "def");
        var newExpression = Expression.New(instanceConstructor, param);
        _constructor = Expression.Lambda<Func<TDef, TInstance>>(newExpression, param).Compile();
    }
    
    public TInstance Create(TDef def)
    {
        if (def == null) throw new ArgumentNullException(nameof(def), "Definition cannot be null.");

        // Create a new instance of TInstance using the definition.
        // This assumes that TInstance has a constructor that accepts TDef.
        var instance = _constructor(def);
        
        return instance;
    }

    public ValueTask<TInstance> CreateAsync(TDef def, CancellationToken ct = default)
    {
        if (def == null) throw new ArgumentNullException(nameof(def), "Definition cannot be null.");

        // Create a new instance of TInstance using the definition.
        // This assumes that TInstance has a constructor that accepts TDef.
        var instance = _constructor(def);
        
        return new ValueTask<TInstance>(instance);
    }

    public void Refresh(TDef def)
    {
        Log.Warning("Refresh method can't be used with GenericFactory. This factory does not support refreshing instances.");
        Log.Warning("If you need to refresh instances, consider using a different factory (like GenericCachedFactory) that supports reloading or refreshing instances.");
        // This factory does not support refreshing instances.
    }

    public void Release(TInstance instance)
    {
        Log.Warning("Release (Instance) method can't be used with GenericFactory. This factory does not support releasing instances.");
        Log.Warning("If you need to release instances, consider using a different factory (like GenericCachedFactory) that supports releasing instances.");
        // This factory does not support releasing instances.
    }

    public void Release(TDef def)
    {
        Log.Warning("Release method can't be used with GenericFactory. This factory does not support releasing instances.");
        Log.Warning("If you need to release instances, consider using a different factory (like GenericCachedFactory) that supports releasing instances.");
        // This factory does not support releasing instances.
    }

    public void Clear()
    {
        Log.Warning("Clear method can't be used with GenericFactory. This factory does not maintain a cache of instances.");
        Log.Warning("If you need to clear instances, consider using a different factory (like GenericCachedFactory) that supports clearing cached instances.");
        // This factory does not maintain a cache of instances.
    }
}