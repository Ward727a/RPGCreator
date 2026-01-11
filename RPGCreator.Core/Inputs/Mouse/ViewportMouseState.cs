using RPGCreator.SDK;
using RPGCreator.SDK.GamePlayer;
using RPGCreator.SDK.Inputs;

namespace RPGCreator.Core.Inputs.Mouse;

public class ViewportMouseState : EngineMouseState
{
    private RawMouseData _pendingData;
    
    public ViewportMouseState()
    {
        EngineProviders.PropertyChanged += (propName) =>
        {
            if (propName != nameof(EngineProviders.GameProvider)) return;

            if (EngineProviders.GameProvider.GameInstance is IGamePlayer game)
            {
                game.OnUpdate += FrameUpdate;
            }

            EngineProviders.GameProvider.PropertyChanging += (_, e) =>
            {
                if (e.PropertyName != nameof(EngineProviders.GameProvider.GameInstance)) return;
            
                if (EngineProviders.GameProvider.GameInstance is IGamePlayer game)
                {
                    game.OnUpdate -= FrameUpdate;
                }
            };
            EngineProviders.GameProvider.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName != nameof(EngineProviders.GameProvider.GameInstance)) return;
            
                if (EngineProviders.GameProvider.GameInstance is IGamePlayer game)
                {
                    game.OnUpdate += FrameUpdate;
                }
            };
        };
    }

    private void FrameUpdate(TimeSpan elapsed)
    {
        PreviousMouseState = PreviousMouseState == default ? _pendingData : MouseState;
        MouseState = _pendingData;
        RefreshLogic();
    }

    public override void Update(RawMouseData rawMouseData)
    {
        _pendingData = rawMouseData;
    }
}