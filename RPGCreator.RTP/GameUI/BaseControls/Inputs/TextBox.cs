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
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using RPGCreator.RTP.GameUI.BaseControls.BackgroundBox;
using RPGCreator.RTP.GameUI.Enums;
using RPGCreator.SDK;
using RPGCreator.SDK.Inputs;
using RPGCreator.SDK.Logging;

namespace RPGCreator.RTP.GameUI.BaseControls.Inputs;

public class TextBox : BaseComplexControl
{
    
    #region Events
    
    public event EventHandler<ControlPropertyChangingEventArgs<Color>>? OnBackgroundColorChanging;
    public event EventHandler<ControlPropertyChangedEventArgs<Color>>? OnBackgroundColorChanged;
    
    public event EventHandler<ControlPropertyChangingEventArgs<string>>? OnTextChanging;
    public event EventHandler<ControlPropertyChangedEventArgs<string>>? OnTextChanged;
    
    public event EventHandler<ControlPropertyChangingEventArgs<int>>? OnFontSizeChanging;
    public event EventHandler<ControlPropertyChangedEventArgs<int>>? OnFontSizeChanged;
    
    public event EventHandler<ControlPropertyChangingEventArgs<Color>>? OnFontColorChanging;
    public event EventHandler<ControlPropertyChangedEventArgs<Color>>? OnFontColorChanged;
    
    public event EventHandler<ControlPropertyChangingEventArgs<string>>? OnPlaceholderTextChanging;
    public event EventHandler<ControlPropertyChangedEventArgs<string>>? OnPlaceholderTextChanged;
    
    public event EventHandler<ControlPropertyChangingEventArgs<Color>>? OnPlaceholderTextColorChanging;
    public event EventHandler<ControlPropertyChangedEventArgs<Color>>? OnPlaceholderTextColorChanged;
    
    #endregion
    
    private SimpleColorBox _Container = null!;
    private SimpleColorBox _SelectionBox = null!;
    private TextControl _TextControl = null!;
    private CaretControl _CaretControl = null!;

    public string PlaceholderText
    {
        get;
        set
        {
            if (Equals(field, value)) return;
            var old = field;
            OnPlaceholderTextChanging?.Invoke(this, new (old, value));
            field = value;
            if(string.IsNullOrEmpty(Text) && _TextControl != null)
            {
                _TextControl.Text = value;
                _TextControl.FontColor = PlaceholderFontColor;
            }
            OnPlaceholderTextChanged?.Invoke(this, new (old, value));
            Invalidate();
        }
    }

    public Color PlaceholderFontColor
    {
        get;
        set
        {
            if (Equals(field, value)) return;
            var old = field;
            OnPlaceholderTextColorChanging?.Invoke(this, new (old, value));
            field = value;
            OnPlaceholderTextColorChanged?.Invoke(this, new (old, value));
            Invalidate();
        }
    }

    public string Text
    {
        get => field;
        set
        {
            if (Equals(field, value)) return;
            var old = field;
            OnTextChanging?.Invoke(this, new(old, value));
            _TextControl?.Text = value;
            field = value;
            OnTextChanged?.Invoke(this, new(old, value));
            Invalidate();
        }
    }
    
    public int FontSize
    {
        get => _TextControl.FontSize;
        set
        {
            if (Equals(_TextControl.FontSize, value)) return;
            var old = _TextControl.FontSize;
            OnFontSizeChanging?.Invoke(this, new(old, value));
            _TextControl.FontSize = value;
            OnFontSizeChanged?.Invoke(this, new(old, value));
            Invalidate();
        }
    }
    
    public Color FontColor
    {
        get => field;
        set
        {
            if (Equals(field, value)) return;
            var old = field;
            OnFontColorChanging?.Invoke(this, new(old, value));
            _TextControl.FontColor = value;
            field = value;
            OnFontColorChanged?.Invoke(this, new(old, value));
            Invalidate();
        }
    } = Color.Black;

    protected override string _Name { get; set; } = "TextBox";
    public override bool IgnoreMouseEvents { get; set; } = false;
    public override bool IgnoreKeyboardEvents { get; set; } = false;
    public override bool CanBeFocused { get; set; } = true;
    
    private readonly StringBuilder _measureBuffer = new StringBuilder();
    private double _lastClickTime;
    private bool _isDragging;

