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

using RPGCreator.SDK.GameUI.Events.Actions;
using RPGCreator.SDK.GameUI.Events.Contexts;
using RPGCreator.SDK.Registry;

namespace RPGCreator.EngineLib.Registry;

public class GuiActionRegistry : IGuiActionRegistry
{
    private readonly HashSet<IGuiAction> _actions = new();
    
    public void RegisterAction(IGuiAction action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        _actions.Add(action);
    }

    public void UnregisterAction(IGuiAction action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));
        
        _actions.Remove(action);
    }

    public IEnumerable<IGuiAction>? GetActionByContext<TEventContext>() where TEventContext : GuiEventContext
    {
        return _actions.Where(a => a.SupportedEventContexts.Contains(typeof(TEventContext)));
    }

    public IEnumerable<IGuiAction>? GetActionByContext(Type contextType)
    {
        return _actions.Where(a => a.SupportedEventContexts.Contains(contextType) || a.SupportedEventContexts.Any(s => s.IsAssignableFrom(contextType)));
    }
}