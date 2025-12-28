using RPGCreator.Core.Types.Assets;
using RPGCreator.Core.Types.Internal;
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

    private readonly AssetsManager _manager;

    private readonly HashSet<Ulid> _borrowedAssets;
    private readonly HashSet<IAssetDef> _scopes;
    
    public string Name { get; }
    
    public AssetScope(AssetsManager manager, string name)
    {
        Name = name;
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
        
        Logger.Debug("Transferred asset {0} from scope {1} to scope {2}", asset.Urn, Name, targetScope.Name);
    }
    
    public void Dispose()
    {
        GC.SuppressFinalize(this);
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
    }

    public T Load<T>(Ulid assetId) where T : class, IAssetDef
    {
        var asset = _manager.RetainAsset(assetId);
        
        if (!_borrowedAssets.Contains(assetId))
        {
            _borrowedAssets.Add(assetId);
        }
        return (T)asset;
    }
}