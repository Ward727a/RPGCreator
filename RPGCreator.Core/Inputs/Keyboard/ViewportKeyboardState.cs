using RPGCreator.SDK;
using RPGCreator.SDK.GamePlayer;
using RPGCreator.SDK.Inputs;

namespace RPGCreator.Core.Inputs.Keyboard;

public class ViewportKeyboardState : EngineKeyboardState
{
    private readonly KeyboardKeys[] _pendingBuffer = new KeyboardKeys[256];
    private int _pendingCount;
    private bool _pendingCapsLock;
    private bool _pendingNumLock;

    public ViewportKeyboardState()
    {
        _pendingCount = 0;
        _pendingCapsLock = false;
        _pendingNumLock = false;
        
        EngineProviders.PropertyChanged += (propName) =>
        {
            if (propName != nameof(EngineProviders.GameProvider)) return;
            if(EngineProviders.GameProvider == null) return;

            {
                var gameInstance = EngineProviders.GameProvider.GameInstance;

                if (gameInstance is IGamePlayer game)
                {
                    game.OnUpdate += UpdateFrame;
                }
            }

            EngineProviders.GameProvider.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName != nameof(EngineProviders.GameProvider.GameInstance)) return;
                
                var gameInstance = EngineProviders.GameProvider.GameInstance;

                if (gameInstance is IGamePlayer game)
                {
                    game.OnUpdate += UpdateFrame;
                }
            };
            
            EngineProviders.GameProvider.PropertyChanging += (_, e) =>
            {
                if (e.PropertyName != nameof(EngineProviders.GameProvider.GameInstance)) return;
                
                var gameInstance = EngineProviders.GameProvider.GameInstance;

                if (gameInstance is IGamePlayer game)
                {
                    game.OnUpdate -= UpdateFrame;
                }
            };
        };
    }
    
    /// <summary>
    /// Collects keyboard state to be applied on the next frame update.
    /// </summary>
    /// <param name="data"></param>
    public override void Update(RawKeyboardData data)
    {
        _pendingCount = Math.Min(data.PressedKeys.Length, _pendingBuffer.Length);
        data.PressedKeys[.._pendingCount].CopyTo(_pendingBuffer);
        _pendingCapsLock = data.CapsLock;
        _pendingNumLock = data.NumLock;
    }

    /// <summary>
    /// Applies the collected keyboard state for the current frame.
    /// </summary>
    /// <param name="elapsedTime"></param>
    private void UpdateFrame(TimeSpan elapsedTime)
    {
        (PreviousPressedKeys, PressedKeys) = (PressedKeys, PreviousPressedKeys);
        PressedKeys.Clear();
        for (int i = 0; i < _pendingCount; i++)
        {
            var key = _pendingBuffer[i];
            PressedKeys.Add(key);
            if (!PreviousPressedKeys.Contains(key))
            {
                OnKeyDown(key);
            }
        }
        foreach (var key in PreviousPressedKeys)
        {
            if (!PressedKeys.Contains(key))
            {
                OnKeyUp(key);
            }
        }
        IsCapsLockActive = _pendingCapsLock;
        IsNumLockActive = _pendingNumLock;
    }
}