namespace RPGCreator.SDK.Assets.Definitions.Skills;

public enum EBuffStackBehavior
{
    /// <summary>
    /// Refreshes the duration of the buff if it is already applied, but does not add a new stack.
    /// </summary>
    RefreshDuration,
    /// <summary>
    /// Ignores the new application if the buff is already applied.
    /// </summary>
    IgnoreIfAlreadyApplied,
    /// <summary>
    /// Replaces the existing buff with the new one, resetting its duration and stacks.
    /// </summary>
    ReplaceExisting,
    /// <summary>
    /// Adds a new stack of the buff, up to the maximum allowed stacks, but does not refresh the duration.
    /// </summary>
    AddStack,
    /// <summary>
    /// Adds a new stack of the buff, up to the maximum allowed stacks, and refreshes the duration.
    /// </summary>
    AddStackAndRefreshDuration
}