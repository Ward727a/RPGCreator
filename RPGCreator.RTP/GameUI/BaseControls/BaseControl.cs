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
using Microsoft.Xna.Framework.Graphics;
using RPGCreator.RTP.GameUI.BaseControls.Containers;
using RPGCreator.RTP.GameUI.DefaultControls;
using RPGCreator.RTP.GameUI.Enums;
using RPGCreator.RTP.GameUI.Layers;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Types;
using Size = MonoGame.Extended.Size;

namespace RPGCreator.RTP.GameUI.BaseControls;

/// <summary>
/// Event arguments for when a control property is changing. Contains the old and new values of the property.<br/>
/// First argument: OldValue<br/>
/// Second argument: NewValue
/// </summary>
/// <param name="OldValue">The old value of the property before the change.</param>
/// <param name="NewValue">The new value of the property after the change.</param>
/// <typeparam name="T"></typeparam>
public record struct ControlPropertyChangingEventArgs<T>(T OldValue, T NewValue);

/// <summary>
/// Event arguments for when a control property has changed. Contains the old and new values of the property.<br/>
/// First argument: OldValue<br/>
/// Second argument: NewValue
/// </summary>
/// <param name="OldValue">The old value of the property before the change.</param>
/// <param name="NewValue">The new value of the property after the change.</param>
/// <typeparam name="T"></typeparam>
public record struct ControlPropertyChangedEventArgs<T>(T OldValue, T NewValue);

public class BaseControl
{
    #region Events
    
    public event Action? OnInvalidate;
    
    public event EventHandler<ControlPropertyChangingEventArgs<string>>? OnNameChanging;
    public event EventHandler<ControlPropertyChangedEventArgs<string>>? OnNameChanged;
    
    public event EventHandler<ControlPropertyChangingEventArgs<ContainerControl?>>? OnParentChanging;
    public event EventHandler<ControlPropertyChangedEventArgs<ContainerControl?>>? OnParentChanged;
    
    public event EventHandler<ControlPropertyChangingEventArgs<Vector2>>? OnPositionChanging;
    public event EventHandler<ControlPropertyChangedEventArgs<Vector2>>? OnPositionChanged;
    
    public event EventHandler<ControlPropertyChangingEventArgs<Vector2>>? OnOriginChanging;
    public event EventHandler<ControlPropertyChangedEventArgs<Vector2>>? OnOriginChanged;
    
    public event EventHandler<ControlPropertyChangingEventArgs<Margin>>? OnMarginChanging;
    public event EventHandler<ControlPropertyChangedEventArgs<Margin>>? OnMarginChanged;
    
    public event EventHandler<ControlPropertyChangingEventArgs<Size>>? OnSizeChanging;
    public event EventHandler<ControlPropertyChangedEventArgs<Size>>? OnSizeChanged;
    
    public event EventHandler<ControlPropertyChangingEventArgs<float>>? OnRotationChanging;
    public event EventHandler<ControlPropertyChangedEventArgs<float>>? OnRotationChanged;
    
    public event EventHandler<ControlPropertyChangingEventArgs<Rectangle>>? OnLocalBoundsChanging;
    public event EventHandler<ControlPropertyChangedEventArgs<Rectangle>>? OnLocalBoundsChanged;
    
    public event EventHandler<ControlPropertyChangingEventArgs<Rectangle>>? OnGlobalBoundsChanging;
    public event EventHandler<ControlPropertyChangedEventArgs<Rectangle>>? OnGlobalBoundsChanged;
    
    public event EventHandler<ControlPropertyChangingEventArgs<int>>? OnAlphaChanging;
    public event EventHandler<ControlPropertyChangedEventArgs<int>>? OnAlphaChanged;
    
    public event EventHandler<ControlPropertyChangingEventArgs<int>>? OnZIndexChanging;
    public event EventHandler<ControlPropertyChangedEventArgs<int>>? OnZIndexChanged;
    
