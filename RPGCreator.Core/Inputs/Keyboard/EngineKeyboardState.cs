using RPGCreator.SDK.GlobalState;
using RPGCreator.SDK.Inputs;

namespace RPGCreator.Core.Inputs.Keyboard;

public class EngineKeyboardState : BaseState, IKeyboardState
{
    
    protected HashSet<KeyboardKeys> PreviousPressedKeys = new();
    protected HashSet<KeyboardKeys> PressedKeys = new();

    public bool IsCapsLockActive { get; protected set; } 
    public bool IsNumLockActive { get; protected set; }
    public event Action<KeyboardKeys>? KeyDown;
    public event Action<KeyboardKeys>? KeyUp;
    
    public event Action<char>? TextInput;

    public virtual void Update(RawKeyboardData data)
    {
        PreviousPressedKeys.Clear();
        foreach (var key in PressedKeys) PreviousPressedKeys.Add(key);

        PressedKeys.Clear();
        foreach (var key in data.PressedKeys)
        {
            PressedKeys.Add(key);
        
            if (!PreviousPressedKeys.Contains(key))
            {
                KeyDown?.Invoke(key);
            }
        }
    
        foreach (var key in PreviousPressedKeys)
        {
            if (!PressedKeys.Contains(key))
            {
                KeyUp?.Invoke(key);
            }
        }
    
        IsCapsLockActive = data.CapsLock;
        IsNumLockActive = data.NumLock;
    }

    public void UpdateInput(char text)
    {
        TextInput?.Invoke(text);
    }

    protected void OnKeyDown(KeyboardKeys key)
    {
        KeyDown?.Invoke(key);
    }
    
    protected void OnKeyUp(KeyboardKeys key)
    {
        KeyUp?.Invoke(key);
    }
    
    public bool IsKeyPressed(KeyboardKeys key)
    {
        return PressedKeys.Contains(key);
    }

    public bool IsKeyReleased(KeyboardKeys key)
    {
        return !PressedKeys.Contains(key);
    }

    public bool WasKeyPressed(KeyboardKeys key)
    {
        return PreviousPressedKeys.Contains(key);
    }

    public bool WasKeyReleased(KeyboardKeys key)
    {
        return !PreviousPressedKeys.Contains(key);
    }

    public bool WasKeyJustPressed(KeyboardKeys key)
    {
        return !PreviousPressedKeys.Contains(key) && PressedKeys.Contains(key);
    }

    public bool WasKeyJustReleased(KeyboardKeys key)
    {
        return PreviousPressedKeys.Contains(key) && !PressedKeys.Contains(key);
    }

    public ReadOnlySpan<KeyboardKeys> GetPressedKeys()
    {
        return PressedKeys.ToArray().AsSpan();
    }

    public override void Reset()
    {
        // Do nothing.
    }
}