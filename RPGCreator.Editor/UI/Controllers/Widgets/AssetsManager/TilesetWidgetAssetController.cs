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
using Hexa.NET.ImGui;
using RPGCreator.Editor.Services;
using RPGCreator.Editor.UI.Models.Widgets.AssetsManager;
using RPGCreator.SDK.Assets.Tileset;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Resources;
using RPGCreator.SDK.Services.Assets;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Types;

namespace RPGCreator.Editor.UI.Controllers.Widgets.AssetsManager;

[EditorUiController]
public class TilesetWidgetAssetController
{
    private readonly IAssetManager _assetManager;
    private readonly IResourceManager _resourceManager;
    private readonly IThumbnailCacheService _thumbnailCacheService;
    public TilesetWidgetAssetModel State { get; private set; }

    private readonly List<TilesetDefinition> _tilesets = [];
    public bool NeedClear { get; private set; } = false;

    public TilesetWidgetAssetController(TilesetWidgetAssetModel state, IAssetManager assetManager, IResourceManager resourceManager, IThumbnailCacheService thumbnailCacheService)
    {
        _assetManager = assetManager;
        _resourceManager = resourceManager;
        _thumbnailCacheService = thumbnailCacheService;
        State = state;
        State.Tilesets = _tilesets;
    }

    public void SetNeedClear() => NeedClear = true;

    public void RefreshList()
    {
        SetNeedClear();
        
        _tilesets.Clear();

        var ids = _assetManager.ListAssets<TilesetDefinition>();

        foreach (var id in ids)
        {
            _assetManager.GetAsset<TilesetDefinition>(id)
                .OnFailure((err) =>
                {
                    Logger.Error("Couldn't load tileset {id} definition due to error: {err}", id, err);
                })
                .OnSuccess(_tilesets.Add);
        }
    }

    public Result<ImTextureRef> GetThumbnail(TilesetDefinition tileset)
    {
        var resourceLocalPath = tileset.TextureResourcePath;

        var resourceObjectResult = _resourceManager.GetResourceObject(resourceLocalPath);
        if(resourceObjectResult.IsFailure)
            return resourceObjectResult.Cast<ImTextureRef>();
        
        var resourceObject = resourceObjectResult.Value;
        var resourceId = resourceObject.ResourceId;
        
        return _thumbnailCacheService.GetThumbnail(resourceId);
    }
    
    public void SelectTileset(TilesetDefinition tileset) => State.SelectedTileset = tileset;
    public bool HasSelectedTileset() => State.SelectedTileset != null;
    public Ulid GetSelectedTilesetId() => State.SelectedTileset?.AssetId ?? Ulid.Empty;
}