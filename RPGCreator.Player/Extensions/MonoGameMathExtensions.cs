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

using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace RPGCreator.Player.Extensions;

/// <summary>
/// Extensions for fast conversion between MonoGame and System.Numerics math types.<br/>
/// It's using Unsafe.As for zero-allocation conversion.<br/>
/// <br/>
/// What is doing under the hood:<br/>
/// - Both types have the same memory layout.<br/>
/// - Unsafe.As simply treats the memory of one type as the other type without any copying or allocation.<br/>
/// <br/>
/// For example, we have a Vector2(a,b) in MonoGame represented in memory like this: [00 00 00 FF](a) [00 00 00 11](b).<br/>
/// Vector2(a,b) in System.Numerics has the same memory placement: [00 00 00 FF](a) [00 00 00 11](b).<br/>
/// So when we use Unsafe.As to convert between them, it just reinterprets the same bytes without any overhead.<br/>
/// <br/>
/// As such, it's only safe to use when both types have identical memory layouts, which is the case for these math types.
/// </summary>
public static class MonoGameMathExtensions
{
    /// <summary>
    /// This is a security, in case MonoGame or System.Numerics changes the size of their types in future versions.<br/>
    /// It should not happen, but well, better be safe than sorry.<br/>
    /// If the sizes do not match, an exception is thrown at type initialization time.
    /// </summary>
    /// <exception cref="NotSupportedException">Thrown if the sizes of the types do not match.</exception>
    static MonoGameMathExtensions()
    {
        // Verify that the sizes of the types match to ensure safe conversion
        if (Unsafe.SizeOf<Vector2>() != Unsafe.SizeOf<Microsoft.Xna.Framework.Vector2>())
            throw new NotSupportedException("Vector2 size mismatch between System.Numerics and MonoGame.");

        if (Unsafe.SizeOf<Matrix4x4>() != Unsafe.SizeOf<Microsoft.Xna.Framework.Matrix>())
            throw new NotSupportedException("Matrix size mismatch between System.Numerics and MonoGame.");
            
        if (Unsafe.SizeOf<Vector3>() != Unsafe.SizeOf<Microsoft.Xna.Framework.Vector3>())
            throw new NotSupportedException("Vector3 size mismatch between System.Numerics and MonoGame.");
    }
 
    #region Unsafe Conversions
    
    public static Microsoft.Xna.Framework.Matrix ToXnaFast(this Matrix4x4 matrix)
    {
        return Unsafe.As<Matrix4x4, Microsoft.Xna.Framework.Matrix>(ref matrix);
    }
    
    public static Matrix4x4 ToNumericFast(this Microsoft.Xna.Framework.Matrix matrix)
    {
        return Unsafe.As<Microsoft.Xna.Framework.Matrix, Matrix4x4>(ref matrix);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 ToNumericFast(this Microsoft.Xna.Framework.Vector2 vector)
    {
        return Unsafe.As<Microsoft.Xna.Framework.Vector2, Vector2>(ref vector);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Microsoft.Xna.Framework.Vector2 ToXnaFast(this Vector2 vector)
    {
        return Unsafe.As<Vector2, Microsoft.Xna.Framework.Vector2>(ref vector);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Microsoft.Xna.Framework.Vector3 ToXnaFast(this Vector3 vector)
    {
        return Unsafe.As<Vector3, Microsoft.Xna.Framework.Vector3>(ref vector);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 ToNumericFast(this Microsoft.Xna.Framework.Vector3 vector)
    {
        return Unsafe.As<Microsoft.Xna.Framework.Vector3, Vector3>(ref vector);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Microsoft.Xna.Framework.Rectangle ToXnaFast(this Rectangle rect)
    {
        return Unsafe.As<Rectangle, Microsoft.Xna.Framework.Rectangle>(ref rect);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Rectangle ToSystemFast(this Microsoft.Xna.Framework.Rectangle rect)
    {
        return Unsafe.As<Microsoft.Xna.Framework.Rectangle, Rectangle>(ref rect);
    }
    
    #endregion
    
    // Here we have standard conversions that involves some copying, but are safer in case the memory layouts change in future versions.
    // The "fast" suffix is kept for consistency, but these methods do involve some overhead due to copying.
    #region Standard Conversions
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Microsoft.Xna.Framework.Color ToXnaFast(this Color color)
    {
        return new Microsoft.Xna.Framework.Color(color.R, color.G, color.B, color.A);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color ToSystemFast(this Microsoft.Xna.Framework.Color color)
    {
        return Color.FromArgb(color.A, color.R, color.G, color.B);
    }
    
    #endregion
}