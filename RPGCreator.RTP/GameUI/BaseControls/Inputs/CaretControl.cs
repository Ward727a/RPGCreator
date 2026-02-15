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
using System.Text;
using FontStashSharp.RichText;
using Microsoft.Xna.Framework;
using RPGCreator.RTP.GameUI.Enums;

namespace RPGCreator.RTP.GameUI.BaseControls.Inputs;

public class CaretControl : BaseControl
{
    
    #region Events
    
    public event EventHandler<ControlPropertyChangingEventArgs<int>>? OnCaretHeightChanging;
    public event EventHandler<ControlPropertyChangedEventArgs<int>>? OnCaretHeightChanged;
    
    public event EventHandler<ControlPropertyChangingEventArgs<int>>? OnCaretWidthChanging;
    public event EventHandler<ControlPropertyChangedEventArgs<int>>? OnCaretWidthChanged;
    
    public event EventHandler<ControlPropertyChangingEventArgs<Color>>? OnCaretColorChanging;
    public event EventHandler<ControlPropertyChangedEventArgs<Color>>? OnCaretColorChanged;
    
    public event EventHandler<ControlPropertyChangingEventArgs<int>>? OnBlinkShowIntervalChanging;
    public event EventHandler<ControlPropertyChangedEventArgs<int>>? OnBlinkShowIntervalChanged;
    
    public event EventHandler<ControlPropertyChangingEventArgs<int>>? OnBlinkHideIntervalChanging;
    public event EventHandler<ControlPropertyChangedEventArgs<int>>? OnBlinkHideIntervalChanged;
    
    #endregion

    public int CaretHeight
    {
        get;
        set
        {
            if (Equals(field, value)) return;
            var old = field;
            OnCaretHeightChanging?.Invoke(this, new (old, value));
            field = value;
            OnCaretHeightChanged?.Invoke(this, new (old, value));
            Invalidate();
        }
    } = 20;

    public int CaretWidth
    {
        get;
        set
        {
            if (Equals(field, value)) return;
            var old = field;
            OnCaretWidthChanging?.Invoke(this, new (old, value));
            field = value;
            OnCaretWidthChanged?.Invoke(this, new (old, value));
            Invalidate();
        }
    } = 2;

    public Color CaretColor
    {
        get;
        set
        {
            if (Equals(field, value)) return;
            var old = field;
            OnCaretColorChanging?.Invoke(this, new (old, value));
            field = value;
            OnCaretColorChanged?.Invoke(this, new (old, value));
            Invalidate();
        }
    } = Color.Black;

    public int BlinkShowInterval
    {
        get;
        set
        {
            if (Equals(field, value)) return;
            var old = field;
            OnBlinkShowIntervalChanging?.Invoke(this, new (old, value));
            field = value;
            OnBlinkShowIntervalChanged?.Invoke(this, new (old, value));
            Invalidate();
        }
    } = 500; // milliseconds

    public int BlinkHideInterval
    {
        get;
        set
        {
            if (Equals(field, value)) return;
            var old = field;
            OnBlinkHideIntervalChanging?.Invoke(this, new (old, value));
            field = value;
            OnBlinkHideIntervalChanged?.Invoke(this, new (old, value));
            Invalidate();
        }
    } = 500; // milliseconds
    
    private bool _isCaretVisible = true;
    private bool _isCaretFrozen = false;
    public bool IsCaretFrozen => _isCaretFrozen;
    
    protected int BlinkTimer { get; set; } = 0;

    public int CaretIndex { get; protected set; } = 0;
    protected RichTextLayout _textLayout;
    
    protected readonly StringBuilder _caretMeasureBuffer = new StringBuilder();

    TextControl _textControl;
    
    public CaretControl(TextControl control, RichTextLayout textLayout, int caretHeight = 20, int caretWidth = 2, Color? caretColor = null, int blinkInterval = 500) : this(control, textLayout, caretHeight, caretWidth, caretColor, blinkInterval, blinkInterval)
    {
        _textControl = control;
    }
    
    public CaretControl(TextControl control, RichTextLayout textLayout, int caretHeight, int caretWidth, Color? caretColor, int blinkShowInterval = 500, int blinkHideInterval = 500)
    {
        _textControl = control;
        _textLayout = textLayout;
        CaretHeight = caretHeight;
        CaretWidth = caretWidth;
        CaretColor = caretColor ?? Color.Black;
        BlinkShowInterval = blinkShowInterval;
        BlinkHideInterval = blinkHideInterval;
    }
    
