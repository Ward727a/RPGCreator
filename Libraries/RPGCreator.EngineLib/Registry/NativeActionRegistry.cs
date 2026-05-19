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

using CommunityToolkit.HighPerformance;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.ECS.Components;
using RPGCreator.SDK.Modules.NativeAction;
using RPGCreator.SDK.Registry;
using RPGCreator.SDK.Types;
using RPGCreator.Shared.Types;

namespace RPGCreator.EngineLib.Registry;

public class NativeActionRegistry : INativeActionRegistry
{
    private readonly Dictionary<Bitmask256, List<URN>> _nativeActionTriggers = new();
    private readonly Dictionary<URN, BaseNativeAction> _nativeActions = new();

    public int NativeActionCount => _nativeActions.Count;
    
    public bool RegisterNativeAction(BaseNativeAction action, bool overwriteIfExists = false)
    {
        if (action.BuildAction())
        {
            if (_nativeActions.TryAdd(action.ClassUrn, action))
            {
                if (_nativeActionTriggers.TryGetValue(action.TriggerMask, out var urns))
                    urns.Add(action.ClassUrn);
                else
                    _nativeActionTriggers.Add(action.TriggerMask, new List<URN> {action.ClassUrn});
                return true;
            }
            Logger.Error("Couldn't register native action: URN already exists.");
            return false;
        }
        
        Logger.Error("Couldn't register native action: Couldn't build the action, see error above.");
        return false;
    }

    public bool UnregisterNativeAction(URN urn)
    {
        return _nativeActions.Remove(urn);
    }

    public bool TryGetNativeAction(URN urn, out BaseNativeAction? action)
    {
        return _nativeActions.TryGetValue(urn, out action);
    }

    public ReadOnlySpan<URN> SearchNativeActionsBySignal(Bitmask256 signalMask)
    {
        foreach (var data in _nativeActionTriggers)
        {
            var triggerMask = data.Key;
            if (triggerMask.HasAny(signalMask))
            {
                return data.Value.AsSpan();
            }
        }
        
        return ReadOnlySpan<URN>.Empty;
    }
    
    public IEnumerable<BaseNativeAction> GetNativeActions()
    {
        return _nativeActions.Values;
    }
    
    public void ClearRegistry()
    {
        _nativeActions.Clear();
    }
}