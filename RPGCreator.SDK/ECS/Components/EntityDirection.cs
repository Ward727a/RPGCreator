using System.Numerics;

namespace RPGCreator.SDK.ECS.Components;

public enum EntityDirection
{
    UpLeft    = 0,
    Up        = 1,
    UpRight   = 2,
    Left      = 3,
    Center    = 4, // no movement
    Right     = 5,
    DownLeft  = 6,
    Down      = 7,
    DownRight = 8
}

public static class EntityDirectionExtensions
{
    /// <summary>
    /// Checks if the direction is diagonal.
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    public static bool IsDiagonal(this EntityDirection direction)
    {
        return direction == EntityDirection.UpLeft ||
               direction == EntityDirection.UpRight ||
               direction == EntityDirection.DownLeft ||
               direction == EntityDirection.DownRight;
    }
    
    /// <summary>
    /// A simple map to convert a vector2 direction to an entity direction int.<br/>
    /// The index is calculated as: (signX + 1) + (signY + 1) * 3<br/>
    /// Where signX and signY are the signs of the X and Y components of the vector2 direction (-1, 0, 1).
    /// </summary>
    public static readonly int[] DirectionMap =
    {
        (int)EntityDirection.UpLeft,    // (-1,-1)
        (int)EntityDirection.Up,        // (0,-1)
        (int)EntityDirection.UpRight,   // (1,-1)
        (int)EntityDirection.Left,      // (-1,0)
        (int)EntityDirection.Center,    // (0,0)
        (int)EntityDirection.Right,     // (1,0)
        (int)EntityDirection.DownLeft,  // (-1,1)
        (int)EntityDirection.Down,      // (0,1)
        (int)EntityDirection.DownRight  // (1,1)
    };
    
    /// <summary>
    /// Return a direction int from a vector2 direction.<br/>
    /// Values are defined in <see cref="DirectionMap"/>, see for more info.
    /// </summary>
    /// <param name="direction">The direction vector.</param>
    /// <returns>The direction as an int.</returns>
    public static int GetDirectionFromVector(this Vector2 direction)
    {
        if (direction.LengthSquared() < 0.01f)
            return (int)EntityDirection.Center;
        int ix = (int)Math.Sign(direction.X) + 1; // -1,0,1 -> 0,1,2
        int iy = (int)Math.Sign(direction.Y) + 1; // -1,0,1 -> 0,1,2
        return DirectionMap[ix + iy * 3];
    }
    
    public static int ToInt(this EntityDirection direction)
    {
        return (int)direction;
    }

    public static string ToString(this EntityDirection direction)
    {
        return direction switch
        {
            EntityDirection.UpLeft => "up_left",
            EntityDirection.Up => "up",
            EntityDirection.UpRight => "up_right",
            EntityDirection.Left => "left",
            EntityDirection.Center => "center",
            EntityDirection.Right => "right",
            EntityDirection.DownLeft => "down_left",
            EntityDirection.Down => "down",
            EntityDirection.DownRight => "down_right",
            _ => "unknown"
        };
    }
}