    public override void Draw(TimeSpan deltaTime, bool shouldEndDraw = true)
    {
        base.Draw(deltaTime, shouldEndDraw);
        if(!IsVisible || OwningLayer == null || !ShouldDrawn) return;
        
        if(!_isCaretFrozen)
        {
            BlinkTimer += deltaTime.Milliseconds;
            if (_isCaretVisible && BlinkTimer >= BlinkShowInterval)
            {
                _isCaretVisible = false;
                BlinkTimer = 0;
                Invalidate();
            }
            else if (!_isCaretVisible && BlinkTimer >= BlinkHideInterval)
            {
                _isCaretVisible = true;
                BlinkTimer = 0;
                Invalidate();
            }
        }
        
        if (_isCaretVisible)
        {
            var caretPosition = GetCaretPosition();
            Renderer.SpriteBatch.Draw(Renderer.PixelTexture, new Rectangle((int)caretPosition.X, (int)caretPosition.Y, CaretWidth, CaretHeight), CaretColor * (AbsoluteAlpha / 255f));
        }
    }
    
    /// <summary>
    /// Reset the blink timer to 0, making the caret visible immediately and restarting the blinking cycle.
    /// </summary>
    public CaretControl ResetBlink()
    {
        BlinkTimer = 0;
        _isCaretVisible = true;
        Invalidate();
        return this;
    }
    
    /// <summary>
    /// Freeze the caret, preventing the blink timer from updating.<br/>
    /// Be aware that freezing the caret when it is invisible will keep it invisible until it is unfrozen or the blink timer is reset.
    /// </summary>
    public void FreezeCaret()
    {
        _isCaretFrozen = true;
    }
    
    /// <summary>
    /// Unfreeze the caret, allowing the blink timer to update and the caret to blink again based on the BlinkInterval.
    /// </summary>
    public void UnfreezeCaret()
    {
        _isCaretFrozen = false;
    }
    
    /// <summary>
    /// Set the caret position based on the given index in the text.
    /// </summary>
    /// <param name="index">
    /// The index in the text where the caret should be placed.<br/>
    /// If -1, it will be placed at the end of the text if the text is not empty, or at the start if the text is empty.<br/>
    /// </param>
    public void SetCaretPosition(int index = -1)
    {
        CaretIndex = index;
    }
    
    /// <summary>
    /// Get the caret position in pixels based on the current CaretIndex.<br/>
    /// If the caret index is invalid, and the text is empty, it returns the position at the start of the text.<br/>
    /// If the caret index is invalid, and the text is not empty, it returns the position at the end of the text.
    /// </summary>
    /// <returns>A Vector2 representing the pixel position of the caret.</returns>
    public Vector2 GetCaretPosition()
    {
        var text = _textLayout.Text;
        var usingIndex = (CaretIndex <= text.Length && CaretIndex >= 0) ? CaretIndex : text.Length;
        
        if (string.IsNullOrEmpty(text) || usingIndex <= 0)
        {
            CaretIndex = 0;
            return new Vector2(_textControl.GlobalsBounds.X, _textControl.GlobalsBounds.Y);
        }
        
        var lineInfo = _textLayout.GetLineByCursorPosition(usingIndex);
        if (lineInfo == null)
        {
            CaretIndex = 0;
            return new Vector2(_textControl.GlobalsBounds.X, _textControl.GlobalsBounds.Y);
        }
        
        _caretMeasureBuffer.Clear();
        
        int lineStartIndex = lineInfo.TextStartIndex; 
        int lengthOnLine = usingIndex - lineStartIndex;
        
        if (lengthOnLine > 0)
        {
            _caretMeasureBuffer.Append(text, lineStartIndex, lengthOnLine);
        }
        
        var sizeOnLine = _textLayout.Font.MeasureString(_caretMeasureBuffer);
        
        float posX = _textControl.GlobalsBounds.X + sizeOnLine.X;
        float posY = (_textControl.GlobalsBounds.Y - ((CaretHeight - _textLayout.Font.LineHeight)/2)) + (lineInfo.LineIndex * _textLayout.Font.LineHeight);
    
        CaretIndex = usingIndex;
        return new Vector2(posX, posY);
    }
    
    public int GetIndexAtMouse(Vector2 mouseWorldPos)
    {var text = _textLayout?.Text ?? "";
        if (string.IsNullOrEmpty(text)) return 0;

        float localMouseX = (mouseWorldPos.X - _textControl.GlobalsBounds.X) - LocalOffset.X;

        _caretMeasureBuffer.Clear();
        float previousWidth = 0;

        for (int i = 0; i < text.Length; i++)
        {
            _caretMeasureBuffer.Append(text[i]);
            float currentWidth = _textLayout?.Font.MeasureString(_caretMeasureBuffer).X ?? 0;

            float midPoint = previousWidth + (currentWidth - previousWidth) / 2f;

            if (localMouseX < midPoint)
            {
                return i;
            }

            previousWidth = currentWidth;
        }

        return text.Length;
    }
    
    public void MoveCaretLeft()
    {
        if (CaretIndex > 0)
        {
            CaretIndex--;
            ResetBlink();
        }
    }
    
    public void MoveCaretRight()
    {
        if (CaretIndex < _textLayout.Text.Length)
        {
            CaretIndex++;
            ResetBlink();
        }
    }
}