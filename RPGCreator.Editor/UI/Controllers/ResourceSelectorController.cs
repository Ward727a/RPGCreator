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
using System.Linq;
using Hexa.NET.ImGui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.Editor.ImGuiLib;
using RPGCreator.Editor.Services;
using RPGCreator.Editor.UI.Models;
using RPGCreator.Editor.UI.Views;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Resources;
using RPGCreator.SDK.SDLHelper;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Types;
using SDL3;

namespace RPGCreator.Editor.UI.Controllers;

[EditorUiController]
public class ResourceSelectorController(
    Game game,
    ImGuiRenderer imGuiRenderer,
    IThumbnailCacheService thumbnailCacheService,
    IResourceManager resourceManager,
    ResourceSelectorModel state)
{
    public readonly ResourceSelectorModel State = state;

    /// <summary>
    /// Variable put to true when the controller needs to clean up its resources.<br/>
    /// More precisely, it will be set to true when the view use <see cref="GetImage"/>.
    /// </summary>
    public bool NeedCleanup { get; private set; }

    private List<IResourceManager.ResourceObject> _resourceTyped = [];
    private List<IResourceManager.ResourceObject> _sortedResource = [];

    private Dictionary<Ulid, ImTextureRef> _resourceIdToTextureRef = [];

    public void SetArguments(ResourceSelector.ResourceSelectorArguments arguments)
    {
        State.OnResourceSelected = arguments.OnResourceSelected;
        State.ResourceTypes = arguments.ResourceTypes;
        State.AllowImportFromOs = arguments.AllowImportFromOs;

        RefreshResourceTypedList();
    }

    public void RefreshResourceList()
    {
        if (!string.IsNullOrWhiteSpace(State.SearchText))
            RefreshResourceListSearch();
        else
            RefreshResourceListNoSearch();
    }

    private void RefreshResourceListSearch()
    {
        _sortedResource.Clear();

        foreach (var resource in _resourceTyped)
        {
            if (resource.ResourceName.StartsWith(State.SearchText, StringComparison.OrdinalIgnoreCase))
            {
                _sortedResource.Add(resource);
            }
        }

        State.Resources = _sortedResource;
    }

    private void RefreshResourceListNoSearch()
    {
        State.Resources = _resourceTyped;
    }

    public ImTextureRef GetImage(IResourceManager.ResourceObject resourceObject)
    {
        NeedCleanup = true;
        if (_resourceIdToTextureRef.TryGetValue(resourceObject.ResourceId, out var textureRef))
            return textureRef;

        var textureRefResult = thumbnailCacheService.GetThumbnail(resourceObject.ResourceId, registerInCache: false);

        if (textureRefResult.IsFailure)
        {
            throw new Exception("Failed to get thumbnail for resource: " + resourceObject.ResourceId);
        }

        textureRef = textureRefResult.Value;

        _resourceIdToTextureRef.Add(resourceObject.ResourceId, textureRef);

        return textureRef;
    }

    public void Cleanup()
    {
        Logger.Debug("Cleaning up {resourceCount} resources image.", _resourceIdToTextureRef.Count);

        var textureRefs = _resourceIdToTextureRef.Values;
        foreach (var textureRef in textureRefs)
        {
            var info = imGuiRenderer.UnbindTexture(textureRef);
            info?.Texture?.Dispose();
        }

        _resourceIdToTextureRef.Clear();

        NeedCleanup = false;
    }

    public nint GetWindowHandle() => game.Window.Handle;

    public ImGuiExt.FileDialogParameters GetFileDialogParameters(Action<string[]> onFileSelected)
    {
        List<(string name, string[] extensions)> fileFilters = [];
        foreach (var resourceType in State.ResourceTypes)
        {
            if(resourceType == IResourceManager.ResourceType.Unknown)
                continue;
            fileFilters.Add((Enum.GetName(resourceType) ?? "ERROR", [..IResourceManager.ResourceFormats[resourceType]]));
        }
        
        return new ImGuiExt.FileDialogParameters(GetWindowHandle(), onFileSelected)
        {
            AllowMany = false,
            Filters = fileFilters
        };
    }

    public void Import(string filePath)
    {
        resourceManager.ImportResource(filePath).OnSuccess((_) =>
        {
            RefreshResourceTypedList();
        }).OnFailure(err =>
        {
            ImNotify.Show("Error on import", $"Couldn't import file due to: \n{err}", NotifyType.Error, 5f);
            Logger.Error(err);
        });
    }

    #region Helpers

    private void RefreshResourceTypedList()
    {
        _resourceTyped.Clear();

        if (State.ResourceTypes.Length <= 0)
            _resourceTyped = [.. resourceManager.GetAllResources()];
        else
        {
            foreach (var resourceType in State.ResourceTypes)
            {
                _resourceTyped.AddRange(resourceManager.GetAllResourcesOfType(resourceType));
            }
        }

        State.Resources = _resourceTyped;
    }

    #endregion
}