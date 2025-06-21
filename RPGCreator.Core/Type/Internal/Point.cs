using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.Core.Type.Internal
{
    /// <summary>
    /// Universal Point structure representing a point in 2D space.<br/>
    /// It can be used to convert to and from MonoGame.Extended.Point and Avalonia.Point.<br/>
    /// </summary>
    public struct Point
    {
        public int X { get; set; }
        public int Y { get; set; }
        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }
        public Point(Microsoft.Xna.Framework.Point point)
        {
            X = point.X;
            Y = point.Y;
        }
        public Point(Avalonia.Point point)
        {
            X = (int)point.X;
            Y = (int)point.Y;
        }
        public Point(Point point)
        {
            X = point.X;
            Y = point.Y;
        }
        public Point(Vector2 vector)
        {
            X = (int)vector.X;
            Y = (int)vector.Y;
        }
        public Point(Microsoft.Xna.Framework.Vector2 vector)
        {
            X = (int)vector.X;
            Y = (int)vector.Y;
        }
        public override string ToString()
        {
            return $"X: {X}, Y: {Y}";
        }
        public Microsoft.Xna.Framework.Point ToMG()
        {
            return new Microsoft.Xna.Framework.Point(X, Y);
        }
        public static implicit operator Microsoft.Xna.Framework.Point(Point point)
        {
            return new Microsoft.Xna.Framework.Point(point.X, point.Y);
        }
        public static implicit operator Point(Microsoft.Xna.Framework.Point point)
        {
            return new Point(point.X, point.Y);
        }

        public Avalonia.Point ToAvalonia()
        {
            return new Avalonia.Point(X, Y);
        }
        public static implicit operator Avalonia.Point(Point point)
        {
            return new Avalonia.Point(point.X, point.Y);
        }
        public static implicit operator Point(Avalonia.Point point)
        {
            return new Point((int)point.X, (int)point.Y);
        }

        public Vector2 ToVector2()
        {
            return new Vector2(X, Y);
        }
        public static implicit operator Vector2(Point point)
        {
            return new Vector2(point.X, point.Y);
        }
        public static implicit operator Point(Vector2 vector)
        {
            return new Point((int)vector.X, (int)vector.Y);
        }

        public Microsoft.Xna.Framework.Vector2 ToMGVector2()
        {
            return new Microsoft.Xna.Framework.Vector2(X, Y);
        }
        public static implicit operator Microsoft.Xna.Framework.Vector2(Point point)
        {
            return new Microsoft.Xna.Framework.Vector2(point.X, point.Y);
        }
        public static implicit operator Point(Microsoft.Xna.Framework.Vector2 vector)
        {
            return new Point((int)vector.X, (int)vector.Y);
        }
    }
}
