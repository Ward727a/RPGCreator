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

namespace RPGCreator.SDK.ECS.Components;

public struct StateComponent : IComponent
{
    private int[] Ints;
    private float[] Floats;
    private string[] Strings;
    private bool[] Bool;
    private Vector2[] Vector2;
    
    public StateComponent(int floatCount, int intCount, int stringCount, int boolCount, int vector2Count)
    {
        Floats = new float[floatCount];
        Ints = new int[intCount];
        Strings = new string[stringCount];
        for(int i = 0; i < stringCount; i++) Strings[i] = string.Empty;
        Bool = new bool[boolCount];
        Vector2 = new Vector2[vector2Count];
    }
    
    /// <summary>
    /// Gets a reference to a float value at the specified index.
    /// </summary>
    /// <param name="index">The index of the float value.</param>
    /// <returns>A reference to the float value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref float GetFloat(int index) => ref Floats.AsSpan()[index];

    /// <summary>
    /// Gets a reference to an int value at the specified index.
    /// </summary>
    /// <param name="index">The index of the int value.</param>
    /// <returns>A reference to the int value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref int GetInt(int index) => ref Ints.AsSpan()[index];

    /// <summary>
    /// Gets a reference to a bool value at the specified index.
    /// </summary>
    /// <param name="index">The index of the bool value.</param>
    /// <returns>A reference to the bool value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref bool GetBool(int index) => ref Bool.AsSpan()[index];

    /// <summary>
    /// Gets a reference to a string value at the specified index.
    /// </summary>
    /// <param name="index">The index of the string value.</param>
    /// <returns>A reference to the string value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref string GetString(int index) => ref Strings.AsSpan()[index];
    
    /// <summary>
    /// Gets a reference to a Vector2 value at the specified index.
    /// </summary>
    /// <param name="index">The index of the Vector2 value.</param>
    /// <returns>A reference to the Vector2 value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref Vector2 GetVector2(int index) => ref Vector2.AsSpan()[index];
    
    /// <summary>
    /// Sets the float value at the specified index.
    /// </summary>
    /// <param name="index">Index of the float value.</param>
    /// <param name="value">The float value to set.</param>
    public void SetFloat(int index, float value) => Floats[index] = value;
    
    /// <summary>
    /// Sets the int value at the specified index.
    /// </summary>
    /// <param name="index">Index of the int value.</param>
    /// <param name="value">The int value to set.</param>
    public void SetInt(int index, int value) => Ints[index] = value;
    
    /// <summary>
    /// Sets the bool value at the specified index.
    /// </summary>
    /// <param name="index">Index of the bool value.</param>
    /// <param name="value">The bool value to set.</param>
    public void SetBool(int index, bool value) => Bool[index] = value;
    
    /// <summary>
    /// Sets the string value at the specified index.
    /// </summary>
    /// <param name="index">Index of the string value.</param>
    /// <param name="value">The string value to set.</param>
    public void SetString(int index, string value) => Strings[index] = value;
    
    /// <summary>
    /// Sets the Vector2 value at the specified index.
    /// </summary>
    /// <param name="index">Index of the Vector2 value.</param>
    /// <param name="value">The Vector2 value to set.</param>
    public void SetVector2(int index, Vector2 value) => Vector2[index] = value;
}