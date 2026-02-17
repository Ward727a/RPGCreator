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
using RPGCreator.RTP.GameUI.Enums;
using RPGCreator.SDK.Types;

namespace RPGCreator.RTP.GameUI.Visual;

public abstract class BaseVisual
{
    #region Dirty flag
    public bool IsDirty { get; protected set; } = true;
    
    /// <summary>
    /// Mark this visual and all of its children as dirty, which means that they need to recalculate their global transform and bounds before the next rendering.<br/>
    /// This should be used with caution, as it can be expensive to mark a large visual tree as dirty (they will need to recalculate their global transform and bounds before the next rendering).<br/>
    /// In general, you should never have the need to call this method directly, as the visual will automatically mark itself as dirty when its needed.
    /// </summary>
    public void MarkDirty()
    {
        IsDirty = true;
        foreach (var child in _children)
        {
            child.MarkDirty();
        }
    }
    #endregion
    
    // This region need a full rework to take the ECS architecture into account.
    // I put it here as a placeholder.
    #region Bindings
    private readonly List<IBindingVisual> _bindings = new();
    
    [Obsolete("This method is just a placeholder, and does absolutely nothing.")]
    public BaseVisual RegisterBinding<T>(Func<T> getter, Action<T> setter)
    {
        // _bindings.Add(new PropertyBindingVisualOneWay<T>(getter, setter));
        return this;
    }
    
    // 17/02/2026
    // Ward:
    // THIS NEED A REWRITE!!!
    // For now this is using a very simple approach, and it's not efficient, neither is compatible with the
    // current ECS architecture, so it will need a complete rewrite in the future, but for now it will do the job.
    // I put it like that, so I remember to rewrite it later, but to be clear:
    // THIS DOES NOTHING, IT'S JUST A PLACEHOLDER (in fact, this is not even called).
    public void UpdateBindings()
    {
        for (int i = 0; i < _bindings.Count; i++)
        {
            _bindings[i].Update();
        }
    }
    #endregion
    
    #region Inheritance and hierarchy
    /// <summary>
    /// A list of the children of this visual.<br/>
    /// This is a protected field, so yes, you can edit it directly if you want to, but it's recommended to use the provided methods to ensure that the parent-child relationship is properly maintained.<br/>
    /// </summary>
    private readonly List<BaseVisual> _children = new();
    
    /// <summary>
    /// A read-only list of the children of this visual.<br/>
    /// If you need to edit it, check <see cref="AddChild"/>, <see cref="RemoveChild"/> and <see cref="ClearChildren"/>.
    /// </summary>
    public IReadOnlyList<BaseVisual> Children => _children.AsReadOnly();
    
    /// <summary>
    /// The parent of this visual, or null if it has no parent.<br/>
    /// A visual can only have one parent, and a parent can have multiple children.<br/>
    /// To edit this, check <see cref="AddChild"/>, <see cref="RemoveChild"/>, <see cref="AddToParent"/> and <see cref="RemoveFromParent"/>.
    /// </summary>
    public BaseVisual? Parent { get; private set; }
    
    public void AddChild(BaseVisual child)
    {
        if (child.Parent != null)
        {
            #if DEBUG
            throw new InvalidOperationException("The visual already has a parent.");
            #endif
            return;
        }
        _children.Add(child);
        child.Parent = this;
        MarkDirty();
    }
    
    /// <summary>
    /// Remove a child from this parent.<br/>
    /// The child must be a <b>direct</b> child of this parent, otherwise an exception will be thrown in debug mode and the method will do nothing in release mode.<br/>
    /// This method will not remove the child from any of its children, so if you want to remove a visual and all of its children.
    /// </summary>
    /// <param name="child"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public void RemoveChild(BaseVisual child)
    {
        if (child.Parent != this)
        {
            #if DEBUG
            throw new InvalidOperationException("The visual is not a child of this parent.");
            #endif
            return;
        }
        _children.Remove(child);
        child.Parent = null;
        MarkDirty();
    }
    
    /// <summary>
    /// This is doing the same thing as <see cref="AddChild"/>.<br/>
    /// It's just a more convenient method to add a visual to a parent without having to call <see cref="AddChild"/> on the parent.
    /// </summary>
    /// <param name="parent">The parent to add this visual to.</param>
    /// <remarks>
    /// For more information, check <see cref="AddChild"/>.
    /// </remarks>
    public void AddToParent(BaseVisual parent)
    {
        parent.AddChild(this);
    }
    
