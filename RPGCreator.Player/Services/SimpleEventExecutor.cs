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

using System;
using System.Collections.Generic;
using CommunityToolkit.Diagnostics;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.SimpleEvent;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Modules.Definition;
using RPGCreator.SDK.Registry;
using RPGCreator.SDK.Services.RuntimeService;

namespace RPGCreator.Player.Services;

public class SimpleEventExecutor : ISimpleEventExecutor
{

    private readonly ISimpleEventRegistry _registry;
    
    private Dictionary<Ulid, BaseSimpleEventDefinition> _definitions = new();

    public SimpleEventExecutor()
    {
        _registry = RegistryServices.SimpleEvents;
    }
    
    public void Execute(BaseSimpleEventDefinition definition, CustomData localContext)
    {
        bool allConditionsMet = true;
        
        foreach (var condition in definition.Conditions)
        {
            if (_registry.TryGetSimpleEventCondition(condition.Key, out var conditionImpl))
            {
                if (!conditionImpl.EvaluateCondition(localContext))
                {
                    allConditionsMet = false;
                    break;
                }
            }
            else
            {
                Logger.Error($"[SimpleEventExecutor] Warning: Condition '{condition.Key}' not found in registry.");
                allConditionsMet = false;
                break;
            }
        }
        
        var actionsToExecute = allConditionsMet ? definition.ThenActions : definition.ElseActions;
        
        foreach (var action in actionsToExecute)
        {
            if (_registry.TryGetSimpleEventAction(action.Key, out var actionImpl))
            {
                actionImpl.Execute(localContext);
            }
            else
            {
                Logger.Error($"[SimpleEventExecutor] Warning: Action '{action.Key}' not found in registry.");
            }
        }
        
    }

    public void Execute(Ulid definitionId, CustomData localContext)
    {
        Guard.IsNotDefault(definitionId);
        Guard.IsNotNull(localContext);

        if (_definitions.TryGetValue(definitionId, out var cachedDefinition))
        {
            Execute(cachedDefinition, localContext);
        }

        EngineServices.AssetsManager.Load<BaseSimpleEventDefinition>(definitionId).OnSuccess((definition) =>
        {
            Execute(definition, localContext);
        })
        .OnFailure((err) =>
        {
            Logger.Error("[SimpleEventExecutor] Failed to load definition with id '{Id}' - Error: {err}", args: [definitionId, err]);
        });
    }
}