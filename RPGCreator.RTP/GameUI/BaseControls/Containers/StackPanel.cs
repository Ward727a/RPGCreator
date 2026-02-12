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
using RPGCreator.RTP.GameUI.Enums;
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
    
    public event EventHandler<ControlPropertyChangingEventArgs<Margin>>? OnPaddingChanging;
    public event EventHandler<ControlPropertyChangedEventArgs<Margin>>? OnPaddingChanged;
    
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
    
    public Margin Padding
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

    public override void Measure()
    {
        base.Measure();

        if (!AutoSize) return;
        
        int totalWidth = 0;
        int totalHeight = 0;
        
        foreach (var child in Children)
        {
            if (!child.AbsoluteVisibility) continue;
            if(child.IsInternal) continue;

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
        
        if (Orientation == ControlOrientation.Vertical)
        {
            totalHeight += Padding.Top + Padding.Bottom - ItemSpacing;
            totalWidth += Padding.Left + Padding.Right;
        }
        else
        {
            totalWidth += Padding.Left + Padding.Right - ItemSpacing;
            totalHeight += Padding.Top + Padding.Bottom;
        }
        
        DesiredSize = new Size(totalWidth, totalHeight);
    }

    public override void UpdateAnchoredBounds()
    {
        base.UpdateAnchoredBounds();

        float currentOffset = 0;
        
        if(Orientation == ControlOrientation.Vertical)
            currentOffset = Padding.Top;
        else
            currentOffset = Padding.Left;

        foreach (var child in Children)
        {
            if (!child.AbsoluteVisibility) continue;
            if(child.IsInternal) continue;

            if (Orientation == ControlOrientation.Vertical)
            {
                child.Position = new(child.Position.X, currentOffset);
                currentOffset += child.Height + ItemSpacing;
            }
            else
            {
                child.Position = new(currentOffset, child.Position.Y);
                currentOffset += child.Width + ItemSpacing;
            }
            
            child.UpdateAnchoredBounds();
        }
    }
}