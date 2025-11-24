using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.Core.Types.Internal
{
    /// <summary>
    /// Universal Point structure representing a point in 2D space.<br/>
    /// It can be used to convert to and from MonoGame.Extended.Point and Avalonia.Point.<br/>
    /// </summary>
    public record struct Point
    {
        public int X { get; set; }
        public int Y { get; set; }
        /// <summary>
        /// The width of the point, which is the same as X.<br/>
        /// </summary>
        /// <remarks>
        /// This is just a convenience property to access the X value as height for better readability.
        /// </remarks>
        public int Width => X;
        /// <summary>
        /// The height of the point, which is the same as Y.
        /// </summary>
        /// <remarks>
        /// This is just a convenience property to access the Y value as height for better readability.
        /// </remarks>
        public int Height => Y;
        
        public static Point Empty => new Point(0, 0);
        
        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Point(double x, double y)
        {
            X = (int)x;
            Y = (int)y;
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

        // Add operator overloads for addition
        public static Point operator +(Point a, Point b)
        {
            return new Point(a.X + b.X, a.Y + b.Y);
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

        public static Point Parse(string value)
        {
            Point point = new Point(0, 0);

            if (value.Contains(','))
            {
                var splitted = value.Split(',');
                if (splitted.Length != 2)
                {
                    throw new FormatException("Invalid Point format. Expected format: 'X Y'");
                }
                var xString = splitted[0].Trim();
                var yString = splitted[1].Trim();
                
                // Replace the "X: " and "Y: " prefixes if they exist
                if (xString.StartsWith("X: "))
                {
                    xString = xString.Substring(3);
                }
                if (yString.StartsWith("Y: "))
                {
                    yString = yString.Substring(3);
                }
                if (!int.TryParse(xString, out int x) || !int.TryParse(yString, out int y))
                {
                    throw new FormatException("Invalid Point format. Expected format: 'X Y'");
                }
                point = new Point(x, y);
            }

            return point;
        }
        
        public bool IsEqualTo(Point other)
        {
            return X == other.X && Y == other.Y;
        }
    }
}
