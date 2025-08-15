using RPGCreator.Core.Type.Assets.Tilesets;
using RPGCreator.Core.Type.Internal;
using Serilog;

namespace RPGCreator.Core.Managers.AssetsManager.Factories;

public class TilesetFactory : IAssetFactory<ITilesetInstance, ITilesetDef>
{
    private readonly Dictionary<Ulid, ITilesetInstance> _instances = [];
    
    public TAwaitedTileset Create<TAwaitedTileset>(ITilesetDef def) where TAwaitedTileset : ITilesetInstance
    {
        ITilesetInstance instance;
        if (_instances.TryGetValue(def.Unique, out instance))
        {
            if(instance is TAwaitedTileset awaitedInstance)
                return awaitedInstance;
            throw new InvalidCastException($"Instance with unique ID {def.Unique} is not of type {typeof(TAwaitedTileset).Name}.");
        }

        switch(def)
        {
            case TilesetDef tilesetDef:
            {
                instance = CreateTilesetInstance(tilesetDef);
                if (instance is TAwaitedTileset awaitedInstance)
                    return awaitedInstance;
                throw new InvalidCastException($"Instance with unique ID {def.Unique} is not of type {typeof(TAwaitedTileset).Name}.");
            }
            case AutoTilesetDef tilesetDef:
            {
                instance = CreateAutoTilesetInstance(tilesetDef);
                if (instance is TAwaitedTileset awaitedInstance)
                    return awaitedInstance;
                throw new InvalidCastException(
                    $"Instance with unique ID {def.Unique} is not of type {typeof(TAwaitedTileset).Name}.");
            }
            default:
            {
                throw new ArgumentException($"Unsupported TilesetDef type: {def.GetType().Name}");
            }
        };
    }
    
    public ITilesetInstance Create(ITilesetDef def)
    {
        if (_instances.TryGetValue(def.Unique, out var instance))
        {
            return instance;
        }

        return def switch
        {
            TilesetDef tilesetDef => CreateTilesetInstance(tilesetDef),
            AutoTilesetDef autotilesetDef => CreateAutoTilesetInstance(autotilesetDef),
            _ => throw new ArgumentException($"Unsupported TilesetDef type: {def.GetType().Name}")
        };
    }

    private TilesetInstance CreateTilesetInstance(TilesetDef tilesetDef)
    {
        var instance = new TilesetInstance(tilesetDef);
        _instances[tilesetDef.Unique] = instance;
        return instance;
    }

    private AutoTilesetInstance CreateAutoTilesetInstance(AutoTilesetDef autotilesetDef)
    {
        var instance = new AutoTilesetInstance(autotilesetDef);
        _instances[autotilesetDef.Unique] = instance;
        return instance;
    }
    
    public ValueTask<ITilesetInstance> CreateAsync(ITilesetDef def, CancellationToken ct = default)
    {
        if (_instances.TryGetValue(def.Unique, out var instance))
        {
            return new ValueTask<ITilesetInstance>(instance);
        }

        return new ValueTask<ITilesetInstance>(Create(def));
    }

    public void Refresh(ITilesetDef def)
    {
        if (_instances.TryGetValue(def.Unique, out var instance))
        {
            if (instance is IReloadable<ITilesetDef> reloadableInstance)
            {
                reloadableInstance.Reload(def);
            }
            else
            {
                Log.Warning("Tileset instance with unique ID {Unique} does not support reloading.", def.Unique);
                Log.Warning("If this is intended, then you can safely ignore this warning.");
                
                // update the cache with the new definition
                Release(def); // Remove the old instance
                Create(def); // Create a new instance with the updated definition
            }
            
        }
        else
        {
            throw new KeyNotFoundException($"Tileset instance with unique ID {def.Unique} not found.");
        }
    }

    public void Release(ITilesetDef def)
    {
        if (_instances.ContainsKey(def.Unique))
        {
            _instances.Remove(def.Unique);
        }
        else
        {
            throw new KeyNotFoundException($"Tileset instance with unique ID {def.Unique} not found.");
        }
    }

    public void Clear()
    {
        _instances.Clear();
    }
}