    /// <summary>
    /// This is doing the same thing as <see cref="RemoveChild"/>.<br/>
    /// It's just a more convenient method to remove a visual from its parent without having to call <see cref="RemoveChild"/> on the parent.
    /// </summary>
    /// <remarks>
    /// For more information, check <see cref="RemoveChild"/>.
    /// </remarks>
    public void RemoveFromParent()
    {
        if (Parent != null)
        {
            Parent.RemoveChild(this);
        }
    }
    
    /// <summary>
    /// Remove all children from this parent.<br/>
    /// If <paramref name="recursive"/> is true, it will also remove all children from the children, and so on recursively.
    /// </summary>
    /// <param name="recursive">Whether to also clear the children of the children recursively.</param>
    public void ClearChildren(bool recursive = false)
    {
        foreach (var child in _children)
        {
            RemoveChild(child);
            if (recursive)
            {
                child.ClearChildren(true);
            }
        }
        _children.Clear();
        MarkDirty();
    }
    #endregion
    
    #region Look properties

    public bool Visible
    {
        get;
        set
        {
            if (Visible != value)
            {
                field = value;
                MarkDirty();
            }
        }
    } = true;
    /// <summary>
    /// Return if the visual is visible, taking into account the visibility of its parents.<br/>
    /// (So if this visual is visible, but one of its parents is not visible, this method will return false).
    /// </summary>
    /// <returns></returns>
    public bool GetAbsoluteVisible()
    {
        bool visible = Visible;
        if (Parent != null)
        {
            visible &= Parent.GetAbsoluteVisible();
        }
        return visible;
    }

    public float Opacity
    {
        get;
        set
        {
            if (Opacity != value)
            {
                field = value;
                MarkDirty();
            }
        }
    } = 1f;
    
    /// <summary>
    /// Return the opacity of this visual, taking into account the opacity of its parents.<br/>
    /// (So if this visual has an opacity of 1, and one of its parents has an opacity of 0.5, this method will return 0.5).
    /// </summary>
    /// <returns></returns>
    public float GetAbsoluteOpacity()
    {
        float opacity = Opacity;
        if (Parent != null)
        {
            opacity *= Parent.GetAbsoluteOpacity();
        }
        return opacity;
    }

    #endregion
    
    #region Placement properties
    
    /// <summary>
    /// Pivot X coordinate.<br/>
    /// This is the point where the visual will be rotated, placed, and scaled from.<br/>
    /// For example, if you have a visual with a width of 100 pixels, and you set the PivotX to 50 pixels, and a X of 0 pixels,<br/>
    /// the visual will be placed in the top-left corner, and it will be "cut" in half, because the pivot is in the middle of the visual.<br/>
    /// </summary>
    public int PivotX { get;
        set
        {
            if (PivotX != value)
            {
                field = value;
                MarkDirty();
            }
        } 
    } = 0;

    public EOriginUnitType PivotXUnit
    {
        get;
        set
        {
            if (PivotXUnit != value)
            {
                field = value;
                MarkDirty();
            }
        }
    } = EOriginUnitType.Pixels;

    private float CalculatePixelPivotX(int width)
    {
        return PivotXUnit switch
        {
            EOriginUnitType.Pixels => PivotX,
            EOriginUnitType.Percentage => width * (PivotX / 100f),
            _ =>
                #if DEBUG
                throw new NotImplementedException($"The unit type {PivotXUnit} is not implemented yet.")
                #else
                PivotX
                #endif
        };
    }

    public int PivotY
    {
        get;
        set
        {
            if (PivotY != value)
            {
                field = value;
                MarkDirty();
            }
        }
    } = 0;

    public EOriginUnitType PivotYUnit
    {
        get;
        set
        {
            if (PivotYUnit != value)
            {
                field = value;
                MarkDirty();
            }
        }
    } = EOriginUnitType.Pixels;

    private float CalculatePixelPivotY(int height)
    {
        return PivotYUnit switch
        {
            EOriginUnitType.Pixels => PivotY,
            EOriginUnitType.Percentage => height * (PivotY / 100f),
            _ =>
                #if DEBUG
                throw new NotImplementedException($"The unit type {PivotYUnit} is not implemented yet.")
                #else
                PivotY
                #endif
        };
    }

    public int X
    {
        get;
        set
        {
            if (X != value)
            {
                field = value;
                MarkDirty();
            }
        }
    } = 0;

    public EPositionUnitType XUnit
    {
        get;
        set
        {
            if (XUnit != value)
            {
                field = value;
                MarkDirty();
            }
        }
    } = EPositionUnitType.Pixels;

