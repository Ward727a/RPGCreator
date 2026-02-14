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
using RPGCreator.RTP.GameUI.Enums;

namespace RPGCreator.RTP.GameUI.BaseControls.Box;

public class ThreeGridBox : SimpleColorBox
{
    #region Events
    public event EventHandler<ControlPropertyChangingEventArgs<Texture2D>>? OnTextureChanging;
    public event EventHandler<ControlPropertyChangedEventArgs<Texture2D>>? OnTextureChanged;
    
    public event EventHandler<ControlPropertyChangingEventArgs<int>>? OnLeftSliceChanging;
    public event EventHandler<ControlPropertyChangedEventArgs<int>>? OnLeftSliceChanged;
    
    public event EventHandler<ControlPropertyChangingEventArgs<int>>? OnRightSliceChanging;
    public event EventHandler<ControlPropertyChangedEventArgs<int>>? OnRightSliceChanged;
    #endregion
    
    protected override string _Name { get; set; } = "ThreeGridBox";
    
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
    /// Left Slice width in pixels.<br/>
    /// In vertical orientation, this represents the top slice height in pixels.
    /// </summary>
    public int LeftSlice
    {
        get;
        set
        {
            OnLeftSliceChanging?.Invoke(this, new (field, value));
            field = value;
            OnLeftSliceChanged?.Invoke(this, new (field, value));
            Invalidate();
        }
    } = 0;
    
    /// <summary>
    /// Right Slice width in pixels.<br/>
    /// In vertical orientation, this represents the bottom slice height in pixels.
    /// </summary>
    public int RightSlice
    {
        get;
        set
        {
            OnRightSliceChanging?.Invoke(this, new (field, value));
            field = value;
            OnRightSliceChanged?.Invoke(this, new (field, value));
            Invalidate();
        }
    } = 0;
    
    public ControlOrientation Orientation { get; set; } = ControlOrientation.Horizontal;

    private Rectangle _srcLeft, _srcCenter, _srcRight;
    private Rectangle _destLeft, _destCenter, _destRight;

    private bool _isInit;
    
    public override void Draw(bool shouldEndDraw = true)
    {
        if(Texture == null || !AbsoluteVisibility) return;
        
        if (!_isInit)
        {
            RefreshSlices();
            _isInit = true;
        }

        DrawPart(_destLeft, _srcLeft);
        DrawPart(_destCenter, _srcCenter);
        DrawPart(_destRight, _srcRight);
    }
    
    private void DrawPart(Rectangle dest, Rectangle source)
    {
        Renderer.SpriteBatch.Draw(Texture, dest, source, BackgroundColor * (AbsoluteAlpha / 255f));
    }

    protected void RefreshSlices()
    {
        Rectangle dest = GlobalsBounds;
        int pathWidth = Texture.Width;
        int pathHeight = Texture.Height;
        
        if (Orientation == ControlOrientation.Horizontal)
        {
            _srcLeft = new Rectangle(0, 0, LeftSlice, pathHeight);
            _srcCenter = new Rectangle(LeftSlice, 0, pathWidth - LeftSlice - RightSlice, pathHeight);
            _srcRight = new Rectangle(pathWidth - RightSlice, 0, RightSlice, pathHeight);

            _destLeft = new Rectangle(dest.X, dest.Y, LeftSlice, dest.Height);
            _destRight = new Rectangle(dest.Right - RightSlice, dest.Y, RightSlice, dest.Height);
            _destCenter = new Rectangle(dest.X + LeftSlice, dest.Y, Math.Max(0, dest.Width - LeftSlice - RightSlice), dest.Height);
        }
        else
        {
            // TOP
            _srcLeft = new Rectangle(0, 0, pathWidth, LeftSlice);
            // CENTER
            _srcCenter = new Rectangle(0, LeftSlice, pathWidth, pathHeight - LeftSlice - RightSlice);
            // BOTTOM
            _srcRight = new Rectangle(0, pathHeight - RightSlice, pathWidth, RightSlice);

            // TOP
            _destLeft = new Rectangle(dest.X, dest.Y, dest.Width, LeftSlice);
            // BOTTOM
            _destRight = new Rectangle(dest.X, dest.Bottom - RightSlice, dest.Width, RightSlice);
            // CENTER
            _destCenter = new Rectangle(dest.X, dest.Y + LeftSlice, dest.Width, Math.Max(0, dest.Height - LeftSlice - RightSlice));
        }
    }
    
    public override void Invalidate()
    {
        RefreshSlices();
        base.Invalidate();
    }
}