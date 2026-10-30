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

namespace RPGCreator.SDK.IO.Config;

public class LocalBlueprintSettings
{
    /// <summary>
    /// Should the blueprint compiler add the debug info (Log, and comment)
    /// </summary>
    public bool CompileDebugInfo = true;
    /// <summary>
    /// A list of added usings to all blueprint. It can be used to add more feature to the debug runtime.
    /// </summary>
    public string[] CompileDefaultUsings = [];
    /// <summary>
    /// Should the blueprint compiler add the library that the <see cref="CompileDefaultUsings"/> refer or not?
    /// </summary>
    public bool CompileAllowAddingUsings = false;

    
    public bool DevModeEnabled = false;
    
    /// <summary>
    /// Where the blueprint in JSON format should be saved.
    /// </summary>
    public string SaveFolder = "";
    /// <summary>
    /// Where the blueprint builded in .cs format should be saved.
    /// </summary>
    public string BuildFolder = "";
    /// <summary>
    /// Where the blueprint compiled in .dll format should be saved
    /// </summary>
    public string CompiledFolder = "";
}