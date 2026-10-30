// RPG Creator - Open-source RPG Engine.
// (c) 2026 Ward
// 
// This file is part of RPG Creator and is distributed under the Apache 2.0 License.
// You are free to use, modify, and distribute this file under the terms of the Apache 2.0 License.
// See LICENSE for details.
// 
// ---
// 
// Ce fichier fait partie de RPG Creator et est distribué sous licence Apache 2.0.
// Vous êtes libre de l'utiliser, de le modifier et de le distribuer sous les termes de la licence Apache 2.0.
// Voir LICENSE pour plus de détails.
// 
// Contact:
// => Mail: Ward727a@gmail.com
//    Please use this object: "RPG Creator [YourObject]"
// => Discord: ward727
// 
// For urgent inquiries, sending both an email and a message on Discord is highly recommended for a quicker response.

using RPGCreator.SDK;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Services.Assets;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Types;
using SDL3;

namespace RPGCreator.EngineLib.Managers.Assets;

[EngineService(typeof(IAssetManager))]
public class AssetManager(
    IAssetCache cache,
    IAssetIO io,
    IAssetRouter router,
    IAssetRegistry registry,
    IAssetDependencyManager dependencyManager,
    AssetGenericProvider assetGenericProvider)
    : IAssetManager
{
    
    private readonly Dictionary<Type, ushort> _assetVersions = new();
    
    public Result<TAsset> GetAsset<TAsset>(Ulid assetId) where TAsset : class, IAssetDto
    {
        if (cache.Has(assetId))
        {
            return cache.Get<TAsset>(assetId);
        }

        var assetDataResult = io.Load(assetId);
        if (assetDataResult.IsFailure)
        {
            return assetDataResult.Cast<TAsset>();
        }

        var assetData = assetDataResult.Value;

        var providerResult = router.GetProvider<TAsset>();
        if (providerResult.IsFailure)
        {
            var fromGenericProvider = assetGenericProvider.Get<TAsset>(assetData);
            
            if(fromGenericProvider.IsFailure)
                return providerResult.Cast<TAsset>();

            var asset = fromGenericProvider.Value;
            cache.Add(asset);
            return Result<TAsset>.Success(asset);
        }
        

        var provider = providerResult.Value;

        return provider.Get(assetData)
            .OnSuccess(asset => { cache.Add(asset); });
    }

    public Result<IAssetDto> GetAsset(Type assetType, Ulid assetId)
    {
        if (cache.Has(assetId))
        {
            return cache.Get(assetId);
        }

        var assetDataResult = io.Load(assetId);
        if (assetDataResult.IsFailure)
        {
            return assetDataResult.Cast<IAssetDto>();
        }

        var assetData = assetDataResult.Value;

        var providerResult = router.GetProvider(assetType);
        if (providerResult.IsFailure)
        {
            var fromGenericProvider = assetGenericProvider.Get(assetType, assetData);
            
            if(fromGenericProvider.IsFailure)
                return providerResult.Cast<IAssetDto>();

            var asset = fromGenericProvider.Value;
            cache.Add(asset);
            return Result<IAssetDto>.Success(asset);
        }

        var provider = providerResult.Value;

        return provider.GetRaw(assetType, assetData)
            .OnSuccess(asset => { cache.Add(asset); });
    }
    
    public bool ClearAssetFromCache(Ulid assetId)
    {
        return cache.Remove(assetId);
    }

    public bool HasAsset(Ulid assetId)
    {
        return cache.Has(assetId) || registry.HasMetadata(assetId);
    }

    public Result DeleteAsset(Ulid assetId, bool force = false)
    {
        if (dependencyManager.GetDependents(assetId).Count > 0)
        {
            if(!force)
                return Result.Fail("Asset has dependents");
            
            // If you got this warning and then got errors or crash,
            // Good luck! You shouldn't have forced the deletion of this asset.
            Logger.Warning("Asset {assetId} has {number} dependencies, but 'force' is enabled!", assetId, dependencyManager.GetDependents(assetId).Count);
            Logger.Warning("This may cause issues with other assets that depend on it.");
            
            // Temporary solution. This will be replaced with a proper dialog and error handling later on.
            // TODO: Replace with a proper dialog and error handling.
            SDL.SDL_ShowSimpleMessageBox(SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR, "Warning", 
                $"Asset {assetId} has {dependencyManager.GetDependents(assetId).Count} dependencies, but 'force' is enabled!\n" +
                $"This may, and will surely, cause issues with other assets that depend on it!!!\n" +
                $"You still have the time to STOP THE ENGINE from your process manager!!", IntPtr.Zero);
        }


        var assetType = registry.GetMetadata(assetId).Match(asset => asset.AssetType, err => null);
        
        
        
        io.Delete(assetId);
        cache.Remove(assetId);
        registry.UnregisterAssetIndex(assetId);
        dependencyManager.ClearAssetDependencies(assetId);
        
        if(assetType != null)
            UpdateVersion(assetType);
        
        return Result.Success();
    }

    public Result SaveAsset<TAsset>(TAsset asset) where TAsset : class, IAssetDto
    {
        dependencyManager.AutoFillDependencies(asset);
        
        // We first clear the dependencies to remove any old ones that were deleted before.
        dependencyManager.ClearAssetDependencies(asset.AssetId);
        dependencyManager.RegisterAssetDependencies(asset.AssetId, asset.Dependencies);
        
        return io.Save(asset)
            .OnSuccess(() =>
            {
                registry.GetMetadata(asset.GetType()).OnSuccess(meta =>
                {
                    
                    if (cache.Has(asset.AssetId))
                        cache.Update(asset);
                    else
                        cache.Add(asset);

                    registry.RegisterAssetIndex(asset.AssetId, meta);
                    UpdateVersion<TAsset>();
                });
            });
    }

    public Result RefreshAssets()
    {
        cache.Clear();
        registry.Clear(clearMetadata: false);
        _assetVersions.Clear();

        var listMetadata = registry.GetAllMetadata();
        var scannedAssets = io.ScanForAssets(listMetadata);
        
        foreach (var (assetId, metadata) in scannedAssets)
        {
            registry.RegisterAssetIndex(assetId, metadata);
            
            
            var header = io.GetHeader(io.Load(assetId));
            
            if(header is null)
                continue;
            
            dependencyManager.LoadHeader(header);
        }

        return Result.Ok();
    }

    public ushort GetVersion(Type assetType)
    {
        if(_assetVersions.TryGetValue(assetType, out var version))
            return version;
        
        _assetVersions[assetType] = 1;
        return 1;
    }

    public IEnumerable<Ulid> ListAssets<TAsset>() where TAsset : class, IAssetDto
    {
        return registry.GetAllAssetIds(typeof(TAsset));
    }

    public IEnumerable<Ulid> ListAssets(Type assetType)
    {
        return registry.GetAllAssetIds(assetType);
    }

    private void UpdateVersion(Type assetType)
    {
        var version = _assetVersions.GetValueOrDefault(assetType, (ushort)1);

        if (version == ushort.MaxValue)
            version = 1;
        else
            version++;

        _assetVersions[assetType] = version;
    }
    
    private void UpdateVersion<TAsset>() where TAsset : class, IAssetDto => UpdateVersion(typeof(TAsset));
}