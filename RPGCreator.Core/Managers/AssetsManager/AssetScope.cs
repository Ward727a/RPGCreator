using RPGCreator.Core.Type.Assets;
using RPGCreator.Core.Type.Internal;
using Serilog;

namespace RPGCreator.Core.Managers.AssetsManager;

/// <summary>
/// A scope for managing the lifecycle of transient assets.
/// When the scope is disposed, all tracked transient assets are destroyed.
/// </summary>
public class AssetScope : IDisposable
{

    private readonly AssetsManager _manager;
    private readonly HashSet<IAssetDef> _scopes;
    
    public string Name { get; }
    
    public AssetScope(AssetsManager manager, string name)
    {
        Name = name;
        _manager = manager;
        _scopes = new HashSet<IAssetDef>();
    }

    internal void Track(IAssetDef asset)
    {
        _scopes.Add(asset);
    }
    
    internal void Untrack(IAssetDef asset)
    {
        _scopes.Remove(asset);
    }
    
    public void TransferTo(AssetScope targetScope, IAssetDef asset)
    {
        if (!_scopes.Contains(asset)) return;
        Untrack(asset);
        targetScope.Track(asset);
        
        Log.Debug("Transferred asset {0} from scope {1} to scope {2}", asset.Urn, Name, targetScope.Name);
    }
    
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        foreach (var asset in _scopes.ToList())
        {
            _manager.DestroyTransientAsset(asset);
        }
        _scopes.Clear();
    }
}