    private float CalculatePixelX(float width)
    {
        return XUnit switch
        {
            EPositionUnitType.Pixels => X,
            EPositionUnitType.PixelsFromRight => width - X,
            EPositionUnitType.PixelsFromCenter => width / 2f + X,
            EPositionUnitType.Percentage => width * (X / 100f),
            EPositionUnitType.PercentageFromRight => width - width * (X / 100f),
            EPositionUnitType.PercentageFromCenter => width / 2f + width * (X / 100f),
            _ =>
                #if DEBUG
                throw new NotImplementedException($"The unit type {XUnit} is not implemented yet.")
                #else
                X
                #endif
        };
    }


    public int Y
    {
        get;
        set
        {
            if (Y != value)
            {
                field = value;
                MarkDirty();
            }
        }
    } = 0;

    public EPositionUnitType YUnit
    {
        get;
        set
        {
            if (YUnit != value)
            {
                field = value;
                MarkDirty();
            }
        }
    } = EPositionUnitType.Pixels;

    private float CalculatePixelY(float height)
    {
        return YUnit switch
        {
            EPositionUnitType.Pixels => Y,
            EPositionUnitType.PixelsFromBottom => height - Y,
            EPositionUnitType.PixelsFromCenter => height / 2f + Y,
            EPositionUnitType.Percentage => height * (Y / 100f),
            EPositionUnitType.PercentageFromBottom => height - height * (Y / 100f),
            EPositionUnitType.PercentageFromCenter => height / 2f + height * (Y / 100f),
            _ =>
                #if DEBUG
                throw new NotImplementedException($"The unit type {YUnit} is not implemented yet.")
                #else
                Y
                #endif
        };
    }

    public int Width
    {
        get;
        set
        {
            if (Width != value)
            {
                field = value;
                MarkDirty();
            }
        }
    } = 0;

    public ESizeUnitType WidthUnit
    {
        get;
        set
        {
            if (WidthUnit != value)
            {
                field = value;
                MarkDirty();
            }
        }
    } = ESizeUnitType.Pixels;

    private float CalculatePixelWidth(Rect parentBounds, Rect screenBounds)
    {
        return WidthUnit switch
        {
            ESizeUnitType.Pixels => Width,
            ESizeUnitType.Percentage => parentBounds.Width * (Width / 100f),
            ESizeUnitType.RelativeToParent => parentBounds.Width + Width,
            ESizeUnitType.RelativeToScreen => screenBounds.Width + Width,
            _ =>
                #if DEBUG
                throw new NotImplementedException($"The unit type {WidthUnit} is not implemented yet.")
                #else
                Width
                #endif
        };
    }

    public int Height
    {
        get;
        set
        {
            if (Height != value)
            {
                field = value;
                MarkDirty();
            }
        }
    } = 0;

    public ESizeUnitType HeightUnit
    {
        get;
        set
        {
            if (HeightUnit != value)
            {
                field = value;
                MarkDirty();
            }
        }
    } = ESizeUnitType.Pixels;

    private float CalculatePixelHeight(Rect parentBounds, Rect screenBounds)
    {
        return HeightUnit switch
        {
            ESizeUnitType.Pixels => Height,
            ESizeUnitType.Percentage => parentBounds.Height * (Height / 100f),
            ESizeUnitType.RelativeToParent => parentBounds.Height + Height,
            ESizeUnitType.RelativeToScreen => screenBounds.Height + Height,
            _ =>
                #if DEBUG
                throw new NotImplementedException($"The unit type {HeightUnit} is not implemented yet.")
                #else
                Height
                #endif
        };
    }
    
    /// <summary>
    /// Rotation of this visual in radians. (Use with caution, no testing has been done with rotation for now.)
    /// </summary>
    public float Rotation { 
        get;
        set
        {
            if (Rotation != value)
            {
                field = value;
                MarkDirty();
            }
        } 
    } = 0f;
    
    /// <summary>
    /// Local bounds of this visual, it is linked to the Parent.
    /// </summary>
    public Rect LocalBounds { get; protected set; } = new Rect(0, 0, 0, 0);

    /// <summary>
    /// Update the placement properties of this visual.<br/>
    /// Meaning that we will calculate the pixel values of the X, Y, Width, Height, PivotX and PivotY properties based on their unit types and the parent bounds.<br/>
    /// Then we will update the LocalBounds property with the new calculated values.<br/>
    /// The LocalBounds will be used later to calculate the global bounds and the global transform of this visual, which will be used for rendering and hit testing.
    /// </summary>
    public void UpdatePlacementProperties(Rect parentBounds, Rect screenBounds)
    {
        if (!IsDirty) return;

        var pixelWidth = CalculatePixelWidth(parentBounds, screenBounds);
        var pixelHeight = CalculatePixelHeight(parentBounds, screenBounds);
        var pixelX = CalculatePixelX(parentBounds.Width);
        var pixelY = CalculatePixelY(parentBounds.Height);
        var pixelPivotX = CalculatePixelPivotX((int)pixelWidth);
        var pixelPivotY = CalculatePixelPivotY((int)pixelHeight);
        
        var finalX = pixelX - pixelPivotX;
        var finalY = pixelY - pixelPivotY;
        
        LocalBounds = new Rect(finalX, finalY, pixelWidth, pixelHeight);
    }
    
