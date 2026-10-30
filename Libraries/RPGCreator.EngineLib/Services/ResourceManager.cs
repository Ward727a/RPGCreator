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
using System.Numerics;
using System.Text.Json;
using MediatR;
using Microsoft.Xna.Framework;
using RPGCreator.SDK;
using RPGCreator.SDK.AssetsPre;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Mediator.Messages;
using RPGCreator.SDK.Resources;
using RPGCreator.SDK.Serializer;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Types;
using SkiaSharp;

namespace RPGCreator.EngineLib.Services;

[EngineService(typeof(IResourceManager))]
public class ResourceManager : IResourceManager, IDisposable
{
    private readonly ISerializerService _serializerService;
    private readonly IMediator _mediator;
    private readonly ITypesRegistry _typesRegistry;
    private Dictionary<Ulid, IResourceManager.ResourceObject> _resourceCache = new();
    private Dictionary<string, Ulid> _resourcePathToIdMap = new();
    private Dictionary<IResourceManager.ResourceType, HashSet<Ulid>> _resourceTypeToIdsMap = new();
    private FileSystemWatcher? _fileSystemWatcher;

    public ResourceManager(ISerializerService serializerService, IMediator mediator, ITypesRegistry typesRegistry)
    {
        _serializerService = serializerService;
        _mediator = mediator;
        _typesRegistry = typesRegistry;
    }

    public Result<ReadOnlyMemory<byte>> GetResource(string resourceLocalPath)
    {
        var localPath = Path.Combine(RpgEnv.PathFormat.GetResourcesPath().Value, resourceLocalPath);

        if (File.Exists(localPath))
        {
            var resourceData = File.ReadAllBytes(localPath);
            return Result<ReadOnlyMemory<byte>>.Success(new ReadOnlyMemory<byte>(resourceData));
        }

        return Result<ReadOnlyMemory<byte>>.Failure("Resource not found");
    }

    public Result<ReadOnlyMemory<byte>> GetResource(Ulid resourceId)
    {
        if(_resourceCache.TryGetValue(resourceId, out var resource))
            return GetResource(resource.ResourceLocalPath);
        return Result<ReadOnlyMemory<byte>>.Failure("Resource not found");
    }

    public Result<IResourceManager.ResourceObject> GetResourceObject(string resourceLocalPath)
    {
        return GetResourceObject(_resourcePathToIdMap[resourceLocalPath]);
    }

    public Result<IResourceManager.ResourceObject> GetResourceObject(Ulid resourceId)
    {
        if (_resourceCache.TryGetValue(resourceId, out var value))
        {
            return Result<IResourceManager.ResourceObject>.Success(value);
        }

        return Result<IResourceManager.ResourceObject>.Failure("Resource not found");
    }

    public Result ExportResource(string resourceLocalPath, string destination)
    {
        var resourceDataResult = GetResource(resourceLocalPath);

        if (resourceDataResult.IsFailure)
            return Result.Fail(resourceDataResult.Error);

        var resourceData = resourceDataResult.Value;

        var finalDestination = destination;

        if (!Path.HasExtension(destination))
        {
            var fileDestinationName = Path.GetFileName(resourceLocalPath);
            finalDestination = Path.Combine(destination, fileDestinationName);
        }

        var directory = Path.GetDirectoryName(finalDestination);

        if (string.IsNullOrWhiteSpace(directory))
            return Result.Fail("Invalid destination path");

        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        try
        {
            File.WriteAllBytes(finalDestination, resourceData.Span);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to export resource: {ex.Message}");
        }

        return Result.Success();
    }

    public Result ExportResource(Ulid resourceId, string destination)
    {
        if (_resourceCache.TryGetValue(resourceId, out var resource))
        {
            return ExportResource(resource.ResourceLocalPath, destination);
        }

        return Result.Failure("Resource not found");
    }

    public Result ExportResource(IResourceManager.ResourceObject resource, string destination)
    {
        return ExportResource(resource.ResourceLocalPath, destination);
    }

    public bool HasResource(string localPath)
    {
        return _resourcePathToIdMap.ContainsKey(localPath) &&
               _resourceCache.ContainsKey(_resourcePathToIdMap[localPath]);
    }

    public bool HasResource(Ulid resourceId)
    {
        return _resourceCache.ContainsKey(resourceId);
    }

