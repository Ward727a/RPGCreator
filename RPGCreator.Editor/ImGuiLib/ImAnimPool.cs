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
using System.Runtime.CompilerServices;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.Shared.Types;

namespace RPGCreator.Editor.ImGuiLib;

public delegate T LerpDelegate<T>(T start, T end, float amount);

public enum AnimEndType
{
    /// <summary>
    /// Return to the Start value at the end
    /// </summary>
    HardLoop,

    /// <summary>
    /// Oscillates between the Start and End values, creating a smooth back-and-forth animation loop.
    /// </summary>
    SoftLoop,

    /// <summary>
    /// Holds the value at its final position at the end of the animation.
    /// </summary>
    Clamp
}

public enum EasingType
{ 
    /// <summary>
    /// Simple: Go from 0 to 1 at a constant speed
    /// </summary>
    Linear, 
    /// <summary>
    /// Start slowly, then speed up
    /// </summary>
    EaseInQuad, 
    /// <summary>
    /// Start fast, then slow down
    /// </summary>
    EaseOutQuad, 
    /// <summary>
    /// Speed up in the middle, slow down at the end
    /// </summary>
    EaseInOutQuad, 
    /// <summary>
    /// Bounce at the end of the animation
    /// </summary>
    EaseOutBounce, 
    /// <summary>
    /// Bounce like an elastic spring that overshoots the target before settling
    /// </summary>
    EaseOutElastic 
}

public static class Easing
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Evaluate(float t, EasingType type)
    {
        t = Math.Clamp(t, 0f, 1f);

        return type switch
        {
            EasingType.Linear => t,
            EasingType.EaseInQuad => t * t, 
            EasingType.EaseOutQuad => t * (2f - t), 
            EasingType.EaseInOutQuad => t < 0.5f ? 2f * t * t : -1f + (4f - 2f * t) * t, 
            EasingType.EaseOutBounce => BounceOut(t), 
            EasingType.EaseOutElastic => ElasticOut(t), 
            _ => t
        };
    }

    private static float BounceOut(float t)
    {
        if (t < 1f / 2.75f) return 7.5625f * t * t;
        if (t < 2f / 2.75f) return 7.5625f * (t -= 1.5f / 2.75f) * t + 0.75f;
        if (t < 2.5f / 2.75f) return 7.5625f * (t -= 2.25f / 2.75f) * t + 0.9375f;
        return 7.5625f * (t -= 2.625f / 2.75f) * t + 0.984375f;
    }

    private static float ElasticOut(float t)
    {
        if (t == 0f || t == 1f) return t;
        float p = 0.3f;
        return MathF.Pow(2f, -10f * t) * MathF.Sin((t - p / 4f) * (2f * MathF.PI) / p) + 1f;
    }
}

public struct ImAnimPointer<TType>
{
    public int Id { get; init; }
    public bool IsValid => Id >= 0;
    public bool IsRemoved { get; internal set; }
    public static ImAnimPointer<TType> Empty => new ImAnimPointer<TType> {
        Id = -1
    };
}

public static class ImAnimPool<T> where T : struct
{
    
    public struct AnimData()
    {
        public bool IsNotRemoved;
        public bool IsActive;
        public bool IsReversed;
        public T Start;
        public T End;
        public float Duration;
        public float TimeElapsed;
        public T CurrentValue;
        public AnimEndType EndType;
        public EasingType EasingType;
    }

    private static readonly AnimData[] _animations = new AnimData[1024];
    
    public static LerpDelegate<T> LerpFunction;

    public static ImAnimPointer<T> Create(T start, T end, float seconds, AnimEndType endType, EasingType easingType)
    {
        for (int i = 0; i < _animations.Length; i++)
        {
            if (!_animations[i].IsNotRemoved)
            {
                _animations[i] = new AnimData
                {
                    IsNotRemoved = true,
                    IsActive = true,
                    Start = start,
                    End = end,
                    Duration = seconds,
                    TimeElapsed = 0f,
                    CurrentValue = start,
                    EndType = endType,
                    EasingType = easingType
                };
                return new ImAnimPointer<T> { Id = i };
            }
        }
        
        Logger.Error("[RPGCreator.Editor.ImGuiLib.ImAnimPool<{T}>.Create] Error: AnimPool of type {T} is full (max: 1024).", typeof(T));
        return ImAnimPointer<T>.Empty;
    }

