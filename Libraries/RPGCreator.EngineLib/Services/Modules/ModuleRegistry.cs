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

using RPGCreator.SDK.Services.ModuleServices;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Types;

namespace RPGCreator.EngineLib.Services.Modules;

[EngineService]
[EngineService(typeof(IModuleRegistry))]
public class ModuleRegistry : IModuleRegistry
{
    private Dictionary<URN, SDK.Modules.Module> _modulesRegistered = new();
        
    public Result<SDK.Modules.Module> GetModule(URN moduleUrn)
    {
        if (!HasUrn(moduleUrn))
        {
            return Result<SDK.Modules.Module>.Fail($"Module '{moduleUrn}' not found.");
        }
        return _modulesRegistered[moduleUrn];
    }

    public IEnumerable<SDK.Modules.Module> GetAllModules() => _modulesRegistered.Values;

    public IEnumerable<URN> GetAllUrns() => _modulesRegistered.Keys;

    public Result RegisterModule(SDK.Modules.Module module, bool overwriteIfExists = false)
    {
        if (HasUrn(module.Manifest.ModuleUrn) && !overwriteIfExists)
            return Result.Fail($"Module '{module.Manifest.ModuleUrn}' is already registered.");
            
        SetModule(module);
        return Result.Success();
    }

    public Result UnregisterModule(URN moduleUrn)
    {
        return RemoveModule(moduleUrn) ? Result.Success() : Result.Fail($"Module '{moduleUrn}' not found.");
    }

    public bool HasUrn(URN moduleUrn) => _modulesRegistered.ContainsKey(moduleUrn);
    private void SetModule(SDK.Modules.Module module) => _modulesRegistered[module.Manifest.ModuleUrn] = module;
    private bool RemoveModule(URN moduleUrn) => _modulesRegistered.Remove(moduleUrn);
}