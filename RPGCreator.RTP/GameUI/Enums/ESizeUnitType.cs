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

namespace RPGCreator.RTP.GameUI.Enums;

public enum ESizeUnitType
{
    /// <summary>
    /// The width and height are defined in pixels.
    /// </summary>
    Pixels,
    /// <summary>
    /// Percentage of the parent container's width and height.<br/>
    /// For example, if the parent container is 200x100 and the size is set to (50, 50) with Percentage, the resulting size will be (100, 50).
    /// </summary>
    Percentage,
    /// <summary>
    /// In this case, the width and height will be offset from the parent container's width and height.<br/>
    /// For example, if the parent container is 200x100 and the size is set to (-50, -20) with RelativeToParent, the resulting size will be (150, 80).
    /// </summary>
    RelativeToParent,
    /// <summary>
    /// In this case, the width and height will be offset from the screen's width and height.<br/>
    /// For example, if the screen is 800x600 and the size is set to (-100, -50) with RelativeToScreen, the resulting size will be (700, 550).
    /// </summary>
    RelativeToScreen
}