    public event EventHandler<ControlPropertyChangingEventArgs<bool>>? OnVisibilityChanging;
    public event EventHandler<ControlPropertyChangedEventArgs<bool>>? OnVisibilityChanged;
    
    public event EventHandler<ControlPropertyChangingEventArgs<bool>>? OnEnabledChanging;
    public event EventHandler<ControlPropertyChangedEventArgs<bool>>? OnEnabledChanged;
    
    public event EventHandler<ControlPropertyChangingEventArgs<ControlSizingMode>>? OnSizingModeChanging;
    public event EventHandler<ControlPropertyChangedEventArgs<ControlSizingMode>>? OnSizingModeChanged;
    
    public event EventHandler<ControlPropertyChangingEventArgs<ControlAnchors>>? OnAnchorsChanging;
    public event EventHandler<ControlPropertyChangedEventArgs<ControlAnchors>>? OnAnchorsChanged;
    
    public event EventHandler<ControlPropertyChangingEventArgs<bool>>? OnCanReceiveMouseEventsChanging;
    public event EventHandler<ControlPropertyChangedEventArgs<bool>>? OnCanReceiveMouseEventsChanged;
    #endregion
    
    public bool IsDirty { get; protected set; } = true;

    public void Invalidate()
    {
        IsDirty = true;
        Parent?.Invalidate();
        if(Parent == null)
            OnInvalidate?.Invoke();
    }
    
    protected virtual string _Name { get; set; } = "Control";
    public string Name { 
        get => _Name;
        set
        {
            if (Equals(value, _Name)) return;
            var old = _Name;
            OnNameChanging?.Invoke(this, new(old, value));
            _Name = value;
            OnNameChanged?.Invoke(this, new(old, value));
            Invalidate();
        }
    }
    
    // If the object is fully hidden, or outside the viewport, it won't be drawn, but it will still be able to receive events and update itself.
    public bool ShouldDrawn { get; set; } = true;
    public bool IsInternal { get; internal set; } = false;

    public BaseLayer? OwningLayer
    {
        get
        {
            if (Parent != null)
                return Parent.OwningLayer;
            return field;
        }
        set;
    }
    
    public ContainerControl? Parent
    {
        get;
        private set
        {
            if (Equals(field, value)) return;
            var old = field;
            OnParentChanging?.Invoke(this, new(old, value));
            field = value;
            OnParentChanged?.Invoke(this, new(old, field));
            RefreshControl();
        }
    }

    public Vector2 Position
    {
        get;
        set
        {
            if (Equals(field, value)) return;
            var old = field;
            OnPositionChanging?.Invoke(this, new(old, value));
            field = value;
            OnPositionChanged?.Invoke(this, new(old, field));
            RefreshControl();
        }
    } = Vector2.Zero;
    
    public int X
    {
        get => (int)Position.X;
        set => Position = new Vector2(value, Position.Y);
    }
    
    public int Y
    {
        get => (int)Position.Y;
        set => Position = new Vector2(Position.X, value);
    }
    
    public Vector2 Origin
    {
        get;
        set 
        {
            if(Equals(field, value)) return;
            var old = field;
            OnOriginChanging?.Invoke(this, new(old, field));
            field = value;
            OnOriginChanged?.Invoke(this, new(old, field));
            RefreshControl();
        }
    } = Vector2.Zero;

    public Margin Margin
    {
        get;
        set
        {
            if(Equals(field, value)) return;
            var old = field;
            OnMarginChanging?.Invoke(this, new(old, field));
            field = value;
            OnMarginChanged?.Invoke(this, new(old, field));
            RefreshControl();
        }
    } = new();

    public Size Size
    {
        get;
        set
        {
            if(Equals(field, value)) return;
            var old = field;
            OnSizeChanging?.Invoke(this, new(old, field));
            field = value;
            OnSizeChanged?.Invoke(this, new(old, field));
            RefreshControl();
        }
    } = Size.Empty;
    
