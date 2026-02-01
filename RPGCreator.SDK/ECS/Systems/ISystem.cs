
namespace RPGCreator.SDK.ECS.Systems;

public abstract class ISystem
{
    public Action? OnEnable;
    public Action? OnDisable;

    /// <summary>
    /// Define the priority of the system. Upper values are executed first.
    /// Default priority is 0.
    /// Negative values are allowed.
    /// </summary>
    public abstract int Priority { get; }
    /// <summary>
    /// Indicate if the system is a drawing system.
    /// True => It will be executed inside the 'Draw' loop.
    /// False => It will be executed inside the 'Update' loop.
    /// Default is false.
    /// </summary>
    public abstract bool IsDrawingSystem { get; } 

    public abstract void Initialize(IEcsWorld ecsWorld);
    public abstract void Update(TimeSpan deltaTime);
}