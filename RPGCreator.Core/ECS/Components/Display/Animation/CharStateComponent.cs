using RPGCreator.SDK.ECS;
using RPGCreator.SDK.ECS.Components;
using Serilog;

namespace RPGCreator.Core.ECS.Components.Display.Animation;

public struct CharStateComponent() : IComponent
{
    private string _currentState;
    public string CurrentState
    {
        get => _currentState;
        set
        {
            if(value == _currentState) return;
            if(!string.IsNullOrWhiteSpace(_currentState))
                PreviousState = _currentState;
            Log.Debug("StateComponent: Changing state from {PreviousState} to {NewState}", _currentState, value);
            _currentState = value.ToLowerInvariant().Trim().ReplaceLineEndings().Replace(" ", "_");
        }
    }

    private string _previousState;
    public string PreviousState { get => _previousState; private set => _previousState = value.ToLowerInvariant().Trim().ReplaceLineEndings().Replace(" ", "_"); }

    private EntityDirection _currentDirection = EntityDirection.Down;
    public EntityDirection CurrentDirection
    {
        get => _currentDirection;
        set
        {
            if(value == _currentDirection) return;
            PreviousDirection = _currentDirection;
            _currentDirection = value;
            Log.Debug("StateComponent: Changing direction from {PreviousDirection} to {NewDirection}", PreviousDirection, _currentDirection);
        }
    }
    public EntityDirection PreviousDirection { get; private set; }
    
    public bool HasChanged => CurrentState != PreviousState || CurrentDirection != PreviousDirection;
    
    public Ulid LastResolvedAnimationId { get; set; } = Ulid.Empty;
}