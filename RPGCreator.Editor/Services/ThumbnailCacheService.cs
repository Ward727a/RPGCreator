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

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Hexa.NET.ImGui;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.Editor.ImGuiLib;
using RPGCreator.SDK;
using RPGCreator.SDK.Mediator.Messages;
using RPGCreator.SDK.Resources;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Enums;
using RPGCreator.Shared.Types;
using SkiaSharp;

namespace RPGCreator.Editor.Services;

[EngineService(null, ServiceLifetime.Singleton, EngineModeFlags.Editor)]
[EngineService(typeof(IThumbnailCacheService), ServiceLifetime.Singleton, EngineModeFlags.Editor)]
[EngineService(typeof(INotificationHandler<ResourceMessage.ResourceChangedMessage>), ServiceLifetime.Singleton, EngineModeFlags.Editor)]
[EngineService(typeof(INotificationHandler<ProjectMessage.ProjectOpened>), ServiceLifetime.Singleton, EngineModeFlags.Editor)]
public class ThumbnailCacheService : IThumbnailCacheService, INotificationHandler<ProjectMessage.ProjectOpened>,
    INotificationHandler<ResourceMessage.ResourceChangedMessage>, IDisposable
{
    private readonly IResourceManager _resourceManager;
    private readonly GraphicsDevice _graphicsDevice;
    private string _cacheDirectory = "";

    private readonly Dictionary<Ulid, Texture2D> _loadedThumbnails = new();
    private readonly Dictionary<Ulid, ImTextureRef> _thumbnailBindings = new();

    public ThumbnailCacheService(IResourceManager resourceManager, GraphicsDevice graphicsDevice)
    {
        _resourceManager = resourceManager;
        _graphicsDevice = graphicsDevice;
    }

    public void Initialize()
    {
        _cacheDirectory = Path.Combine(RpgEnv.PathFormat.GetCacheFolder(), "thumbnails");
        
        if(!Directory.Exists(_cacheDirectory))
        {
            Directory.CreateDirectory(_cacheDirectory);
        }
    }

    /// <summary>
    /// Retrieves a thumbnail for the specified resource from the cache or generates a new one if not found.
    /// </summary>
    /// <param name="resourceId">The unique identifier of the resource for which the thumbnail is required.</param>
    /// <param name="size">The desired size (width and height) of the thumbnail. Default is 128.</param>
    /// <param name="registerInCache">Whether to register the generated thumbnail in the cache. Default is true.</param>
    /// <returns>
    /// A result containing the requested thumbnail texture on success, or an error message if the operation fails.
    /// </returns>
    public Result<Texture2D> GetOrGenerateThumbnail(Ulid resourceId, int size = 128, bool registerInCache = true)
    {
        if (_loadedThumbnails.TryGetValue(resourceId, out var thumbnail) && registerInCache)
        {
            return Result<Texture2D>.Success(thumbnail);
        }

        var resourceResult = _resourceManager.GetResourceObject(resourceId);
        Texture2D? texture = null;
        string? errorMessage = null;

        resourceResult.OnSuccess(resObject =>
        {
            var cacheFileName = GetCacheFileName(resourceId);

            var cacheFilePath = Path.Combine(_cacheDirectory, cacheFileName);
            var localResPath = Path.Combine(RpgEnv.PathFormat.GetResourcesPath().Value, resObject.ResourceLocalPath);

            if (File.Exists(cacheFilePath))
            {
                var originalTime = File.GetLastWriteTimeUtc(localResPath);
                var cacheTime = File.GetLastWriteTimeUtc(cacheFilePath);

                // If the cache file is newer or the same as the original file, use the cached thumbnail
                if (cacheTime >= originalTime)
                {
                    using var fileStream = File.OpenRead(cacheFilePath);
                    texture = Texture2D.FromStream(_graphicsDevice, fileStream);
                    texture.Name = "Thumbnail?";
                    return;
                }
            }

            if (!File.Exists(localResPath))
            {
                errorMessage = "Thumbnail not found";
                return;
            }

            using var originalBitmap = SKBitmap.Decode(localResPath);
            if (originalBitmap == null)
            {
                errorMessage = "Failed to decode thumbnail";
                return;
            }

            var imageInfo = new SKImageInfo(size, size, SKColorType.Rgba8888, SKAlphaType.Premul);
            using var resizedBitmap = originalBitmap.Resize(imageInfo, SKSamplingOptions.Default);

            using (var image = SKImage.FromBitmap(resizedBitmap))
            using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
            using (var stream = File.OpenWrite(cacheFilePath))
            {
                data.SaveTo(stream);
            }

            texture = new Texture2D(_graphicsDevice, size, size, false, SurfaceFormat.Color);

            texture.Name = $"Thumbnail_{resourceId}";
            texture.Tag = resObject.ResourceLocalPath;

            texture.SetDataPointerEXT(
                0,
                null,
                resizedBitmap.GetPixels(),
                resizedBitmap.ByteCount
            );
        }).OnFailure((err) => { errorMessage = err; });

        if (errorMessage != null)
        {
            return Result.Fail(errorMessage);
        }

        if (texture == null)
            return Result.Fail("Failed to create texture");

        if (registerInCache)
            _loadedThumbnails[resourceId] = texture;
        return texture;
    }

    /// <summary>
    /// Retrieves or generates a thumbnail for the specified resource.
    /// </summary>
    /// <param name="resourceId">The unique identifier of the resource.</param>
    /// <param name="size">The size (width and height) of the thumbnail to generate. Default is 128.</param>
    /// <param name="registerInCache">Whether to register the generated thumbnail in the cache. Default is true.</param>
    /// <returns>
    /// A result containing the thumbnail texture reference on success, or an error message on failure.
    /// </returns>
    public Result<ImTextureRef> GetThumbnail(Ulid resourceId, int size = 128, bool registerInCache = true)
    {
        if (_thumbnailBindings.TryGetValue(resourceId, out var thumbnail) && registerInCache)
            return thumbnail;

        string? errorMessage = null;

        GetOrGenerateThumbnail(resourceId, size, registerInCache)
            .OnSuccess((texture) =>
            {
                thumbnail = ImConfig.ImGuiRenderer.BindTexture(texture);

                if (registerInCache)
                    _thumbnailBindings[resourceId] = thumbnail;
            })
            .OnFailure((err) => { errorMessage = err; });

        if (errorMessage != null)
            return Result.Fail(errorMessage);

        return thumbnail;
    }

    public string GetCacheFileName(Ulid resourceUlid)
    {
        return $"{resourceUlid}.png";
    }

    public void ClearCache()
    {
        _loadedThumbnails.Clear();
    }

    public Task Handle(ProjectMessage.ProjectOpened notification, CancellationToken cancellationToken)
    {
        ClearCache();
        Initialize();
        return Task.CompletedTask;
    }

    public Task Handle(ResourceMessage.ResourceChangedMessage notification, CancellationToken cancellationToken)
    {
        Ulid id = notification.ResourceId;

        if (_loadedThumbnails.TryGetValue(id, out var texture))
        {
            ImConfig.ImGuiRenderer.UnbindTexture(_thumbnailBindings[id]);
            texture.Dispose();

            _loadedThumbnails.Remove(id);
            _thumbnailBindings.Remove(id);
            File.Delete(Path.Combine(_cacheDirectory, GetCacheFileName(id)));
        }

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        foreach (var loadedThumbnail in _loadedThumbnails)
        {
            loadedThumbnail.Value.Dispose();
        }
        _loadedThumbnails.Clear();

        foreach (var thumbnailBinding in _thumbnailBindings)
        {
            thumbnailBinding.Value.Destroy();
        }
        _thumbnailBindings.Clear();
    }
}