    public TextBox(string placeholderText = "My text here...", string text = "")
    {
        _state = EngineStates.ViewportKeyboardState;
        
        PlaceholderText = placeholderText;
        Padding = new Thickness(5, 0);
        ClipToBounds = true;
        
        _Container = new SimpleColorBox(Color.White)
        {
            Name="ContainerTextBox",
            IsInternal = true,
            IsAffectedByParentPadding = false,
            Anchors = ControlAnchors.AnchorFull
        };
        
        _SelectionBox = new SimpleColorBox(new Color(0, 120, 215, 100))
        {
            Name = "SelectionBoxTextBox",
            IsVisible = false,
            IsAffectedByParentPadding = true,
            Anchors = ControlAnchors.AnchorCenterVertical
        };
        
        Text = text;
        _TextControl = new TextControl()
        {
            Name = "TextControlTextBox",
            Text = string.IsNullOrEmpty(text) ? placeholderText : text,
            Anchors = ControlAnchors.AnchorCenterVertical,
            FontColor = string.IsNullOrEmpty(text) ? PlaceholderFontColor : FontColor
        };
        var textLayout = _TextControl._textLayout;

        _CaretControl = new CaretControl(_TextControl, textLayout)
        {
        };
        
        AddInternalComponent(_Container);
        AddInternalComponent(_SelectionBox);
        AddInternalComponent(_TextControl);
        AddInternalComponent(_CaretControl);

        if (HasFocus)
        {
            StartCaret();
        }
        else
        {
            StopCaret();
        }

        OnFocusGained += StartCaret;
        OnFocusLost += StopCaret;
        OnTextChanged += TextChanged;
        OnPressed += () =>
        {
            if (!_isDragging)
            {
                var mousePos = LastMousePosition;

                int newIndex = _CaretControl.GetIndexAtMouse(mousePos);
                if (string.IsNullOrEmpty(Text))
                    newIndex = 0;
                _CaretControl.SetCaretPosition(newIndex);

                _CaretControl.ResetBlink();

                UpdateScroll();
            }
        };
        OnLeftClicked += () =>
        {
            double currentTime = EngineStates.TotalTime.TotalMilliseconds;
            if (currentTime - _lastClickTime < 300 && !string.IsNullOrEmpty(Text)) 
            {
                _selectionStartIndex = 0;
                _CaretControl.SetCaretPosition(Text.Length);
                UpdateSelectionBounds();

                UpdateScroll();
            }
            else 
            {
                if (_isDragging)
                {
                    _isDragging = false;
                }
                else
                {
                    _selectionStartIndex = -1;
                    UpdateSelectionBounds();
                }

                var mousePos = LastMousePosition;

                int newIndex = _CaretControl.GetIndexAtMouse(mousePos);
                if(string.IsNullOrEmpty(Text))
                    newIndex = 0;
                _CaretControl.SetCaretPosition(newIndex);

                _CaretControl.ResetBlink();

                UpdateScroll();
            }
            _lastClickTime = currentTime;
        };
        OnMouseMove += (_) =>
        {
            if (IsPressed && !string.IsNullOrEmpty(Text))
            {
                _isDragging = true;
                _selectionStartIndex = _selectionStartIndex == -1 ? _CaretControl.CaretIndex : _selectionStartIndex;
                var mousePos = LastMousePosition;
                int newIndex = _CaretControl.GetIndexAtMouse(mousePos);
                _CaretControl.SetCaretPosition(newIndex);

                UpdateSelectionBounds();
                _CaretControl.ResetBlink();
                UpdateScroll();
            }
        };
        
    }

    public void SetBackground<T>(T newBackground) where T : SimpleColorBox
    {
        if (Equals(newBackground, _Container)) return;
        
        RemoveInternalComponent(_Container);
        
        newBackground.Anchors = ControlAnchors.AnchorFull;
        newBackground.IsAffectedByParentPadding = false;
        
        _Container = newBackground;
        this.InsertInternalComponent(0, newBackground);
    }
    
    protected void TextChanged(object? sender, ControlPropertyChangedEventArgs<string> e)
    {
        if (string.IsNullOrEmpty(e.NewValue))
        {
            _TextControl.Text = PlaceholderText;
            _TextControl.FontColor = PlaceholderFontColor;
        }
        else if (_TextControl.FontColor == PlaceholderFontColor)
        {
            _TextControl.FontColor = FontColor;
        }
    }
    
