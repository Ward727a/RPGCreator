using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Types;
using RPGCreator.SDK.Types.Collections;
using RPGCreator.SDK.Types.Internals;

namespace RPGCreator.SDK.Assets;

public abstract class RegistryBase <T> : IAssetRegistry<T> where T : class, IHasUniqueId
{
    protected readonly Dictionary<Ulid, T> _assets = new();
    private readonly Dictionary<Ulid, int> _refCounts = new();
    protected readonly Dictionary<URN, Ulid> _uniqueIds = new();
    
    public System.Type ManagedType => typeof(T);
    public abstract string ModuleName { get; }

    public virtual IEnumerable<System.Type> SupportedTypes
    {
        get
        {
            yield return typeof(T);
        }
    }

    public bool HasAsset(IHasUniqueId asset)
    {
        if (asset is T typedAsset)
        {
            return Contains(typedAsset.Unique);
        }
        return false;
    }

    public bool HasAsset(Ulid unique)
    {
        return Contains(unique);
    }


    public void RegisterUntyped(IHasUniqueId asset, bool overwrite = false)
    {
        if(asset is T typedAsset)
        {
            Register(typedAsset, overwrite);
        }
        else
        {
            Logger.Error("Registry {ModuleName} cannot register asset of type {AssetType}", ModuleName, asset.GetType().FullName);
        }
    }

    public void UnregisterUntyped(IHasUniqueId asset)
    {
        if(asset is T typedAsset)
        {
            Unregister(typedAsset);
        }
        else
        {
            Logger.Error("Registry {ModuleName} cannot unregister asset of type {AssetType}", ModuleName, asset.GetType().FullName);
        }
    }

    public bool TryResolveUrnUntyped(URN urn, out IHasUniqueId? asset)
    {
        asset = null;
        if (_uniqueIds.TryGetValue(urn, out var unique))
        {
            return TryGetUntyped(unique, out asset);
        }
        return false;
    }

    public bool TryGetUntyped(Ulid unique, out IHasUniqueId? asset)
    {
        if (_assets.TryGetValue(unique, out var typedAsset))
        {
            asset = typedAsset;
            return true;
        }
        asset = null;
        return false;
    }

    public bool TryRetainUntyped(Ulid id, out object? asset)
    {
        asset = null;
        if (_assets.TryGetValue(id, out var typedAsset))
        {
            asset = typedAsset;
            if (_refCounts.ContainsKey(id))
            {
                _refCounts[id]++;
            }
            else
            {
                _refCounts[id] = 1;
            }
            return true;
        }
        return false;
    }
    
    public void ReleaseUntyped(Ulid id)
    {
        if (!_refCounts.ContainsKey(id))
        {
            Logger.Warning("[{ModuleName}] Attempted to release asset {Id} which is not tracked.", ModuleName, id);
            return;
        }

        _refCounts[id]--;
        int newCount = _refCounts[id];
    
        Logger.Debug("[{ModuleName}] Released {Id}. New Count: {Count}", ModuleName, id, newCount);

        if (newCount <= 0)
        {
            if (_assets.TryGetValue(id, out var asset))
            {
                if (asset is IDisposable disposable)
                {
                    disposable.Dispose();
                    Logger.Debug("[{ModuleName}] Disposed asset {Id}", ModuleName, id);
                }

                Unregister(asset);
            }

            _refCounts.Remove(id);
        
            Logger.Info("[{ModuleName}] Asset {Id} unloaded form RAM.", ModuleName, id);
        }
    }

    public event EventHandler<T>? AssetRegistered;
    public event EventHandler<T>? AssetUnregistered;
    public virtual void Register(T asset, bool overwrite = false)
    {
        if (_assets.TryGetValue(asset.Unique, out var existingAsset))
        {
            if (overwrite)
            {
                Unregister(existingAsset);
            }
            else
            {
                Logger.Error("Asset with unique ID {assetUnique} already exists in the {ModuleName} registry.", asset.Unique, ModuleName);
                return;
            }
        }

        if (_uniqueIds.ContainsKey(asset.Urn))
        {
            Logger.Error("Asset with URN {assetUrn} already exists in the {ModuleName} registry.", asset.Urn, ModuleName);
            return;
        }

        _assets[asset.Unique] = asset;
        _uniqueIds[asset.Urn] = asset.Unique;
        
        _refCounts[asset.Unique] = 1; 

        AssetRegistered?.Invoke(this, asset);
    }

    public virtual void Unregister(T asset)
    {
        if (_assets.Remove(asset.Unique))
        {
            _uniqueIds.Remove(asset.Urn);
            _refCounts.Remove(asset.Unique);
            AssetUnregistered?.Invoke(this, asset);
        }
    }

    public T? Get(Ulid unique)
    {
        return _assets.GetValueOrDefault(unique);
    }

    public T? GetUrn(URN urn)
    {
        return _uniqueIds.TryGetValue(urn, out var unique) ? Get(unique) : null;
    }

    public bool TryGet(Ulid unique, out T? asset)
    {
        return _assets.TryGetValue(unique, out asset);
    }

    public bool TryGetUrn(URN urn, out T? asset)
    {
        asset = null;
        if (_uniqueIds.TryGetValue(urn, out var unique))
        {
            return TryGet(unique, out asset);
        }
        return false;
    }

    public bool Contains(Ulid unique)
    {
        return _assets.ContainsKey(unique);
    }

    public bool ContainsUrn(URN urn)
    {
        return _uniqueIds.ContainsKey(urn);
    }

    public IEnumerable<T> All()
    {
        return _assets.Values;
    }

    public void Clear()
    {
        _assets.Clear();
        _uniqueIds.Clear();
    }
}