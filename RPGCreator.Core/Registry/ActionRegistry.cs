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

using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Registry;
using RPGCreator.SDK.Types;

namespace RPGCreator.Core.Registry;

public class ActionRegistry : IActionRegistry
{
    
    private readonly Dictionary<URN, ActionInfo> _actions = new();
    
    public ActionRegistry()
    {
        RegisterAction(new ActionInfo
        {
            DisplayName = "Test Action",
            Description = "This is a test action for demonstration purposes.",
            Urn = new URN("rpgcreator:action:test"),
            Action = (args) =>
            {
                Logger.Info("Test Action executed with arguments: " +
                            (args != null ? string.Join(", ", args) : "none"));
            }
        });
    }

    public void RegisterAction(ActionInfo actionInfo, bool overrideIfExists = false)
    {
        if (_actions.ContainsKey(actionInfo.Urn))
        {
            if (overrideIfExists)
            {
                _actions[actionInfo.Urn] = actionInfo;
            }
            else
            {
                #if DEBUG
                throw new InvalidOperationException($"Action with URN {actionInfo.Urn} is already registered.");
                #else
                return; // In release mode, we simply ignore the registration if it already exists and override is not allowed.
                #endif
            }
        }
        else
        {
            _actions.Add(actionInfo.Urn, actionInfo);
        }
    }

    public void UnregisterAction(URN urn)
    {
        if (!_actions.Remove(urn))
        {
            Logger.Error($"Failed to unregister action with URN {urn}: not found.");
        }
    }

    public ActionInfo? GetAction(URN urn)
    {
        if (_actions.TryGetValue(urn, out var actionInfo))
        {
            return actionInfo;
        }
        else
        {
            Logger.Warning($"Action with URN {urn} not found.");
            return null;
        }
    }
}