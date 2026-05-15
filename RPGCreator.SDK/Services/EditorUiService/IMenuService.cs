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

namespace RPGCreator.SDK.EditorUiService;

public interface IMenuService : IService
{
    /// <summary>
    /// Opens a context menu at the specified host with the given menu actions.
    /// </summary>
    /// <param name="host">The UI element that will host the context menu.</param>
    /// <param name="actions">The list of menu actions to display in the context menu.</param>
    void OpenContextMenu(object host, IEnumerable<MenuAction> actions);
    /// <summary>
    /// Opens a context menu at the specified host with a pre-defined menu object.
    /// </summary>
    /// <param name="host">The UI element that will host the context menu.</param>
    /// <param name="menu">The pre-defined menu object to display as the context menu.</param>
    void OpenContextMenu(object host, object menu);
}

/// <summary>
/// Defines a menu action for context menus.
/// </summary>
/// <param name="Header">The display text of the menu action.</param>
/// <param name="Command">The action to execute when the menu item is selected.</param>
/// <param name="Icon">Optional icon for the menu item.</param>
/// <param name="SubMenu">Optional submenu actions.</param>
/// <param name="IsSeparator">Indicates if this action is a separator.</param>
public record MenuAction(
    string Header,
    Action? Command = null,
    string? Icon = null,
    IEnumerable<MenuAction>? SubMenu = null,
    bool IsSeparator = false)
{
    /// <summary>
    /// Gets a separator menu action.<br/>
    /// This can be used to visually separate groups of menu items.
    /// </summary>
    public static MenuAction Separator => new(string.Empty, IsSeparator: true);
}