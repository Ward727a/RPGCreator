using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.core.types.Math.Transform
{
    internal struct Position()
    {

        public float X;
        public float Y;
        public float? Z;

        public Position(float x, float y, float? z = null) : this()
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Position(Point point) : this()
        {
            X = point.X;
            Y = point.Y;
        }

        public Position(Vector2 v2) : this()
        {
            X = v2.X;
            Y = v2.Y;
        }

        public Position(Vector3 v3) : this()
        {
            X = v3.X;
            Y = v3.Y;
            Z = v3.Z;
        }

        public readonly bool Is2D => Z.HasValue;

        public void Move(float x, float y, float? z = null)
        {
            X = x; Y = y; Z = z;
        }

        private readonly bool IsSameDimension(Position CompareTo)
        {
            return (Is2D == CompareTo.Is2D);
        }

        public static bool operator <(Position pos1, Position pos2)
        {

            if (!pos1.IsSameDimension(pos2))
            {
                throw new InvalidOperationException("Cannot compare 2D and 3D position.");
            }
            return (pos1.X < pos2.X && pos1.Y < pos2.Y && pos1.Z < pos2.Z);
        }
        public static bool operator >(Position pos1, Position pos2)
        {
            if (!pos1.IsSameDimension(pos2))
            {
                throw new InvalidOperationException("Cannot compare 2D and 3D position.");
            }
            return (pos1.X > pos2.X && pos1.Y > pos2.Y && pos1.Z > pos2.Z);
        }

        public static Position operator +(Position pos1, Position pos2)
        {
            if (!pos1.IsSameDimension(pos2))
            {
                throw new InvalidOperationException("Cannot add 2D and 3D position.");
            }

            pos1.X += pos2.X;
            pos1.Y += pos2.Y;
            if (pos1.Is2D)
            {
                pos1.Z += pos2.Z;
            }
            return pos1;
        }

        public static Position operator +(Position pos, Scale scale)
        {
            if (pos.Is2D != scale.Is2D)
            {
                throw new InvalidOperationException("Cannot add 2D and 3D scale/position");
            }

            pos.X += scale.X;
            pos.Y += scale.Y;
            if(pos.Is2D)
            {
                pos.Z += scale.Z;
            }
            return pos;
        }

        public static Position operator ++(Position pos)
        {
            pos.X += 1;
            pos.Y += 1;
            if (!pos.Is2D)
            {
                pos.Z += 1;
            }
            return pos;
        }

        public override string ToString()
        {
            return $"Position{{X: {X}; Y: {Y}; Z: {Z}}}";
        }

        public readonly Vector2 ToVector2()
        {
            return new(X, Y);
        }

        public readonly Vector3 ToVector3()
        {
            if(Is2D)
            {
                return new(0, 0, 0);
            }
            return new(X, Y, (float)Z);
        }

    }
}
