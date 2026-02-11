using RPGCreator.SDK.Assets.Definitions;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Types.Collections;

namespace RPGCreator.Core.Managers.AssetsManager;

/// <summary>
/// A scope for managing the lifecycle of transient assets.
/// When the scope is disposed, all tracked transient assets are destroyed.
/// </summary>
internal class AssetScope : IAssetScope
{

    private readonly ScopedLogger Logger = SDK.Logging.Logger.ForContext<AssetScope>();
    private readonly AssetsManager _manager;

    private readonly HashSet<Ulid> _borrowedAssets;
    private readonly HashSet<IBaseAssetDef> _scopes;
    
    public string Name { get; }
    
    public AssetScope(AssetsManager manager, string name)
    {
        Name = name;
        Logger.SetCustomPrefix(Name);
        _manager = manager;
        _scopes = new HashSet<IBaseAssetDef>();
        _borrowedAssets = new HashSet<Ulid>();
    }

    public void Track(IBaseAssetDef baseAsset)
    {
        _scopes.Add(baseAsset);
    }

    public void Untrack(IBaseAssetDef baseAsset)
    {
        _scopes.Remove(baseAsset);
    }
    
    public void TransferTo(IAssetScope targetScope, IBaseAssetDef baseAsset)
    {
        if (!_scopes.Contains(baseAsset)) return;
        Untrack(baseAsset);
        targetScope.Track(baseAsset);
        
        Logger.Debug("Transferred asset {0} from scope {1} to scope {2}", args:[baseAsset.Urn, Name, targetScope.Name]);
    }
    
    public void Dispose()
    {
        foreach (var asset in _scopes.ToList())
        {
            _manager.DestroyTransientAsset(asset);
        }
        _scopes.Clear();
        
        foreach (var id in _borrowedAssets)
        {
            _manager.ReleaseAsset(id);
        }
        _borrowedAssets.Clear();
        
        GC.SuppressFinalize(this);
    }

    public T Load<T>(Ulid assetId) where T : class, IBaseAssetDef
    {
        try
        {
            var asset = _manager.RetainAsset(assetId);
            _borrowedAssets.Add(assetId);

            if (asset is not T typedAsset)
                throw new InvalidCastException(
                    $"Asset with ID {assetId} is of type {asset.GetType().FullName}, cannot cast to {typeof(T).FullName}");

            return typedAsset;
        }
        catch (InvalidOperationException e)
        {
            Logger.Error(e, "Failed to load asset with ID {AssetId}", args: assetId);
            throw;
        }
        catch (InvalidCastException e)
        {
            Logger.Error(e, "Failed to cast asset with ID {AssetId} to type {AssetType}", args: [assetId, typeof(T)?.FullName ?? "null"]);
            throw;
        }
    }

    public BaseAssetDef? Load(Ulid assetId, out Type assetType)
    {
        try
        {
            var asset = _manager.RetainAsset(assetId);
            assetType = asset.GetType();
            
            if (!typeof(BaseAssetDef).IsAssignableFrom(assetType))
                return null;
            
            _borrowedAssets.Add(assetId);
            
            return asset as BaseAssetDef;
        }
        catch (InvalidOperationException e)
        {
            Logger.Error(e, "Failed to load asset with ID {AssetId}", args: assetId);
            assetType = typeof(object);
            return null;
        }
    }

    public void Unload<T>(T asset) where T : class, IBaseAssetDef
    {
        if (_borrowedAssets.Contains(asset.Unique))
        {
            _manager.ReleaseAsset(asset.Unique);
            _borrowedAssets.Remove(asset.Unique);
        }
    }
}