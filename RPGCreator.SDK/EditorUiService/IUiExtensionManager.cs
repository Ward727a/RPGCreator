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

using RPGCreator.SDK.Modules.UIModule;

namespace RPGCreator.SDK.EditorUiService;

public interface IUiExtensionManager : IService
{
    /// <summary>
    /// Registers an extension for a specific UI region. The extension is defined as an Action that takes the target control and an optional context object, allowing for dynamic modification of the UI based on the provided context.<br/>
    /// For context, the use of RPGCreator.SDK.EditorUI is advised, lots of information can be found with the use of the correct contexts types. More details coming soon.<br/>
    /// </summary>
    /// <param name="region">The UI region for which the extension is being registered.</param>
    /// <param name="extension">
    /// An Action that defines the extension logic.<br/>
    /// It takes two parameters:
    /// <ul>
    ///     <li>
    ///     the target control (object) to which the extension will be applied.
    ///     </li>
    ///     <li>
    ///     an optional context object (object?) that can be used to provide additional information for the extension logic.
    ///     </li>
    /// </ul>
    /// The context can be null if not needed.</param>
    public void RegisterExtension(UIRegion region, Action<object, object?> extension);
    
    /// <summary>
    /// Applies all registered extensions for the specified region to the target control, providing an optional context for extension logic.<br/>
    /// For context, the use of the RPGCreator.Generator is advised. More details coming soon.<br/>
    /// </summary>
    /// <param name="region">The UI region for which to apply extensions.</param>
    /// <param name="targetControl">The control to which the extensions will be applied.</param>
    /// <param name="context">The context object that can be used by extensions to determine how to modify the target control. This is optional and can be null.</param>
    public void ApplyExtensions(UIRegion region, object targetControl, object? context = null);
}