    /// <summary>
    /// Imports a resource from the specified source path into the resource manager's internal cache.
    /// </summary>
    /// <param name="resourceSourcePath">
    /// The source path of the resource to import.
    /// </param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the imported <see cref="IResourceManager.ResourceObject"/>
    /// if the operation succeeds, or an error message if it fails.
    /// </returns>
    public Result<IResourceManager.ResourceObject> ImportResource(string resourceSourcePath)
    {
        if (!File.Exists(resourceSourcePath))
            return Result.Fail($"Resource file {resourceSourcePath} does not exist.");

        var resourceFormat = Path.GetExtension(resourceSourcePath).ToLowerInvariant();
        var resourceType = GetResourceType(resourceFormat);

        var targetFolderResult =
            RpgEnv.PathFormat.GetResourcesPath(resourceType != IResourceManager.ResourceType.Unknown
                ? resourceType.ToString()
                : "");

        if (targetFolderResult.IsFailure)
            return Result.Fail(targetFolderResult.Error);

        var targetFolder = targetFolderResult.Value;

        var resourceName = GetResourceName(targetFolder, resourceSourcePath);

        var resourceLocalPath = Path.Combine(targetFolder, resourceName);
        var resourceLocalRelativePath = AbsolutePathToRelativePath(resourceLocalPath);

        var resourceId = Ulid.NewUlid();

        File.Copy(resourceSourcePath, resourceLocalPath);

        URN resourceMetaType = URN.Empty;
        JsonElement resourceMeta = default;
        object? resourceMetaObject = null;

        if (resourceType == IResourceManager.ResourceType.Image)
        {
            using Stream stream = File.OpenRead(resourceSourcePath);
            var info = SKBitmap.DecodeBounds(stream);
            resourceMetaType = new URN("rpgc", "sdk", "resource/meta/image");
            resourceMetaObject = new ResourceImageMeta()
            {
                ImageSize = new Point(info.Width, info.Height)
            };
            resourceMeta = JsonSerializer.SerializeToElement(resourceMetaObject, _serializerService.JsonOptions);
        }
        
        var resourceObject = new IResourceManager.ResourceObject()
        {
            ResourceType = resourceType,
            ResourceName = Path.GetFileNameWithoutExtension(resourceSourcePath),
            ResourceFormat = resourceFormat,
            ResourceId = resourceId,
            ResourceLocalPath = resourceLocalRelativePath,
            ResourceSourcePath = resourceSourcePath,
            ResourceMetaTypeUrn = resourceMetaType,
            ResourceMeta = resourceMeta,
            ResourceMetaObject = resourceMetaObject
        };

        _resourceCache[resourceId] = resourceObject;
        _resourcePathToIdMap[resourceLocalRelativePath] = resourceId;

        if (!_resourceTypeToIdsMap.ContainsKey(resourceType))
            _resourceTypeToIdsMap[resourceType] = [];
        _resourceTypeToIdsMap[resourceType].Add(resourceId);

        _serializerService.SerializeTo(resourceObject,
            Path.Combine(targetFolder, $"{Path.GetFileNameWithoutExtension(resourceName)}.json"));

        return Result<IResourceManager.ResourceObject>.Success(resourceObject);
    }

