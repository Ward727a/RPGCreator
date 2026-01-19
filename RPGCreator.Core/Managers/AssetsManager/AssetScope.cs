using RPGCreator.Core.Types.Assets;
using RPGCreator.Core.Types.Internal;
using RPGCreator.SDK.Assets.Definitions;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Types.Collections;
using RPGCreator.SDK.Types.Interfaces;
using Serilog;

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
    private readonly HashSet<IAssetDef> _scopes;
    
    public string Name { get; }
    
    public AssetScope(AssetsManager manager, string name)
    {
        Name = name;
        Logger.SetCustomPrefix(Name);
        _manager = manager;
        _scopes = new HashSet<IAssetDef>();
        _borrowedAssets = new HashSet<Ulid>();
    }

    public void Track(IAssetDef asset)
    {
        _scopes.Add(asset);
    }

    public void Untrack(IAssetDef asset)
    {
        _scopes.Remove(asset);
    }
    
    public void TransferTo(IAssetScope targetScope, IAssetDef asset)
    {
        if (!_scopes.Contains(asset)) return;
        Untrack(asset);
        targetScope.Track(asset);
        
        Logger.Debug("Transferred asset {0} from scope {1} to scope {2}", args:[asset.Urn, Name, targetScope.Name]);
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

    public T Load<T>(Ulid assetId) where T : class, IAssetDef
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

    public IAssetDef? Load(Ulid assetId, out Type assetType)
    {
        try
        {
            var asset = _manager.RetainAsset(assetId);
            assetType = asset.GetType();
            
            if (!typeof(IAssetDef).IsAssignableFrom(assetType))
                return null;
            
            _borrowedAssets.Add(assetId);
            
            return asset as IAssetDef;
        }
        catch (InvalidOperationException e)
        {
            Logger.Error(e, "Failed to load asset with ID {AssetId}", args: assetId);
            assetType = typeof(object);
            return null;
        }
    }

    public void Unload<T>(T asset) where T : class, IAssetDef
    {
        if (_borrowedAssets.Contains(asset.Unique))
        {
            _manager.ReleaseAsset(asset.Unique);
            _borrowedAssets.Remove(asset.Unique);
        }
    }
}