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

namespace RPGCreator.SDK.Modules.Editor.Ui;

public enum EDrawAs
{
    /// <summary>
    /// The engine isn't able to determine the draw type.
    /// </summary>
    Unknown,
    /// <summary>
    /// The UI will be drawn as a content (in a window, or as a widget)
    /// </summary>
    Content,
    /// <summary>
    /// The UI will be drawn as a window (need to create a window)
    /// </summary>
    Window,
    /// <summary>
    /// The UI will be drawn as a popup (need to create a popup)
    /// </summary>
    Popup,
    /// <summary>
    /// The UI will be drawn as a modal (need to create a modal)
    /// </summary>
    Modal
}