using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input;
using AvaloniaInside.MonoGame;
using RPGCreator.SDK;
using RPGCreator.SDK.Inputs;
using RPGCreator.UI.Test;

namespace RPGCreator.UI.Common.Bridge;

public class AvaloniaKeyboardBridge
{
    private readonly HashSet<KeyboardKeys> _activeKeys = new();
    private readonly KeyboardKeys[] _keyBuffer = new KeyboardKeys[256];

    public void RegisterEvents(Control control)
    {
        control.KeyDown += (s, e) => HandleKey(e, true);
        control.KeyUp += (s, e) => HandleKey(e, false);
        control.TextInput += TextInput;
        control.LostFocus += (s, e) =>
        {
            _activeKeys.Clear();
            PushState(control);
        };
    }

    private void TextInput(object? sender, TextInputEventArgs e)
    {
        if (sender != null && sender is Control control)
        {
            if (control.Name == "MonoGameImage")
            {
                e.Handled = true;
                foreach (char c in e.Text)
                {
                    EngineProviders.KeyboardProvider?.UpdateInputViewport(c);
                }
            }
        }
    }

    private void HandleKey(KeyEventArgs e, bool isDown)
    {
        if (e.Source is TextBox) return;
        if (AvaloniaKeyMapping.TryMapKey(e.Key, out var sdkKey))
        {
            if (isDown) _activeKeys.Add(sdkKey);
            else _activeKeys.Remove(sdkKey);
            PushState(e.Source as Control);
        }
    }

    private void PushState(Control? control)
    {
        int count = 0;
        foreach (var key in _activeKeys)
        {
            if (count < _keyBuffer.Length)
                _keyBuffer[count++] = key;
        }

        var raw = new RawKeyboardData(_keyBuffer.AsSpan(0, count), false, false);
        
        EngineProviders.KeyboardProvider?.Update(raw);
        
        if (control?.Name == "MonoGameImage")
        {
            EngineProviders.KeyboardProvider?.UpdateViewport(raw);
        }
        else
        {
            EngineProviders.KeyboardProvider?.UpdateViewport(new RawKeyboardData(ReadOnlySpan<KeyboardKeys>.Empty,
                false, false));
        }
    }
    
}