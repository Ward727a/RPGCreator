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
/// Provides an interface for managing asset metadata registrations and lookups
/// within the system.
/// </summary>
public interface IAssetRegistry
{
    Result<AssetTypeMetadata> GetMetadata(Ulid assetId);
    Result<AssetTypeMetadata> GetMetadata(Type assetType);
    
    bool HasMetadata(Ulid assetId);
    bool HasMetadata(Type assetType);
    
    void RegisterAssetType(AssetTypeMetadata metadata);
    void UnregisterAssetType(AssetTypeMetadata metadata);
    void RegisterAssetIndex(Ulid assetId, AssetTypeMetadata metadata);
    void UnregisterAssetIndex(Ulid assetId);
    
    IReadOnlySet<AssetTypeMetadata> GetAllMetadata();
    IReadOnlySet<Ulid> GetAllAssetIds(Type? type = null);
    
    void Clear(bool clearMetadata = false);
}