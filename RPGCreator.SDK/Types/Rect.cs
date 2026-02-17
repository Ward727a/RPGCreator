using System.Numerics;

namespace RPGCreator.SDK.Types;

public struct Rect : IEquatable<Rect>
{
    public Vector2 Position;
    public Vector2 Size;
    
    public float X => Position.X;
    public float Y => Position.Y;
    public float Width => Size.X;
    public float Height => Size.Y;
    
    public Rect(Vector2 position, Vector2 size)
    {
        Position = position;
        Size = size;
    }
    public Rect(float x, float y, float width, float height)
    {
        Position = new Vector2(x, y);
        Size = new Vector2(width, height);
    }
    
    public bool Contains(Vector2 point)
    {
        return point.X >= Position.X && point.X <= Position.X + Size.X &&
               point.Y >= Position.Y && point.Y <= Position.Y + Size.Y;
    }
    
    public bool Intersects(Rect other)
    {
        return Position.X < other.Position.X + other.Size.X &&
               Position.X + Size.X > other.Position.X &&
               Position.Y < other.Position.Y + other.Size.Y &&
               Position.Y + Size.Y > other.Position.Y;
    }
    
    public static Rect Intersect(Rect? rectA, Rect? rectB)
    {
        if (!rectA.HasValue) return rectB ?? new Rect(0, 0, 0, 0);
        if (!rectB.HasValue) return rectA.Value;
        
        var a = rectA.Value;
        var b = rectB.Value;
        
        float x1 = Math.Max(a.Left, b.Left);
        float y1 = Math.Max(a.Top, b.Top);
        float x2 = Math.Min(a.Right, b.Right);
        float y2 = Math.Min(a.Bottom, b.Bottom);
        
        if (x2 > x1 && y2 > y1)
        {
            return new Rect(x1, y1, x2 - x1, y2 - y1);
        }

        return new Rect(0, 0, 0, 0);
    }
    
    public override string ToString()
    {
        return $"Rect(Position: {Position}, Size: {Size})";
    }
    
    public float Left => Position.X;
    public float Right => Position.X + Size.X;
    public float Top => Position.Y;
    public float Bottom => Position.Y + Size.Y;
    
    public Vector2 Center => new Vector2(Position.X + Size.X / 2, Position.Y + Size.Y / 2);
    
    public static Rect FromCenter(Vector2 center, Vector2 size)
    {
        return new Rect(new Vector2(center.X - size.X / 2, center.Y - size.Y / 2), size);
    }

    public bool Equals(Rect other)
    {
        return Position.Equals(other.Position) && Size.Equals(other.Size);
    }

    public override bool Equals(object? obj)
    {
        return obj is Rect other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Position, Size);
    }
    
    public static bool operator ==(Rect left, Rect right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Rect left, Rect right)
    {
        return !(left == right);
    }
    
    public static bool operator ==(Rect? left, Rect? right)
    {
        if (!left.HasValue && !right.HasValue) return true;
        if (!left.HasValue || !right.HasValue) return false;
        return left.Value.Equals(right.Value);
    }
    
    public static bool operator !=(Rect? left, Rect? right)
    {
        return !(left == right);
    }
}
