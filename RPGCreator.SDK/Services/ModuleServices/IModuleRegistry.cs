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

using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Modules;
using RPGCreator.Shared.Types;

namespace RPGCreator.SDK.Services.ModuleServices;

public interface IModuleRegistry
{
    public Result<Module> GetModule(URN moduleUrn);

    public IEnumerable<Module> GetModules(IEnumerable<URN> moduleUrns)
    {
        List<Module> modules = [];
        foreach (var moduleUrn in moduleUrns)
        {
            GetModule(moduleUrn)
                .OnFailure((ex) =>
                {
                    Logger.Error($"Error when trying to get module '{moduleUrn}': {ex}");
                })
                .OnSuccess(modules.Add);
        }
        return modules;
    }
    public IEnumerable<Module> GetAllModules();
    public IEnumerable<URN> GetAllUrns();
    
    public Result RegisterModule(Module module, bool overwriteIfExists = false);
    public void RegisterModules(IEnumerable<Module> modules)
    {
        foreach (var module in modules)
        {
            RegisterModule(module);
        }
    }

    public Result UnregisterModule(URN moduleUrn);
    public void UnregisterModule(Module module) => UnregisterModule(module.Manifest.ModuleUrn);
    public void UnregisterModules(IEnumerable<Module> modules)
    {
        foreach (var module in modules)
        {
            UnregisterModule(module);
        }
    }

    public bool HasUrn(URN moduleUrn);
    public bool HasModule(Module module) => HasUrn(module.Manifest.ModuleUrn);
    
}