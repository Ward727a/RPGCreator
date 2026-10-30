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
using Fluxor;
using RPGCreator.SDK.Modules.Editor.Ui;
using RPGCreator.SDK.Scenes;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Types;

namespace RPGCreator.Editor.UI.Controllers.EditorTools;

[EditorUiController]
public class PayloadToolController(IState<SceneState> sceneState)
{
    private const string TILESET_TYPE_PARAMETER = "tileset_type";

    private record ChildItem(URN ViewId, URN TilesetTypeId)
    {
        public Ulid Id { get; } = Ulid.NewUlid();
    };

    private URN _lastActiveLayerType;
    private ChildItem? _lastActiveLayerChildItem;
    private readonly Dictionary<URN, ChildItem> _childItems = new();

    public void AddPayloadUi(URN childViewId, EditorUiPayloadData payloadData)
    {
        if (payloadData.TryGet<URN>(TILESET_TYPE_PARAMETER, out var tilesetTypeUrn))
        {
            var childItem = new ChildItem(childViewId, tilesetTypeUrn);
            _childItems[tilesetTypeUrn] = childItem;
        }
    }

    public void RemovePayloadUi(URN childViewId)
    {
        _childItems.Remove(childViewId);
    }

    public URN GetPayloadUi(URN tilesetId)
    {
        if(_childItems.TryGetValue(tilesetId, out var item))
            return item.ViewId;
        return URN.Empty;
    }

    public URN GetPayloadUi()
    {
        if(_lastActiveLayerChildItem != null && _lastActiveLayerType == sceneState.Value.ActiveLayerType)
            return _lastActiveLayerChildItem.ViewId;
        
        _lastActiveLayerChildItem = _childItems.GetValueOrDefault(sceneState.Value.ActiveLayerType);
        _lastActiveLayerType = sceneState.Value.ActiveLayerType;
        return _lastActiveLayerChildItem?.ViewId ?? URN.Empty;
    }
    
    public URN GetLayerType() => _lastActiveLayerType;
}