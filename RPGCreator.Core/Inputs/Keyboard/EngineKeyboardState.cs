using RPGCreator.SDK.Inputs;

namespace RPGCreator.Core.Inputs.Keyboard;

public class EngineKeyboardState : IKeyboardState
{
    
    private readonly HashSet<KeyboardKeys> _previousPressedKeys = new();
    private readonly HashSet<KeyboardKeys> _pressedKeys = new();
    
    public void Update(RawKeyboardData data)
    {
        _previousPressedKeys.Clear();
        foreach (var key in _pressedKeys)
        {
            _previousPressedKeys.Add(key);
        }
        _pressedKeys.Clear();
        foreach (var key in data.PressedKeys)
        {
            _pressedKeys.Add(key);
        }
        
        IsCapsLockActive = data.CapsLock;
        IsNumLockActive = data.NumLock;
    }

    public bool IsKeyPressed(KeyboardKeys key)
    {
        return _pressedKeys.Contains(key);
    }

    public bool IsKeyReleased(KeyboardKeys key)
    {
        return !_pressedKeys.Contains(key);
    }

    public bool WasKeyPressed(KeyboardKeys key)
    {
        return _previousPressedKeys.Contains(key);
    }

    public bool WasKeyReleased(KeyboardKeys key)
    {
        return !_previousPressedKeys.Contains(key);
    }

    public bool WasKeyJustPressed(KeyboardKeys key)
    {
        return !_previousPressedKeys.Contains(key) && _pressedKeys.Contains(key);
    }

    public bool WasKeyJustReleased(KeyboardKeys key)
    {
        return _previousPressedKeys.Contains(key) && !_pressedKeys.Contains(key);
    }

    public bool IsCapsLockActive { get; private set; } 
    public bool IsNumLockActive { get; private set; }
}