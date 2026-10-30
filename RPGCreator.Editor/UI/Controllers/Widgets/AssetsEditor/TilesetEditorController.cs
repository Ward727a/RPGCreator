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
using Fluxor;
using Hexa.NET.ImGui;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.Editor.ImGuiLib;
using RPGCreator.Editor.UI.Models.Widgets.AssetsEditor;
using RPGCreator.Editor.UI.Views;
using RPGCreator.RuntimeLib.FNA;
using RPGCreator.SDK.Assets.Tileset;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Resources;
using RPGCreator.SDK.Services.Assets;
using RPGCreator.Shared.Attributes;
using SDL3;

namespace RPGCreator.Editor.UI.Controllers.Widgets.AssetsEditor;

[EditorUiController]
public class TilesetEditorController
{
    public readonly ResourceSelector.ResourceSelectorArguments ResourceSelectorArguments;
    public readonly TilesetEditorModel State;
    public bool CanOpen { get; private set; } = true;
    public bool ShouldSave { get; private set; }
    private readonly ImGuiRenderer _imGuiRenderer;
    private readonly IResourceManager _resourceManager;
    private readonly GraphicsDevice _graphicsDevice;
    private readonly IAssetManager _assetManager;

    public TilesetEditorController(TilesetEditorModel state, ImGuiRenderer imGuiRenderer,
        IResourceManager resourceManager, GraphicsDevice graphicsDevice, IAssetManager assetManager)
    {
        _imGuiRenderer = imGuiRenderer;
        _resourceManager = resourceManager;
        _graphicsDevice = graphicsDevice;
        _assetManager = assetManager;
        State = state;
        ResourceSelectorArguments = new()
        {
            AllowImportFromOs = true,
            ResourceTypes = [IResourceManager.ResourceType.Image],
            OnResourceSelected = resource =>
            {
                SetTileset(resource.ResourceLocalPath);
                var metaResult = resource.GetMeta<ResourceImageMeta>(resourceManager);
                if (metaResult.IsFailure)
                {
                    Logger.Error(metaResult.Error);
                    return;
                }

                var meta = metaResult.Value;
                State.TilesetImageSize = meta.ImageSize;
            }
        };
    }


    public void LoadTileset(TilesetDefinition tilesetDefinition)
    {
        CanOpen = true;
        ShouldSave = false;
        State.Definition = tilesetDefinition;
        State.TilesetName = tilesetDefinition.Name;
        State.TilesetTileSize = [tilesetDefinition.TileWidth, tilesetDefinition.TileHeight];
        State.TilesetImagePath = tilesetDefinition.TextureResourcePath;

        RefreshTilesetTexture();
    }

    public void RefreshTilesetTexture()
    {
        if (State.TilesetTexture != null)
        {
            var value = _imGuiRenderer.UnbindTexture(State.TilesetTexture.Value);
            value?.Texture.Dispose();
        }

        State.TilesetTexture = null;

        _resourceManager.GetResource(State.TilesetImagePath)
            .OnSuccess(data =>
            {
                var texture2D = Texture2D.FromMemory(_graphicsDevice, data);

                State.TilesetTexture = _imGuiRenderer.BindTexture(texture2D);
            }).OnFailure(err => { Logger.Error(err); });
    }

    public void SetShouldSave()
    {
        ShouldSave = true;
    }

    public ImGuiWindowFlags GetFlags() => ShouldSave
        ? ImGuiWindowFlags.NoResize | ImGuiWindowFlags.UnsavedDocument
        : ImGuiWindowFlags.NoResize;

    public void BlockOpen()
    {
        CanOpen = false;
    }

    public void SetTileset(string tilesetImagePath)
    {
        State.TilesetImagePath = tilesetImagePath;
        SetShouldSave();
        RefreshTilesetTexture();
    }

    public bool CanSave()
    {
        return !string.IsNullOrWhiteSpace(State.Definition.Name) &&
               !string.IsNullOrWhiteSpace(State.TilesetImagePath) &&
               State.TilesetTileSize[0] > 0 &&
               State.TilesetTileSize[1] > 0;
    }

    public void Save()
    {
        if (State.Definition == null)
        {
            SDL.SDL_ShowSimpleMessageBox(SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR,
                "ERROR - COULD NOT SAVE MODIFICATION", "Tileset definition is null!!", IntPtr.Zero);
            return;
        }

        State.Definition.Name = State.TilesetName;
        State.Definition.TextureResourcePath = State.TilesetImagePath;
        State.Definition.TileWidth = State.TilesetTileSize[0];
        State.Definition.TileHeight = State.TilesetTileSize[1];
        State.Definition.TilesByRow = State.TilesetImageSize.X / State.Definition.TileWidth;
        _assetManager.SaveAsset(State.Definition);

        if (State.TilesetTexture != null)
        {
            var value = _imGuiRenderer.UnbindTexture(State.TilesetTexture.Value);
            value?.Texture.Dispose();
        }

        ShouldSave = false;
    }
}