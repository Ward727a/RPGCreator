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
using System.IO;
using System.Text.Json.Serialization;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.Editor.ImGuiLib;
using RPGCreator.Editor.UI.Models.Widgets.AssetsCreator;
using RPGCreator.RuntimeLib.FNA;
using RPGCreator.SDK.Assets.Tileset;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Resources;
using RPGCreator.SDK.Serializer;
using RPGCreator.SDK.Services.Assets;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Types;
using static RPGCreator.SDK.RpgEnv;
using Path = System.IO.Path;

namespace RPGCreator.Editor.UI.Controllers.Widgets.AssetsCreator;

[EditorUiController]
public class TilesetCreatorController(TilesetCreatorModel state, ImGuiRenderer renderer, GraphicsDevice graphicsDevice, IResourceManager resourceManager, IAssetManager assetManager)
{
    public TilesetCreatorModel State { get; private set; } = state;
    public bool NeedClearState { get; private set; } = false;

    public void SetClearState() => NeedClearState = true;

    public void ClearState()
    {
        State.TilesetName = "";
        State.TilesetImageLocalPath = "";
        State.TileWidth = 32;
        State.TileHeight = 32;
        if (State.TilesetImageTexture.HasValue)
            renderer.UnbindTexture(State.TilesetImageTexture.Value);
        State.TilesetImageTexture = null;
        
        Logger.Debug("Cleared TilesetCreator state.");
        
        NeedClearState = false;
    }

    public Result SetTilesetImage(IResourceManager.ResourceObject resource)
    {
        NeedClearState = true;
        
        if(resource.ResourceType != IResourceManager.ResourceType.Image)
            return Result.Fail("Invalid resource type");
        
        var resourceDataResult = resourceManager.GetResource(resource.ResourceId);
        
        if(resourceDataResult.IsFailure)
            return Result.Fail(resourceDataResult.Error);

        var resourceData = resourceDataResult.Value;
        
        var texture2D = Texture2D.FromMemory(graphicsDevice, resourceData);
        texture2D.Name = "tileset_image";
        
        if(State.TilesetImageTexture.HasValue)
            renderer.UnbindTexture(State.TilesetImageTexture.Value);
        
        State.TilesetImageLocalPath = resource.ResourceLocalPath;
        State.TilesetImageTexture = renderer.BindTexture(texture2D);

        return Result.Ok();
    }

    public Result SubmitTileset()
    {
        var folderResult = PathFormat.GetAssetsPath();
        
        if(folderResult.IsFailure)
            return Result.Fail(folderResult.Error);
        
        var importResult = resourceManager.HasResource(State.TilesetImageLocalPath);

        if (!importResult)
            return Result.Fail($"Resource file {State.TilesetImageLocalPath} does not exist.");
        
        var tilesetDto = new TilesetDefinition()
        {
            AssetId = Ulid.NewUlid(),
            Name = State.TilesetName,
            TextureResourcePath = State.TilesetImageLocalPath,
            TileWidth = State.TileWidth,
            TileHeight = State.TileHeight
        };
        
        var folder = folderResult.Value;

        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        return assetManager.SaveAsset(tilesetDto);
    }
}