using System.Numerics;

namespace RPGCreator.SDK.ECS.Components;

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

    public Vector2 Position
    {
        get => new Vector2(X, Y);
        set
        {
            X = value.X;
            Y = value.Y;
        }
    }
    
    public Vector2 Scale
    {
        get => new Vector2(ScaleX, ScaleY);
        set
        {
            ScaleX = value.X;
            ScaleY = value.Y;
        }
    }
}