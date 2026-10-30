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

namespace RPGCreator.SDK.Modules.Editor.Ui;

public sealed class EditorUiPayloadData
{
    private readonly Dictionary<string, object> _materializedData = new();

    public void SetData(string key, object value)
    {
        _materializedData[key] = value;
    }

    public bool TryGet<T>(string key, [NotNullWhen(true)] out T? value)
    {
        if (_materializedData.TryGetValue(key, out var obj) && obj is T typedObj)
        {
            value = typedObj;
            return true;
        }
        
        value = default;
        return false;
    }
}