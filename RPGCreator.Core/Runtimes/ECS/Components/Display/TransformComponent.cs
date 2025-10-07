using System.Numerics;

namespace RPGCreator.Core.Runtimes.ECS.Components.Display;

public struct TransformComponent : IComponent
{
    public float X;
    public float Y;
    /// <summary>
    /// In degrees
    /// </summary>
    public float Rotation;
    public float ScaleX;
    public float ScaleY;
    
    public Vector2 Position => new Vector2(X, Y);
}