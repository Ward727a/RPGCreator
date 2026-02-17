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
using System.Numerics;
using RPGCreator.RTP.GameUI.Visual;
using RPGCreator.SDK.Inputs;
using RPGCreator.SDK.Types;

namespace RPGCreator.RTP.GameUI.Controls;

public abstract class BaseControl
{
    public bool IsVisible { get; set; } = true;
    public bool IsHitTestVisible { get; set; } = true;
    /// <summary>
    /// If true, the control and its children will only receive mouse events if the mouse is within this control's bounds.<br/>
    /// If false, the control and its children can receive mouse events even if the mouse is outside this control's bounds.
    /// </summary>
    public bool ClipHitTestToBounds { get; set; } = true;
    public BaseVisual Visual { get; protected set; }
    
    protected bool _isMouseOver = false;
    public bool IsMouseOver => _isMouseOver;
    
    protected bool _isFocused = false;
    public bool IsFocused => _isFocused;
    
    protected bool _isPressed = false;
    public bool IsPressed => _isPressed;
    
    protected bool _isDragging = false;
    public bool IsDragging => _isDragging;

    #region Inheritance and Hierarchy
    protected readonly List<BaseControl> _children = new List<BaseControl>();
    public IReadOnlyList<BaseControl> Children => _children.AsReadOnly();
    public BaseControl? Parent { get; private set; }
    
    public void AddChild(BaseControl child)
    {
        if (child.Parent != null)
            throw new InvalidOperationException("The control already has a parent.");
        
        _children.Add(child);
        child.Parent = this;
        child.Visual.AddToParent(Visual);
        child.CallAddedToParent();
    }
    
    public void RemoveChild(BaseControl child)
    {
        if (child.Parent != this)
            throw new InvalidOperationException("The control is not a child of this parent.");
        
        _children.Remove(child);
        child.Parent = null;
        child.Visual.RemoveFromParent();
        child.CallRemovedFromParent();
    }
    
    public void ClearChildren()
    {
        foreach (var child in _children)
        {
            child.Parent = null;
            child.Visual.RemoveFromParent();
            child.CallRemovedFromParent();
        }
        _children.Clear();
    }
    #endregion

    public BaseControl? GetControlAt(Vector2 screenPosition)
    {
        if (!IsVisible)
            return null;

        var isOverParent = Visual.HitTest(screenPosition);
        if (!isOverParent && ClipHitTestToBounds)
            return null;
        
        for (int i = _children.Count - 1; i >= 0; i--)
        {
            var child = _children[i];
            var result = child.GetControlAt(screenPosition);
            if (result != null)
                return result;
        }
        
        if (IsHitTestVisible && isOverParent)
            return this;
        
        return null;
    }
    
    /// <summary>
    /// Update the control's state (e.g., handle animations, update properties, etc.) before the visual is updated and drawn. This method is called every frame and should contain any logic that needs to be processed regularly for the control.
    /// </summary>
    public abstract void OnUpdate();
    
    /// <summary>
    /// Synchronise the visual state (if for example the control has properties that affect the visual, such as a button's pressed state) before the visual is updated and drawn.
    /// </summary>
    public abstract void SyncVisual();
    
    public virtual void UpdateControl(Rect parentBounds, Rect screenBounds)
    {
        OnUpdate();
        
        SyncVisual();
        
        Visual.UpdateVisual(parentBounds, screenBounds);

        Rect childrenArea = new Rect(0, 0, Visual.LocalBounds.Width, Visual.LocalBounds.Height);
        foreach (var child in _children)
        {
            child.UpdateControl(childrenArea, screenBounds);
        }
    }
    
    public virtual void DrawControl(UiRendererContext renderer)
    {
        if (!IsVisible)
            return;
        
        Visual.DrawVisual(renderer);

        foreach (var child in _children)
        {
            child.DrawControl(renderer);
        }
    }
    
    #region Events

    #region Control Events
    
    /// <summary>
    /// Triggered when the control is added to a parent control.<br/>
    /// This event is called after the control has been added to the parent's children list and the Parent property has been set.<br/>
    /// It is not triggered when the control is added as a root control (i.e., without a parent).
    /// </summary>
    public event EventHandler? OnAddedToParent;
    
