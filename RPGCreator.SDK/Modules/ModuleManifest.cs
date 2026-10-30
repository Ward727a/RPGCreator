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

using RPGCreator.SDK.Serializer;
using RPGCreator.Shared.Types;

namespace RPGCreator.SDK.Modules;

public struct ModuleManifest()
{
    /// <summary>
    /// URN of the module (e.g. 'RPGCreator.Module.Example' or 'Fr.Ward727.MyModule')
    /// </summary>
    public URN ModuleUrn = URN.Empty;
    /// <summary>
    /// Name of the module
    /// </summary>
    public string ModuleName = string.Empty;
    /// <summary>
    /// Description of the module
    /// </summary>
    public string ModuleDescription = string.Empty;
    /// <summary>
    /// Version of the module
    /// </summary>
    public Version ModuleVersion = new(0, 0, 0);
    /// <summary>
    /// Target version of the engine
    /// </summary>
    public Version TargetEngineVersion = new(0, 0, 0);
    /// <summary>
    /// List of dependencies
    /// </summary>
    public URN[] Dependencies = [];
    /// <summary>
    /// List of incompatible modules
    /// </summary>
    public URN[] IncompatibleModules = [];
    /// <summary>
    /// Path to the entry DLL
    /// </summary>
    public string EntryDll = string.Empty;
    /// <summary>
    /// Indicates if the module is enabled
    /// </summary>
    public bool IsEnabled = false;
    /// <summary>
    /// List of authors
    /// </summary>
    public string[] Authors = [];

    /// <summary>
    /// List of features provided by the module (e.g. "editor_ui").<br/>
    /// This allows the engine to scan folders for specific .json files.
    /// </summary>
    public StringName[] ModuleFeatures = [];
    
    public static Result<ModuleManifest> CreateManifest(string moduleManifestJsonPath, ISerializerService serializerService)
    {
        if(!File.Exists(moduleManifestJsonPath))
        {
            return Result<ModuleManifest>.Fail("Module manifest file does not exist");
        }

        serializerService.DeserializeFrom(moduleManifestJsonPath, out ModuleManifest manifest);

        return Result<ModuleManifest>.Success(manifest);
    }
}