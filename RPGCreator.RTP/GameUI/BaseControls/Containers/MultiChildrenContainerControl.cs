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
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RPGCreator.RTP.GameUI.BaseControls.Containers;

public abstract class MultiChildrenContainerControl : ContainerControl
{
    public event EventHandler<BaseControl>? OnAddedChildren;
    public event EventHandler<BaseControl>? OnRemovedChildren;

    protected ObservableCollection<BaseControl> _childrens = new();
    public IReadOnlyCollection<BaseControl> Childrens => _childrens;
    
    public virtual void RemoveChild(BaseControl child)
    {
        child.SetParent(null);
        RefreshControl();
        OnRemovedChildren?.Invoke(this, child);
    }
    
    public virtual void AddChild(BaseControl child)
    {
        child.SetParent(this);
        RefreshControl();
        OnAddedChildren?.Invoke(this, child);
    }

    public override void UpdateAnchoredBounds()
    {
        base.UpdateAnchoredBounds();
        foreach (var child in _childrens)
        {
            child.UpdateAnchoredBounds();
        }
    }

    protected override void UpdateChildrenBounds()
    {
        base.UpdateChildrenBounds();
        foreach (var child in _childrens)
        {
            child.RefreshControl();
        }
    }

    internal override void AddChildInternal(BaseControl child)
    {
        _childrens.Add(child);
    }

    internal override void RemoveChildInternal(BaseControl? child = null)
    {
        if(child != null)
            _childrens.Remove(child);
    }

    public override BaseControl? GetControlAt(Vector2 worldPosition)
    {
        if(IgnoreMouseEvents) return null;
        
        var self = base.GetControlAt(worldPosition);
        
        if (self != this) return self;

        for (int i = _childrens.Count - 1; i >= 0; i--)
        {
            var found = _childrens[i].GetControlAt(worldPosition);
            if (found != null) return found;
        }

        return this;
    }
    
    public override void Measure()
    {
        base.Measure();
        
        foreach (var child in _childrens)
        {
            child.Measure();
            
            if (!GlobalsBounds.Intersects(child.GlobalsBounds))
            {
                child.ShouldDrawn = false;
                continue;
            }
            
            child.ShouldDrawn = true;
            
            if(!child.AbsoluteVisibility)
                child.ShouldDrawn = false;
        }
    }

    public override void Arrange()
    {
        base.Arrange();
        
        foreach (var child in _childrens)
        {
            child.Arrange();
        }
    }

    public override void Draw(TimeSpan deltaTime, bool shouldEndDraw = true)
    {
        base.Draw(deltaTime, false);
        foreach (var child in _childrens)
        {
            child.Draw(deltaTime);
        }
        
        if(shouldEndDraw)
            EndContainerDraw();
    }

    public override void Update(TimeSpan deltaTime)
    {
        base.Update(deltaTime);
        foreach (var child in _childrens)
        {
            child.Update(deltaTime);
        }
    }

    public override void UpdateMouseInput(Vector2 mousePosition, bool isLeftButtonDown, bool isMiddleButtonDown, bool isRightButtonDown,
        ref int verticalWheelDelta, ref int horizontalWheelDelta, ref bool isHandled)
    {
        foreach (var child in _childrens)
        {
            child.UpdateMouseInput(mousePosition, isLeftButtonDown, isMiddleButtonDown, isRightButtonDown, ref verticalWheelDelta, ref horizontalWheelDelta, ref isHandled);
            if (isHandled) break;
        }
        
        if(IgnoreMouseEvents || isHandled) return;
        
        base.UpdateMouseInput(mousePosition, isLeftButtonDown, isMiddleButtonDown, isRightButtonDown, ref verticalWheelDelta, ref horizontalWheelDelta, ref isHandled);

    }

    public override string ToString()
    {
        var baseString = base.ToString();
        if(_childrens.Count == 0) return baseString;
        baseString = baseString.TrimEnd(')');
        baseString += $", Childrens({_childrens.Count})=[{string.Join(", ", _childrens)}])";
        return baseString;
    }
}