    public Rect GlobalBounds { get; private set; }

    private void GetGlobalBounds()
    {
        if (!IsDirty) return;
        var parentPos = Parent?.GlobalBounds.Position ?? Vector2.Zero;
        GlobalBounds = new Rect(
            parentPos.X + LocalBounds.X,
            parentPos.Y + LocalBounds.Y,
            LocalBounds.Width,
            LocalBounds.Height
        );
    }
    #endregion
    
    #region Global transform
    public Matrix3x2 GlobalTransform { get; private set; } = Matrix3x2.Identity;

    /// <summary>
    /// Create the global transform of this visual based on its local bounds, rotation and the global transform of its parent.<br/>
    /// This is where the visual will be on the screen, and how it will be rotated and scaled.<br/>
    /// </summary>
    public void MakeGlobalTransform()
    {
        if (!IsDirty) return;
        
        float pivotX = CalculatePixelPivotX((int)LocalBounds.Width);
        float pivotY = CalculatePixelPivotY((int)LocalBounds.Height);
        
        float anchorX = LocalBounds.X + pivotX;
        float anchorY = LocalBounds.Y + pivotY;
        
        Matrix3x2 LocalTransform = Matrix3x2.CreateTranslation(pivotX, pivotY) *
                                  Matrix3x2.CreateRotation(Rotation) *
                                  Matrix3x2.CreateTranslation(anchorX, anchorY);
        
        var parentGlobalTransform = Parent?.GlobalTransform ?? Matrix3x2.Identity;
        GlobalTransform = LocalTransform * parentGlobalTransform;
        
        IsDirty = false;
    }
    #endregion
    
    #region Inputs and hit testing

    public bool HitTest(float x, float y)
    {
        return HitTest(new Vector2(x, y));
    }
    public bool HitTest(Vector2 screenPosition)
    {
        if (!GetAbsoluteVisible()) return false;
        
        if (Rotation == 0)
        {
            return GlobalBounds.Contains(screenPosition);
        }
        
        if (Matrix3x2.Invert(GlobalTransform, out var invertedGlobalTransform))
        {
            var localPosition = Vector2.Transform(screenPosition, invertedGlobalTransform);
        
            var hitRect = new Rect(0, 0, LocalBounds.Width, LocalBounds.Height);
            return hitRect.Contains(localPosition);
        }
    
        return false;
    }
    
    #endregion
    
    public void UpdateVisual(Rect parentBounds, Rect screenBounds)
    {
        UpdatePlacementProperties(parentBounds, screenBounds);
        GetGlobalBounds(); 
        MakeGlobalTransform();
    }

    public void DrawVisual(UiRendererContext context)
    {
        if (!GetAbsoluteVisible() || !Visible) return;

        int transformId = -1;
        if (Rotation != 0f) 
            transformId = context.PushTransform(GlobalTransform);

        Vector2 pos = Rotation == 0f ? GlobalBounds.Position : Vector2.Zero;
        Vector2 size = Rotation == 0f ? GlobalBounds.Size : Vector2.Zero;

        DrawVisualAt(context, pos, size, out bool handledChildren);

        if (!handledChildren)
        {
            foreach (var child in Children) child.DrawVisual(context);
        }

        if (transformId != -1)
            context.PopTransform(transformId);
    }

    protected virtual void DrawVisualAt(UiRendererContext context, Vector2 drawPosition, Vector2 drawSize, out bool handledChildren)
    {
        handledChildren = false;
        // This method should be overridden by the derived classes to draw the visual on the screen.
    }
}

internal interface IBindingVisual
{
    void Update();
}

internal class PropertyBindingVisualOneWay<T> : IBindingVisual
{
    private readonly Action<T> _setter;
    private readonly Func<T> _getter;
    private T _lastValue;
    
    public PropertyBindingVisualOneWay(Func<T> getter, Action<T> setter)
    {
        _getter = getter;
        _setter = setter;
    }
    
    public void Update()
    {
        var newValue = _getter();
        if (!EqualityComparer<T>.Default.Equals(_lastValue, newValue))
        {
            _setter(newValue);
            _lastValue = newValue;
        }
    }
}