    /// <summary>
    /// Loads all resources from disk into the resource manager's internal cache.
    /// </summary>
    /// <returns>
    /// A <see cref="Result"/> indicating the success or failure of the operation.
    /// </returns>
    public Result LoadAllResourcesFromDisk()
    {
        var resourcesPathResult = RpgEnv.PathFormat.GetResourcesPath();
        if (resourcesPathResult.IsFailure)
        {
            return Result.Fail(resourcesPathResult.Error);
        }

        var resourcesDir = resourcesPathResult.Value;

        if (!Directory.Exists(resourcesDir))
        {
            return Result.Fail("Resources directory does not exist.");
        }

        if (_fileSystemWatcher != null)
        {
            _fileSystemWatcher.EnableRaisingEvents = false;
            _fileSystemWatcher.Changed -= OnResourceChanged;
            _fileSystemWatcher.Created -= OnResourceChanged;
            _fileSystemWatcher.Deleted -= OnResourceChanged;
            _fileSystemWatcher.Renamed -= OnResourceChanged;
            _fileSystemWatcher.Dispose();
        }

        _resourceCache.Clear();
        _resourcePathToIdMap.Clear();
        _resourceTypeToIdsMap.Clear();

        _fileSystemWatcher =
            new FileSystemWatcher(RpgEnv.PathFormat.GetResourcesPath(nameof(IResourceManager.ResourceType.Image)).Value,
                "*.png");
        _fileSystemWatcher.EnableRaisingEvents = true;
        _fileSystemWatcher.Changed += OnResourceChanged;
        _fileSystemWatcher.Created += OnResourceChanged;
        _fileSystemWatcher.Deleted += OnResourceChanged;
        _fileSystemWatcher.Renamed += OnResourceChanged;

        var metaFiles = Directory.EnumerateFiles(resourcesDir, "*.json", SearchOption.AllDirectories);

        var dictionaryLock = new Lock();

        ConcurrentBag<string> errors = [];

        Parallel.ForEach(metaFiles, filePath =>
        {
            try
            {
                _serializerService.DeserializeFrom<IResourceManager.ResourceObject?>(filePath, out var resourceObject);

                if (resourceObject is null) return;

                lock (dictionaryLock)
                {
                    _resourceCache[resourceObject.ResourceId] = resourceObject;
                    _resourcePathToIdMap[resourceObject.ResourceLocalPath] = resourceObject.ResourceId;

                    if (!_resourceTypeToIdsMap.TryGetValue(resourceObject.ResourceType, out HashSet<Ulid>? value))
                    {
                        value = [];
                        _resourceTypeToIdsMap[resourceObject.ResourceType] = value;
                    }

                    value.Add(resourceObject.ResourceId);
                }
            }
            catch (Exception ex)
            {
                errors.Add(ex.Message);
            }
        });

        if (errors.IsEmpty)
        {
            Logger.Debug("Loaded {count} resources from disk successfully.", _resourceCache.Count);
            return Result.Success();
        }

        Logger.Error("Errors occurred while loading resources: {errors}", string.Join("; ", errors));
        return Result.Fail($"Got {errors.Count} errors while loading resources: Please check log for more info.");
    }

    public Result<TMeta> GetResourceMeta<TMeta>(Ulid resourceId) where TMeta : class
    {
        var tMetaType = typeof(TMeta);
        if (!_typesRegistry.HasType(tMetaType))
        {
            return Result<TMeta>.Fail($"Type {tMetaType} is not registered");
        }

        var typeUrn = _typesRegistry.GetKey(tMetaType).Match(urn => urn, err => URN.Empty);

        if (typeUrn == URN.Empty)
        {
            return Result<TMeta>.Fail($"Type {tMetaType} is not registered");
        }

        return GetResourceObject(resourceId).Match(resource =>
            {
                if (resource.ResourceMetaObject != null)
                {
                    return resource.ResourceMetaObject.GetType() == tMetaType
                        ? Result<TMeta>.Ok((TMeta)resource.ResourceMetaObject)
                        : Result<TMeta>.Fail($"Type {tMetaType} does not match resource meta type");
                }

                if (typeUrn != resource.ResourceMetaTypeUrn)
                {
                    return Result<TMeta>.Fail($"Type {tMetaType} does not match resource meta type");
                }

                var meta = resource.ResourceMeta.Deserialize<TMeta>(_serializerService.JsonOptions);

                if (meta == null)
                {
                    return Result<TMeta>.Fail($"Failed to deserialize resource meta");
                }
                
                resource.ResourceMetaObject = meta;

                return Result<TMeta>.Ok(meta);
            },
            Result<TMeta>.Fail);
    }

    public Result<TMeta> GetResourceMeta<TMeta>(string resourcePath) where TMeta : class
    {
        return _resourcePathToIdMap.TryGetValue(resourcePath, out var resourceId)
            ? GetResourceMeta<TMeta>(resourceId)
            : Result<TMeta>.Fail($"Resource not found: {resourcePath}");
    }

