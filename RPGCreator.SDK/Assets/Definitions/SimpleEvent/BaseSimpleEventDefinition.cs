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
using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.ECS;
using RPGCreator.SDK.Modules.Definition;
using RPGCreator.SDK.Modules.SimpleEvents;
using RPGCreator.SDK.Serializer;
using RPGCreator.SDK.Types;
using RPGCreator.SDK.Types.Internals;

namespace RPGCreator.SDK.Assets.Definitions.SimpleEvent;

[SerializingType("SimpleEventConditionEntry")]
public class SimpleEventConditionEntry
{
    public bool ExpectedResult { get; set; }
    public CustomData Parameters { get; set; } = new();
}

[SerializingType("SimpleEventActionEntry")]
public class SimpleEventActionEntry
{
    public CustomData Parameters { get; set; } = new();
}

[SerializingType("SimpleEventDefinition")]
public class BaseSimpleEventDefinition : BaseAssetDef, ISerializable, IDeserializable
{
    #region SAVED PROPERTIES
    /// <summary>
    /// A dictionary storing the conditions of this simple event.
    /// The key is the URN of the condition, and the value is the expected result of the condition (true or false).
    /// </summary>
    public Dictionary<URN, SimpleEventConditionEntry> Conditions { get; private set; } = new();
    /// <summary>
    /// The action URNs to execute if the conditions are met (true).
    /// </summary>
    public Dictionary<URN, SimpleEventActionEntry> ThenActions { get; private set; } = new();
    /// <summary>
    /// The action URNs to execute if the conditions are not met (false).
    /// </summary>
    public Dictionary<URN, SimpleEventActionEntry> ElseActions { get; private set; } = new();

    public string Tag { get; set; } = string.Empty;
    
    #endregion
    
    #region RUNTIME PROPERTIES
    
    public Bitmask256 SignalInterestsMask { get; private set; } = new();
    
    private readonly Dictionary<URN, BaseSimpleEventCondition> _conditionsByUrnCache = new();
    private readonly Dictionary<URN, BaseSimpleEventAction> _thenActionsByUrnCache = new();
    private readonly Dictionary<URN, BaseSimpleEventAction> _elseActionsByUrnCache = new();
    
    #endregion
    
    public override UrnSingleModule UrnModule => "simple_event".ToUrnSingleModule();

    public SerializationInfo GetObjectData()
    {
        return new SerializationInfo(typeof(BaseSimpleEventDefinition))
            .AddValue(nameof(Unique), Unique)
            .AddValue(nameof(Conditions), Conditions)
            .AddValue(nameof(ThenActions), ThenActions)
            .AddValue(nameof(ElseActions), ElseActions);
    }

    public bool TryGetCondition(URN condition, out bool expectedResult, [NotNullWhen(true)] out BaseSimpleEventCondition? expectedCondition)
    {
        var expectedResultFound = Conditions.TryGetValue(condition, out var entry);
        
        expectedResult = entry?.ExpectedResult ?? false;
        
        if (!expectedResultFound) throw new Exception($"Condition with URN {condition} not found in simple event definition with URN {Urn}!");
        
        if (!_conditionsByUrnCache.TryGetValue(condition, out expectedCondition))
        {
            if (RegistryServices.SimpleEventRegistry.TryGetSimpleEventCondition(condition, out expectedCondition))
            {
                expectedCondition.Parameters = entry?.Parameters ?? new CustomData();
                _conditionsByUrnCache.Add(condition, expectedCondition);
                return true;
            }

            return false;
        }
        return true;
    }
    
    public bool TryGetThenAction(URN action, [NotNullWhen(true)] out BaseSimpleEventAction? actionDefinition)
    {
        
        var expectedResultFound = ThenActions.TryGetValue(action, out var entry);
        
        if (!expectedResultFound) throw new Exception($"Then action with URN {action} not found in simple event definition with URN {Urn}!");
        
        if (!_thenActionsByUrnCache.TryGetValue(action, out actionDefinition))
        {
            if (RegistryServices.SimpleEventRegistry.TryGetSimpleEventAction(action, out actionDefinition))
            {
                actionDefinition.Parameters = entry?.Parameters ?? new CustomData();
                _thenActionsByUrnCache.Add(action, actionDefinition);
                return true;
            }

            return false;
        }
        return true;
    }
    
    public bool TryGetElseAction(URN action, [NotNullWhen(true)] out BaseSimpleEventAction? actionDefinition)
    {
        var expectedResultFound = ThenActions.TryGetValue(action, out var entry);
        
        if (!expectedResultFound) throw new Exception($"Then action with URN {action} not found in simple event definition with URN {Urn}!");
        
        if (!_elseActionsByUrnCache.TryGetValue(action, out actionDefinition))
        {
            if (RegistryServices.SimpleEventRegistry.TryGetSimpleEventAction(action, out actionDefinition))
            {
                actionDefinition.Parameters = entry?.Parameters ?? new CustomData();
                _elseActionsByUrnCache.Add(action, actionDefinition);
                return true;
            }

            return false;
        }
        return true;
    }

    public List<Ulid> GetReferencedAssetIds()
    {
        var referencedIdsSize = _conditionsByUrnCache.Count + _thenActionsByUrnCache.Count + _elseActionsByUrnCache.Count;
        Ulid[] referencedIds = new Ulid[referencedIdsSize];
        int index = 0;
        foreach (var condition in _conditionsByUrnCache.Values)
        {
            referencedIds[index++] = condition.Unique;
        }
        foreach (var action in _thenActionsByUrnCache.Values)
        {
            referencedIds[index++] = action.Unique;
        }

        foreach (var action in _elseActionsByUrnCache.Values)
        {
            referencedIds[index++] = action.Unique;
        }
        return referencedIds.ToList();
    }

    public void SetObjectData(DeserializationInfo info)
    {
        info.TryGetValue(nameof(Unique), out Ulid unique);
        
        Unique = unique;

        info.TryGetValue(nameof(Tag), out string? tag);
        if (tag != null) Tag = tag;
        
        info.TryGetValue(nameof(Conditions), out Dictionary<URN, SimpleEventConditionEntry>? conditions);
        info.TryGetValue(nameof(ThenActions), out Dictionary<URN, SimpleEventActionEntry>? thenActions);
        info.TryGetValue(nameof(ElseActions), out Dictionary<URN, SimpleEventActionEntry>? elseActions);
        
        if (conditions != null) Conditions = conditions;
        if (thenActions != null) ThenActions = thenActions;
        if (elseActions != null) ElseActions = elseActions;
    }
    
    public void BakeInterests()
    {
        SignalInterestsMask.Clear();

        foreach (var conditionsKey in Conditions.Keys)
        {
            if(RegistryServices.SignalRegistry.TryGetSignalMask(conditionsKey, out var conditionMask))
            {
                SignalInterestsMask.Set(conditionMask, true);
            }
        }
    }
}