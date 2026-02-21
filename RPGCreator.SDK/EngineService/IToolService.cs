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

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using RPGCreator.SDK.Editor.Brushes;
using RPGCreator.SDK.GlobalState;
using RPGCreator.SDK.Inputs;
using RPGCreator.SDK.Types;

namespace RPGCreator.SDK.EngineService;


/// <summary>
/// The tool service is responsible for managing the currently selected tool (brush) and handling drawing actions in the editor.
/// </summary>
public interface IToolService : IService
{
    /// <summary>
    /// Gets the currently selected brush.<br/>
    /// Returns null if no brush is selected.
    /// </summary>
    /// <returns>
    /// The currently selected brush info if any; otherwise, null.
    /// </returns>
    public ToolLogic? GetSelectedTool();

    /// <summary>
    /// Use the selected tool at the specified position.<br/>
    /// This method uses the current brush selected in the brush state to perform the using action.
    /// </summary>
    /// <param name="at">The position where to use.</param>
    /// <param name="button"></param>
    public void UseAt(Vector2 at, MouseButton button);

    /// <summary>
    /// Handles the movement of the mouse while a tool is active.<br/>
    /// This method can be used to update the preview of the tool as the mouse moves, allowing for a more interactive drawing experience.<br/>
    /// But is not limited to that, it depends on the tool logic being currently used.
    /// </summary>
    /// <param name="at">The position where the mouse is at.</param>
    /// <param name="deltaPosition">The change in position since the last move.</param>
    public void MoveAt(Vector2 at, Vector2 deltaPosition);
    public void ClearPreview();
}