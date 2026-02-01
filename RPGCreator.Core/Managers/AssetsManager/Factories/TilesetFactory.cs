using RPGCreator.Core.Types.Assets.Tilesets;
using RPGCreator.Core.Types.Internal;
using RPGCreator.SDK.Assets;
using RPGCreator.SDK.Assets.Definitions.Tilesets;
using Serilog;

namespace RPGCreator.Core.Managers.AssetsManager.Factories;

public class TilesetFactory : IAssetFactory<TilesetInstance, TilesetDef>
{
    private readonly Dictionary<Ulid, TilesetInstance> _instances = [];

    public TilesetInstance Create(TilesetDef def)
    {
        if (_instances.TryGetValue(def.Unique, out var instance))
        {
            return instance;
        }
        instance = new TilesetInstance(def);
        _instances[def.Unique] = instance;
        return instance;
    }

    public ValueTask<TilesetInstance> CreateAsync(TilesetDef def, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public void Refresh(TilesetDef def)
    {
        if (_instances.TryGetValue(def.Unique, out var instance))
        {
            if (instance is IReloadable<BaseTilesetDef> reloadableInstance)
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

    public void Release(TilesetInstance instance)
    {
        Release((TilesetDef)instance.Definition);
    }

    public void Release(TilesetDef def)
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