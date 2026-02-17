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

using RPGCreator.SDK.Commands;

namespace RPGCreator.SDK.EngineService;

public interface ICommandManager : IService
{
    /// <summary>
    /// Event triggered whenever the state of the command history changes (e.g., after executing, undoing, or redoing a command).
    /// </summary>
    public event Action? StateChanged;
    
    /// <summary>
    /// Indicates whether there are commands available to undo.
    /// </summary>
    public bool CanUndo { get; }
    /// <summary>
    /// Indicates whether there are commands available to redo.
    /// </summary>
    public bool CanRedo { get; }
    
    /// <summary>
    /// Executes a command and adds it to the history for potential undo/redo operations.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    public void ExecuteCommand(ICommand command);
    /// <summary>
    /// Undoes the last executed command.
    /// </summary>
    public void UndoLastCommand();
    /// <summary>
    /// Redoes the last undone command.
    /// </summary>
    public void RedoLastCommand();
    /// <summary>
    /// Clears the command history.
    /// </summary>
    public void ClearHistory();
    /// <summary>
    /// Gets the name of the command that would be undone next.
    /// </summary>
    /// <returns>The name of the undo command.</returns>
    public string GetUndoCommandName();
    /// <summary>
    /// Gets the name of the command that would be redone next.
    /// </summary>
    /// <returns>The name of the redo command.</returns>
    public string GetRedoCommandName();
}