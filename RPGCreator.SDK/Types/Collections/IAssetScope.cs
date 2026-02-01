using RPGCreator.SDK.Assets.Definitions;

namespace RPGCreator.SDK.Types.Collections;

public interface IAssetScope : IDisposable
{
    public string Name { get; }
    
    void Track(IAssetDef asset);
    void Untrack(IAssetDef asset);
    void TransferTo(IAssetScope targetScope, IAssetDef asset);
    /// <summary>
    /// Attempts to load an asset by its unique identifier.
    /// </summary>
    /// <param name="assetId">The unique identifier of the asset to load.</param>
    /// <typeparam name="T">The type of the asset to load, must implement IAssetDef.</typeparam>
    /// <returns>The loaded asset of type T if found; otherwise, null.</returns>
    /// <exception cref="InvalidCastException">Thrown if the loaded asset cannot be cast to type T.</exception>
    T Load<T>(Ulid assetId) where T : class, IAssetDef;
    IAssetDef? Load(Ulid assetId, out Type assetType);
    void Unload<T>(T asset) where T : class, IAssetDef;
}