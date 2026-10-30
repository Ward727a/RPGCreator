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
using RPGCreator.Editor.Tools;
using RPGCreator.Editor.UI.Models.Widgets;
using RPGCreator.SDK.Scenes;
using RPGCreator.SDK.Services.Map;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Types;

namespace RPGCreator.Editor.UI.Controllers.Widgets;

[EditorUiController]
public class LayerExplorerController(ISceneManager sceneManager, IDispatcher dispatcher, IState<SceneState> state, ToolManager toolManager, LayerExplorerModel model)
{

    public LayerExplorerModel Model => model;
    public Ulid ActualMap => state.Value.CurrentMapId;
    
    public bool HasMapLoaded() => sceneManager.Simulation.CurrentWorld != null;

    public IReadOnlyList<IMapLayer> GetLayers() => sceneManager.ActiveLayers;

    public void OnLayerSelected(int index, URN layerType)
    {
        
        if(index == state.Value.ActiveLayerIndex)
            return;
        
        var lastIndex = state.Value.ActiveLayerIndex;
        if (GetLayers().Count > lastIndex)
        {
            var lastLayer = GetLayers()[lastIndex];
            lastLayer.SetVisibility(sceneManager.Simulation, model._lastActiveDefaultVisibility);
        }

        toolManager.SelectToolLogic(layerType);
        dispatcher.Dispatch(new SceneStateActions.ActiveLayerChanged(index, layerType));
        model._lastActiveDefaultVisibility = sceneManager.ActiveLayers[index].IsVisible;
        sceneManager.ActiveLayers[index].SetVisibility(sceneManager.Simulation, true);
    }
    public int GetActiveLayerIndex() => state.Value.ActiveLayerIndex;

    public void SetLayerVisibility(IMapLayer mapLayer, bool isVisible)
    {
        mapLayer.SetVisibility(sceneManager.Simulation, isVisible);
    }
}