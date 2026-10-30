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
using Hexa.NET.ImGui;

namespace RPGCreator.Editor.ImGuiLib;

public static class ImAnim
{
    
    public static void Initialize()
    {
        ImAnimPool<float>.LerpFunction = (s, e, t) => s + (e - s) * t;
        ImAnimPool<System.Numerics.Vector2>.LerpFunction = System.Numerics.Vector2.Lerp;
        ImAnimPool<System.Numerics.Vector4>.LerpFunction = System.Numerics.Vector4.Lerp;
    }

    public static void UpdateAll()
    {
        var deltaTime = ImGui.GetIO().DeltaTime;
        ImAnimPool<float>.UpdateAll(deltaTime);
        ImAnimPool<System.Numerics.Vector2>.UpdateAll(deltaTime);
        ImAnimPool<System.Numerics.Vector4>.UpdateAll(deltaTime);
    }

    public static ImAnimPointer<T> Create<T>(T start, T end, float seconds, AnimEndType endType = AnimEndType.Clamp, EasingType easing = EasingType.Linear) where T : struct
    {
        return ImAnimPool<T>.Create(start, end, seconds, endType, easing);
    }

    public static void Destroy<T>(ImAnimPointer<T> pointer) where T : struct
    {
        ImAnimPool<T>.Destroy(pointer);
    }

    public static T Get<T>(ImAnimPointer<T> pointer) where T : struct
    {
        return ImAnimPool<T>.GetValue(pointer);
    }

    public static bool Start<T>(ImAnimPointer<T> pointer) where T : struct
    {
        return ImAnimPool<T>.Start(pointer);
    }
    
    public static bool Stop<T>(ImAnimPointer<T> pointer) where T : struct
    {
        return ImAnimPool<T>.Stop(pointer);
    }
    
    public static bool Pause<T>(ImAnimPointer<T> pointer) where T : struct
    {
        return ImAnimPool<T>.Pause(pointer);
    }

    public static void Reverse<T>(ImAnimPointer<T> pointer) where T : struct
    {
        ImAnimPool<T>.Reverse(pointer);
    }
    
    public static bool IsActive<T>(ImAnimPointer<T> pointer) where T : struct
    {
        return ImAnimPool<T>.IsActive(pointer);
    }

    public static bool IsReverse<T>(ImAnimPointer<T> pointer) where T : struct
    {
        return ImAnimPool<T>.IsReversed(pointer);
    }
    
    public static bool IsAtEnd<T>(ImAnimPointer<T> pointer) where T : struct
    {
        return ImAnimPool<T>.IsAtEnd(pointer);
    }

    public static float GetTimeElapsed<T>(ImAnimPointer<T> pointer) where T : struct
    {
        return ImAnimPool<T>.GetTimeElapsed(pointer);
    }

    public static bool GoTo<T>(ImAnimPointer<T> pointer, float newTime) where T : struct
    {
        return ImAnimPool<T>.GoTo(pointer, newTime);
    }
}