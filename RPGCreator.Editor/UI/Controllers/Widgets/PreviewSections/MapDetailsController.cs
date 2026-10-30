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
using Arch.Core;
using Fluxor;
using RPGCreator.Editor.UI.Models.Widgets.PreviewSections;
using RPGCreator.RuntimeLib.ECS.Components;
using RPGCreator.SDK.Assets.Map;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Scenes;
using RPGCreator.SDK.Services.Assets;
using RPGCreator.Shared.Attributes;

namespace RPGCreator.Editor.UI.Controllers.Widgets.PreviewSections;

[EditorUiController]
public class MapDetailsController(IState<SceneState> sceneState, IAssetManager assetManager, MapDetailsModel model, ISceneManager sceneManager)
{
    public readonly MapDetailsModel State = model;
    private static readonly ScopedLogger Logger = SDK.Common.Logging.Logger.ForContext<MapDetailsController>();
    public bool HasMapLoaded() => sceneState.Value.CurrentMapId != Ulid.Empty;

    private MapDefinition? _currentMapDefinition = null;

    public MapDefinition? GetMapDefinition()
    {
        if (!HasMapLoaded())
            return null;
        
        if(_currentMapDefinition != null && _currentMapDefinition.AssetId == sceneState.Value.CurrentMapId)
            return _currentMapDefinition;

        return assetManager.GetAsset<MapDefinition>(sceneState.Value.CurrentMapId)
            .Match(
            definition =>
            {
                _currentMapDefinition = definition;
                State.Load(_currentMapDefinition);
                return definition;
            },
            (err) =>
            {
                Logger.Error(err);
                return null;
            });
    }

    public void SetName()
    {
        if(!HasMapLoaded() || _currentMapDefinition == null)
            return;
        State.Update(ref _currentMapDefinition);
    }
    
    public void SetBackgroundColor()
    {
        if(!HasMapLoaded() || _currentMapDefinition == null)
            return;
        State.Update(ref _currentMapDefinition);
        sceneManager.Simulation.CurrentWorld?.Query(in new QueryDescription().WithAll<SkyComponent>(),
            (ref SkyComponent skyC) =>
            {
                skyC.Color = _currentMapDefinition.BackgroundColor;
            });
    }
}