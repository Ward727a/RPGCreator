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

namespace RPGCreator.SDK.Commands;

public abstract class BaseCommand : ICommand
{
    public event Action<BaseCommand>? Executed;
    public event Action<BaseCommand>? Undone;
    
    public abstract string Name { get; }
    protected abstract void OnExecute();
    protected abstract void OnUndo();
    
    public void Execute()
    {
        OnExecute();
        Executed?.Invoke(this);
    }

    public void Undo()
    {
        OnUndo();
        Undone?.Invoke(this);
    }
    
    public BaseCommand WhenUndone(Action<BaseCommand> action)
    {
        Undone += action;
        return this;
    }
    
    public BaseCommand WhenExecuted(Action<BaseCommand> action)
    {
        Executed += action;
        return this;
    }
}