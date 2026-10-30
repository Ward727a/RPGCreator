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

using System.Collections.Generic;
using System.Collections.Immutable;
using Fluxor;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Inputs;
using Microsoft.Xna.Framework;
using RPGCreator.SDK.Scenes;
using RPGCreator.SDK.Tools;

namespace RPGCreator.Editor.Tools;

[Tool("rpgc://tools/stamp")]
public class StampTool(IState<SceneState> sceneState, ISceneManager sceneManager) : Tool
{
    public override string DisplayName => "Stamp Tool";

    public override HashSet<ToolCapability> Capabilities { get; } = [RpgcToolCapabilities.MultiTileSelection];

    public override IImmutableList<IToolParameter> GetDefaultParameters()
    {
        return [];
    }

    public override void ClickedAt(IToolLogic logic, Point gridPosition, MouseButton button = MouseButton.Left)
    {
        var activeLayerIndex = sceneState.Value.ActiveLayerIndex;
        var activeLayer = sceneManager.GetLayer(activeLayerIndex);

        if (activeLayer == null) return;

        if (button == MouseButton.Left)
        {
            logic.Apply(activeLayer, gridPosition);
        }
        else if (button == MouseButton.Right)
        {
            logic.Erase(activeLayer, gridPosition);
        }

        Logger.Debug("Stamp tool used at {position} with mouse {button}", gridPosition.ToString() ?? "<unknown>", button);
    }

    public override void ReleasedAt(IToolLogic logic, Point gridPosition, MouseButton button = MouseButton.Left)
    {
        Logger.Debug("Stamp tool released at {position} with mouse {button}", gridPosition.ToString() ?? "<unknown>",
            button);
    }

    private IToolLogic? _lastLogic;
    private IReadOnlyList<Point> _lastHoveredPosition = [];

    public override void HoveredAt(IToolLogic logic, Point gridPosition, MouseButton button = MouseButton.Left)
    {
        // Here we get the first element because it's **always** relative to the gridPosition we had given before.
        if (_lastHoveredPosition.Count > 0 && _lastHoveredPosition[0] == gridPosition) return;

        var activeLayerIndex = sceneState.Value.ActiveLayerIndex;
        var activeLayer = sceneManager.GetLayer(activeLayerIndex);

        if (activeLayer == null) return;
        
        var phantomLayer = sceneManager.EnsurePhantomLayer(activeLayer.LayerDefinition.DataType);

        if (phantomLayer == null) return;

        foreach (var point in _lastHoveredPosition)
        {
            _lastLogic?.Erase(phantomLayer, point, true);
        }

        _lastHoveredPosition = logic.Apply(phantomLayer, gridPosition, true);
        _lastLogic = logic;
    }

    public override void OnDeactivate()
    {
        if (_lastHoveredPosition.Count == 0 || _lastLogic == null) return;
        
        var activeLayerIndex = sceneState.Value.ActiveLayerIndex;
        var activeLayer = sceneManager.GetLayer(activeLayerIndex);

        if (activeLayer == null) return;

        var phantomLayer = sceneManager.EnsurePhantomLayer(activeLayer.LayerDefinition.DataType);

        if (phantomLayer == null) return;

        foreach (var point in _lastHoveredPosition)
        {
            _lastLogic?.Erase(phantomLayer, point, true);
        }
    }
}