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

namespace RPGCreator.SDK.Tools.ParameterTypes;

public class IntParameter(string displayName, string description, int defaultValue, int min, int max, int step)
    : IToolParameterRanged<int>
{
    public string DisplayName { get; } = displayName;
    public string Description { get; } = description;
    public int Min { get; } = min;
    public int Max { get; } = max;
    public int Step { get; } = step;

    private int _value = defaultValue;
    public ref int ValueRef => ref _value;

    public Type ValueType => typeof(int);
}