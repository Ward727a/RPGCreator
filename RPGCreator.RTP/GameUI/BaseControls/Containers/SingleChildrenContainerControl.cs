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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RPGCreator.RTP.GameUI.BaseControls.Containers;

public class SingleChildrenContainerControl : ContainerControl
{
    #region Events
    public event EventHandler<ControlPropertyChangingEventArgs<BaseControl>>? OnContentChanging;
    public event EventHandler<ControlPropertyChangedEventArgs<BaseControl>>? OnContentChanged;
    #endregion

    internal BaseControl? _content
    {
        get;
        set
        {
            if (Equals(value, field)) 
                return;
            var old = field;
            field?.SetParent(null);
            OnContentChanging?.Invoke(this, new (old, value));
            field = value;
            OnContentChanged?.Invoke(this, new (old, value));
            field?.SetParent(this);
        }
    }
    
    public BaseControl? Content
    {
        get => _content;
        set => _content = value;
    }

    public override void UpdateAnchoredBounds()
    {
        base.UpdateAnchoredBounds();
        
        if (_content != null)
        {
            _content.UpdateAnchoredBounds();
        }
    }

    protected override void UpdateChildrenBounds()
    {
        base.UpdateChildrenBounds();
        
        if (_content != null)
        {
            _content.UpdateAnchoredBounds();
        }
    }

    public override void Arrange()
    {
        base.Arrange();
        
        if (_content != null)
        {
            _content.Arrange();
        }
    }

    public override BaseControl? GetControlAt(Vector2 worldPosition)
    {
        if(IgnoreMouseEvents) return null;
        
        var self = base.GetControlAt(worldPosition);
        
        if(self != this) return self;
        
        if (_content != null)
        {
            var childResult = _content.GetControlAt(worldPosition);
            if (childResult != null) return childResult;
        }
        return this;
    }

    public override void Draw(TimeSpan deltaTime, bool shouldEndDraw = true)
    {
        base.Draw(deltaTime, false);
        if (_content != null)
        {
            _content.Draw(deltaTime);
        }
        EndContainerDraw();
    }
    
    public override void Measure()
    {
        base.Measure();
        _content?.Measure();
    }

    public override void Update(TimeSpan deltaTime)
    {
        base.Update(deltaTime);
        _content?.Update(deltaTime);
    }

    public override void UpdateMouseInput(Vector2 mousePosition, bool isLeftButtonDown, bool isMiddleButtonDown, bool isRightButtonDown,
        ref int verticalWheelDelta, ref int horizontalWheelDelta, ref bool isHandled)
    {
        _content?.UpdateMouseInput(mousePosition, isLeftButtonDown, isMiddleButtonDown, isRightButtonDown, ref verticalWheelDelta, ref horizontalWheelDelta, ref isHandled);
        
        if (isHandled) return;
        
        if(IgnoreMouseEvents || isHandled) return;
        
        base.UpdateMouseInput(mousePosition, isLeftButtonDown, isMiddleButtonDown, isRightButtonDown, ref verticalWheelDelta, ref horizontalWheelDelta, ref isHandled);

        
    }

    public override string ToString()
    {
        var baseString = base.ToString();
        baseString = baseString.TrimEnd(')');
        baseString += $", Content: {Content?.ToString() ?? "null"})";
        return baseString;
    }

    internal override void AddChildInternal(BaseControl child)
    {
        _content = child;
    }

    internal override void RemoveChildInternal(BaseControl? child = null)
    {
        _content = null;
    }
}   