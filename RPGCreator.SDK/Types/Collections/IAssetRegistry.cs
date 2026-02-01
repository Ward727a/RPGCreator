using RPGCreator.SDK.Types.Internals;

namespace RPGCreator.SDK.Types.Collections;

public interface IAssetRegistry
{
    System.Type ManagedType { get; }
    string ModuleName { get; }
    
    IEnumerable<System.Type> SupportedTypes { get; }
    
    bool HasAsset(IHasUniqueId asset);
    bool HasAsset(Ulid unique);
    
    void RegisterUntyped(IHasUniqueId asset, bool overwrite = false);
    void UnregisterUntyped(IHasUniqueId asset);
    
    bool TryResolveUrnUntyped(URN urn, out IHasUniqueId? asset);
    bool TryGetUntyped(Ulid unique, out IHasUniqueId? asset);
    
    bool TryRetainUntyped(Ulid id, out object? asset);
    void ReleaseUntyped(Ulid id);
}

/// <summary>
/// Defines a registry for assets.<br/>
/// Be it a registry for actors, items, or any other type of asset, this interface is used to define the basic structure of an asset registry.<br/>
/// ALL REGISTRIES WITHOUT EXCEPTIONS MUST IMPLEMENT THIS INTERFACE.<br/>
/// This is used to ensure that all registries have a common structure and can be used interchangeably in the system.
/// </summary>
public interface IAssetRegistry<TDef> : IAssetRegistry where TDef : IHasUniqueId
{
    
    event EventHandler<TDef>? AssetRegistered;
    event EventHandler<TDef>? AssetUnregistered;
    
    /// <summary>
    /// Register an asset in the registry.<br/>
    /// This method is used to add an asset to the registry, ensuring that it is unique and does not already exist in the registry.
    /// </summary>
    /// <param name="asset">The asset to register.</param>
    public void Register(TDef asset, bool overwrite = false);
    /// <summary>
    /// Unregister an asset from the registry.<br/>
    /// This method is used to remove an asset from the registry, ensuring that it is no longer tracked by the system.
    /// </summary>
    /// <param name="asset">The asset to unregister.</param>
    public void Unregister(TDef asset);
    /// <summary>
    /// Get an asset by its unique identifier.<br/>
    /// This method is used to retrieve an asset from the registry using its unique identifier (Ulid).<br/>
    /// If you need to retrieve an asset by its URN, use <see cref="GetUrn"/> instead.
    /// </summary>
    /// <param name="unique">Unique identifier of the asset to retrieve.</param>
    /// <returns>
    /// The asset if found, otherwise null.
    /// </returns>
    public TDef? Get(Ulid unique);
    /// <summary>
    /// Get an asset by its URN.<br/>
    /// This method is used to retrieve an asset from the registry using its URN (Uniform Resource Name).<br/>
    /// If you need to retrieve an asset by its unique identifier, use <see cref="Get"/> instead.
    /// </summary>
    /// <param name="urn">URN of the asset to retrieve.</param>
    /// <returns>
    /// The asset if found, otherwise null.
    /// </returns>
    public TDef? GetUrn(URN urn);
    /// <summary>
    /// Try to get an asset by its unique identifier.<br/>
    /// This method is used to attempt to retrieve an asset from the registry using its unique identifier (Ulid).<br/>
    /// If you need to retrieve an asset by its URN, use <see cref="TryGetUrn"/> instead.
    /// </summary>
    /// <param name="unique">Unique identifier of the asset to retrieve.</param>
    /// <param name="asset">When this method returns, contains the asset if found; otherwise, null.</param>
    /// <returns>
    /// Returns true if the asset was found; otherwise, false.
    /// </returns>
    public bool TryGet(Ulid unique, out TDef? asset);
    /// <summary>
    /// Try to get an asset by its URN.<br/>
    /// This method is used to attempt to retrieve an asset from the registry using its URN (Uniform Resource Name).<br/>
    /// If you need to retrieve an asset by its unique identifier, use <see cref="TryGet"/> instead.
    /// </summary>
    /// <param name="urn">URN of the asset to retrieve.</param>
    /// <param name="asset">When this method returns, contains the asset if found; otherwise, null.</param>
    /// <returns>
    /// Returns true if the asset was found; otherwise, false.
    /// </returns>
    public bool TryGetUrn(URN urn, out TDef? asset);
    /// <summary>
    /// Check if the registry contains an asset with the specified unique identifier.<br/>
    /// This method is used to determine if an asset exists in the registry based on its unique identifier (Ulid).<br/>
    /// If you need to check if an asset exists by its URN, use <see cref="ContainsUrn"/> instead.
    /// </summary>
    /// <param name="unique">The unique identifier of the asset to check.</param>
    /// <returns>
    /// Returns true if the asset exists in the registry; otherwise, false.
    /// </returns>
    public bool Contains(Ulid unique);
    /// <summary>
    /// Check if the registry contains an asset with the specified URN.<br/>
    /// This method is used to determine if an asset exists in the registry based on its URN (Uniform Resource Name).<br/>
    /// If you need to check if an asset exists by its unique identifier, use <see cref="Contains"/> instead.
    /// </summary>
    /// <param name="urn">URN of the asset to check.</param>
    /// <returns>
    /// Returns true if the asset exists in the registry; otherwise, false.
    /// </returns>
    public bool ContainsUrn(URN urn);
    /// <summary>
    /// Returns all assets in the registry.<br/>
    /// This method is used to retrieve all assets currently registered in the registry.
    /// </summary>
    /// <returns>
    /// An enumerable collection of all assets in the registry.
    /// </returns>
    public IEnumerable<TDef> All();
}