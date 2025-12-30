using System.Numerics;

namespace RPGCreator.SDK.Types;

public struct Rect
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }

    public Rect(float x, float y, float width, float height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }
    
    public Rect(Vector2 position, Size size)
    {
        X = position.X;
        Y = position.Y;
        Width = size.Width;
        Height = size.Height;
    }

    public float Left => X;
    public float Right => X + Width;
    public float Top => Y;
    public float Bottom => Y + Height;
    
    public Vector2 Position => new(X, Y);
    public Size Size => new(Width, Height);
    
    public bool Contains(Vector2 point)
    {
        return (point.X >= X && point.X <= Right && point.Y >= Y && point.Y <= Bottom);
    }
}