    private float _scrollOffset = 0f;
    private void UpdateScroll()
    {
        Vector2 caretWorldPos = _CaretControl.GetCaretPosition();
    
        float caretXInBox = (caretWorldPos.X - GlobalsBounds.X) - _scrollOffset;

        float viewWidth = GlobalsBounds.Width - (Padding.Left + Padding.Right);

        if (caretXInBox + _scrollOffset > viewWidth)
        {
            _scrollOffset = viewWidth - caretXInBox - 10;
        }
        else if (caretXInBox + _scrollOffset < 0)
        {
            _scrollOffset = -caretXInBox + 10;
        }

        if (_scrollOffset > 0) _scrollOffset = 0;

        _TextControl.LocalOffset = new Vector2(_scrollOffset, 0);
        _CaretControl.LocalOffset = new Vector2(_scrollOffset, 0);
    }
    
    private int _selectionStartIndex = -1;
    
    private void UpdateSelectionBounds()
    {
        if (_selectionStartIndex == -1 || _selectionStartIndex == _CaretControl.CaretIndex)
        {
            _SelectionBox.IsVisible = false;
            return;
        }

        _SelectionBox.IsVisible = true;

        int start = Math.Min(_selectionStartIndex, _CaretControl.CaretIndex);
        int end = Math.Max(_selectionStartIndex, _CaretControl.CaretIndex);

        _measureBuffer.Clear();
        _measureBuffer.Append(Text, 0, start);
        float startX = _TextControl._textLayout.Font.MeasureString(_measureBuffer).X;

        _measureBuffer.Clear();
        _measureBuffer.Append(Text, 0, end);
        float endX = _TextControl._textLayout.Font.MeasureString(_measureBuffer).X;

        _SelectionBox.Position = new Vector2(startX + _scrollOffset, 0);
        _SelectionBox.Size = new Size((int)(endX - startX), _TextControl.Height);
    }

    protected void UpdateTextInput(char input)
    {
        if(input == 127) // Handle backspace separately
        {
            return;
        }
        if (!HasFocus) return;

        if (_selectionStartIndex != -1 && _selectionStartIndex != _CaretControl.CaretIndex)
        {
            Text = Text.Remove(Math.Min(_selectionStartIndex, _CaretControl.CaretIndex), Math.Abs(_CaretControl.CaretIndex - _selectionStartIndex));
            _CaretControl.SetCaretPosition(Math.Min(_selectionStartIndex, _CaretControl.CaretIndex));
            _selectionStartIndex = -1;
             UpdateSelectionBounds();
             UpdateScroll();
        }
        
        var caretIndex = _CaretControl.CaretIndex;
        Text = Text.Insert(caretIndex, input.ToString());
        _CaretControl.SetCaretPosition(caretIndex + 1);
    }

    protected void StopCaret()
    {
        _CaretControl.IsVisible = false;
        _CaretControl.ResetBlink().FreezeCaret();
        _state.TextInput -= UpdateTextInput;
    }

    protected void StartCaret()
    {
        
        _CaretControl.IsVisible = true;
        _CaretControl.ResetBlink().UnfreezeCaret();
        _state.TextInput += UpdateTextInput;
    }

    private bool _isShiftDown;
    private bool _isCtrlDown;
    private KeyboardKeys _lastHoldKey;
    private double _repeatTimer;
    private bool _isFirstRepeat;

    private IKeyboardState _state;
    private KeyboardKeys GetActiveKey(ReadOnlySpan<KeyboardKeys> pressedKeys)
    {
        foreach (var key in pressedKeys)
        {
            if (key is KeyboardKeys.LeftControl or KeyboardKeys.RightControl or 
                KeyboardKeys.LeftShift or KeyboardKeys.RightShift)
            {
                continue;
            }
            return key;
        }
        return KeyboardKeys.None;
    }
    
    private int GetNextWordIndex(int currentIndex)
    {
        if (currentIndex >= Text.Length) return Text.Length;

        int index = currentIndex;
        while (index < Text.Length && char.IsWhiteSpace(Text[index])) index++;
        while (index < Text.Length && !char.IsWhiteSpace(Text[index])) index++;
    
        return index;
    }

    private int GetPreviousWordIndex(int currentIndex)
    {
        if (currentIndex <= 0) return 0;

        int index = currentIndex - 1;
        while (index > 0 && char.IsWhiteSpace(Text[index])) index--;
        while (index > 0 && !char.IsWhiteSpace(Text[index - 1])) index--;
    
        return index;
    }
    
