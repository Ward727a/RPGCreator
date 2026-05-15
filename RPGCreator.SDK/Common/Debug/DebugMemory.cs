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

using System.Diagnostics;

namespace RPGCreator.SDK.Debug;

public static class DebugMemory
{
    #if DEBUG
        private static readonly Dictionary<string, object?> _data = new();

        public static void Set(string key, object? value) => _data[key] = value;

        public static T? Get<T>(string key)
        {
            if (_data.TryGetValue(key, out var val) && val is T typedVal)
                return typedVal;
            return default;
        }
        
        public static void Unset(string key) => _data.Remove(key);

        public static IEnumerable<KeyValuePair<string, object?>> GetAll() => _data;
    #else
        [Conditional("DEBUG")] 
        public static void Set(string key, object? value) { }
        
        [Conditional("DEBUG")]
        public static void Unset(string key) { }

        [Conditional("DEBUG")]
        public static T? Get<T>(string key) => default;
    #endif
}