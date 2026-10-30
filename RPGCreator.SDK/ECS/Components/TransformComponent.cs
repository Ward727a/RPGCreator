using Microsoft.Xna.Framework;
using Vector2 = System.Numerics.Vector2;

namespace RPGCreator.SDK.ECS.Components;

public struct TransformComponent
{
    /// <summary>
    /// Position of the entity in the game world.
    /// </summary>
    public Vector2 Position;

    /// <summary>
    /// Size of the entity in pixels (Width and Height).
    /// </summary>
    public Point Size;

    /// <summary>
    /// Multiplicator of the sprite rendering scale (1.0 = real size).
    /// </summary>
    public Vector2 Scale = Vector2.One;
    
    
    public Rectangle Bounds => new(
        (int)Position.X, 
        (int)Position.Y, 
        (int)(Size.X * Scale.X), 
        (int)(Size.Y * Scale.Y)
    );

    public TransformComponent(Vector2 position, Point size)
    {
        Position = position;
        Size = size;
    }
}