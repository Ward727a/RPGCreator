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

using RPGCreator.SDK.Types;

namespace RPGCreator.SDK.EngineService;

public interface IModulePathResolver : IService
{
    /// <summary>
    /// Return the path registered for the given URN.<br/>
    /// If no path is registered, returns an empty string.
    /// </summary>
    /// <param name="targetUrn">The target URN to resolve the path for.</param>
    /// <returns>
    /// The resolved path as a string, or an empty string if not found.
    /// </returns>
    public string ResolvePath(URN targetUrn);
    
    /// <summary>
    /// Return the full file path for a given URN and relative file path.<br/>
    /// Combines the registered path for the URN with the provided file path.<br/>
    /// If no path is registered for the URN, returns the original file path.
    /// <example>
    ///
    /// If we have [rpgc://modules/MyModuleFolder] registered to "C:/Games/MyGame/Modules/MyModuleFolder",<br/>
    /// then calling <code>ResolveFilePath(rpgc://modules/MyModuleFolder, "Assets/Textures/texture.png")</code> will return:<br/>
    /// "C:/Games/MyGame/Modules/MyModuleFolder/Assets/Textures/texture.png"
    ///
    /// </example>
    /// </summary>
    /// <param name="targetUrn">The target URN to resolve the path for.</param>
    /// <param name="filePath">The relative file path to combine with the resolved path.</param>
    /// <returns>
    /// The combined full file path as a string.
    /// </returns>
    public string ResolveFilePath(URN targetUrn, string filePath);
    
    /// <summary>
    /// Registers a path for a given URN.<br/>
    /// This path will be used when resolving paths for the specified URN.
    /// </summary>
    /// <param name="targetUrn">The target URN to register the path for.</param>
    /// <param name="path">The path to register.</param>
    public void RegisterPath(URN targetUrn, string path);
    
    public void UnregisterPath(URN targetUrn);
}