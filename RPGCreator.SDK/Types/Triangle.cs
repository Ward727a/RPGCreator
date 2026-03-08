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

using System.Numerics;

namespace RPGCreator.SDK.Types;

public record struct Triangle(Vector2 A, Vector2 B, Vector2 C)
{
    private readonly float _area = -1;
    /// <summary>
    /// Calculate the center of the shape.
    /// </summary>
    public readonly Vector2 Center => (A + B + C) * 0.33333334f;

    /// <summary>
    /// Calculate the area of collision for the shape.
    /// </summary>
    public readonly float Area => _area >= 0 ? _area : CalculateArea();
    private readonly float CalculateArea() => Math.Abs((A.X * (B.Y - C.Y) + B.X * (C.Y - A.Y) + C.X * (A.Y - B.Y)) * 0.5f);
    
    public readonly bool Equals(Triangle other) => A == other.A && B == other.B && C == other.C;

    public readonly override int GetHashCode()
    {
        unchecked 
        {
            int hash = 17;
            hash = hash * 31 + A.GetHashCode();
            hash = hash * 31 + B.GetHashCode();
            hash = hash * 31 + C.GetHashCode();
            return hash;
        }
    }
    
    public readonly bool Contains(Vector2 p)
    {
        // We get the 3 signs
        float s1 = (p.X - B.X) * (A.Y - B.Y) - (A.X - B.X) * (p.Y - B.Y);
        float s2 = (p.X - C.X) * (B.Y - C.Y) - (B.X - C.X) * (p.Y - C.Y);
        float s3 = (p.X - A.X) * (C.Y - A.Y) - (C.X - A.X) * (p.Y - A.Y);

        
        return (s1 >= 0 && s2 >= 0 && s3 >= 0) || (s1 <= 0 && s2 <= 0 && s3 <= 0);
    }
}