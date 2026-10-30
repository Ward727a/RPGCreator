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

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using RPGCreator.SDK.Services.Assets;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Types;

namespace RPGCreator.EngineLib.Managers.Assets;

[EngineService(typeof(IAssetCache))]
public class AssetCache : IAssetCache
{
    private readonly ConcurrentDictionary<Ulid, IAssetDto> _assets = [];

    public Result<TAsset> Get<TAsset>(Ulid assetId) where TAsset : class, IAssetDto
    {
        if (!_assets.TryGetValue(assetId, out var asset))
        {
            return Result<TAsset>.Fail($"Asset not found: {assetId}");
        }

        return asset is not TAsset typedAsset
            ? Result<TAsset>.Fail($"Asset is not of type {typeof(TAsset)}")
            : Result<TAsset>.Ok(typedAsset);
    }

    public Result<IAssetDto> Get(Ulid assetId)
    {
        if (!_assets.TryGetValue(assetId, out var asset))
        {
            return Result<IAssetDto>.Fail($"Asset not found: {assetId}");
        }
        
        return Result<IAssetDto>.Ok(asset);
    }

    public Result Add<TAsset>(TAsset asset) where TAsset : class, IAssetDto
    {
        return !_assets.TryAdd(asset.AssetId, asset)
            ? Result.Fail($"Asset with id {asset.AssetId} already exists")
            : Result.Ok();
    }

    public Result Update<TAsset>(TAsset asset) where TAsset : class, IAssetDto
    {
        return !_assets.TryUpdate(asset.AssetId, asset, asset)
            ? Result.Fail($"Asset with id {asset.AssetId} already exists")
            : Result.Ok();
    }

    public bool Has(Ulid assetId)
    {
        return _assets.ContainsKey(assetId);
    }

    public bool Remove(Ulid assetId, [NotNullWhen(true)] out IAssetDto? asset, [NotNullWhen(true)] out Type? type)
    {
        var success = _assets.TryRemove(assetId, out asset);
        type = asset?.GetType();
        return success;
    }

    public void Clear()
    {
        _assets.Clear();
    }
}