    public Result<TMeta> GetResourceMeta<TMeta>(IResourceManager.ResourceObject @object) where TMeta : class
    {
        var tMetaType = typeof(TMeta);
            
        if (!_typesRegistry.HasType(tMetaType))
        {
            return Result<TMeta>.Fail($"Type {tMetaType} is not registered");
        }

        var typeUrn = _typesRegistry.GetKey(tMetaType).Match(urn => urn, err => URN.Empty);

        if (typeUrn == URN.Empty)
        {
            return Result<TMeta>.Fail($"Type {tMetaType} is not registered");
        }

        if (@object.ResourceMetaObject != null)
        {
            return @object.ResourceMetaObject.GetType() == tMetaType
                ? Result<TMeta>.Ok((TMeta)@object.ResourceMetaObject)
                : Result<TMeta>.Fail($"Type {tMetaType} does not match resource meta type");
        }

        if (typeUrn != @object.ResourceMetaTypeUrn)
        {
            return Result<TMeta>.Fail($"Type {tMetaType} does not match resource meta type");
        }

        var meta = @object.ResourceMeta.Deserialize<TMeta>(_serializerService.JsonOptions);

        if (meta == null)
        {
            return Result<TMeta>.Fail($"Failed to deserialize resource meta");
        }
                
        @object.ResourceMetaObject = meta;

        return Result<TMeta>.Ok(meta);
    }

    private void OnResourceChanged(object sender, FileSystemEventArgs e)
    {
        string localPath = AbsolutePathToRelativePath(e.FullPath);

        if (_resourcePathToIdMap.TryGetValue(localPath, out var resourceId))
        {
            _mediator.Publish(new ResourceMessage.ResourceChangedMessage(resourceId));
        }
    }

    /// <summary>
    /// Retrieves all resources currently managed by the resource manager.
    /// </summary>
    /// <returns>
    /// A collection of <see cref="IResourceManager.ResourceObject"/> instances representing
    /// all resources stored in the resource manager's cache.
    /// </returns>
    public IEnumerable<IResourceManager.ResourceObject> GetAllResources()
    {
        return _resourceCache.Values;
    }

    /// <summary>
    /// Retrieves all resources of a specified type managed by the resource manager.
    /// </summary>
    /// <param name="resourceType">
    /// The type of resources to retrieve, specified as a value of <see cref="IResourceManager.ResourceType"/>.
    /// </param>
    /// <returns>
    /// A collection of <see cref="IResourceManager.ResourceObject"/> instances representing
    /// all resources of the specified type stored in the resource manager's cache.
    /// </returns>
    public IEnumerable<IResourceManager.ResourceObject> GetAllResourcesOfType(
        IResourceManager.ResourceType resourceType)
    {
        if (_resourceTypeToIdsMap.TryGetValue(resourceType, out HashSet<Ulid>? ids))
        {
            return ids.Select(id => _resourceCache[id]);
        }

        return [];
    }

    private static IResourceManager.ResourceType GetResourceType(string resourceFormat)
    {
        return IResourceManager.ResourceTypes.GetValueOrDefault(resourceFormat, IResourceManager.ResourceType.Unknown);
    }

    private static string GetResourceName(string targetFolder, string resourceSourcePath)
    {
        var fileName = Path.GetFileNameWithoutExtension(resourceSourcePath);
        var fileExtension = Path.GetExtension(resourceSourcePath).ToLowerInvariant();

        string finalName = fileName + fileExtension;
        int index = 1;

        while (File.Exists(Path.Combine(targetFolder, finalName)))
        {
            finalName = $"{fileName}_{index}{fileExtension}";
            index++;
        }

        return finalName;
    }

    private static string AbsolutePathToRelativePath(string localPath)
    {
        var resourcesPathResult = RpgEnv.PathFormat.GetResourcesPath();
        if (resourcesPathResult.IsFailure)
            throw new InvalidOperationException($"Failed to get resources path: {resourcesPathResult.Error}");

        var resourcesPath = resourcesPathResult.Value;

        string relativePath = Path.GetRelativePath(resourcesPath, localPath);

        return relativePath.Replace('\\', '/');
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        if (_fileSystemWatcher != null)
        {
            _fileSystemWatcher.EnableRaisingEvents = false;
            _fileSystemWatcher.Changed -= OnResourceChanged;
            _fileSystemWatcher.Created -= OnResourceChanged;
            _fileSystemWatcher.Deleted -= OnResourceChanged;
            _fileSystemWatcher.Renamed -= OnResourceChanged;
            _fileSystemWatcher.Dispose();
            _fileSystemWatcher = null;
        }
    }
}