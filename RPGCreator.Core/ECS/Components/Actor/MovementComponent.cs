using System.Numerics;
using RPGCreator.SDK.ECS;

namespace RPGCreator.Core.Runtimes.ECS.Components.Actor;

public enum MovementMode
{
    Grid4,
    Grid8,
    Free
}

public struct MovementComponent : IComponent
{
    public MovementMode Mode;
    public float Speed;
    public Vector2 TargetDirection;
    public bool IsMoving;
}