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

namespace RPGCreator.SDK.Assets.Stats;

public enum StatKind : byte
{
    /// <summary>
    /// Simple stat (e.g. strength, intelligence, etc.)
    /// </summary>
    Attribute = 0,
    /// <summary>
    /// Gauge stat that lives inside a min and a max (e.g. health, mana, etc.)
    /// </summary>
    Resource = 1,
    /// <summary>
    /// Calculated stat with a formula (e.g. dodge = agility * 0.5%).
    /// </summary>
    Derived = 2,
}