    public int Width
    {
        get => Size.Width;
        set => Size = new Size(value, Size.Height);
    }
    
    public int Height
    {
        get => Size.Height;
        set => Size = new Size(Size.Width, value);
    }

    public float Rotation
    {
        get;
        set
        {
            if (Equals(field, value)) return;
            var old = field;
            OnRotationChanging?.Invoke(this, new(old, value));
            field = value;
            OnRotationChanged?.Invoke(this, new(old, field));
            RefreshControl();
        }
    } = 0;

    public Rectangle LocalBounds
    {
        get;
        private set
        {
            if (Equals(field, value)) return;
            var old = field;
            OnLocalBoundsChanging?.Invoke(this, new(old, value));
            field = value;
            OnLocalBoundsChanged?.Invoke(this, new(old, field));
        }
    } = Rectangle.Empty;

    public Rectangle GlobalsBounds
    {
        get;
        private set
        {
            if (Equals(field, value)) return;
            var old = field;
            OnGlobalBoundsChanging?.Invoke(this, new(old, value));
            field = value;
            OnGlobalBoundsChanged?.Invoke(this, new(old, field));
        }
    } = Rectangle.Empty;

    public int Alpha
    {
        get;
        set
        {
            if (Equals(field, value)) return;
            var old = field;
            OnAlphaChanging?.Invoke(this, new(old, value));
            field = value;
            OnAlphaChanged?.Invoke(this, new(old, field));
            Invalidate();
        }
    } = 255;
    
    public int ZIndex
    {
        get;
        set
        {
            if (Equals(field, value)) return;
            var old = field;
            OnZIndexChanging?.Invoke(this, new(old, value));
            field = value;
            OnZIndexChanged?.Invoke(this, new(old, field));
            Invalidate();
        }
    } = 0;

    public bool IsVisible
    {
        get;
        set
        {
            if (Equals(field, value)) return;
            var old = field;
            OnVisibilityChanging?.Invoke(this, new(old, value));
            field = value;
            OnVisibilityChanged?.Invoke(this, new(old, field));
            Invalidate();
        }
    } = true;
    
    public bool IsEnabled
    {
        get;
        set
        {
            if (Equals(field, value)) return;
            var old = field;
            OnEnabledChanging?.Invoke(this, new(old, value));
            field = value;
            OnEnabledChanged?.Invoke(this, new(old, field));
        }
    } = true;

    public ControlSizingMode SizingMode
    {
        get;
        set
        {
            if (Equals(field, value)) return;
            var old = field;
            OnSizingModeChanging?.Invoke(this, new(old, value));
            field = value;
            OnSizingModeChanged?.Invoke(this, new(old, field));
            Invalidate();
        }
    } = ControlSizingMode.Manual;

    public ControlAnchors Anchors
    {
        get;
        set
        {
            if (Equals(field, value)) return;
            var old = field;
            OnAnchorsChanging?.Invoke(this, new(old, value));
            field = value;
            OnAnchorsChanged?.Invoke(this, new(old, field));
            Invalidate();
        }
    } = ControlAnchors.AnchorTop | ControlAnchors.AnchorLeft;
    
    public Size DesiredSize { get; protected set; } = Size.Empty;

    public bool AbsoluteVisibility => IsVisible && (Parent?.AbsoluteVisibility ?? true);
    public bool AbsoluteEnabled => IsEnabled && (Parent?.AbsoluteEnabled ?? true);
    public int AbsoluteAlpha => (int)((Alpha / 255f) * (Parent?.AbsoluteAlpha ?? 255));
    
    protected virtual bool _canReceiveMouseEvents { get; set; } = true;

    public bool CanReceiveMouseEvents
    {
        get => _canReceiveMouseEvents;
        set
        {
            if (Equals(value, _canReceiveMouseEvents)) return;
            var old = _canReceiveMouseEvents;
            OnCanReceiveMouseEventsChanging?.Invoke(this, new(old, value));
            _canReceiveMouseEvents = value;
            OnCanReceiveMouseEventsChanged?.Invoke(this, new(old, value));
        }
    }

