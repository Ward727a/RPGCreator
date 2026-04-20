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
using RPGCreator.SDK.Types;

namespace RPGCreator.SDK.GameUI;


public abstract class ControlEventDescriptor(
    string name,
    Type eventGuiContext,
    PipedPath? category = null,
    string description = "")
{
    protected static readonly PipedPath DefaultCategory = "Default".ToPipedPath();
    
    public PipedPath Category { get; private set; } = category ?? DefaultCategory;
    public Type GuiContextType { get; private set; } = eventGuiContext;
    public string Name { get; private set; } = name;
    public string Description { get; private set; } = description;
    
    public IGuiAction? Action { get; set; }
}

public class ControlEventDescriptor<TGuiContext>(string name, PipedPath? category = null, string description = "") : ControlEventDescriptor(name, typeof(TGuiContext), category, description) where TGuiContext : GuiEventContext
{
}