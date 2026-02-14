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
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RPGCreator.RTP.GameUI.BaseControls.Containers;

public abstract class ContainerControl : BaseControl
{
    #region Events
    
    public event EventHandler<ControlPropertyChangingEventArgs<bool>>? OnClipsToBoundsChanging;
    public event EventHandler<ControlPropertyChangedEventArgs<bool>>? OnClipsToBoundsChanged;
    
    #endregion
    
    protected override string _Name { get; set; } = "ContainerControl";
    
    private readonly List<BaseControl> _internalComponents = new();
    
    internal void AddInternalComponent(BaseControl component)
    {
        _internalComponents.Add(component);
        component.Parent = (this);
        UpdateChildrenBounds();
    }
    
    internal void InsertInternalComponent(int index, BaseControl component)
    {
        _internalComponents.Insert(index, component);
        component.Parent = (this);
        UpdateChildrenBounds();
    }
    
    internal void RemoveInternalComponent(BaseControl? component = null)
    {
        if (component != null)
        {
            if (_internalComponents.Remove(component))
            {
                component.Parent = (null);
                UpdateChildrenBounds();
            }
        }
        else
        {
            foreach (var internalComponent in _internalComponents)
            {
                internalComponent.Parent = (null);
            }
            _internalComponents.Clear();
            UpdateChildrenBounds();
        }
    }
    
    public bool ClipToBounds
    {
        get;
        set
        {
            if (Equals(value, field)) return;
            var old = field;
            OnClipsToBoundsChanging?.Invoke(this, new (old, value));
            field = value;
            OnClipsToBoundsChanged?.Invoke(this, new (old, value));
        }
    } = false;

    public override void UpdateAnchoredBounds()
    {
        base.UpdateAnchoredBounds();

        foreach (var internalComponent in _internalComponents)
        {
            internalComponent.UpdateAnchoredBounds();
        }
    }

    protected virtual void UpdateChildrenBounds()
    {
        foreach (var internalComponent in _internalComponents)
        {
            internalComponent.RefreshControl();
        }
    }

    public override BaseControl? GetControlAt(Vector2 worldPosition)
    {
        if (IgnoreMouseEvents) return null;
        
        var self = base.GetControlAt(worldPosition);

        if (self != this) return self;
        
        for (int i = _internalComponents.Count - 1; i >= 0; i--)
        {
            var childResult = _internalComponents[i].GetControlAt(worldPosition);
            if (childResult != null) return childResult;
        }
            
        return this;

    }

    public override void Arrange()
    {
        base.Arrange();
        foreach (var internalComponent in _internalComponents)
        {
            internalComponent.Arrange();
        }
    }

    public override void Measure()
    {
        base.Measure();
        foreach (var internalComponent in _internalComponents)
        {
            internalComponent.Measure();
        }
    }

    private Rectangle LastScissor { get; set; }
    
    protected void BeginContainerDraw()
    {
        LastScissor = Renderer.GraphicsDevice.ScissorRectangle;
        
        if (!ClipToBounds)
        {
            if (Parent == null)
            {
                Renderer.SpriteBatch.Begin(rasterizerState: Renderer.ClippingRasterizerState, transformMatrix: Renderer.TransformMatrix);
                
                Renderer.DrawDebugBounds(Renderer.SpriteBatch, this);
            }

            return;
        };
        
        Rectangle contentArea = new Rectangle(
            GlobalsBounds.X + Padding.Left,
            GlobalsBounds.Y + Padding.Top,
            Math.Max(0, GlobalsBounds.Width - Padding.Width),
            Math.Max(0, GlobalsBounds.Height - Padding.Height)
        );
        
        Rectangle newScissor;
        if (Parent == null || LastScissor.Width == 0) {
            newScissor = contentArea;
        } else {
            newScissor = Rectangle.Intersect(LastScissor, contentArea);
        }
        
        if (newScissor == LastScissor) return;
        
        if(Parent != null)
            Renderer.SpriteBatch.End();
        Renderer.GraphicsDevice.ScissorRectangle = newScissor;
        Renderer.SpriteBatch.Begin(rasterizerState: Renderer.ClippingRasterizerState, transformMatrix: Matrix.Identity);
    }
    
    protected void EndContainerDraw()
    {
        if(Parent == null)
        {
            Renderer.SpriteBatch.End();
            return;
        }
        if (!ClipToBounds || Renderer.GraphicsDevice.ScissorRectangle == LastScissor) return;
        
        Renderer.SpriteBatch.End();
        
        Renderer.GraphicsDevice.ScissorRectangle = LastScissor;
        
        if(Parent != null)
            Renderer.SpriteBatch.Begin(rasterizerState: Renderer.ClippingRasterizerState, transformMatrix: Matrix.Identity);
    }
    
    public override void Draw(bool shouldEndDraw = true)
    {
        base.Draw(false);

        foreach (var internalNonAffectedByPadding in _internalComponents.Where(c => !c.IsAffectedByParentPadding))
        {
            internalNonAffectedByPadding.Draw(false);
        }
        
        BeginContainerDraw();
        foreach (var internalAffectedByPadding in _internalComponents.Where(c => c.IsAffectedByParentPadding))
        {
            internalAffectedByPadding.Draw(false);
        }
        if(shouldEndDraw && Parent != null)
            EndContainerDraw();
    }

    public override void Update(TimeSpan deltaTime)
    {
        base.Update(deltaTime);
        foreach (var internalComponent in _internalComponents)
        {
            internalComponent.Update(deltaTime);
        }
    }

    public override void UpdateInput(Vector2 mousePosition, bool isLeftButtonDown, bool isMiddleButtonDown, bool isRightButtonDown,
        ref int verticalWheelDelta, ref int horizontalWheelDelta, ref bool isHandled)
    {
        foreach (var internalComponent in _internalComponents)
        {
            internalComponent.UpdateInput(mousePosition, isLeftButtonDown, isMiddleButtonDown, isRightButtonDown, ref verticalWheelDelta, ref horizontalWheelDelta, ref isHandled);
            if (isHandled) break;
        }
        
        if(IgnoreMouseEvents || isHandled) return;
        
        base.UpdateInput(mousePosition, isLeftButtonDown, isMiddleButtonDown, isRightButtonDown, ref verticalWheelDelta, ref horizontalWheelDelta, ref isHandled);
    }

    public override string ToString()
    {
        var baseString = base.ToString();
        if (_internalComponents.Count == 0) return baseString;
        baseString = baseString.TrimEnd(')');
        baseString += $", InternalComponents({_internalComponents.Count})=[{string.Join(", ", _internalComponents.Select(c => c.ToString()))}])";
        return baseString;
    }

    internal abstract void AddChildInternal(BaseControl child);
    
    internal abstract void RemoveChildInternal(BaseControl? child = null);
}