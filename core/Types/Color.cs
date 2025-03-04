using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.core.Types
{
    class Color
    {
        public Vector4 Value;

        /// <summary>
        /// Return the int value (0 - 255) to a valid float value (0.0 - 1.0)
        /// </summary>
        /// <param name="value">Color value as int (0 - 255)</param>
        /// <returns>Color value as float (0.0 - 1.0)</returns>
        public static float I2F(int value)
        {
            if(value >= 255)
            {
                return 1f;
            } else if(value <= 0)
            {
                return 0f;
            }

            return ((float)value / 255f);
        }

        public static Color FromRGBA(float r, float g, float b, float a)
        {
            return new Color()
            {
                Value = new(r, g, b, a)
            };
        }

        public static Color FromRGB(float r, float g, float b)
        {
            return FromRGBA(r, g, b, 1f);
        }

        public static Color FromRGBAi(int r, int g, int b, int a)
        {
            return FromRGBA(
                I2F(r),
                I2F(g),
                I2F(b),
                I2F(a)
                );
        }

        public static Color FromRGBi(int r, int g, int b)
        {
            return FromRGBAi(
                r,
                g,
                b,
                255
                );
        }

        public static Color FromCSColor(System.Drawing.Color color)
        {
            return FromRGBAi(
                color.R,
                color.G,
                color.B,
                color.A
                );
        }

        private Color(){ } // Base construct;
        private Color(int r, int g, int b)
        {
            Value = new(
                I2F(r),
                I2F(g),
                I2F(b),
                1
                );
        }

        public readonly static Color Black = new(0, 0, 0);
        public readonly static Color White = new(255, 255, 255);

        public readonly static Color DarkBlue = new(0, 0, 120);
        public readonly static Color Blue = new(0, 0, 255);
        public readonly static Color LightBlue = new(80, 80, 255);

        public readonly static Color DarkRed = new(120, 0, 0);
        public readonly static Color Red = new(255, 0, 0);
        public readonly static Color LightRed = new(255, 80, 80);

        public readonly static Color DarkYellow = new(170, 170, 0);
        public readonly static Color Yellow = new(255, 255, 0);
        public readonly static Color LightYellow = new(255, 255, 60);

        public readonly static Color DarkGreen = new(0, 120, 0);
        public readonly static Color Green = new(0, 255, 0);
        public readonly static Color LightGreen = new(80, 255, 80);

        public readonly static Color DarkPink = new(120, 0, 120);
        public readonly static Color Pink = new(255, 0, 255);
        public readonly static Color LightPink = new(255, 80, 255);

        public readonly static Color DarkMagenta = new(140, 0, 195);
        public readonly static Color Magenta = new(170, 0, 255);
        public readonly static Color LightMagenta = new(170, 80, 255);

        public readonly static Color DarkGray = new(60, 60, 60);
        public readonly static Color Gray = new(100, 100, 100);
        public readonly static Color LightGray = new(140, 140, 140);

        public readonly static Color DarkOrange = new(200, 120, 0);
        public readonly static Color Orange = new(255, 140, 0);
        public readonly static Color LightOrange = new(255, 160, 50);
    }
}
