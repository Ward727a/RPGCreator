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

using System.Runtime.CompilerServices;
using RPGCreator.SDK.Modules;
using RPGCreator.Shared.Types;

namespace RPGCreator.SDK.Services.ModuleServices;

public interface IModuleService : IService
{
    /// <summary>
    /// This will try to load a module from the given path.<br/>
    /// If the module is already loaded, it will do nothing and return true.<br/>
    /// If the module cannot be found or loaded, it will return false.<br/>
    /// <br/>
    /// <b>WARNING - PLEASE READ:</b> Loading a module can't be done from an external module, only from the engine or the editor itself for security reasons.<br/>
    /// This is to prevent malicious modules from loading other modules without engine controls.
    /// </summary>
    /// <param name="moduleDirectoryPath">The path to the module to load (directory where the plugin.meta.json is located).</param>
    /// <param name="token">The security token to authorize the operation.</param>
    /// <returns>
    /// Success if the module was loaded successfully or is already loaded; otherwise, failure with an error message.
    /// </returns>
    public Result TryLoadModuleFromDir(string moduleDirectoryPath, EngineSecurityToken token);
    
    /// <summary>
    /// This will try to load a module from the given path.<br/>
    /// If the module is already loaded, it will do nothing and return true.<br/>
    /// If the module cannot be found or loaded, it will return false.<br/>
    /// <br/>
    /// <b>WARNING - PLEASE READ:</b> Loading a module can't be done from an external module, only from the engine or the editor itself for security reasons.<br/>
    /// This is to prevent malicious modules from loading other modules without engine controls.
    /// </summary>
    /// <param name="dllPath">The path to the module .dll file to load.</param>
    /// <param name="token">The security token to authorize the operation.</param>
    /// <returns>
    /// Success if the module was loaded successfully or is already loaded; otherwise, failure with an error message.
    /// </returns>
    public Result TryLoadModuleFromDll(string dllPath, EngineSecurityToken token);

    /// <summary>
    /// Check if a directory is a valid module directory. Meaning that the directory contains a plugin.meta.json AND at least one .dll file.
    /// </summary>
    /// <param name="directory">The directory to check.</param>
    /// <returns>
    /// Success if the directory is a valid module directory; otherwise, failure with an error message.
    /// </returns>
    public Result IsValidModuleDir(string directory);
    
    /// <summary>
    /// Checks if a module is already loaded by its URN.
    /// </summary>
    /// <param name="moduleUrn">The URN of the module to check.</param>
    /// <returns>
    /// True if the module is loaded; otherwise, false.
    /// </returns>
    public bool IsModuleLoaded(URN moduleUrn);
    
    /// <summary>
    /// Gets a loaded module by its URN.<br/>
    /// Returns null if the module is not loaded.
    /// </summary>
    /// <param name="moduleUrn">The URN of the module to get.</param>
    /// <returns>
    /// The module if found; otherwise, null.
    /// </returns>
    public Result<Module> GetLoadedModule(URN moduleUrn);
    
    /// <summary>
    /// Gets all loaded modules.
    /// </summary>
    /// <returns>
    /// An enumerable of all loaded modules.
    /// </returns>
    public IEnumerable<Module> GetAllLoadedModules(EngineSecurityToken token);
    
    /// <summary>
    /// Unloads a loaded module by its URN.<br/>
    /// If the module is not loaded, it will do nothing.<br/>
    /// If the module is started, it will be stopped before unloading.
    /// </summary>
    /// <param name="moduleUrn">The URN of the module to unload.</param>
    public void UnloadModule(URN moduleUrn);
    
    /// <summary>
    /// Forces the unloading of a module by its URN.<br/>
    /// This method will unload the module even if it is started or has dependencies.<br/>
    /// Use with caution, as it may lead to instability if other modules depend on it.<br/>
    /// <br/>
    /// When I say 'with caution', I mean it, seriously. This method is dangerous and should only be used in extreme cases where you know what you're doing.<br/>
    /// It will not check for dependencies or if the module is started, and will try for 10 seconds to unload it properly before giving up, and just removing all references to it, and delete the context.<br/>
    /// This may lead to memory leaks, crashes, or other unexpected behavior if other parts of the engine are still using the module.<br/>
    /// Only use this method if you are absolutely sure that the module is no longer needed and that no other parts of the engine are using it.<br/>
    /// <br/>
    /// <b>WARNING - PLEASE READ:</b> Stopping a module can't be done from an external module, only from the engine or the editor itself for security reasons.<br/>
    /// This is to prevent malicious modules from stopping other modules without user permission.
    /// </summary>
    /// <param name="moduleUrn">The URN of the module to force unload.</param>
    /// <param name="token">The security token to authorize the operation.</param>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public void ForceUnloadModule(URN moduleUrn, EngineSecurityToken token);
}