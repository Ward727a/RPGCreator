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

using RPGCreator.SDK.EngineService;
using RPGCreator.SDK.Types;

namespace _BaseModule;

public static class Events
{
    public static EventRegistration OnDamageEvent = new EventRegistration()
    {
        EventId = new URN("rpgc", "events", "onDamage"),
        DisplayName = "On Damage",
        Description = "Triggered when an entity takes damage.",
        Schema = new Dictionary<string, Type>()
        {
            ["target"] = typeof(int),
            ["amount"] = typeof(double),
            ["damageType"] = typeof(URN)
        }
    };
    
    public static EventRegistration OnResourceReachedLimitEvent = new EventRegistration()
    {
        EventId = new URN("rpgc", "events", "onResourceReachedLimit"),
        DisplayName = "On Resource Reached Limit",
        Description = "Triggered when an entity's resource stat reaches its limit.",
        Schema = new Dictionary<string, Type>()
        {
            ["target"] = typeof(int),
            ["statId"] = typeof(Ulid)
        }
    };
}