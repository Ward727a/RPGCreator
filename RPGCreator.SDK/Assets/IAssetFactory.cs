
namespace RPGCreator.SDK.Assets;

public interface IAssetFactory<TInstance, in TDef>
{
    
    TInstance Create(TDef def);
    ValueTask<TInstance> CreateAsync(TDef def, CancellationToken ct = default);

    void Refresh(TDef def);
    void Release(TInstance instance);
    void Release(TDef def);
    void Clear();
    

}