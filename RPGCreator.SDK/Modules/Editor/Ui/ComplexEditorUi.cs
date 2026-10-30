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

using RPGCreator.Shared.Types;

namespace RPGCreator.SDK.Modules.Editor.Ui;

/// <summary>
/// Represents the base class for complex editor UI components in the RPG Creator editor framework.<br/>
/// This is based on the MVC pattern, providing a structured approach to building editor UI components.
/// </summary>
/// <typeparam name="TController">The type of the controller associated with this UI component.</typeparam>
/// <remarks>
/// This abstract class provides a foundation for implementing editable UI views with an associated controller,<br/>
/// allowing modular and reusable design patterns within the editor.<br/>
/// <br/>
/// Key features of this class implementation include:<br/>
/// - Strong integration with the routing system via <see cref="IEditorUiRouter"/>.<br/>
/// - Provides a shared and reusable base setup for editor UI components.<br/>
/// - Facilitates controller-based logic separation for better maintainability.
/// </remarks>
public abstract class ComplexEditorUi<TController> : EditorUiBindableViewBase
    where TController : class
{
    /// <summary>
    /// The controller in the MVC pattern associated with this editor UI.<br/>
    /// Responsible for handling user actions, controlling the flow of data,
    /// and interacting with the underlying model.
    /// </summary>
    protected TController Controller { get; }

    protected ComplexEditorUi(IEditorViewContext context, EditorViewDefinition definition, TController controller) : base(context, definition)
    {
        Controller = controller;
    }
}