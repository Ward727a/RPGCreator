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

using System.Diagnostics.CodeAnalysis;
using RPGCreator.Shared.Types;

namespace RPGCreator.SDK.Services.Assets;

/// <summary>
/// Represents an in-memory cache for storing and managing assets.
/// Provides methods for retrieving, adding, updating, removing, and clearing assets from the cache.
/// </summary>
public interface IAssetCache
{
    Result<TAsset> Get<TAsset>(Ulid assetId) where TAsset : class, IAssetDto;
    Result<IAssetDto> Get(Ulid assetId);
    Result Add<TAsset>(TAsset asset) where TAsset : class, IAssetDto;
    
    Result Update<TAsset>(TAsset asset) where TAsset : class, IAssetDto;
    
    bool Has(Ulid assetId);
    
    bool Remove(Ulid assetId, [NotNullWhen(true)]out IAssetDto? asset, [NotNullWhen(true)]out Type? type);
    bool Remove(Ulid assetId) => Remove(assetId, out _, out _);
    
    void Clear();
}