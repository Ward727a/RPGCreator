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
using Microsoft.Xna.Framework;

namespace RPGCreator.RTP.Extensions;

public static class MatrixExtension
{
    public static Matrix ToXna(this Matrix3x2 m)
    {
        return new Matrix(
            m.M11, m.M12, 0f, 0f, // Axe X (Scale/Rotation)
            m.M21, m.M22, 0f, 0f, // Axe Y (Scale/Rotation)
            0f,    0f,    1f, 0f, // Axe Z (Depth, not used here)
            m.M31, m.M32, 0f, 1f  // Translation (X, Y)
        );
    }
}