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

public interface IEditorUiBindableView : IEditorUi
{
    /// <summary>
    /// Called when data needs to be bound to this view.<br/>
    /// It's used for example in the EditorUiRouter that can be found inside the EngineLib.
    /// </summary>
    /// <param name="childViewId">The unique resource name (URN) identifying the child view.</param>
    /// <param name="bindId">The identifier of the binding target within the child view.</param>
    /// <param name="data">The data payload to be bound to the child view.</param>
    void OnBind(URN childViewId, StringName bindId, EditorUiPayloadData data);

    /// <summary>
    /// Called when data needs to be unbound from this view.
    /// This method is intended for cleaning up or removing associations established during binding.
    /// </summary>
    /// <param name="childViewId">The unique resource name (URN) identifying the child view.</param>
    /// <param name="bindId">The identifier of the binding target within the child view.</param>
    /// <param name="data">The data payload that was previously bound to the child view.</param>
    void OnUnbind(URN childViewId, StringName bindId, EditorUiPayloadData data);

    /// <summary>
    /// Triggered when this view is activated with specific arguments.
    /// Typically used for initializing or responding to specific activation scenarios in the Editor UI framework.
    /// </summary>
    /// <param name="args">An optional object containing activation parameters specific to the view.</param>
    void OnActivate(object? args);

    /// <summary>
    /// Triggered when this view is deactivated.
    /// Typically used for cleaning up resources, saving state, or performing
    /// any necessary shutdown operations before the view is unloaded or hidden.
    /// </summary>
    void OnDeactivate();

    /// <summary>
    /// Define if the view has been disposed or not.
    /// </summary>
    bool Disposed { get; }
}