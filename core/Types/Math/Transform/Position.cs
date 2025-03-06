using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.core.types.Math.Transform
{
    internal class Position
    {

        public float X;
        public float Y;
        public float? Z = null;

        public Position(float x, float y, float? z = null)
        {
            X = x;
            Y = y;
            Z = z;
        }

    }
}
