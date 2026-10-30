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
using System.Diagnostics.Contracts;
using System.Linq;
using RPGCreator.Editor.UI.Models.Widgets;
using RPGCreator.SDK.Assets.Map;
using RPGCreator.SDK.Services.Assets;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Types;

namespace RPGCreator.Editor.UI.Controllers.Widgets;

[EditorUiController]
public class MapExplorerController
{
    public readonly MapExplorerModel State;
    private readonly IAssetManager _assetManager;
    
    private List<Ulid> _maps;
    private bool _needRefreshMap = true;

    public MapExplorerController(MapExplorerModel state, IAssetManager assetManager)
    {
        State = state;
        _assetManager = assetManager;
    }

    public void SetNeedRefresh()
    {
        _needRefreshMap = true;
    }
    
    public void RefreshList()
    {
        if(!_needRefreshMap)
            return;
        _maps = [.. _assetManager.ListAssets<MapDefinition>()];
        State.Maps = _maps;
        _needRefreshMap = false;
    }

    [Pure]
    public Result<MapDefinition> GetMapDefinition(Ulid mapId)
    {
       return _assetManager.GetAsset<MapDefinition>(mapId);
    }
}