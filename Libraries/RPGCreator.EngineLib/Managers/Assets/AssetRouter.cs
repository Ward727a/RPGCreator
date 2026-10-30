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

using RPGCreator.SDK.Services.Assets;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Types;

namespace RPGCreator.EngineLib.Managers.Assets;

[EngineService(typeof(IAssetRouter))]
public class AssetRouter : IAssetRouter
{
    private readonly Dictionary<Type, IAssetProvider> _providers = new();

    public Result<IAssetProvider<TAsset>> GetProvider<TAsset>() where TAsset : class, IAssetDto
    {
        return _providers.TryGetValue(typeof(TAsset), out var provider)
            ? Result<IAssetProvider<TAsset>>.Success((IAssetProvider<TAsset>)provider)
            : Result<IAssetProvider<TAsset>>.Failure("Provider not found");
    }

    public Result<IAssetProvider> GetProvider(Type assetType)
    {
        return _providers.TryGetValue(assetType, out var provider)
            ? Result<IAssetProvider>.Success(provider)
            : Result<IAssetProvider>.Failure("Provider not found");
    }

    public void RegisterProvider<TAsset>(IAssetProvider<TAsset> provider) where TAsset : class, IAssetDto
    {
        _providers[typeof(TAsset)] = provider;
    }

    public void RemoveProvider<TAsset>(IAssetProvider<TAsset> provider) where TAsset : class, IAssetDto
    {
        _providers.Remove(typeof(TAsset));
    }

    public void Clear()
    {
        _providers.Clear();
    }
}