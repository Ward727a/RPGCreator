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
using RPGCreator.Editor.UI.Models.Widgets.AssetsCreator;
using RPGCreator.SDK.Assets.Map;
using RPGCreator.SDK.Scenes;
using RPGCreator.SDK.Services.Map;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Types;

namespace RPGCreator.Editor.UI.Controllers.Widgets.AssetsCreator;

[EditorUiController]
public class MapLayerCreatorController(MapLayerCreatorModel model, ILayerRegistry layerRegistry, ISceneManager sceneManager, IState<SceneState> sceneState)
{
    public readonly MapLayerCreatorModel State = model;
    
    public IReadOnlyDictionary<URN, (string displayName, Type type)> GetLayersTypes() => layerRegistry.GetLayerTypes();

    public void SelectLayerType(URN urn)
    {
        if (urn == URN.Empty || !layerRegistry.GetLayerTypes().ContainsKey(urn))
        {
            State.SelectedLayerType = URN.Empty;
            State.SelectedLayerTypeData = ("none", typeof(void));
            return;
        }
        
        State.SelectedLayerType = urn;
        State.SelectedLayerTypeData = layerRegistry.GetLayerTypes()[urn];
    }
    
    public string GetSelectedLayerTypeName() => State.SelectedLayerTypeData.displayName;
    
    public bool IsLayerTypeSelected(URN urn) => urn == State.SelectedLayerType;

    public void CreateLayer()
    {
        if (string.IsNullOrWhiteSpace(State.LayerName))
        {
            return;
        }

        var nextZIndex = 0;
        if (sceneManager.ActiveLayers.Count > 0)
        {
            int currentMax = sceneManager.ActiveLayers[^1].LayerDefinition.ZIndex;
            nextZIndex = currentMax < int.MaxValue - 1 ? currentMax + 1 : currentMax;
        }

        var mapLayer = new MapLayerDefinition()
        {
            Name = State.LayerName,
            DataType = State.SelectedLayerType,
            ZIndex = nextZIndex,
            Size = sceneState.Value.MapSize
        };
        
        sceneManager.AddNewLayer(mapLayer);
    }
}