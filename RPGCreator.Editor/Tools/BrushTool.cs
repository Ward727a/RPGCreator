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
using Microsoft.Xna.Framework;
using RPGCreator.SDK.Inputs;
using RPGCreator.SDK.Scenes;
using RPGCreator.SDK.Services.Map;
using RPGCreator.SDK.Tools;
using RPGCreator.SDK.Tools.ParameterTypes;

namespace RPGCreator.Editor.Tools;

[Tool("rgpc://tools/brush")]
public class BrushTool(IState<SceneState> sceneState, ISceneManager sceneManager) : Tool
{
    public override string DisplayName => "Brush";
    
    private readonly IntParameter _sizeParameter = new("Size", "Brush size", 1, 1, 12, 1);

    public override IImmutableList<IToolParameter> GetDefaultParameters()
    {
        return [_sizeParameter];
    }

    private bool _leftClicked;
    private bool _rightClicked;
    
    public override void ClickedAt(IToolLogic logic, Point gridPosition, MouseButton button = MouseButton.Left)
    {
        var activeLayerIndex = sceneState.Value.ActiveLayerIndex;
        var activeLayer = sceneManager.GetLayer(activeLayerIndex);

        if (activeLayer == null) return;

        if (button == MouseButton.Left)
        {
            ApplyWithSize(logic, activeLayer, gridPosition);
            _leftClicked = true;
        }
        else if (button == MouseButton.Right)
        {
            EraseWithSize(logic, activeLayer, gridPosition);
            _rightClicked = true;
        } else if (button == MouseButton.Middle)
        {
            logic.Pick(activeLayer, gridPosition);
            ReloadPhantom(activeLayer, logic, gridPosition, button);
        }
    }

    public override void ReleasedAt(IToolLogic logic, Point gridPosition, MouseButton button = MouseButton.Left)
    {
        if(button == MouseButton.Left && _leftClicked)
            _leftClicked = false;
        else if(button == MouseButton.Right && _rightClicked)
            _rightClicked = false;
    }

    private IToolLogic? _lastLogic;
    private IReadOnlyList<Point> _lastHoveredPosition = [];
    
    public override void HoveredAt(IToolLogic logic, Point gridPosition, MouseButton button = MouseButton.Left)
    {
        var activeLayerIndex = sceneState.Value.ActiveLayerIndex;
        var activeLayer = sceneManager.GetLayer(activeLayerIndex);

        if (activeLayer == null) return;
        
        if (_leftClicked)
        {
            ApplyWithSize(logic, activeLayer, gridPosition);
        }
        else if (_rightClicked)
        {
            EraseWithSize(logic, activeLayer, gridPosition);
        }
        
        if (_lastHoveredPosition.Count > 0 && _lastHoveredPosition[0] == gridPosition) return;
        
        var phantomLayer = sceneManager.EnsurePhantomLayer(activeLayer.LayerDefinition.DataType);

        if (phantomLayer == null) return;

        foreach (var point in _lastHoveredPosition)
        {
            EraseWithSize(_lastLogic!, phantomLayer, point);
        }

        _lastHoveredPosition = ApplyWithSize(logic, phantomLayer, gridPosition, true);
        _lastLogic = logic;
    }

    private void ReloadPhantom(IMapLayer activeLayer, IToolLogic logic, Point gridPosition, MouseButton button)
    {
        var phantomLayer = sceneManager.EnsurePhantomLayer(activeLayer.LayerDefinition.DataType);

        if (phantomLayer == null) return;

        foreach (var point in _lastHoveredPosition)
        {
            EraseWithSize(_lastLogic!, phantomLayer, point);
        }

        _lastHoveredPosition = [];
        HoveredAt(logic, gridPosition, button);
    }

    private List<Point> ApplyWithSize(IToolLogic logic, IMapLayer mapLayer, Point originPosition, bool forPhantom = false)
    {
        var size = _sizeParameter.ValueRef;
        
        var points = new List<Point>();

        if (size == 1)
            points.AddRange(logic.Apply(mapLayer, originPosition, forPhantom));
        else
        {
            int offset = -((size - 1) / 2);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    points.AddRange(logic.Apply(mapLayer, originPosition + new Point(offset + x, offset + y), forPhantom));
                }
            }
        }

        return points;
    }

    private void EraseWithSize(IToolLogic logic, IMapLayer layer, Point originPosition)
    {
        var size = _sizeParameter.ValueRef;

        if (size == 1)
            logic.Erase(layer, originPosition);
        else
        {
            int offset = -((size - 1) / 2);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    logic.Erase(layer, originPosition + new Point(offset + x, offset + y));
                }
            }
        }
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
            EraseWithSize(_lastLogic!, phantomLayer, point);
        }
    }
}