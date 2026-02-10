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

namespace RPGCreator.Core;

public class EngineEventsRegister : IEventsRegisterService
{
    private readonly Dictionary<URN, EventRegistration> EventRegistrations = new();
    
    public void Register(EventRegistration registration)
    {
        EventRegistrations.TryAdd(registration.EventId, registration);
    }

    public EventRegistration? GetEventRegistration(URN eventId)
    {
        if (EventRegistrations.TryGetValue(eventId, out var registration))
            return registration;
        return null;
    }

    public IEnumerable<EventRegistration> GetAllEventRegistrations() => EventRegistrations.Values;

    public IEnumerable<URN> GetAllRegisteredEventIds() => EventRegistrations.Keys;
}