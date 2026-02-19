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
using System.Numerics;
using RPGCreator.RTP.GameUI.Controls;
using RPGCreator.RTP.Viewport;
using RPGCreator.SDK.GameUI.Interfaces;
using RPGCreator.SDK.Inputs;
using RPGCreator.SDK.Types;

namespace RPGCreator.RTP.GameUI;

public class UiManager : IUiManager
{
    
    private const float DragThreshold = 5f; // Minimum distance in pixels to start a drag operation.
    
    #region Internal State
    
    private UiViewport _owningViewport;
    private Rect _owningViewportRect;
    private IMouseState _mouseState;
    private bool _isMouseStateInitialized = false;
    
    private UiRendererContext _uiRendererContext;
    public IUiRendererContext UiRendererContext => _uiRendererContext;
    private bool _isRendererInitialized = false;
    
    
    private BaseControl? _lastHoveredControl;
    private BaseControl? _focusedControl;
    private BaseControl? _pressedControl;
    private BaseControl? _draggedControl;
    
    private Vector2 _lastMouseDownPosition;
    
    #endregion

    public UiManager(UiViewport owningViewport)
    {
        _owningViewport = owningViewport;
        _owningViewport.Resized += (_, _) => UpdateViewportRect();
    }
    
    #region Hierarchy
    private readonly List<BaseControl> _rootControls = [];
    public void AddRootControl(BaseControl control)
    {
        _rootControls.Add(control);
        control.CallAddedAsRoot();
    }
    public void RemoveRootControl(BaseControl control)
    {
        _rootControls.Remove(control);
        control.CallRemovedAsRoot();
    }
    public void BringToFront(BaseControl control)
    {
        if (_rootControls.Contains(control))
        {
            _rootControls.Remove(control);
            _rootControls.Add(control);
            control.CallBringToFront();
        }
    }
    #endregion
    
    public void Initialize(IMouseState mouseState, IUiRendererContext? uiRendererContext = null)
    {
        
        if(_isMouseStateInitialized) 
            #if DEBUG 
            throw new System.InvalidOperationException("Mouse state has already been initialized.");
            #else
            return;
            #endif
        _mouseState = mouseState;
        
        _mouseState.ButtonDown += OnMouseButtonDown;
        _mouseState.ButtonUp += OnMouseButtonUp;
        _mouseState.Clicked += OnMouseClicked;
        _mouseState.DoubleClicked += OnMouseDoubleClicked;
        _mouseState.Moved += OnMouseMoved;
        _mouseState.WheelScrolled += OnMouseWheelScrolled;
        _mouseState.HorizontalWheelScrolled += OnMouseHorizontalWheelScrolled;
        _isMouseStateInitialized = true;
    }

    private void UpdateViewportRect()
    {
        _owningViewportRect = new Rect(0, 0, _owningViewport.Size.Width, _owningViewport.Size.Height);
        foreach (var rootControl in _rootControls)
        {
            rootControl.Visual.MarkDirty();
        }
    }
    
    public void InitializeRenderer(UiRendererContext uiRendererContext)
    {
        if(_isRendererInitialized)
            #if DEBUG 
            throw new System.InvalidOperationException("Mouse state has already been initialized.");
            #else
            return;
            #endif
        _uiRendererContext = uiRendererContext;
        _isRendererInitialized = true;
    }

    public void Update(double elapsedTime)
    {
        foreach (var control in _rootControls)
        {
            control.UpdateControl(_owningViewportRect, _owningViewportRect);
        }
    }

    public void Draw()
    {
        if(!_isRendererInitialized)
            #if DEBUG
            throw new System.InvalidOperationException("Renderer must be initialized before drawing.");
            #else
            return;
            #endif
        
        foreach (var rootControl in _rootControls)
        {
            rootControl.DrawControl(_uiRendererContext);
        }
        
        _uiRendererContext.Execute();
    }
    
    #region Mouse Testing Methods
    private BaseControl? FindControlAt(Vector2 position)
    {
        for (int i = _rootControls.Count - 1; i >= 0; i--)
        {
            var hit = _rootControls[i].GetControlAt(position);
            if (hit != null) return hit;
        }
        return null;
    }
    
    #endregion

    #region EVENTS
    private void OnMouseButtonDown(MouseButton button)
    {
        Vector2 mousePos = _mouseState.Position;
        _pressedControl = FindControlAt(mousePos);
        _pressedControl?.CallMouseDown(button);
        _lastMouseDownPosition = mousePos;
    }

    private void OnMouseButtonUp(MouseButton button)
    {
        if (_draggedControl != null)
        {
            _draggedControl.CallDragEnd();
            _draggedControl = null;
        }

        _pressedControl?.CallMouseUp(button);
        _pressedControl = null;
    }

    private void OnMouseClicked(MouseButton button)
    {
        Vector2 mousePos = _mouseState.Position;
        
        BaseControl? clickedControl = FindControlAt(mousePos);

        if (clickedControl != _pressedControl)
            return;
        
        if (button == MouseButton.Left)
        {
            if (_focusedControl != clickedControl)
            {
                _focusedControl?.CallFocusLost();
            }
            _focusedControl = clickedControl;
            _focusedControl?.CallFocusGain();
        }
        
        clickedControl?.CallMouseClick(button);
    }

    private void OnMouseDoubleClicked(MouseButton button)
    {
        Vector2 mousePos = _mouseState.Position;
        
        BaseControl? clickedControl = FindControlAt(mousePos);
        
        if (clickedControl != _pressedControl)
            return;
        
        clickedControl?.CallMouseDoubleClick(button);
    }

    private void OnMouseMoved(Vector2 deltaPosition)
    {
        Vector2 mousePos = _mouseState.Position;
        
        BaseControl? hoveredControl = FindControlAt(mousePos);
        
        if (hoveredControl != _lastHoveredControl)
        {
            _lastHoveredControl?.CallMouseLeave();
            hoveredControl?.CallMouseEnter();
            _lastHoveredControl = hoveredControl;
        }
        
        if (_pressedControl != null)
        {
            if (_draggedControl == null)
            {
                if (Vector2.Distance(mousePos, _lastMouseDownPosition) >= DragThreshold)
                {
                    _draggedControl = _pressedControl;
                    _draggedControl.CallDragStart();
                }
            }
        
            _draggedControl?.CallDragMove(deltaPosition);
        }
    
        hoveredControl?.CallMouseMove(deltaPosition);
    }

    private void OnMouseWheelScrolled(int delta)
    {
        if(_lastHoveredControl != null)
        {
            _lastHoveredControl.CallMouseWheelScroll(delta);
        }
    }

    private void OnMouseHorizontalWheelScrolled(int delta)
    {
        // Not used for now, as the implementation of the horizontal wheel is not working for some reason.
        // As it's also kinda useless, I won't spend time trying to fix it for now.
    }
    #endregion
    
}