using Serilog;

namespace RPGCreator.Core.Runtimes.ECS.Components.Display.Animation;

public struct StateComponent : IComponent
{
    private string _currentState;
    public string CurrentState
    {
        get => _currentState;
        set
        {
            if(!string.IsNullOrWhiteSpace(_currentState))
                PreviousState = _currentState;
            Log.Debug("StateComponent: Changing state from {PreviousState} to {NewState}", _currentState, value);
            _currentState = value.ToLowerInvariant().Trim().ReplaceLineEndings().Replace(" ", "_");
        }
    }

    private string _previousState;
    public string PreviousState { get => _previousState; set => _previousState = value.ToLowerInvariant().Trim().ReplaceLineEndings().Replace(" ", "_"); }
}