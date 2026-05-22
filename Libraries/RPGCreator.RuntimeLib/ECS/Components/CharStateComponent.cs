using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.ECS.Components;

namespace RPGCreator.RuntimeLib.ECS.Components;

public struct CharStateComponent() : IComponent
{
    public string CurrentState
    {
        get;
        set
        {
            if (value == field) return;
            if (!string.IsNullOrWhiteSpace(field))
                PreviousState = field;
            Logger.Debug("StateComponent: Changing state from {PreviousState} to {NewState}", field, value);
            field = value.ToLowerInvariant().Trim().ReplaceLineEndings().Replace(" ", "_");
        }
    }

    public string PreviousState
    {
        get;
        private set => field = value.ToLowerInvariant().Trim().ReplaceLineEndings().Replace(" ", "_");
    }

    public EntityDirection CurrentDirection
    {
        get;
        set
        {
            if (value == field) return;
            PreviousDirection = field;
            field = value;
            Logger.Debug("StateComponent: Changing direction from {PreviousDirection} to {NewDirection}",
                PreviousDirection, field);
        }
    } = EntityDirection.Down;

    public EntityDirection PreviousDirection { get; private set; }
    
    public bool HasChanged => CurrentState != PreviousState || CurrentDirection != PreviousDirection;
    
    public Ulid LastResolvedAnimationId { get; set; } = Ulid.Empty;
}