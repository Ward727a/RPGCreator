// RPG Creator - Open-source RPG Engine.
// (c) 2026 Ward
// 
// This file is part of RPG Creator and is distributed under the Apache 2.0 License.
// You are free to use, modify, and distribute this file under the terms of the Apache 2.0 License.
// See LICENSE for details.
// 
// ---
// 
// Ce fichier fait partie de RPG Creator et est distribué sous licence Apache 2.0.
// Vous êtes libre de l'utiliser, de le modifier et de le distribuer sous les termes de la licence Apache 2.0.
// Voir LICENSE pour plus de détails.
// 
// Contact:
// => Mail: Ward727a@gmail.com
//    Please use this object: "RPG Creator [YourObject]"
// => Discord: ward727
// 
// For urgent inquiries, sending both an email and a message on Discord is highly recommended for a quicker response.

namespace RPGCreator.SDK.Types;

public struct Color : IEquatable<Color>
{
    public byte R, G, B, A;
    
    public Color(byte r, byte g, byte b, byte a = 255)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }
    
    public Color(uint rgba)
    {
        R = (byte)((rgba >> 24) & 0xFF);
        G = (byte)((rgba >> 16) & 0xFF);
        B = (byte)((rgba >> 8) & 0xFF);
        A = (byte)(rgba & 0xFF);
    }
    
    public static Color operator *(Color color, float alpha)
    {
        return new Color(
            color.R,
            color.G,
            color.B,
            (byte)(color.A * alpha)
        );
    }
    
    public static Color operator *(float alpha, Color color)
    {
        return color * alpha;
    }

    public static Color FromHex(string hex)
    {
        if (hex.StartsWith("#"))
            hex = hex.Substring(1);

        if (hex.Length == 6)
            hex += "FF";

        if (hex.Length != 8)
            throw new ArgumentException("Hex string must be in the format RRGGBBAA or RRGGBB.");

        byte r = Convert.ToByte(hex.Substring(0, 2), 16);
        byte g = Convert.ToByte(hex.Substring(2, 2), 16);
        byte b = Convert.ToByte(hex.Substring(4, 2), 16);
        byte a = Convert.ToByte(hex.Substring(6, 2), 16);

        return new Color(r, g, b, a);
    }
    
    public string ToHex()
    {
        return $"#{R:X2}{G:X2}{B:X2}{A:X2}";
    }
    
    public static Color FromUnsignedInt(uint value)
    {
        byte r = (byte)((value >> 24) & 0xFF);
        byte g = (byte)((value >> 16) & 0xFF);
        byte b = (byte)((value >> 8) & 0xFF);
        byte a = (byte)(value & 0xFF);
        return new Color(r, g, b, a);
    }
    
    public uint ToUnsignedInt()
    {
        return ((uint)R << 24) | ((uint)G << 16) | ((uint)B << 8) | A;
    }
    
    public Color Darken(float factor)
    {
        return new Color(
            (byte)(R * factor),
            (byte)(G * factor),
            (byte)(B * factor),
            A
        );
    }
    
    public Color Lighten(float factor)
    {
        return new Color(
            (byte)(R + (255 - R) * factor),
            (byte)(G + (255 - G) * factor),
            (byte)(B + (255 - B) * factor),
            A
        );
    }
    
    public Color WithAlpha(byte alpha)
    {
        return new Color(R, G, B, alpha);
    }
    
    public Color MixWith(Color other, float ratio)
    {
        return new Color(
            (byte)(R * (1 - ratio) + other.R * ratio),
            (byte)(G * (1 - ratio) + other.G * ratio),
            (byte)(B * (1 - ratio) + other.B * ratio),
            (byte)(A * (1 - ratio) + other.A * ratio)
        );
    }
    
    public static Color GetOrDefault(Color? color, Color defaultColor)
    {
        return color ?? defaultColor;
    }
    
    public static Color GetOrDefault(Color? color)
    {
        return GetOrDefault(color, White);
    }
    
    public static Color Transparent => new Color(0, 0, 0, 0);
    public static Color White => new Color(255, 255, 255);
    public static Color Black => new Color(0, 0, 0);
    public static Color Red => new Color(255, 0, 0);
    public static Color Green => new Color(0, 255, 0);
    public static Color Blue => new Color(0, 0, 255);
    public static Color Yellow => new Color(255, 255, 0);
    public static Color Cyan => new Color(0, 255, 255);
    public static Color Magenta => new Color(255, 0, 255);
    public static Color Gray => new Color(128, 128, 128);
    public static Color LightGray => new Color(192, 192, 192);
    public static Color DarkGray => new Color(64, 64, 64);
    public static Color Orange => new Color(255, 165, 0);
    public static Color Purple => new Color(128, 0, 128);
    public static Color Brown => new Color(165, 42, 42);
    public static Color Pink => new Color(255, 192, 203);
    public static Color Lime => new Color(0, 255, 0);
    public static Color Navy => new Color(0, 0, 128);
    public static Color Teal => new Color(0, 128, 128);
    public static Color Olive => new Color(128, 128, 0);
    public static Color Maroon => new Color(128, 0, 0);
    public static Color Silver => new Color(192, 192, 192);
    public static Color Gold => new Color(255, 215, 0);
    public static Color Violet => new Color(238, 130, 238);
    public static Color Indigo => new Color(75, 0, 130);
    public static Color Coral => new Color(255, 127, 80);
    public static Color Salmon => new Color(250, 128, 114);
    public static Color Turquoise => new Color(64, 224, 208);
    public static Color Plum => new Color(221, 160, 221);
    public static Color Khaki => new Color(240, 230, 140);
    public static Color Lavender => new Color(230, 230, 250);
    public static Color Beige => new Color(245, 245, 220);
    public static Color Mint => new Color(189, 252, 201);
    public static Color SalmonPink => new Color(255, 145, 164);
    public static Color DeepSkyBlue => new Color(0, 191, 255);

    public bool Equals(Color other)
    {
        return R == other.R && G == other.G && B == other.B && A == other.A;
    }

    public override bool Equals(object? obj)
    {
        return obj is Color other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(R, G, B, A);
    }
    
    public static bool operator ==(Color left, Color right)
    {
        return left.Equals(right);
    }
    
    public static bool operator !=(Color left, Color right)
    {
        return !left.Equals(right);
    }
}