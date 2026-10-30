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

using CommunityToolkit.HighPerformance;
using RPGCreator.SDK.Serializer;
using RPGCreator.SDK.Services.Assets;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Types;

namespace RPGCreator.EngineLib.Managers.Assets;

[EngineService(typeof(IAssetProvider))]
public class AssetGenericProvider(ISerializerService serializer) : IAssetProvider
{
    public Result<TAsset> Get<TAsset>(ReadOnlyMemory<byte> assetData)
        where TAsset : class, IAssetDto
    {
        if (serializer.Deserialize(assetData.AsStream(), out TAsset? asset))
        {
            return Result<TAsset>.Success(asset!);
        }
        return Result<TAsset>.Failure("Failed to deserialize asset data.");
    }

    public Result<IAssetDto> Get(Type assetType, ReadOnlyMemory<byte> assetData)
    {
        if (serializer.Deserialize(assetData.AsStream(), assetType, out var asset))
        {
            if (asset is IAssetDto assetDto)
            {
                return Result<IAssetDto>.Success(assetDto);
            }
            return Result<IAssetDto>.Failure("Failed to deserialize asset data. Asset is not AssetDto");
        }
        return Result<IAssetDto>.Failure("Failed to deserialize asset data.");
    }

    Result Validate(IAssetDto asset)
    {
        if (asset.AssetId != Ulid.Empty)
        {
            return Result.Success();
        }
        return Result.Failure("Asset ID is empty.");
    }

    public Result<IAssetDto> GetRaw(Type type, ReadOnlyMemory<byte> assetData)
    {
        return Get(type, assetData);
    }
}