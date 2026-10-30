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

using RPGCreator.Shared.Types;

namespace RPGCreator.SDK.Services.Assets;

/// <summary>
/// Provides functionality to manage assets within the system. This includes loading, saving, deleting,
/// and refreshing assets, as well as checking for the presence of specific assets by their unique identifier.
/// <br/>
/// By itself, this class does not perform any operations. It will use each assets services to delegate the actual work.
/// </summary>
/// <seealso cref="IAssetCache"/>
/// <seealso cref="IAssetIO"/>
/// <seealso cref="IAssetRouter"/>
/// <seealso cref="IAssetRegistry"/>
public interface IAssetManager
{
    Result<TAsset> GetAsset<TAsset>(Ulid assetId)
        where TAsset : class, IAssetDto;

    Result<IAssetDto> GetAsset(Type assetType, Ulid assetId);

    bool ClearAssetFromCache(Ulid assetId);
    bool HasAsset(Ulid assetId);
    Result DeleteAsset(Ulid assetId, bool force = false);
    Result SaveAsset<TAsset>(TAsset asset)
        where TAsset : class, IAssetDto;

    Result RefreshAssets();

    ushort GetVersion(Type assetType);

    ushort GetVersion<TAsset>()
        where TAsset : class, IAssetDto => GetVersion(typeof(TAsset));

    IEnumerable<Ulid> ListAssets<TAsset>()
        where TAsset : class, IAssetDto;
    
    IEnumerable<Ulid> ListAssets(Type assetType);
}