    protected void RecalculateBounds()
    {
        var offset = (Position - Origin).ToPoint();
        LocalBounds = new Rectangle(offset, Size);
        var parentPos = Parent?.GlobalsBounds.Location ?? Point.Zero;
        GlobalsBounds = new Rectangle(parentPos + offset, Size);
    }

    public virtual void Measure()
    {
        DesiredSize = new Size(
            Size.Width + Margin.Left + Margin.Right,
            Size.Height + Margin.Top + Margin.Bottom
        );
    }

    public virtual void Arrange()
    {
        UpdateAnchoredBounds();
    }

    public virtual void UpdateAnchoredBounds()
    {
        if (Parent == null)
        {
            RecalculateBounds();
            return;
        }

        if (this is TextControl && this.Parent is TextButton)
        {
            Logger.Debug("Text");
        }

        if (this is TextButton)
        {
            Logger.Debug("button");
        }
        
        Rectangle pBounds = Parent.GlobalsBounds;
    
        float workWidth = pBounds.Width - Margin.Left - Margin.Right;
        float workHeight = pBounds.Height - Margin.Top - Margin.Bottom;
        float startX = pBounds.X + Margin.Left;
        float startY = pBounds.Y + Margin.Top;

        float finalX = Position.X;
        float finalY = Position.Y;
        float finalW = Size.Width;
        float finalH = Size.Height;

        if ((Anchors & ControlAnchors.AnchorFullHorizontal) == ControlAnchors.AnchorFullHorizontal)
        {
            finalX = Position.X;
            finalW = workWidth - Position.X;
        }
        else if (Anchors.HasFlag(ControlAnchors.AnchorRight))
        {
            finalX = workWidth - Size.Width - Position.X;
        }
        else if (Anchors.HasFlag(ControlAnchors.AnchorCenterHorizontal))
        {
            finalX = (workWidth / 2f) - (Size.Width / 2f) + Position.X;
        }

        if ((Anchors & ControlAnchors.AnchorFullVertical) == ControlAnchors.AnchorFullVertical)
        {
            finalY = Position.Y;
            finalH = workHeight - Position.Y;
        }
        else if (Anchors.HasFlag(ControlAnchors.AnchorBottom))
        {
            finalY = workHeight - Size.Height - Position.Y;
        }
        else if (Anchors.HasFlag(ControlAnchors.AnchorCenterVertical))
        {
            finalY = (workHeight / 2f) - (Size.Height / 2f) + Position.Y;
        }
        
        GlobalsBounds = new Rectangle(
            (int)(startX + finalX),
            (int)(startY + finalY),
            (int)Math.Max(0, finalW),
            (int)Math.Max(0, finalH)
        );
        
        LocalBounds = new Rectangle((int)finalX, (int)finalY, (int)finalW, (int)finalH);
    }

    public virtual void RefreshControl()
    {
        Measure();
        Arrange();
        Invalidate();
    }

    public void SetParent(ContainerControl? newParent)
    {
        if (Equals(Parent, newParent)) return;
        if(Equals(this, newParent)) return;
        Parent?.Children.Remove(this);
        Parent = newParent;
        Parent?.Children.Add(this);
        Parent?.Children.Sort((a, b) => a.ZIndex.CompareTo(b.ZIndex));
    }
    
    public virtual BaseControl? GetControlAt(Vector2 worldPosition)
    {
        if(!CanReceiveMouseEvents) return null;
        if (!AbsoluteVisibility || !GlobalsBounds.Contains(worldPosition.ToPoint()))
            return null;

        return this;
    }

    public virtual void Draw(SpriteBatch sb)
    {
    }

    public virtual void Update(TimeSpan deltaTime)
    {
    }
}