    public override void Update(TimeSpan deltaTime)
    {
        base.Update(deltaTime);
        if (!HasFocus) return;
        
        // Handle caret movement here
        var pressedKeys = _state.GetPressedKeys();

        var key = GetActiveKey(pressedKeys);
    
        if(_state.IsKeyPressed(KeyboardKeys.LeftControl) || _state.IsKeyPressed(KeyboardKeys.RightControl))
        {
            _isCtrlDown = true;
        }
        else
        {
            _isCtrlDown = false;
        }
    
        if(_state.IsKeyPressed(KeyboardKeys.LeftShift) || _state.IsKeyPressed(KeyboardKeys.RightShift))
        {
            _isShiftDown = true;
        }
        else        
        {
            _isShiftDown = false;
        }
    
        if (_state.IsKeyPressed(key))
        {
            if (key != _lastHoldKey)
            {
                HandleSpecialKey(key);
                _lastHoldKey = key;
                _repeatTimer = 0;
                _isFirstRepeat = true;
                _CaretControl.ResetBlink();
            }
            else
            {
                _repeatTimer += deltaTime.TotalMilliseconds;
                var delay = _isFirstRepeat ? 500 : 50;
            
                if (_repeatTimer >= delay)
                {
                    HandleSpecialKey(key);
                    _repeatTimer = 0;
                    _isFirstRepeat = false;
                    _CaretControl.FreezeCaret();
                    _CaretControl.ResetBlink();
                }
            }
        }
        
        if(key == KeyboardKeys.None)
        {
            if (_CaretControl.IsCaretFrozen)
            {
                _CaretControl.UnfreezeCaret();
            }
            _lastHoldKey = KeyboardKeys.None;
        }
        
        UpdateScroll();
    }
    
    private void HandleSpecialKey(KeyboardKeys key)
    {
        var caretIndex = _CaretControl.CaretIndex;
        switch (key)
        {
            case KeyboardKeys.Back:
                    
                if(_selectionStartIndex != -1 && _selectionStartIndex != caretIndex)
                {
                    Text = Text.Remove(Math.Min(_selectionStartIndex, caretIndex), Math.Abs(caretIndex - _selectionStartIndex));
                    _CaretControl.SetCaretPosition(Math.Min(_selectionStartIndex, caretIndex));
                    _selectionStartIndex = -1;
                    UpdateSelectionBounds();
                    UpdateScroll();
                    return;
                }
                if (caretIndex > 0)
                {
                    
                    if (_isCtrlDown)
                    {
                        int target = GetPreviousWordIndex(caretIndex);
                        Text = Text.Remove(target, caretIndex - target);
                        _CaretControl.SetCaretPosition(target);
                    }
                    else
                    {
                        Text = Text.Remove(caretIndex - 1, 1);
                        _CaretControl.SetCaretPosition(caretIndex - 1);
                    }
                }
                break;
            case KeyboardKeys.Delete:
                    
                if(_selectionStartIndex != -1 && _selectionStartIndex != caretIndex)
                {
                    Text = Text.Remove(Math.Min(_selectionStartIndex, caretIndex), Math.Abs(caretIndex - _selectionStartIndex));
                    _CaretControl.SetCaretPosition(Math.Min(_selectionStartIndex, caretIndex));
                    _selectionStartIndex = -1;
                    UpdateSelectionBounds();
                    UpdateScroll();
                    return;
                }
                if (caretIndex < Text.Length)
                {
                    
                    if (_isCtrlDown)
                    {
                        int target = GetNextWordIndex(caretIndex);
                        Text = Text.Remove(caretIndex, target - caretIndex);
                    }
                    else
                        Text = Text.Remove(caretIndex, 1);
                }
                break;
            case KeyboardKeys.Left:
                if (caretIndex > 0)
                {
                    if(_isCtrlDown)
                        _CaretControl.SetCaretPosition(GetPreviousWordIndex(caretIndex));
                    else
                        _CaretControl.MoveCaretLeft();
                    if (_isShiftDown)
                    {
                        if (_selectionStartIndex == -1)
                        {
                            _selectionStartIndex = caretIndex;
                        }
                        UpdateSelectionBounds();
                    }
                    else
                    {
                        _selectionStartIndex = -1;
                        UpdateSelectionBounds();
                    }
                }
                break;
            case KeyboardKeys.Right:
                if (caretIndex < Text.Length)
                {
                    if(_isCtrlDown)
                        _CaretControl.SetCaretPosition(GetNextWordIndex(caretIndex));
                    else
                        _CaretControl.MoveCaretRight();
                    if (_isShiftDown)
                    {
                        if (_selectionStartIndex == -1)
                        {
                            _selectionStartIndex = caretIndex;
                        }
                        UpdateSelectionBounds();
                    }
                    else
                    {
                        _selectionStartIndex = -1;
                        UpdateSelectionBounds();
                    }
                }
                break;
        }
    }
}