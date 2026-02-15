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
using System.Numerics;
using System.Reflection;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.RTP.Extensions;
using RPGCreator.RTP.GameUI.BaseControls;
using RPGCreator.RTP.GameUI.BaseControls.Containers;
using RPGCreator.RTP.Viewport;
using RPGCreator.SDK;
using RPGCreator.SDK.Editor.Rendering;
using RPGCreator.SDK.Inputs;
using RPGCreator.SDK.Logging;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace RPGCreator.RTP.GameUI.Layers;

public class BaseLayer
{
    
    #region STATIC METHODS
    
    public static byte[] GetShaderByteCode(string fileName)
    {
        // Path should be like : Namespace.Folder.File.Extension
        string resourceName = $"RPGCreator.RTP.Resources.{fileName}";
    
        var assembly = Assembly.GetExecutingAssembly();
        using (Stream stream = assembly.GetManifestResourceStream(resourceName))
        {
            if (stream == null)
                throw new Exception($"Resource {resourceName} not found in assembly.");

            using (MemoryStream ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                return ms.ToArray();
            }
        }
    }
    
    #endregion
    
    public string Name { get; set; }
    public bool IsVisible { get; set; } = true;
    public bool IsEnabled { get; set; } = true;
    public int ZOrder { get; set; } = 0;
    public BaseControl? RootControl { get; private set; }
    
    public Matrix? TransformMatrix { get; set; } = null;
    
    public readonly RasterizerState ClippingRasterizerState = new RasterizerState { ScissorTestEnable = true };
    
    
    private readonly GraphicsDevice _graphicsDevice;
    private readonly SpriteBatch sb;

    private BaseGameViewport ParentViewport;
    
    private UiRenderer _uiRenderer;
    
    public void SetParentViewport(BaseGameViewport viewport)
    {
        ParentViewport = viewport;
        (ParentViewport as GameViewport).SetRenderer(_uiRenderer);
    }
    
    public BaseLayer(string name, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, FontSystem fontSystem)
    {
        Name = name;
        _graphicsDevice = graphicsDevice;
        sb = spriteBatch;
        
        _mouseState = EngineStates.ViewportMouseState;
        _keyboardState = EngineStates.ViewportKeyboardState;
        _uiRenderer = new UiRenderer(_graphicsDevice, sb, fontSystem, Matrix4x4.Identity);
    }
    
    public virtual void Draw(TimeSpan deltaTime)
    {
        if(RootControl == null || !IsVisible) return;
        
        RootControl.Draw(deltaTime);
    }

    private readonly IMouseState _mouseState;
    private int _verticalWheelDelta = 0;
    private int _horizontalWheelDelta = 0;
    internal Vector2 _mousePosition;
    private bool _isLeftButtonDown;
    private bool _isMiddleButtonDown;
    private bool _isRightButtonDown;
    private readonly IKeyboardState _keyboardState;
    
    public virtual void Update(TimeSpan deltaTime)
    {
        if(RootControl == null || !IsEnabled) return;
        _mousePosition = _mouseState.Position.ToXnaFast();
        _isLeftButtonDown = _mouseState.LeftButtonPressed;
        _isMiddleButtonDown = _mouseState.MiddleButtonPressed;
        _isRightButtonDown = _mouseState.RightButtonPressed;
        _verticalWheelDelta = _mouseState.WheelDelta;
        _horizontalWheelDelta = _mouseState.HorizontalWheelDelta;
        
        UpdateControl(RootControl, deltaTime);
    }

    public virtual void UpdateControl(BaseControl control, TimeSpan deltaTime)
    {
        if (!control.AbsoluteEnabled) return;
        
        var isHandled = false;
        control.Update(deltaTime);
        control.UpdateMouseInput(_mousePosition, _isLeftButtonDown, _isMiddleButtonDown, _isRightButtonDown, ref _verticalWheelDelta, ref _horizontalWheelDelta, ref isHandled);
        if (BaseControl.FocusedControl != null)
        {
            BaseControl.FocusedControl.UpdateKeyboardInput(_keyboardState);
        }
    }
    
    public void SetRootControl(BaseControl root)
    {
        RootControl?.OnInvalidate -= RefreshTarget;
        RootControl?.OwningLayer = null;
        RootControl?.Renderer = null;
        RootControl = root;
        RootControl.Renderer = _uiRenderer;
        RootControl.OwningLayer = this;
        RootControl?.OnInvalidate += RefreshTarget;
    }

    public void RefreshTarget()
    {
        ParentViewport.DoNewFrame();
    }
}