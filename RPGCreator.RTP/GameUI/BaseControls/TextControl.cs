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
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace RPGCreator.RTP.GameUI.BaseControls;

public class TextControl : BaseControl
{
    #region Events
    
    public event EventHandler<ControlPropertyChangingEventArgs<string>>? OnTextChanging;
    public event EventHandler<ControlPropertyChangedEventArgs<string>>? OnTextChanged;
    
    public event EventHandler<ControlPropertyChangingEventArgs<int>>? OnFontSizeChanging;
    public event EventHandler<ControlPropertyChangedEventArgs<int>>? OnFontSizeChanged;
    
    public event EventHandler<ControlPropertyChangingEventArgs<Color>>? OnFontColorChanging;
    public event EventHandler<ControlPropertyChangedEventArgs<Color>>? OnFontColorChanged;
    
    #endregion

    protected override string _Name { get; set; } = "TextControl";
    protected override bool _canReceiveMouseEvents { get; set; } = false;

    public string Text
    {
        get;
        set
        {
            OnTextChanging?.Invoke(this, new (field, value));
            field = value;
            Measure(); 
            Arrange();
            OnTextChanged?.Invoke(this, new (field, value));
            Invalidate();
        }
    } = "My Text";

    public int FontSize
    {
        get;
        set
        {
            OnFontSizeChanging?.Invoke(this, new (field, value));
            field = value;
            RefreshFont();
            OnFontSizeChanged?.Invoke(this, new (field, value));
            Invalidate();
        }
    } = 16;

    public Color FontColor
    {
        get;
        set
        {
            OnFontColorChanging?.Invoke(this, new (field, value));
            field = value;
            OnFontColorChanged?.Invoke(this, new (field, value));
            Invalidate();
        }
    } = Color.White;

    private DynamicSpriteFont? _font;
    
    public override void Draw(SpriteBatch sb)
    {
        if (_font == null)
        {
            RefreshFont();
        }

        _font!.DrawText(sb, Text, GlobalsBounds.Location.ToVector2() + Origin, FontColor * (AbsoluteAlpha / 255f), origin: Origin);
        sb.DrawCircle(Origin + GlobalsBounds.Location.ToVector2(), 2, 16, Color.Red * .2f, 2);
    }

    public override void Measure()
    {
        if (_font == null) RefreshFont();

        if (_font != null && !string.IsNullOrEmpty(Text))
        {
            var size = _font.MeasureString(Text);
        
            Size = new Size((int)size.X, (int)size.Y);
            Origin = new Vector2(Size.Width / 2f, Size.Height / 2f);
        }

        base.Measure();
    }

    public override void Arrange()
    {
        base.Arrange();
    }

    private void RefreshFont()
    {
        if (OwningLayer == null) return;
        _font = OwningLayer.FontSystem.GetFont(FontSize);
        if(_font == null)
            throw new Exception($"Failed to get font with size {FontSize} from the FontSystem.");
    }
}