    public static void UpdateAll(float deltaTime)
    {
        if (LerpFunction == null) return;

        Span<AnimData> span = _animations.AsSpan();

        for (int i = 0; i < span.Length; i++)
        {
            ref AnimData anim = ref span[i];
            if (!anim.IsActive) continue;

            anim.TimeElapsed += deltaTime;
            var t = anim.TimeElapsed / anim.Duration;
            float loopT;
            
            if (anim.EndType == AnimEndType.SoftLoop)
            {
                loopT = 1.0f - MathF.Abs((t % 2.0f) - 1.0f);
            }
            else if (anim.EndType == AnimEndType.HardLoop)
            {
                loopT = t % 1.0f;
            }
            else
            {
                loopT = Math.Clamp(t, 0f, 1f);
                if (loopT >= 1f) anim.IsActive = false;
            }
            
            float easedT = Easing.Evaluate(loopT, anim.EasingType);

            anim.CurrentValue = LerpFunction(anim.Start, anim.End, easedT);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T GetValue(ImAnimPointer<T> pointer)
    {
        if (pointer.IsRemoved) return default;
        
        var id = pointer.Id;
        if (id >= 0 && id < _animations.Length)
            return _animations[id].CurrentValue;
        
        return default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Start(ImAnimPointer<T> pointer)
    {
        if (pointer.IsRemoved) return false;

        var id = pointer.Id;
        if (id < 0 || id >= _animations.Length)
            return false;

        ref AnimData anim = ref _animations[id];

        if (anim.IsActive)
            return false;
        
        anim.IsActive = true;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Pause(ImAnimPointer<T> pointer)
    {
        if (pointer.IsRemoved) return false;

        var id = pointer.Id;
        if (id < 0 || id >= _animations.Length)
            return false;

        ref AnimData anim = ref _animations[id];

        if (!anim.IsActive)
            return false;
        
        anim.IsActive = false;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Stop(ImAnimPointer<T> pointer)
    {
        if (pointer.IsRemoved) return false;

        var id = pointer.Id;
        if (id < 0 || id >= _animations.Length)
            return false;

        ref AnimData anim = ref _animations[id];

        if (!anim.IsActive)
            return false;
        
        anim.IsActive = false;
        anim.CurrentValue = anim.Start;
        anim.TimeElapsed = 0f;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsActive(ImAnimPointer<T> pointer)
    {
        if (pointer.IsRemoved) return false;

        var id = pointer.Id;
        if (id < 0 || id >= _animations.Length)
            return false;

        ref AnimData anim = ref _animations[id];
        
        return anim.IsActive;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsReversed(ImAnimPointer<T> pointer)
    {
        if (pointer.IsRemoved) return false;

        var id = pointer.Id;
        if (id < 0 || id >= _animations.Length)
            return false;
        
        ref AnimData anim = ref _animations[id];
        
        return anim.IsReversed;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Reverse(ImAnimPointer<T> pointer)
    {
        if (pointer.IsRemoved) return;

        var id = pointer.Id;
        if (id < 0 || id >= _animations.Length)
            return;
        
        ref AnimData anim = ref _animations[id];
        
        anim.IsReversed = !anim.IsReversed;
        
        (anim.Start, anim.End) = (anim.End, anim.Start);
        
        anim.TimeElapsed = Math.Clamp(anim.Duration - anim.TimeElapsed, 0f, anim.Duration);
        Start(pointer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float GetTimeElapsed(ImAnimPointer<T> pointer)
    {
        if (pointer.IsRemoved) return 0f;
        
        var id = pointer.Id;
        if (id < 0 || id >= _animations.Length)
            return 0f;
        
        ref AnimData anim = ref _animations[id];

        return anim.TimeElapsed;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAtEnd(ImAnimPointer<T> pointer)
    {
        if (pointer.IsRemoved) return true;
        
        var id = pointer.Id;
        if (id < 0 || id >= _animations.Length)
            return true;

        ref AnimData anim = ref _animations[id];

        return anim.TimeElapsed >= anim.Duration;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool GoTo(ImAnimPointer<T> pointer, float newTime)
    {
        if (pointer.IsRemoved) return false;

        var id = pointer.Id;
        if (id < 0 || id >= _animations.Length)
            return false;
        
        ref AnimData anim = ref _animations[id];

        anim.TimeElapsed = Math.Clamp(newTime, 0f, anim.Duration);

        return true;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Destroy(ImAnimPointer<T> pointer)
    {
        if (pointer.IsRemoved) return;

        var id = pointer.Id;
        if (id < 0 || id >= _animations.Length)
            return;
        
        ref AnimData anim = ref _animations[id];
        anim.IsActive = false;
        anim.IsNotRemoved = false;
        pointer.IsRemoved = true;
    }
}