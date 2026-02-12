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

namespace RPGCreator.RTP.GameUI.BaseControls.Box;

public class NineGridBox : SimpleColorBox
{
    #region Events
    public event EventHandler<ControlPropertyChangingEventArgs<Texture2D>>? OnTextureChanging;
    public event EventHandler<ControlPropertyChangedEventArgs<Texture2D>>? OnTextureChanged;
    
    public event EventHandler<ControlPropertyChangingEventArgs<int>>? OnLeftChanging;
    public event EventHandler<ControlPropertyChangedEventArgs<int>>? OnLeftChanged;
    
    public event EventHandler<ControlPropertyChangingEventArgs<int>>? OnTopChanging;
    public event EventHandler<ControlPropertyChangedEventArgs<int>>? OnTopChanged;
    
    public event EventHandler<ControlPropertyChangingEventArgs<int>>? OnRightChanging;
    public event EventHandler<ControlPropertyChangedEventArgs<int>>? OnRightChanged;
    
    public event EventHandler<ControlPropertyChangingEventArgs<int>>? OnBottomChanging;
    public event EventHandler<ControlPropertyChangedEventArgs<int>>? OnBottomChanged;
    #endregion
    
    protected override string _Name { get; set; } = "NineGridBox";
    
    public Texture2D Texture
    {
        get;
        set
        {
            OnTextureChanging?.Invoke(this, new (field, value));
            field = value;
            OnTextureChanged?.Invoke(this, new (field, value));
            Invalidate();
        }
    } = null!;
    
    /// <summary>
    /// Left corner width in pixels.
    /// </summary>
    public int Left
    {
        get;
        set
        {
            OnLeftChanging?.Invoke(this, new (field, value));
            field = value;
            OnLeftChanged?.Invoke(this, new (field, value));
            Invalidate();
        }
    } = 0;
    
    /// <summary>
    /// Top corner height in pixels.
    /// </summary>
    public int Top
    {
        get;
        set
        {
            OnTopChanging?.Invoke(this, new (field, value));
            field = value;
            OnTopChanged?.Invoke(this, new (field, value));
            Invalidate();
        }
    } = 0;
    
    /// <summary>
    /// Right corner width in pixels.
    /// </summary>
    public int Right
    {
        get;
        set
        {
            OnRightChanging?.Invoke(this, new (field, value));
            field = value;
            OnRightChanged?.Invoke(this, new (field, value));
            Invalidate();
        }
    } = 0;
    
    /// <summary>
    /// Bottom corner height in pixels.
    /// </summary>
    public int Bottom
    {
        get;
        set
        {
            OnBottomChanging?.Invoke(this, new (field, value));
            field = value;
            OnBottomChanged?.Invoke(this, new (field, value));
            Invalidate();
        }
    } = 0;

    public override void Draw(SpriteBatch sb)
    {
        if(Texture == null || !AbsoluteVisibility) return;

        Rectangle dest = GlobalsBounds;
        int pathWidth = Texture.Width;
        int pathHeight = Texture.Height;
        
        // ============ TOP PART ============
        
        // Top left corner
        DrawPart(sb, new Rectangle(dest.X, dest.Y, Left, Top), 
            new Rectangle(0, 0, Left, Top));
    
        // Top Border (stretched horizontally)
        DrawPart(sb, new Rectangle(dest.X + Left, dest.Y, dest.Width - Left - Right, Top), 
            new Rectangle(Left, 0, pathWidth - Left - Right, Top));
    
        // Top right corner
        DrawPart(sb, new Rectangle(dest.X + dest.Width - Right, dest.Y, Right, Top), 
            new Rectangle(pathWidth - Right, 0, Right, Top));
    
        // ============ CENTER PART ============
        
        // Left Border (stretched vertically)
        DrawPart(sb, new Rectangle(dest.X, dest.Y + Top, Left, dest.Height - Top - Bottom), 
            new Rectangle(0, Top, Left, pathHeight - Top - Bottom));
    
        // Center (stretched both horizontally and vertically)
        DrawPart(sb, new Rectangle(dest.X + Left, dest.Y + Top, dest.Width - Left - Right, dest.Height - Top - Bottom), 
            new Rectangle(Left, Top, pathWidth - Left - Right, pathHeight - Top - Bottom));
    
        // Right Border (stretched vertically)
        DrawPart(sb, new Rectangle(dest.X + dest.Width - Right, dest.Y + Top, Right, dest.Height - Top - Bottom), 
            new Rectangle(pathWidth - Right, Top, Right, pathHeight - Top - Bottom));
    
        // ============ BOTTOM PART ============
        
        // Bottom left corner
        DrawPart(sb, new Rectangle(dest.X, dest.Y + dest.Height - Bottom, Left, Bottom), 
            new Rectangle(0, pathHeight - Bottom, Left, Bottom));
    
        // Bottom Border (stretched horizontally)
        DrawPart(sb, new Rectangle(dest.X + Left, dest.Y + dest.Height - Bottom, dest.Width - Left - Right, Bottom), 
            new Rectangle(Left, pathHeight - Bottom, pathWidth - Left - Right, Bottom));
    
        // Bottom right corner
        DrawPart(sb, new Rectangle(dest.X + dest.Width - Right, dest.Y + dest.Height - Bottom, Right, Bottom), 
            new Rectangle(pathWidth - Right, pathHeight - Bottom, Right, Bottom));
    }
    
    private void DrawPart(SpriteBatch sb, Rectangle dest, Rectangle source)
    {
        sb.Draw(Texture, dest, source, BackgroundColor * (AbsoluteAlpha / 255f));
    }
}