    /// <summary>
    /// Triggered when the control is removed from its parent control.<br/>
    /// This event is called after the control has been removed from the parent's children list and the Parent property has been cleared.<br/>
    /// It is not triggered when the control is removed as a root control (i.e., without a parent).
    /// </summary>
    public event EventHandler? OnRemovedFromParent;
    
    /// <summary>
    /// Triggered when the control is added as a root control (i.e., without a parent).<br/>
    /// This event is called after the control has been added to the UI system as a root control and is now part of the active UI hierarchy.<br/>
    /// It is not triggered when the control is added to a parent control.
    /// </summary>
    public event EventHandler? OnAddedAsRoot;
    
    /// <summary>
    /// Triggered when the control is removed as a root control (i.e., without a parent).<br/>
    /// This event is called after the control has been removed from the UI system as a root control and is no longer part of the active UI hierarchy.<br/>
    /// It is not triggered when the control is removed from a parent control.
    /// </summary>
    public event EventHandler? OnRemovedAsRoot;
    
    /// <summary>
    /// Triggered when the control is brought to the front of the UI hierarchy (i.e., becomes the topmost control).<br/>
    /// This event can only be called on controls that are a root control (i.e., without a parent) and is called after the control has been moved to the front of the UI hierarchy.
    /// </summary>
    public event EventHandler? OnBringToFront;
    
    /// <summary>
    /// Triggered when the control gains focus.<br/>
    /// This event is called after the control has become the focused control in the UI system.<br/>
    /// </summary>
    public event EventHandler? OnFocusGained;
    
    /// <summary>
    /// Triggered when the control loses focus.<br/>
    /// This event is called after the control has lost focus and is no longer the focused control in the UI system.<br/>
    /// </summary>
    public event EventHandler? OnFocusLoss;
    
    #region Non-Virtual Events
    
    public void CallAddedToParent()
    {
        OnAddedToParent?.Invoke(this, EventArgs.Empty);
        OnAddToParent();
    }
    
    public void CallRemovedFromParent()
    {
        OnRemovedFromParent?.Invoke(this, EventArgs.Empty);
        OnRemoveFromParent();
    }
    
    public void CallAddedAsRoot()
    {
        OnAddedAsRoot?.Invoke(this, EventArgs.Empty);
        OnAddAsRoot();
    }
    
    public void CallRemovedAsRoot()
    {
        OnRemovedAsRoot?.Invoke(this, EventArgs.Empty);
        OnRemoveAsRoot();
    }
    
    public void CallBringToFront()
    {
        OnBringToFront?.Invoke(this, EventArgs.Empty);
        OnMovedToFront();
    }
    
    public void CallFocusGain()
    {
        OnFocusGained?.Invoke(this, EventArgs.Empty);
        OnFocusGain();
    }
    
    public void CallFocusLost()
    {
        OnFocusLoss?.Invoke(this, EventArgs.Empty);
        OnFocusLost();
    }
    
    #endregion
    
    #region Virtual Events
    
    protected virtual void OnAddToParent(){}
    protected virtual void OnRemoveFromParent(){}
    protected virtual void OnAddAsRoot(){}
    protected virtual void OnRemoveAsRoot(){}
    protected virtual void OnMovedToFront(){}
    protected virtual void OnFocusGain(){}
    protected virtual void OnFocusLost(){}
    
    #endregion
    
    #endregion
    
    #region Mouse Events
    /// <summary>
    /// Triggered when the control is clicked with a mouse button.<br/>
    /// Returns the button that was clicked.
    /// </summary>
    public event EventHandler<MouseButton>? OnMouseClicked;
    
    /// <summary>
    /// Triggered when the control is double-clicked with a mouse button.<br/>
    /// Returns the button that was double-clicked.
    /// </summary>
    public event EventHandler<MouseButton>? OnMouseDoubleClicked;
    
    /// <summary>
    /// Triggered when a mouse button is pressed down on the control.<br/>
    /// Returns the button that was pressed.
    /// </summary>
    public event EventHandler<MouseButton>? OnMousePressed;
    
