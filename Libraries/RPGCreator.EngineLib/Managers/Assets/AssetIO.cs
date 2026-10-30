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
using CommunityToolkit.HighPerformance;
using RPGCreator.SDK;
using RPGCreator.SDK.Serializer;
using RPGCreator.SDK.Services.Assets;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Types;

namespace RPGCreator.EngineLib.Managers.Assets;

[EngineService(typeof(IAssetIO))]
public class AssetIO : IAssetIO
{
    private readonly IAssetRegistry _assetRegistry;
    private readonly ISerializerService _serializerService;

    public AssetIO(IAssetRegistry assetRegistry, ISerializerService serializerService)
    {
        _assetRegistry = assetRegistry;
        _serializerService = serializerService;
    }

    public Result<ReadOnlyMemory<byte>> Load(Ulid assetId)
    {
        if (!GetAssetFilePathFromId(assetId, out var assetPath))
        {
            return Result<ReadOnlyMemory<byte>>.Failure("Could not find asset path");
        }

        return !File.Exists(assetPath)
            ? Result<ReadOnlyMemory<byte>>.Failure("Asset path does not exist")
            : Result<ReadOnlyMemory<byte>>.Success(File.ReadAllBytes(assetPath));
    }

    public Result Save<TAsset>(TAsset assetData) where TAsset : class, IAssetDto
    {
        if(!GetAssetFilePathFromType(assetData.AssetId, assetData.GetType(), out var assetPath))
        {
            return Result.Failure("Could not find asset path");
        }

        return _serializerService.SerializeTo(assetData, assetPath);
    }

    public Result Delete(Ulid assetId)
    {
        if (!GetAssetFilePathFromId(assetId, out var assetPath))
        {
            return Result.Failure("Could not find asset path");
        }

        if (File.Exists(assetPath))
        {
            File.Delete(assetPath);
        }

        return Result.Success();
    }

    public bool Exists(Ulid assetId)
    {
        if (!GetAssetFilePathFromId(assetId, out var assetPath))
        {
            return false;
        }

        return File.Exists(assetPath);
    }

    public IEnumerable<(Ulid assetId, AssetTypeMetadata metadata)> ScanForAssets(IEnumerable<AssetTypeMetadata> definitions)
    {
        var assetsFolder = GetAssetDir(out var success);

        if (!success) yield break;
        
        foreach (var metadataType in definitions)
        {
            var specificFolder = Path.Combine(assetsFolder!, metadataType.Folder);

            if (!Directory.Exists(specificFolder))
                continue;

            var files = Directory.EnumerateFiles(specificFolder, $"*.{metadataType.FileExtension}");

            foreach (var file in files)
            {
                if (Ulid.TryParse(Path.GetFileNameWithoutExtension(file), out var assetId))
                    yield return (assetId, metadataType);

            }
        }
    }

    public AssetHeader? GetHeader(ReadOnlyMemory<byte> data)
    {
        _serializerService.Deserialize<AssetHeader>(data.AsStream(), out var header);
        return header;
    }

    #region Helper

    private string? GetAssetDir(out bool success)
    {
        var assetDirectoryPathResult = RpgEnv.PathFormat.GetAssetsPath();
        success = assetDirectoryPathResult.IsSuccess;

        return assetDirectoryPathResult.IsSuccess ? assetDirectoryPathResult.Value : null;
    }

    private bool GetAssetFilePathFromId(Ulid assetId, [NotNullWhen(true)] out string? assetFilePath)
    {
        assetFilePath = null;
        var dir = GetAssetDir(out var successAssetDir);

        if (!successAssetDir)
        {
            return false;
        }

        var metaDataResult = _assetRegistry.GetMetadata(assetId);

        if (metaDataResult.IsFailure)
        {
            return false;
        }

        var (_, folder, fileExtension, _) = metaDataResult.Value;
        
        var assetFolder = Path.Combine(dir!, folder);

        if (!Directory.Exists(assetFolder))
        {
            Directory.CreateDirectory(assetFolder);
        }

        assetFilePath = Path.Combine(assetFolder, $"{assetId}.{fileExtension}");

        return true;
    }

    private bool GetAssetFilePathFromType(Ulid assetId, Type assetType, [NotNullWhen(true)] out string? assetFilePath)
    {
        assetFilePath = null;
        var dir = GetAssetDir(out var successAssetDir);

        if (!successAssetDir)
        {
            return false;
        }

        var metaDataResult = _assetRegistry.GetMetadata(assetType);

        if (metaDataResult.IsFailure)
        {
            return false;
        }

        var (_, folder, fileExtension, _) = metaDataResult.Value;

        var assetFolder = Path.Combine(dir!, folder);

        if (!Directory.Exists(assetFolder))
        {
            Directory.CreateDirectory(assetFolder);
        }
        
        assetFilePath = Path.Combine(assetFolder, $"{assetId}.{fileExtension}");

        return true;
    }

    #endregion
}