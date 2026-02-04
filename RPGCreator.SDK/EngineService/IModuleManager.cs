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
using RPGCreator.SDK.Types;

namespace RPGCreator.SDK.EngineService;

public interface IModuleManager : IService
{
    /// <summary>
    /// This will try to load a module from the given path.<br/>
    /// If the module is already loaded, it will do nothing and return true.<br/>
    /// If the module cannot be found or loaded, it will return false.<br/>
    /// <br/>
    /// <b>WARNING - PLEASE READ:</b> Loading a module can't be done from an external module, only from the engine or the editor itself for security reasons.<br/>
    /// This is to prevent malicious modules from loading other modules without engine controls.
    /// </summary>
    /// <param name="modulePath">The path to the module to load (the .dll file).</param>
    /// <param name="token">The security token to authorize the operation.</param>
    /// <returns>
    /// True if the module was loaded successfully or is already loaded; otherwise, false.
    /// </returns>
    public bool TryLoadModule(string modulePath, EngineSecurityToken token);
    
    /// <summary>
    /// Checks if a module is already loaded by its URN.
    /// </summary>
    /// <param name="moduleUrn">The URN of the module to check.</param>
    /// <returns>
    /// True if the module is loaded; otherwise, false.
    /// </returns>
    public bool IsModuleLoaded(URN moduleUrn);
    
    /// <summary>
    /// Checks if a module is started by its URN.
    /// </summary>
    /// <param name="moduleUrn">The URN of the module to check.</param>
    /// <returns>
    /// True if the module is started; otherwise, false.
    /// </returns>
    public bool IsModuleStarted(URN moduleUrn);
    
    /// <summary>
    /// Gets a loaded module by its URN.<br/>
    /// Returns null if the module is not loaded.
    /// </summary>
    /// <param name="moduleUrn">The URN of the module to get.</param>
    /// <returns>
    /// The module if found; otherwise, null.
    /// </returns>
    public ModuleCandidate? GetLoadedModule(URN moduleUrn);
    
    /// <summary>
    /// Gets all loaded modules.
    /// </summary>
    /// <returns>
    /// An enumerable of all loaded modules.
    /// </returns>
    public IEnumerable<ModuleCandidate> GetAllLoadedModules(EngineSecurityToken token);
    
    /// <summary>
    /// Gets all started modules.
    /// </summary>
    /// <returns>
    /// An enumerable of all started modules.
    /// </returns>
    public IEnumerable<IEngineModuleInfo> GetAllStartedModules();

    /// <summary>
    /// Clears all temporary module shadow copies created during module loading.<br/>
    /// This is useful to free up disk space after modules have been loaded.<br/>
    /// Note: This should be called only when no modules are being loaded or unloaded to avoid issues.<br/>
    /// This will do nothing if there are active module load/unload operations.
    /// </summary>
    public void ClearTempModulesShadowCopies(string path = "", EngineSecurityToken? token = null);


    /// <summary>
    /// Calculates the order in which modules should be started based on their dependencies and incompatibilities.<br/>
    /// Also identifies any incompatibilities that would prevent certain modules from being started together.<br/>
    /// <br/>
    /// The <paramref name="modulesToStart"/> parameter allows specifying a subset of modules to consider for starting.<br/>
    /// If null, all loaded modules will be considered.<br/>
    /// </summary>
    /// <param name="startOrder"></param>
    /// <param name="incompatibilities"></param>
    /// <param name="modulesToStart"></param>
    public void PlanStarting(out HashSet<URN> startOrder, out HashSet<URN> incompatibilities,
        List<URN>? modulesToStart = null);
    
    /// <summary>
    /// Starts a loaded module by its URN.<br/>
    /// If the module is not loaded, it will do nothing and return false.<br/>
    /// If the module is already started, it will do nothing and return true.<br/>
    /// <br/>
    /// <b>WARNING - PLEASE READ:</b> Starting a module can't be done from an external module, only from the engine or the editor itself for security reasons.<br/>
    /// This is to prevent malicious modules from starting other modules without user permission.
    /// </summary>
    /// <param name="moduleUrn">The URN of the module to start.</param>
    /// <param name="token">The security token to authorize the operation.</param>
    /// <returns>
    /// True if the module was started successfully or is already started; otherwise, false.
    /// </returns>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public bool StartModule(URN moduleUrn, EngineSecurityToken token);
    
    /// <summary>
    /// Stops a started module by its URN.<br/>
    /// If the module is not started, it will do nothing and return true.<br/>
    /// If the module is not loaded, it will do nothing and return true.<br/>
    /// <br/>
    /// <b>WARNING - PLEASE READ:</b> Stopping a module can't be done from an external module, only from the engine or the editor itself for security reasons.<br/>
    /// This is to prevent malicious modules from stopping other modules without user permission.
    /// </summary>
    /// <param name="moduleUrn">The URN of the module to stop.</param>
    /// <param name="token">The security token to authorize the operation.</param>
    /// <returns>
    /// True if the module was stopped, is stopped, or not loaded; otherwise, false.
    /// </returns>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public bool StopModule(URN moduleUrn, EngineSecurityToken token);
    
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