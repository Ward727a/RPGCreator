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
using Microsoft.Xna.Framework;

namespace RPGCreator.RTP.GameUI.BaseControls.Containers;

public class ContainerControl : BaseControl
{
    #region Events
    
    public event EventHandler<BaseControl>? OnAddedChildren;
    public event EventHandler<BaseControl>? OnRemovedChildren;
    
    public event EventHandler<ControlPropertyChangingEventArgs<bool>>? OnClipsToBoundsChanging;
    public event EventHandler<ControlPropertyChangedEventArgs<bool>>? OnClipsToBoundsChanged;
    
    #endregion
    
    protected override string _Name { get; set; } = "ContainerControl";
    protected virtual bool _canHaveChildren { get; set; } = true;
    public List<BaseControl> Children { get; } = new();

    public bool ClipsToBounds
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

    protected virtual void UpdateChildrenBounds()
    {
        foreach (var child in Children)
        {
            child.RefreshControl();
        }
    }
    public override BaseControl? GetControlAt(Vector2 worldPosition)
    {
        if (!CanReceiveMouseEvents) return null;
        
        if (!AbsoluteVisibility || !GlobalsBounds.Contains(worldPosition.ToPoint()))
            return null;
        
        if(ClipsToBounds && !GlobalsBounds.Contains(worldPosition.ToPoint()))
            return null;

        for (int i = Children.Count - 1; i >= 0; i--)
        {
            var found = Children[i].GetControlAt(worldPosition);
            if (found != null) return found;
        }

        return this;
    }

    public override void Measure()
    {
        base.Measure();

        foreach (var child in Children)
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
        
        foreach (var child in Children)
        {
            child.Arrange();
        }
    }

    public override void RefreshControl()
    {
        Measure();
        Arrange();
    }
    
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
}