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
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using RPGCreator.RTP.GameUI.Enums;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Types;
using Size = MonoGame.Extended.Size;

namespace RPGCreator.RTP.GameUI.BaseControls.Containers;

public class StackPanel : Panel
{
    #region Events
    
    public event EventHandler<ControlPropertyChangingEventArgs<ControlOrientation>>? OnOrientationChanging;
    public event EventHandler<ControlPropertyChangedEventArgs<ControlOrientation>>? OnOrientationChanged;
    
    public event EventHandler<ControlPropertyChangingEventArgs<int>>? OnItemSpacingChanging;
    public event EventHandler<ControlPropertyChangedEventArgs<int>>? OnItemSpacingChanged;
    
    public event EventHandler<ControlPropertyChangingEventArgs<Thickness>>? OnPaddingChanging;
    public event EventHandler<ControlPropertyChangedEventArgs<Thickness>>? OnPaddingChanged;
    
    public event EventHandler<ControlPropertyChangingEventArgs<bool>>? OnAutoSizeChanging;
    public event EventHandler<ControlPropertyChangedEventArgs<bool>>? OnAutoSizeChanged;
    
    #endregion

    public ControlOrientation Orientation
    {
        get;
        set
        {
            if (Equals(value, field)) return;
            var old = field;
            OnOrientationChanging?.Invoke(this, new (old, value));
            field = value;
            OnOrientationChanged?.Invoke(this, new (old, value));
        }
    } = ControlOrientation.Vertical;

    public int ItemSpacing
    {
        get;
        set
        {
            if (Equals(value, field)) return;
            var old = field;
            OnItemSpacingChanging?.Invoke(this, new (old, value));
            field = value;
            OnItemSpacingChanged?.Invoke(this, new (old, value));
        }
    } = 0;
    
    public Thickness Padding
    {
        get;
        set
        {
            if (Equals(value, field)) return;
            var old = field;
            OnPaddingChanging?.Invoke(this, new (old, value));
            field = value;
            OnPaddingChanged?.Invoke(this, new (old, value));
        }
    } = new (0);

    public bool AutoSize
    {
        get;
        set
        {
            if (Equals(value, field)) return;
            var old = field;
            OnAutoSizeChanging?.Invoke(this, new (old, value));
            field = value;
            OnAutoSizeChanged?.Invoke(this, new (old, value));
        }
    } = true;

    private bool AlreadyMeasured { get; set; } = false;
    
    public override void Measure()
    {
        base.Measure();

        if (!AutoSize) return;
        
        int totalWidth = 0;
        int totalHeight = 0;

        foreach (var child in _childrens)
        {
            if (!child.AbsoluteVisibility) continue;
            if (child.IsInternal) continue;

            var childWidth = child.DesiredSize.Width;
            var childHeight = child.DesiredSize.Height;

            if (Orientation == ControlOrientation.Vertical)
            {
                totalHeight += childHeight + ItemSpacing;
                totalWidth = Math.Max(totalWidth, childWidth);
            }
            else
            {
                totalWidth += childWidth + ItemSpacing;
                totalHeight = Math.Max(totalHeight, childHeight);
            }
        }
        
        int spacingAdjustment = (_childrens.Count > 0) ? ItemSpacing : 0;

        if (Orientation == ControlOrientation.Vertical)
        {
            totalHeight = Math.Max(0, totalHeight + Padding.Top + Padding.Bottom - spacingAdjustment);
            totalWidth += Padding.Left + Padding.Right;
        }
        else
        {
            totalWidth = Math.Max(0, totalWidth + Padding.Left + Padding.Right - spacingAdjustment);
            totalHeight += Padding.Top + Padding.Bottom;
        }
        
        DesiredSize = new Size(totalWidth, totalHeight);
        
        this.Size = DesiredSize;
    }

    public override void UpdateAnchoredBounds()
    {
        base.UpdateAnchoredBounds();

        float currentOffset = (Orientation == ControlOrientation.Vertical) ? Padding.Top : Padding.Left;

        foreach (var child in _childrens)
        {
            if (!child.AbsoluteVisibility || child.IsInternal) continue;

            if (Orientation == ControlOrientation.Vertical)
            {
                float xPos = Padding.Left + child.Origin.X;
                float yPos = currentOffset + child.Origin.Y;

                child.Position = new(xPos, yPos);
                currentOffset += child.Height + ItemSpacing;
            }
            else
            {
                float xPos = currentOffset + child.Origin.X;
                float yPos = Padding.Top + child.Origin.Y;

                child.Position = new(xPos, yPos);
                currentOffset += child.Width + ItemSpacing;
            }
        
            child.Arrange();
        }
    }

    public override string ToString()
    {
        var baseString = base.ToString();
        baseString = baseString.TrimEnd(')');
        baseString += $", Orientation={Orientation}, ItemSpacing={ItemSpacing}, Padding={Padding}, AutoSize={AutoSize})";
        return baseString;
    }
}