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
using System.Runtime.CompilerServices;

namespace RPGCreator.SDK.Extensions;

public static class Vector2Extensions
{
    /// <summary>
    /// Converts a Vector2 to a tuple of integers by flooring the X and Y components.
    /// </summary>
    /// <param name="vector">The Vector2 to convert.</param>
    /// <returns>A tuple containing the floored X and Y components as integers. (X, Y)</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (int, int) ToIntFloored(this Vector2 vector)
    {
        int x = (int)Math.Floor(vector.X);
        int y = (int)Math.Floor(vector.Y);
        return (x, y);
    }

    /// <summary>
    /// Converts a Vector2 to a tuple of integers by rounding the X and Y components.
    /// </summary>
    /// <param name="vector">The Vector2 to convert.</param>
    /// <returns>A tuple containing the rounded X and Y components as integers. (X, Y)</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (int, int) ToIntRounded(this Vector2 vector)
    {
        int x = (int)Math.Round(vector.X);
        int y = (int)Math.Round(vector.Y);
        return (x, y);
    }
}