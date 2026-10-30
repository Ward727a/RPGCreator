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
using Hexa.NET.ImGui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.Editor.ECS.Flags;
using RPGCreator.Editor.ImGuiLib;
using RPGCreator.Editor.Tools;
using RPGCreator.Editor.UI.Models.Widgets;
using RPGCreator.RuntimeLib.FNA;
using RPGCreator.RuntimeLib.MethodsExtension;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.ECS.Components;
using RPGCreator.SDK.Inputs;
using RPGCreator.SDK.Scenes;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Types;
using Point = Microsoft.Xna.Framework.Point;
using Vector2 = System.Numerics.Vector2;

namespace RPGCreator.Editor.UI.Controllers.Widgets;

[EditorUiController]
public class GamePreviewController
{
    public GamePreviewModel State { get; }

    private readonly GraphicsDevice _graphicsDevice;
    private readonly ImGuiRenderer _renderer;
    private readonly RenderTargetManager _renderTargetManager;
    private readonly ISceneManager _sceneManager;
    private readonly ToolManager _toolManager;

    private Vector2 _lastPreviewSize;
    private bool _currentlyResizing;
    
    public GamePreviewController(GamePreviewModel state, GraphicsDevice graphicsDevice, ImGuiRenderer renderer, RenderTargetManager renderTargetManager, ISceneManager sceneManager, ToolManager toolManager)
    {
        State = state;
        _graphicsDevice = graphicsDevice;
        _renderer = renderer;
        _renderTargetManager = renderTargetManager;
        _sceneManager = sceneManager;
        _toolManager = toolManager;
    }
    
    public static readonly StringName PreviewTargetName = new("GamePreview");
    
    public void Init()
    {
        State.PreviewSize = Vector2.One;
        State.PreviewTarget = _renderTargetManager.GetOrCreateTarget2D(PreviewTargetName, (int)State.PreviewSize.X, (int)State.PreviewSize.Y);
        State.GamePreviewRef = _renderer.BindTexture(State.PreviewTarget);
    }
    
    public void ResizePreviewWindowIfNeeded(Vector2 availableSize, bool isLeftMouseButtonPressed)
    {
        if(availableSize.X <= 0 || availableSize.Y <= 0)
            return;

        if (availableSize != _lastPreviewSize)
        {
            _lastPreviewSize = availableSize;
            _currentlyResizing = true;
        }

        if (_currentlyResizing && !isLeftMouseButtonPressed)
        {
            // Apply resize there.
            _currentlyResizing = false;
            var lastRenderTarget = State.PreviewTarget;
            
            State.PreviewTarget = _renderTargetManager.SwitchTarget2D(PreviewTargetName, new RenderTarget2D(_graphicsDevice, (int)availableSize.X, (int)availableSize.Y));
            
            _renderer.UnbindTexture(State.GamePreviewRef);
            
            lastRenderTarget.Dispose();
            
            State.GamePreviewRef = _renderer.BindTexture(State.PreviewTarget);
            State.PreviewSize = availableSize;
        }
    }

    public MouseButton LastPressedButton { get; private set; } = MouseButton.None;
    public bool WasClicked { get; private set; } = false;
    
    private Vector2 _delta;
    private Vector2 _lastHoverPosition;
    
    private bool _inPreview = false;

    public void StopHoveringPreview()
    {
        _inPreview = false;
    }
    public void HoverOnPreview(int buttonIndex)
    {
        _inPreview = true;
        _delta = ImGui.GetMousePos() - _lastHoverPosition;
        
        if(_delta.Length() > 1)
        {
            var gridPosition = GetGridPosition();
            _lastHoverPosition = ImGui.GetMousePos();
            
            _toolManager.OnTileHovered(gridPosition.X, gridPosition.Y, LastPressedButton);
        }
    }
    
    public void ClickOnPreview(int mouseIndex)
    {
        if(!_inPreview)
            return;

        LastPressedButton = MouseIndexToButton(mouseIndex);
        WasClicked = true;

        var gridPosition = GetGridPosition();
        
        _toolManager.OnTileClicked(gridPosition.X, gridPosition.Y, LastPressedButton);
        {
            _lastHoverPosition = ImGui.GetMousePos();

            _toolManager.OnTileHovered(gridPosition.X, gridPosition.Y, LastPressedButton);
        }
    }

    public void ReleasedOnPreview(int buttonIndex)
    {
        if(!_inPreview || !WasClicked)
            return;
        
        if(MouseIndexToButton(buttonIndex).HasFlag(LastPressedButton))
            return;
        
        WasClicked = false;
        var gridPosition = GetGridPosition();
        _toolManager.OnTileReleased(gridPosition.X, gridPosition.Y, LastPressedButton);
        LastPressedButton = MouseButton.None;
    }
    
    #region Helpers

    private MouseButton MouseIndexToButton(int index)=>
        index switch
        {
            1 => MouseButton.Left,
            2 => MouseButton.Right,
            3 => MouseButton.Left | MouseButton.Right,
            4 => MouseButton.Middle,
            5 => MouseButton.Left | MouseButton.Middle,
            6 => MouseButton.Right | MouseButton.Middle,
            7 => MouseButton.Left | MouseButton.Right | MouseButton.Middle,
            _ => MouseButton.None
        };

    private readonly QueryDescription _cameraQuery =
        new QueryDescription().WithAll<CameraComponent, EditorCameraFlag, TransformComponent>();

    private (CameraComponent camComp, TransformComponent transformComponent) GetCamera()
    {
        var camComp = default(CameraComponent);
        var transformComponent = default(TransformComponent);
        _sceneManager.Simulation.CurrentWorld?.Query(in _cameraQuery,
            (ref CameraComponent cameraComponent, ref TransformComponent transComponent) =>
            {
                camComp = cameraComponent;
                transformComponent = transComponent;
            });
        return (camComp, transformComponent);
    }

    private Point GetGridPosition()
    {
        var cam = GetCamera();
        
        var viewMatrix = CameraComponent.GetViewMatrix(in cam.camComp, in cam.transformComponent);
        Matrix.Invert(ref viewMatrix, out var invertedMatrix);
    
        var absoluteClickPosition = ImGui.GetMousePos();
        var topLeftPosition = ImGui.GetItemRectMin();
        var relativeClickPosition = absoluteClickPosition - topLeftPosition;

        var worldPosition = Vector2.Transform(relativeClickPosition, invertedMatrix.ToNumericFast());
    
        var gridX = (int)Math.Floor(worldPosition.X / 32);
        var gridY = (int)Math.Floor(worldPosition.Y / 32);
        
        return new Point(gridX, gridY);
    }
    
    #endregion
}