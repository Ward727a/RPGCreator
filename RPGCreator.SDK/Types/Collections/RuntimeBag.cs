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
using RPGCreator.SDK.Assets.Definitions.Tilesets;

namespace RPGCreator.SDK.Types.Collections;

/// <summary>
/// This class allows to store runtime data in a bag-like structure.<br/>
/// It can be used to store various types of data that need to be accessed during runtime.<br/>
/// Note: This is used for example inside the <see cref="BaseTilesetDef"/> for the Tags runtime storage.
/// </summary>
public class RuntimeBag
{
    private readonly Dictionary<Type, object> _data = new();
    
    public void Set<T>(T value) where T : notnull
    {
        _data[typeof(T)] = value;
    }
    
    public bool TryGet<T>([NotNullWhen(true)]out T? value) where T : notnull
    {
        if (_data.TryGetValue(typeof(T), out var obj) && obj is T typedValue)
        {
            value = typedValue;
            return true;
        }

        value = default;
        return false;
    }
    
}