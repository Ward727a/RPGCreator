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

using System.Buffers;
using System.Runtime.InteropServices;
using RPGCreator.SDK.Types.Internals;

namespace RPGCreator.SDK.Graph;

[StructLayout(LayoutKind.Explicit)]
public struct BpValue
{
    [FieldOffset(0)] public int ValueType;
    
    [FieldOffset(4)] public int IntValue; // 0
    [FieldOffset(4)] public float FloatValue; // 1
    [FieldOffset(4)] public bool BoolValue; // 3
    
    // String value is stored as an object, it ValueType is '2'
    [FieldOffset(8)] public object? ObjectValue; // 4

    public static BpValue FromInt(int val) => new BpValue() { ValueType = 0, IntValue = val };
    public static BpValue FromFloat(float val) => new BpValue() { ValueType = 1, FloatValue = val };
    public static BpValue FromString(string val) => new BpValue() { ValueType = 2, ObjectValue = val };
    public static BpValue FromBool(bool val) => new BpValue() { ValueType = 3, BoolValue = val };
    public static BpValue FromObject(object? val) => new BpValue() { ValueType = 4, ObjectValue = val };
    
    public int AsInt() => ValueType switch
    {
        0 => IntValue,
        _ => throw new InvalidCastException("Cannot cast value to int")
    };

    public float AsFloat() => ValueType switch
    {
        1 => FloatValue,
        _ => throw new InvalidCastException("Cannot cast value to float")
    };
    
    public string AsString() => ValueType switch
    {
        2 => ObjectValue as string ?? throw new InvalidCastException("Stored object is not a string"),
        _ => throw new InvalidCastException("Cannot cast value to string")
    };
    
    public bool AsBool() => ValueType switch
    {
        3 => BoolValue,
        _ => throw new InvalidCastException("Cannot cast value to bool")
    };
    
    public object? AsObject() => ValueType switch
    {
        4 => ObjectValue,
        _ => throw new InvalidCastException("Cannot cast value to object")
    };
    
    public static bool operator ==(BpValue left, BpValue right) => left.Equals(right);
    public static bool operator !=(BpValue left, BpValue right) => !left.Equals(right);
}

public class CompiledBpContextArgs(int requiredArgumentCount) : ICompiledBpContextArguments, IDisposable
{
    private readonly BpValue[] _arguments = ArrayPool<BpValue>.Shared.Rent(requiredArgumentCount);
    public int Count { get; } = requiredArgumentCount;

    public void SetArgument(int index, BpValue value)
    {
        _arguments[index] = value;
    }

    public BpValue GetArgument(int index)
    {
        return _arguments[index];
    }

    public bool IsArgumentOfType(int index, Type type)
    {
        if (Count <= index)
            return false;
        var arg = _arguments[index];
        return arg.ValueType switch
        {
            0 => type == typeof(int),
            1 => type == typeof(float),
            2 => type == typeof(string),
            3 => type == typeof(bool),
            4 => true, // Object can be any type
            _ => false
        };
    }

    public BpValue this[int index] { get => GetArgument(index); set => SetArgument(index, value); }

    public void Dispose()
    {
        ArrayPool<BpValue>.Shared.Return(_arguments, clearArray: true);
        GC.SuppressFinalize(this);
    }
}

public class CompiledBpContext : ICompiledBpContext
{
    private readonly CompiledBpContextArgs _arguments;
    public ICompiledBpContextArguments Arguments => _arguments;
    
    public CompiledBpContext() : this(0) { }
    
    public CompiledBpContext(CompiledBpContextArgs arguments)
    {
        _arguments = arguments;
    }
    
    public CompiledBpContext(int requiredArgumentCount)
    {
        _arguments = new CompiledBpContextArgs(requiredArgumentCount);
    }
}