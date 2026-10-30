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

[EngineService(typeof(IAssetRegistry))]
public class AssetRegistry : IAssetRegistry
{
    private readonly Dictionary<Type, AssetTypeMetadata> _assetTypeToMetadata = new();
    private readonly Dictionary<Ulid, AssetTypeMetadata> _assetIdToMetadata = new();
    
    private readonly Dictionary<StringName, AssetTypeMetadata> _assetFileExtensionToMetadata = new();

    public Result<AssetTypeMetadata> GetMetadata(Ulid assetId)
    {
        if (_assetIdToMetadata.TryGetValue(assetId, out var metadata))
        {
            return Result<AssetTypeMetadata>.Success(metadata);
        }

        return Result<AssetTypeMetadata>.Failure($"Asset metadata not found for asset ID: {assetId}");
    }

    public Result<AssetTypeMetadata> GetMetadata(Type assetType)
    {
        if (_assetTypeToMetadata.TryGetValue(assetType, out var metadata))
        {
            return Result<AssetTypeMetadata>.Success(metadata);
        }

        return Result<AssetTypeMetadata>.Failure($"Asset metadata not found for asset type: {assetType}");
    }

    public bool HasMetadata(Ulid assetId)
    {
        return _assetIdToMetadata.ContainsKey(assetId);
    }

    public bool HasMetadata(Type assetType)
    {
        return _assetTypeToMetadata.ContainsKey(assetType);
    }

    public void RegisterAssetType(AssetTypeMetadata metadata)
    {
        _assetTypeToMetadata[metadata.AssetType] = metadata;
        _assetFileExtensionToMetadata[metadata.FileExtension] = metadata;
    }

    public void UnregisterAssetType(AssetTypeMetadata metadata)
    {
        _assetTypeToMetadata.Remove(metadata.AssetType);
        _assetFileExtensionToMetadata.Remove(metadata.FileExtension);
    }

    public void RegisterAssetIndex(Ulid assetId, AssetTypeMetadata metadata)
    {
        _assetIdToMetadata[assetId] = metadata;
    }

    public void UnregisterAssetIndex(Ulid assetId)
    {
        _assetIdToMetadata.Remove(assetId);
    }

    public IReadOnlySet<AssetTypeMetadata> GetAllMetadata()
    {
        return _assetTypeToMetadata.Values.ToHashSet();
    }
    
    public IReadOnlySet<Ulid> GetAllAssetIds(Type? type = null)
    {
        if(type == null)
            return _assetIdToMetadata.Keys.ToHashSet();
        
        return _assetIdToMetadata.Keys.Where(id => _assetIdToMetadata[id].AssetType == type).ToHashSet();
    }

    public void Clear(bool clearMetadata = false)
    {
        _assetIdToMetadata.Clear();
        if (clearMetadata)
        {
            _assetTypeToMetadata.Clear();
        }
    }
}