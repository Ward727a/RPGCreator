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
using FontStashSharp.RichText;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using RPGCreator.RTP.GameUI.Enums;
using RPGCreator.SDK.Logging;

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
    
    public event EventHandler<ControlPropertyChangingEventArgs<TextWrapping>>? OnWrappingChanging;
    public event EventHandler<ControlPropertyChangedEventArgs<TextWrapping>>? OnWrappingChanged;
    
    public event EventHandler<ControlPropertyChangingEventArgs<HorizontalAlignment>>? OnHorizontalAlignmentChanging;
    public event EventHandler<ControlPropertyChangedEventArgs<HorizontalAlignment>>? OnHorizontalAlignmentChanged;
    
    public event EventHandler<ControlPropertyChangingEventArgs<VerticalAlignment>>? OnVerticalAlignmentChanging;
    public event EventHandler<ControlPropertyChangedEventArgs<VerticalAlignment>>? OnVerticalAlignmentChanged;
    
    #endregion

    protected override string _Name { get; set; } = "TextControl";

    public string Text
    {
        get => _textLayout.Text;
        set
        {
            var oldValue = _textLayout.Text;
            OnTextChanging?.Invoke(this, new (oldValue, value));
            _textLayout.Text = value;
            RefreshControl();
            OnTextChanged?.Invoke(this, new (oldValue, value));
        }
    }

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

    public TextWrapping Wrapping
    {
        get;
        set
        {
            if (Equals(field, value))
            {
                return;
            }

            var old = field;
            OnWrappingChanging?.Invoke(this, new (old, value));
            field = value;
            OnWrappingChanged?.Invoke(this, new (old, value));
            RefreshControl();
        }
    } = TextWrapping.NoWrap;

    private DynamicSpriteFont? _font;
    internal RichTextLayout _textLayout;
    private Size? DefaultSize = null;

    public HorizontalAlignment HorizontalAlignment
    {
        get;
        set
        {
            if (Equals(field, value)) return;
            var old = field;
            OnHorizontalAlignmentChanging?.Invoke(this, new (old, value));
            field = value;
            OnHorizontalAlignmentChanged?.Invoke(this, new (old, value));
            Invalidate();
        }
    } = HorizontalAlignment.Left;

    public VerticalAlignment VerticalAlignment
    {
        get;
        set
        {
            if (Equals(field, value)) return;
            var old = field;
            OnVerticalAlignmentChanging?.Invoke(this, new (old, value));
            field = value;
            OnVerticalAlignmentChanged?.Invoke(this, new (old, value));
            Invalidate();
        }
    } = VerticalAlignment.Top;

    public Vector2 TextOffset => CalculateTextOffset();

    public TextControl()
    {
        if(_font == null)
            RefreshFont();
        _textLayout = new RichTextLayout()
        {
            Font = _font,
            Text = "My text",
            Width = Wrapping == TextWrapping.Wrap ? (Size.Width > 0 ? Size.Width : null) : null
        };
    }
    
    public override void Draw(TimeSpan deltaTime, bool shouldEndDraw = true)
    {
        if (_font == null)
        {
            RefreshFont();
        }
        
        var finalPos = GlobalsBounds.Location.ToVector2() + TextOffset;
        
        _textLayout.Draw(Renderer.SpriteBatch, finalPos, FontColor * (AbsoluteAlpha / 255f));
    }

    public override void Measure()
    {
        if (!DefaultSize.HasValue)
        {
            DefaultSize = Size;
        }

        if (Anchors.HasFlag(ControlAnchors.AnchorFullHorizontal) && GlobalsBounds.Width != DefaultSize.Value.Width)
        {
            DefaultSize = new Size(GlobalsBounds.Width, DefaultSize.Value.Height);
        }
        if (Anchors.HasFlag(ControlAnchors.AnchorFullVertical) && GlobalsBounds.Height != DefaultSize.Value.Height)
        {
            DefaultSize = new Size(DefaultSize.Value.Width, GlobalsBounds.Height);
        }
        
        if (_font == null) RefreshFont();

        if (_font != null && !string.IsNullOrEmpty(Text))
        {
            var size = _textLayout.Measure(null);

            if (Wrapping == TextWrapping.Wrap)
            {
                _textLayout.Width = DefaultSize.Value.Width > 0 ? DefaultSize.Value.Width : null;
                var sizeP = _textLayout.Measure(DefaultSize.Value.Width > 0 ? DefaultSize.Value.Width : null);

                Size = new Size(sizeP.X, sizeP.Y);
            }
            else
            {
                Size = new Size((int)size.X, (int)size.Y);
            }
        }

        base.Measure();
    }

    public override void Arrange()
    {
        base.Arrange();

    }
    
    private Vector2 CalculateTextOffset()
    {
        if (_textLayout == null) return Vector2.Zero;

        var textSize = _textLayout.Measure(DefaultSize.Value.Width > 0 ? DefaultSize.Value.Width : null);
        var bounds = Parent?.GlobalsBounds ?? GlobalsBounds;

        float offsetX = 0;
        float offsetY = 0;

        switch (HorizontalAlignment)
        {
            case HorizontalAlignment.Center:
                offsetX = (bounds.Width - textSize.X) / 2f;
                break;
            case HorizontalAlignment.Right:
                offsetX = bounds.Width - textSize.X;
                break;
            case HorizontalAlignment.Left:
            default:
                offsetX = 0;
                break;
        }

        switch (VerticalAlignment)
        {
            case VerticalAlignment.Center:
                offsetY = (bounds.Height - textSize.Y) / 2f;
                break;
            case VerticalAlignment.Bottom:
                offsetY = bounds.Height - textSize.Y;
                break;
            case VerticalAlignment.Top:
            default:
                offsetY = 0;
                break;
        }

        return new Vector2(offsetX, offsetY);
    }

    private void RefreshFont()
    {
        if (OwningLayer == null) return;
        _font = Renderer.FontSystem.GetFont(FontSize);
        if(_font == null)
            throw new Exception($"Failed to get font with size {FontSize} from the FontSystem.");
        _textLayout.Font = _font;
    }
}