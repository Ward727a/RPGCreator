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

public enum ComparisonType
{
    Equal,
    NotEqual,
    Greater,
    Less,
    GreaterOrEqual,
    LessOrEqual
}

public static class ComparisonTypeExtensions
{
    public static string ToReadableString(this ComparisonType comparisonType)
    {
        return comparisonType switch
        {
            ComparisonType.Equal => "is equal to",
            ComparisonType.NotEqual => "is not equal to",
            ComparisonType.Greater => "is greater than",
            ComparisonType.Less => "is less than",
            ComparisonType.GreaterOrEqual => "is greater than or equal to",
            ComparisonType.LessOrEqual => "is less than or equal to",
            _ => "unknown comparison"
        };
    }
    
    public static string ToSymbol(this ComparisonType comparisonType)
    {
        return comparisonType switch
        {
            ComparisonType.Equal => "==",
            ComparisonType.NotEqual => "!=",
            ComparisonType.Greater => ">",
            ComparisonType.Less => "<",
            ComparisonType.GreaterOrEqual => ">=",
            ComparisonType.LessOrEqual => "<=",
            _ => "?"
        };
    }
}