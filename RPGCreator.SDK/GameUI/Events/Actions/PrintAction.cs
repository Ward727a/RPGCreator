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

using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.GameUI.Events.Contexts;

namespace RPGCreator.SDK.GameUI.Events.Actions;

public partial class PrintAction : IGuiAction
{
    public string Name => "Print";
    public string Description => "Prints a message in the console.";
    public Type[] SupportedEventContexts { get; } = [typeof(GuiEventContext)];

    [GuiControlProperty(DisplayName = "Message", Description = "The message to print.")]
    private string _message = "";

    public void LinkTo(Type context)
    {
        return;
    }

    public void Execute(GuiEventContext context)
    {
        Console.WriteLine(_message);
    }

    public bool Match(GuiEventContext context)
    {
        return true;
    }

    public IGuiAction Clone()
    {
        return new PrintAction();
    }
}