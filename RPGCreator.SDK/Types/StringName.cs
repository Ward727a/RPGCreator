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

using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.HighPerformance.Buffers;

namespace RPGCreator.SDK.Types;

/// <summary>
/// A non-modifiable string that is shared between multiple objects.<br/>
/// It allows reducing memory usage and improving performance.<br/>
/// It should, however, be not used for string manipulation or operations that require frequent changes.<br/>
/// </summary>
public readonly struct StringName : 
    IEquatable<StringName>, 
    IComparable<StringName>, 
    ISpanParsable<StringName>,
    ISpanFormattable
{

    public static readonly StringName Empty = new();
    public bool IsEmpty => string.IsNullOrEmpty(_value);
    
    private static readonly StringPool _sharedPool = StringPool.Shared;

    private readonly string _value = string.Empty;
    private readonly int _hashCode = 0;

    public int Length => _value.Length;

    public StringName(ReadOnlySpan<char> charactersSpan)
    {
        _value = _sharedPool.GetOrAdd(charactersSpan);
        _hashCode = _value.GetHashCode();
    }

    public StringName(string value) : this(value.AsSpan())
    {
    }

    public bool Equals(StringName other)
    {
        return ReferenceEquals(_value, other._value);
    }

    public int CompareTo(StringName other)
    {
        if (ReferenceEquals(_value, other._value)) return 0;
        return string.Compare(_value, other._value, StringComparison.Ordinal);
    }

    public override bool Equals([NotNullWhen(true)] object? obj) => obj is StringName other && Equals(other);

    public override int GetHashCode() => _hashCode;

    public override string ToString() => _value;

    public string ToString(string? format, IFormatProvider? formatProvider) => _value;

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format,
        IFormatProvider? provider)
    {
        if (_value.AsSpan().TryCopyTo(destination))
        {
            charsWritten = _value.Length;
            return true;
        }

        charsWritten = 0;
        return false;
    }

    public static bool operator ==(StringName left, StringName right) => left.Equals(right);
    public static bool operator !=(StringName left, StringName right) => !left.Equals(right);

    public static implicit operator string(StringName name) => name._value;
    public static implicit operator StringName(string name) => new(name);

    /// <summary>
    /// Clears the shared string pool.<br/>
    /// It should be used with caution, as it can make the performance worse during a certain period of time.
    /// </summary>
    internal static void ClearPool() => _sharedPool.Reset();

    public static StringName Parse(string s, IFormatProvider? provider)
    {
        return new StringName(s);
    }

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out StringName result)
    {
        result = new StringName(s ?? string.Empty);
        return true;
    }

    public static StringName Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        return new StringName(s);
    }

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out StringName result)
    {
        result = new StringName(s);
        return true;
    }
    
    public ReadOnlyMemory<char> AsMemory() => _value.AsMemory();
    public ReadOnlySpan<char> AsSpan() => _value.AsSpan();
}