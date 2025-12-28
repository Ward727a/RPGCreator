using RPGCreator.SDK.Types.Interfaces;

namespace RPGCreator.SDK.Types.Collections;

public interface IAssetScope : IDisposable
{
    public string Name { get; }
    
    void Track(IAssetDef asset);
    void Untrack(IAssetDef asset);
    void TransferTo(IAssetScope targetScope, IAssetDef asset);
    T Load<T>(Ulid assetId) where T : class, IAssetDef;
}