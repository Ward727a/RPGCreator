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

using RPGCreator.SDK.Inputs;

namespace RPGCreator.SDK.GameUI.Interfaces;

/// <summary>
/// The UI manager is responsible for managing all UI elements in the game, including menus, HUDs, dialogs, and other interactive components.<br/>
/// It works closely with the input system and with the IUiRendererContext to ensure that UI elements are displayed correctly and respond to user interactions.
/// </summary>
public interface IUiManager
{
    /// <summary>
    /// The UI renderer context provides necessary methods and properties for rendering UI visual elements on the screen.<br/>
    /// It serves as an abstraction layer between the UI system and the underlying rendering engine.
    /// </summary>
    IUiRendererContext UiRendererContext { get; }

    /// <summary>
    /// Initializes the UI manager with the provided mouse state, allowing it to track mouse interactions and update UI elements accordingly.<br/>
    /// This is typically here that the UI manager will subscribe to mouse events and set up any necessary state for handling user input through the mouse.
    /// </summary>
    /// <param name="mouseState">The mouse state to be used by the UI manager for tracking mouse interactions and updating UI elements accordingly.</param>
    /// <param name="uiRendererContext">
    /// The UI renderer context to be used by the UI manager for rendering UI visual elements on the screen.<br/>
    /// If null, the UI manager will use the default one provided by the implematation of the UiManager that is being used by the game runner.
    /// </param>
    public void Initialize(IMouseState mouseState, IUiRendererContext? uiRendererContext = null);
    
    /// <summary>
    /// Updates the UI manager and all its managed UI elements based on the elapsed time since the last update.<br/>
    /// This method is typically called once per frame by the game runner, and it allows the UI manager to process user input, update animations, and manage the state of all active UI elements.
    /// </summary>
    /// <param name="elapsedTime">The time elapsed since the last update, in seconds.</param>
    public void Update(double elapsedTime);
}