    /// <summary>
    /// Triggered when a mouse button is released on the control.<br/>
    /// Returns the button that was released.
    /// </summary>
    public event EventHandler<MouseButton>? OnMouseReleased;
    
    /// <summary>
    /// Triggered when the mouse cursor enters the control's area.
    /// </summary>
    public event EventHandler? OnMouseEntered;
    
    /// <summary>
    /// Triggered when the mouse cursor leaves the control's area.
    /// </summary>
    public event EventHandler? OnMouseLeaved;
    
    /// <summary>
    /// Triggered when the mouse cursor moves while over the control.<br/>
    /// Returns the delta position of the mouse movement since the last event.
    /// </summary>
    public event EventHandler<Vector2>? OnMouseMoved;
    
    /// <summary>
    /// Triggered when the mouse wheel is scrolled while over the control.<br/>
    /// Returns the amount of scroll (positive for scrolling up, negative for scrolling down).
    /// </summary>
    public event EventHandler<int>? OnMouseWheelScrolled;
    
    #region Non-Virtual Events
    public void CallMouseClick(MouseButton button)
    {
        OnMouseClicked?.Invoke(this, button);
        OnMouseClick(button);
    }
    
    public void CallMouseDoubleClick(MouseButton button)
    {
        OnMouseDoubleClicked?.Invoke(this, button);
        OnMouseDoubleClick(button);
    }
    
    public void CallMouseDown(MouseButton button)
    {
        OnMousePressed?.Invoke(this, button);
        _isPressed = true;
        OnMouseDown(button);
    }
    
    public void CallMouseUp(MouseButton button)
    {
        OnMouseReleased?.Invoke(this, button);
        _isPressed = false;
        OnMouseUp(button);
    }
    
    public void CallMouseEnter()
    {
        OnMouseEntered?.Invoke(this, EventArgs.Empty);
        _isMouseOver = true;
        OnMouseEnter();
    }
    
    public void CallMouseLeave()
    {
        OnMouseLeaved?.Invoke(this, EventArgs.Empty);
        _isMouseOver = false;
        OnMouseLeave();
    }
    
    public void CallMouseMove(Vector2 deltaPosition)
    {
        OnMouseMoved?.Invoke(this, deltaPosition);
        OnMouseMove(deltaPosition);
    }
    
    public void CallMouseWheelScroll(int scrollAmount)
    {
        OnMouseWheelScrolled?.Invoke(this, scrollAmount);
        OnMouseWheelScroll(scrollAmount);
    }
    #endregion
    
    #region Virtual Events
    
    protected virtual void OnMouseClick(MouseButton button) {}
    protected virtual void OnMouseDoubleClick(MouseButton button) {}
    protected virtual void OnMouseDown(MouseButton button) {}
    protected virtual void OnMouseUp(MouseButton button) {}
    protected virtual void OnMouseEnter() {}
    protected virtual void OnMouseLeave() {}
    protected virtual void OnMouseMove(Vector2 deltaPosition) {}
    protected virtual void OnMouseWheelScroll(int scrollAmount) {}
    
    #endregion
    
    #endregion
    
    #region Mouse Drag Events
    
    public event EventHandler? OnDragStarted;
    public event EventHandler? OnDragEnded;
    public event EventHandler<Vector2>? OnDragMoved;
    
    #region Non-Virtual Events
    
    public void CallDragStart()
    {
        OnDragStarted?.Invoke(this, EventArgs.Empty);
        _isDragging = true;
        OnDragStart();
    }
    
    public void CallDragEnd()
    {
        OnDragEnded?.Invoke(this, EventArgs.Empty);
        _isDragging = false;
        OnDragEnd();
    }
    
    public void CallDragMove(Vector2 deltaPosition)
    {
        OnDragMoved?.Invoke(this, deltaPosition);
        OnDragMove(deltaPosition);
    }
    
    #endregion
    
    #region Virtual Events

    protected virtual void OnDragStart()
    {
    }
    
    protected virtual void OnDragEnd()
    {
    }
    
    protected virtual void OnDragMove(Vector2 deltaPosition)
    {
    }

    #endregion
    
    #endregion
    
    #endregion
}