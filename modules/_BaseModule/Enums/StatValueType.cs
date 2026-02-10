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

namespace _BaseModule.Enums;

public enum StatValueType
{
    ActualValue,
    FinalValue,
    BaseValue,
    MinValue
}

public static class StatValueTypeExtensions
{
    public static string ToReadableString(this StatValueType valueType)
    {
        return valueType switch
        {
            StatValueType.ActualValue => "actual value",
            StatValueType.FinalValue => "final value",
            StatValueType.BaseValue => "base value",
            StatValueType.MinValue => "min value",
            _ => "unknown value type"
        };
    }

    public static string ToCompressedString(this StatValueType valueType)
    {
        return valueType switch
        {
            StatValueType.ActualValue => "ActualValue",
            StatValueType.FinalValue => "FinalValue",
            StatValueType.BaseValue => "BaseValue",
            StatValueType.MinValue => "MinValue",
            _ => "unknown"
        };
    }
    
    public static string ToDisplayString(this StatValueType valueType)
    {
        return valueType switch
        {
            StatValueType.ActualValue => "Actual",
            StatValueType.FinalValue => "Total",
            StatValueType.BaseValue => "Base",
            StatValueType.MinValue => "Minimum",
            _ => "Unknown"
        };
    }
}