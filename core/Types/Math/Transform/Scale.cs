using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.core.types.Math.Transform
{
    internal struct Scale(float x, float y, float? z = null)
    {
        public float X = x;

        public float Y = y;

        public float? Z = z;

        public readonly bool Is2D => Z.HasValue;

        public void Rescale(float x, float y, float? z = null)
        {
            X = x; Y = y; Z = z;
        }

        private readonly bool IsSameDimension(Scale CompareTo)
        {
            return (Is2D == CompareTo.Is2D);
        }

        public static bool operator <(Scale scale1, Scale scale2)
        {

            if (scale1.IsSameDimension(scale2))
            {
                throw new InvalidOperationException("Cannot compare 2D and 3D scales.");
            }
            return (scale1.X < scale2.X && scale1.Y < scale2.Y && scale1.Z < scale2.Z);
        }
        public static bool operator >(Scale scale1, Scale scale2)
        {
            if (scale1.IsSameDimension(scale2))
            {
                throw new InvalidOperationException("Cannot compare 2D and 3D scales.");
            }
            return (scale1.X > scale2.X && scale1.Y > scale2.Y && scale1.Z > scale2.Z);
        }

        public static Scale operator +(Scale scale1, Scale scale2)
        {
            if (scale1.IsSameDimension(scale2))
            {
                throw new InvalidOperationException("Cannot add 2D and 3D scales.");
            }

            scale1.X += scale2.X;
            scale1.Y += scale2.Y;
            if (scale1.Is2D)
            {
                scale1.Z += scale2.Z;
            }
            return scale1;
        }

        public static Scale operator ++(Scale scale)
        {
            scale.X += 1;
            scale.Y += 1;
            if (!scale.Is2D)
            {
                scale.Z += 1;
            }
            return scale;
        }
    }
}
