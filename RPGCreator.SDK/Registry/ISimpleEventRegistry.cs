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
using RPGCreator.SDK.Modules.SimpleEvents;
using RPGCreator.SDK.Types;

namespace RPGCreator.SDK.EngineService;

public interface ISimpleEventRegistry : IService
{
    public static UrnSingleModule ConditionModule => "simple_events_conditions".ToUrnSingleModule();
    public static UrnSingleModule ActionModule => "simple_events_actions".ToUrnSingleModule();
    
    public bool RegisterSimpleEventCondition(BaseSimpleEventCondition condition, bool overwriteIfExists = false);
    public bool RegisterSimpleEventAction(BaseSimpleEventAction action, bool overwriteIfExists = false);
    
    public bool UnregisterSimpleEventCondition(URN urn);
    public bool UnregisterSimpleEventAction(URN urn);
    
    public bool TryGetSimpleEventCondition(URN urn, [NotNullWhen(true)] out BaseSimpleEventCondition? condition);
    public bool TryGetSimpleEventAction(URN urn, [NotNullWhen(true)] out BaseSimpleEventAction? action);
    
    public IEnumerable<BaseSimpleEventCondition> GetSimpleEventConditions();
    public IEnumerable<BaseSimpleEventAction> GetSimpleEventActions();
    
    public int SimpleEventConditionCount { get; }
    public int SimpleEventActionCount { get; }
    
    public void ClearSimpleEventConditions();
    public void ClearSimpleEventActions();
    
    public void ClearRegistry();
}