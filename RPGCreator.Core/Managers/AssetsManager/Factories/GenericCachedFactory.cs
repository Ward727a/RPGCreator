using System.Linq.Expressions;
using RPGCreator.Core.Types.Internal;
using RPGCreator.SDK.Assets;
using RPGCreator.SDK.Types.Internals;
using Serilog;

namespace RPGCreator.Core.Managers.AssetsManager.Factories;

/// <summary>
/// A simple factory for creating instances of a specific type based on a definition.
/// This factory does not handle any complex logic or dependencies, and is intended for straightforward instantiation.
/// </summary>
/// <typeparam name="TInstance"></typeparam>
/// <typeparam name="TDef"></typeparam>
public sealed class GenericCachedFactory<TInstance, TDef> : IAssetFactory<TInstance, TDef>
    where TDef : IHasUniqueId
{
    private readonly Func<TDef, TInstance> _constructor;
    
    private readonly Dictionary<Ulid, TInstance> _instances = new();

    public GenericCachedFactory()
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
    
    /// <summary>
    /// Create or retrieved an instance of TInstance based on the provided definition TDef.<br/>
    /// The instance will be created only if it does not already exist in the cache.<br/>
    /// If an instance with the same unique ID already exists, it will be returned from the cache.<br/>
    /// </summary>
    /// <param name="def">Definition of the instance to create.</param>
    /// <returns></returns>
    public TInstance Create(TDef def)
    {
        if (_instances.TryGetValue(def.Unique, out var instance))
        {
            return instance;
        }

        // Create a new instance of TInstance using the definition.
        // This assumes that TInstance has a constructor that accepts TDef.
        instance = _constructor(def);
        
        _instances[def.Unique] = instance;
        return instance;
    }

    public ValueTask<TInstance> CreateAsync(TDef def, CancellationToken ct = default)
    {
        if (_instances.TryGetValue(def.Unique, out var instance))
        {
            return new ValueTask<TInstance>(instance);
        }
        
        return new ValueTask<TInstance>(Create(def));
    }

    public void Refresh(TDef def)
    {
        if (_instances.TryGetValue(def.Unique, out var instance))
        {
            if (instance is IReloadable<TDef> reloadableInstance)
            {
                reloadableInstance.Reload(def);
            }
            else
            {
                Log.Warning("Instance with unique ID {Unique} does not support reloading.", def.Unique);
                Log.Warning("If this is intended, then you can safely ignore this warning.");
                // Update the cache with the new definition
                Release(def);
                Create(def); // Recreate the instance with the updated definition
            }
        }
        else
        {
            throw new KeyNotFoundException($"No instance found for definition with unique ID {def.Unique}.");
        }
    }

    public void Release(TInstance instance)
    {
        var entry = _instances.FirstOrDefault(kvp => EqualityComparer<TInstance>.Default.Equals(kvp.Value, instance));
        if (!EqualityComparer<KeyValuePair<Ulid, TInstance>>.Default.Equals(entry, default))
        {
            switch (entry.Value)
            {
                // If the instance implements IDisposable, dispose it before removing it.
                case IDisposable disposableInstance:
                    disposableInstance.Dispose();
                    break;
                // If the instance implements IAsyncDisposable, dispose it asynchronously.
                case IAsyncDisposable asyncDisposableInstance:
                    asyncDisposableInstance.DisposeAsync().AsTask().Wait();
                    break;
            }

            _instances.Remove(entry.Key);
        }
        else
        {
            throw new KeyNotFoundException("The provided instance was not found in the cache.");
        }
    }

    public void Release(TDef def)
    {
        if (_instances.ContainsKey(def.Unique))
        {
            switch (_instances[def.Unique])
            {
                // If the instance implements IDisposable, dispose it before removing it.
                case IDisposable disposableInstance:
                    disposableInstance.Dispose();
                    break;
                // If the instance implements IAsyncDisposable, dispose it asynchronously.
                case IAsyncDisposable asyncDisposableInstance:
                    asyncDisposableInstance.DisposeAsync().AsTask().Wait();
                    break;
            }

            _instances.Remove(def.Unique);
        }
        else
        {
            throw new KeyNotFoundException($"No instance found for definition with unique ID {def.Unique}.");
        }
    }

    public void Clear()
    {
        _instances.Clear();
    }
}