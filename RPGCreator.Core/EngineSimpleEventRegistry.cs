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
using RPGCreator.SDK.EngineService;
using RPGCreator.SDK.Modules.SimpleEvents;
using RPGCreator.SDK.Types;

namespace RPGCreator.Core;

public class EngineSimpleEventRegistry : ISimpleEventRegistry
{
    
    private readonly Dictionary<URN, BaseSimpleEventCondition> _conditions = new();
    private readonly Dictionary<URN, BaseSimpleEventAction> _actions = new();
    public int SimpleEventConditionCount => _conditions.Count; // Return the count of registered simple event conditions
    public int SimpleEventActionCount => _actions.Count; // Return the count of registered simple event actions
    
    public bool RegisterSimpleEventCondition(BaseSimpleEventCondition condition, bool overwriteIfExists = false)
    {
        _conditions.TryGetValue(condition.Urn, out var existingCondition);
        if (existingCondition != null && !overwriteIfExists)
        {
            return false; // Condition with the same URN already exists and overwrite is not allowed
        }
        _conditions[condition.Urn] = condition; // Add or overwrite the condition
        return true;
    }

    public bool RegisterSimpleEventAction(BaseSimpleEventAction action, bool overwriteIfExists = false)
    {
        _actions.TryGetValue(action.Urn, out var existingAction);
        if (existingAction != null && !overwriteIfExists)
        {
            return false; // Action with the same URN already exists and overwrite is not allowed
        }
        _actions[action.Urn] = action; // Add or overwrite the action
        return true;
    }

    public bool UnregisterSimpleEventCondition(URN urn)
    {
        return _conditions.Remove(urn); // Remove the condition with the specified URN, returns true if removed
    }

    public bool UnregisterSimpleEventAction(URN urn)
    {
        return _actions.Remove(urn); // Remove the action with the specified URN, returns true if removed
    }

    public bool TryGetSimpleEventCondition(URN urn, [NotNullWhen(true)] out BaseSimpleEventCondition? condition)
    {
        return _conditions.TryGetValue(urn, out condition); // Try to get the condition with the specified URN
    }

    public bool TryGetSimpleEventAction(URN urn, [NotNullWhen(true)] out BaseSimpleEventAction? action)
    {
        return _actions.TryGetValue(urn, out action); // Try to get the action with the specified URN
    }

    public IEnumerable<BaseSimpleEventCondition> GetSimpleEventConditions()
    {
        return _conditions.Values; // Return all registered simple event conditions
    }

    public IEnumerable<BaseSimpleEventAction> GetSimpleEventActions()
    {
        return _actions.Values; // Return all registered simple event actions
    }

    public void ClearSimpleEventConditions()
    {
        _actions.Clear(); // Clear all registered simple event conditions
    }

    public void ClearSimpleEventActions()
    {
        _actions.Clear(); // Clear all registered simple event actions
    }

    public void ClearRegistry()
    {
        _conditions.Clear(); // Clear all registered simple event conditions
        _actions.Clear(); // Clear all registered simple event actions
    }
}