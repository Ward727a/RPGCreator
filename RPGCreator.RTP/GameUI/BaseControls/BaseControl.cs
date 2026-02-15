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
using System.Globalization;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using RPGCreator.RTP.GameUI.BaseControls.Containers;
using RPGCreator.RTP.GameUI.Enums;
using RPGCreator.RTP.GameUI.Layers;
using RPGCreator.SDK.Inputs;
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
    internal UiRenderer? Renderer
    {
        get
        {
            if (Parent != null)
                return Parent.Renderer;
            return field;
        }
        set;
    }

    #region Events

    public event Action? OnFocusGained;
    public event Action? OnFocusLost;
    public event Action? OnInvalidate;
    
    public event EventHandler<ControlPropertyChangingEventArgs<string>>? OnNameChanging;
    public event EventHandler<ControlPropertyChangedEventArgs<string>>? OnNameChanged;
    
    public event EventHandler<ControlPropertyChangingEventArgs<BaseComplexControl?>>? OnParentChanging;
    public event EventHandler<ControlPropertyChangedEventArgs<BaseComplexControl?>>? OnParentChanged;
    
    public event EventHandler<ControlPropertyChangingEventArgs<Vector2>>? OnPositionChanging;
    public event EventHandler<ControlPropertyChangedEventArgs<Vector2>>? OnPositionChanged;
    
    public event EventHandler<ControlPropertyChangingEventArgs<Vector2>>? OnOriginChanging;
    public event EventHandler<ControlPropertyChangedEventArgs<Vector2>>? OnOriginChanged;
    
    public event EventHandler<ControlPropertyChangingEventArgs<Thickness>>? OnMarginChanging;
    public event EventHandler<ControlPropertyChangedEventArgs<Thickness>>? OnMarginChanged;
    
    public event EventHandler<ControlPropertyChangingEventArgs<Thickness>>? OnPaddingChanging;
    public event EventHandler<ControlPropertyChangedEventArgs<Thickness>>? OnPaddingChanged;
    
    public event EventHandler<ControlPropertyChangingEventArgs<Size>>? OnSizeChanging;
    public event EventHandler<ControlPropertyChangedEventArgs<Size>>? OnSizeChanged;

    public event EventHandler<ControlPropertyChangingEventArgs<Size>>? OnMinimumSizeChanging;
    public event EventHandler<ControlPropertyChangedEventArgs<Size>>? OnMinimumSizeChanged;
    
    public event EventHandler<ControlPropertyChangingEventArgs<Size>>? OnMaximumSizeChanging;
    public event EventHandler<ControlPropertyChangedEventArgs<Size>>? OnMaximumSizeChanged;
    
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

    protected bool HasFocus
    {
        get;
        set
        {
            if(Equals(field, value)) return;
            field = value;
            if(field)
                OnFocusGained?.Invoke();
            else
                OnFocusLost?.Invoke();
        }
    } = false;

    public virtual bool CanBeFocused { get; set; } = false;
    public static BaseControl? FocusedControl { get; private set; }

    public virtual void Invalidate()
    {
        IsDirty = true;
        Parent?.Invalidate();
        if(Parent == null)
            OnInvalidate?.Invoke();
    }
    public bool IsInternal { get; internal init; }
    
    protected virtual string _Name { get; set; } = "BaseControl";
    public string Name { get => _Name; set
        {
            if (Equals(value, _Name)) return;
            var old = _Name;
            OnNameChanging?.Invoke(this, new(old, value));
            _Name = value;
            OnNameChanged?.Invoke(this, new(old, value));
        }
    }
    
    // If the object is fully hidden, or outside the viewport, it won't be drawn, but it will still be able to receive events and update itself.
    // Note: Should it be kept? I don't know, in fact this is not used much, and it can be easily achieved by setting IsVisible to false.
    //       Need to think about it.
    public bool ShouldDrawn { get; set; } = true;

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

    public BaseComplexControl? Parent
    {
        get;
        internal set
        {
            if (Equals(field, value)) return;
            var old = field;
            OnParentChanging?.Invoke(this, new(old, value));
            field = value;
            OnParentChanged?.Invoke(this, new(old, value));
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

    public Thickness Margin
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

    public Thickness Padding
    {
        get;
        set
        {
            if(Equals(field, value)) return;
            var old = field;
            OnPaddingChanging?.Invoke(this, new(old, field));
            field = value;
            OnPaddingChanged?.Invoke(this, new(old, field)); 
            RefreshControl();
        }
        
    }

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

    public Size MinimumSize
    {
        get;
        set
        {
            if (Equals(field, value))
                return;
            var old = field;
            OnMinimumSizeChanging?.Invoke(this, new(old, value));
            field = value;
            OnMinimumSizeChanged?.Invoke(this, new(old, value));
            RefreshControl();
        }
    } = Size.Empty;

    public Size MaximumSize
    {
        get;
        set
        {
            if (Equals(field, value))
                return;
            var old = field;
            OnMaximumSizeChanging?.Invoke(this, new(old, value));
            field = value;
            OnMaximumSizeChanged?.Invoke(this, new(old, value));
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

    public Vector2 LocalOffset
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

    internal bool IsAffectedByParentPadding = true;

    public bool AbsoluteVisibility => IsVisible && (Parent?.AbsoluteVisibility ?? true);
    public bool AbsoluteEnabled => IsEnabled && (Parent?.AbsoluteEnabled ?? true);
    public int AbsoluteAlpha => (int)((Alpha / 255f) * (Parent?.AbsoluteAlpha ?? 255));

    protected void RecalculateBounds()
    {
        var offset = (Position - Origin + LocalOffset).ToPoint();
        LocalBounds = new Rectangle(offset, Size);
        var parentPos = Parent?.GlobalsBounds.Location ?? Point.Zero;
        GlobalsBounds = new Rectangle(parentPos + offset, Size);
    }

    /// <summary>
    /// Calculates the desired size of the control based on its content and properties.<br/>
    /// This method should be overridden by derived controls to provide specific measurement logic.<br/>
    /// By default, it sets the DesiredSize to the Size of the control plus its margins.
    /// </summary>
    public virtual void Measure()
    {
        int targetW = Size.Width + Margin.Width + Padding.Width;
        int targetH = Size.Height + Margin.Height + Padding.Height;

        if (MaximumSize.Width > 0) targetW = Math.Min(targetW, MaximumSize.Width);
        if (MaximumSize.Height > 0) targetH = Math.Min(targetH, MaximumSize.Height);

        targetW = Math.Max(targetW, MinimumSize.Width);
        targetH = Math.Max(targetH, MinimumSize.Height);
        
        DesiredSize = new Size(targetW, targetH);
    }

    /// <summary>
    /// Arranges the control's position and size based on its properties, parent container, and anchors.<br/>
    /// This method should be overridden by derived controls to provide specific arrangement logic.
    /// </summary>
    public virtual void Arrange()
    {
        UpdateAnchoredBounds();
    }

    /// <summary>
    /// Updates the control's LocalBounds and GlobalsBounds based on its Position, Size, Margin, and Anchors relative to its parent container.<br/>
    /// This method should be called whenever the control's position, size, margin, or anchors change to ensure the bounds are correctly updated.
    /// </summary>
    public virtual void UpdateAnchoredBounds()
    {
        if (Parent == null)
        {
            RecalculateBounds();
            return;
        }
        
        Rectangle pBounds = Parent.GlobalsBounds;
        var pPadding = IsAffectedByParentPadding ? Parent.Padding : new(0);
        
        float workWidth = pBounds.Width - pPadding.Width - Margin.Width;
        float workHeight = pBounds.Height - pPadding.Height - Margin.Height;
        float startX = pBounds.X + pPadding.Left + Margin.Left;
        float startY = pBounds.Y + pPadding.Top + Margin.Top;

        float finalX = Position.X - Origin.X;
        float finalY = Position.Y - Origin.Y;
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
            (int)(startX + finalX + LocalOffset.X),
            (int)(startY + finalY + LocalOffset.Y),
            (int)Math.Max(0, finalW),
            (int)Math.Max(0, finalH)
        );
        
        LocalBounds = new Rectangle((int)(pPadding.Left + Margin.Left + LocalOffset.X + finalX), 
            (int)(pPadding.Top + Margin.Top + LocalOffset.Y + finalY), 
            (int)finalW, (int)finalH);
    }

    /// <summary>
    /// Refreshes the control by recalculating its bounds and invalidating it for redraw.<br/>
    /// This method should be called whenever a property that affects the control's layout or appearance changes to ensure it is properly updated on the screen.
    /// </summary>
    public virtual void RefreshControl()
    {
        Measure();
        Arrange();
        Invalidate();
        Parent?.RefreshControl();
    }

    public void SetParent(BaseComplexControl? newParent)
    {
        if (Equals(Parent, newParent)) return;
        if(Equals(this, newParent)) return;
        if(Parent is ContainerControl oldContainerParent)
            oldContainerParent?.RemoveChildInternal(this);
        Parent = newParent;
        if(Parent is ContainerControl newContainerParent)
            newContainerParent?.AddChildInternal(this);
    }

    /// <summary>
    /// Recursively searches for the control at the given world position, starting from this control and going through its children if it's a container.<br/>
    /// If this control can receive mouse events and the world position is within its global bounds, it returns this control. Otherwise, it returns null.<br/>
    /// </summary>
    /// <param name="worldPosition">The position in world coordinates to check for a control.</param>
    /// <returns></returns>
    public virtual BaseControl? GetControlAt(Vector2 worldPosition)
    {
        if(IgnoreMouseEvents) return null;
        if (!AbsoluteVisibility || !GlobalsBounds.Contains(worldPosition.ToPoint()))
            return null;
        
        if(!IsPointVisible(worldPosition))
            return null;

        return this;
    }
    
    public bool IsPointVisible(Vector2 worldPosition)
    {
        if (!GlobalsBounds.Contains(worldPosition)) return false;

        var current = Parent;
        while (current != null)
        {
            if (current.ClipToBounds && !current.GlobalsBounds.Contains(worldPosition))
                return false;
            current = current.Parent;
        }

        return true;
    }

    public override string ToString()
    {
        return $"(Parent: {Parent?.Name ?? "None"}, Type: {GetType().Name}, Name: {Name}" +
               $", Position: {Position}, Size: {Size}, Margin: {Margin}, Padding: {{T:{Padding.Top}, B:{Padding.Bottom}, L:{Padding.Left}, R:{Padding.Right}}}" +
               $", GlobalBounds: {GlobalsBounds}, LocalBounds: {LocalBounds}" +
               $", Rotation: {Rotation}, Alpha: {Alpha}, ZIndex: {ZIndex}, SizingMode: {SizingMode}" +
               $", Anchors: {Anchors}, AbsoluteVisibility: {AbsoluteVisibility}" +
               $", AbsoluteEnabled: {AbsoluteEnabled}, AbsoluteAlpha: {AbsoluteAlpha}" +
               $", Visible: {IsVisible}, Enabled: {IsEnabled})";
    }

    #region InputsManager
    public bool IsMouseOver { get; private set; } = false;
    public bool IsPressed { get; private set; } = false;

    public event Action? OnMouseEnter;
    public event Action? OnMouseLeave;
    public event Action<Vector2>? OnMouseMove;
    public event Action? OnPressed;
    public event Action? OnReleased;
    public event Action? OnLeftClicked;
    public event Action? OnMiddleClicked;
    public event Action? OnRightClicked;
    public event Action<int>? OnVerticalWheelScrolled;
    public event Action<int>? OnHorizontalWheelScrolled;
    
    public Vector2 LastMousePosition { get; private set; }
    
    private bool _leftButtonDown, _middleButtonDown, _rightButtonDown;
    
    /// <summary>
    /// Default on true.
    /// </summary>
    public virtual bool IgnoreMouseEvents { get; set; } = true;
    
    public virtual void UpdateMouseInput(Vector2 mousePosition, bool isLeftButtonDown, bool isMiddleButtonDown,
        bool isRightButtonDown, ref int verticalWheelDelta, ref int horizontalWheelDelta, ref bool isHandled)
    {
        if (IgnoreMouseEvents)
        {
            LastMousePosition = mousePosition;
            return;
        }
        
        if (!IsEnabled || !AbsoluteEnabled || !AbsoluteVisibility)
        {
            if (IsMouseOver)
            {
                IsMouseOver = false;
                OnMouseLeave?.Invoke();
            }

            return;
        }
        
        bool currentlyOver = !isHandled && IsPointVisible(mousePosition);
        
        if (currentlyOver && !IsMouseOver)
        {
            IsMouseOver = true;
            OnMouseEnter?.Invoke();
        } 
        else if (!currentlyOver && IsMouseOver)
        {
            IsMouseOver = false;
            OnMouseLeave?.Invoke();
        }

        if (currentlyOver)
        {
            if (mousePosition != LastMousePosition)
            {
                OnMouseMove?.Invoke(mousePosition - GlobalsBounds.Location.ToVector2());
                LastMousePosition = mousePosition;
            }

            bool anyClick = isLeftButtonDown || isMiddleButtonDown || isRightButtonDown;
            bool anyLastClick = _leftButtonDown || _middleButtonDown || _rightButtonDown;

            if (anyClick && !anyLastClick)
            {
                IsPressed = true;
                if (CanBeFocused)
                {
                    if (FocusedControl != null)
                        FocusedControl.HasFocus = false;
                    HasFocus = true;
                    FocusedControl = this;
                }
                OnPressed?.Invoke();
            }

            if (_leftButtonDown && !isLeftButtonDown && IsPressed) OnLeftClicked?.Invoke();
            if (_rightButtonDown && !isRightButtonDown && IsPressed) OnRightClicked?.Invoke();
            if (_middleButtonDown && !isMiddleButtonDown && IsPressed) OnMiddleClicked?.Invoke();

            if (anyLastClick && !anyClick)
            {
                IsPressed = false;
                OnReleased?.Invoke();
            }

            if (verticalWheelDelta != 0)
            {
                OnVerticalWheelScrolled?.Invoke(verticalWheelDelta);
                verticalWheelDelta = 0;
            }

            if (horizontalWheelDelta != 0)
            {
                OnHorizontalWheelScrolled?.Invoke(horizontalWheelDelta);
                horizontalWheelDelta = 0;
            }

            isHandled = true;
        }
        else
        {
            IsPressed = false;
        }

        _leftButtonDown = isLeftButtonDown;
        _middleButtonDown = isMiddleButtonDown;
        _rightButtonDown = isRightButtonDown;
    }

    /// <summary>
    /// Default on true.
    /// </summary>
    public virtual bool IgnoreKeyboardEvents { get; set; } = true;
    public virtual void UpdateKeyboardInput(IKeyboardState keyboardState)
    {
    }


    protected void InvokeMouseClick(MouseButton button)
    {
        switch (button)
        {
            case MouseButton.Left:
                OnLeftClicked?.Invoke();
                break;
            case MouseButton.Middle:
                OnMiddleClicked?.Invoke();
                break;
            case MouseButton.Right:
                OnRightClicked?.Invoke();
                break;
        }
    }
    
    #endregion

    #region Rendering


    public virtual void Draw(TimeSpan deltaTime, bool shouldEndDraw = true)
    {
    }

    public virtual void Update(TimeSpan deltaTime)
    {
    }
    #endregion
}