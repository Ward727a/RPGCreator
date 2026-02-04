using RPGCreator.SDK;
using RPGCreator.SDK.Inputs;
using RPGCreator.SDK.RuntimeService;

namespace RPGCreator.Core.Inputs.Mouse;

public class ViewportMouseState : EngineMouseState
{
    private RawMouseData _pendingData;
    
    public ViewportMouseState()
    {
        RuntimeServices.OnceServiceReady((IGameRunner gameRunner) =>
        {
            gameRunner.OnUpdate += FrameUpdate;
        });
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