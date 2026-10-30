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
using RPGCreator.SDK.Services.ModuleServices;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Types;

namespace RPGCreator.EngineLib.Services.Modules;

[EngineService]
public class ModuleDependencyResolver
{
    private readonly ScopedLogger _logger = Logger.ForContext<ModuleDependencyResolver>();
    private readonly IModuleRegistry _moduleRegistry;

    public ModuleDependencyResolver(IModuleRegistry moduleRegistry)
    {
        _moduleRegistry = moduleRegistry;
    }

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
        List<URN>? modulesToStart = null)
    {
        modulesToStart ??= _moduleRegistry.GetAllUrns().ToList();

        startOrder = [];
        var visiting = new HashSet<URN>();
        incompatibilities = [];

        foreach (var urn in modulesToStart)
        {
            VisitAndCheck(urn, startOrder, visiting, incompatibilities);
        }

        if (startOrder.Count > modulesToStart.Count)
        {
            _logger.Warning("Some modules were started due to dependencies but were not explicitly requested:");
            foreach (var urn in startOrder)
            {
                if (!modulesToStart.Contains(urn))
                {
                    _logger.Warning(" - {ModuleUrn}", args: urn);
                }
            }

            if (incompatibilities.Count > 0)
            {
                _logger.Warning("Incompatibilities detected with the following modules:");
                foreach (var urn in incompatibilities)
                {
                    _logger.Warning(" - {ModuleUrn}", args: urn);
                }
            }

            _logger.Warning("Need to wait for user confirmation to proceed...");
        }
    }

    private void VisitAndCheck(URN urn, HashSet<URN> startOrder, HashSet<URN> visiting,
        HashSet<URN> incompatibilities)
    {
        if (startOrder.Contains(urn)) return;
        if (visiting.Contains(urn)) throw new Exception($"Circle dependencies detected for {urn}");

        visiting.Add(urn);

        var candidateResult = _moduleRegistry.GetModule(urn);

        if (candidateResult.IsFailure)
        {
            _logger.Warning("Module {ModuleUrn} is not loaded, skipping dependency check.", args: urn);
            visiting.Remove(urn);
            return;
        }

        var candidate = candidateResult.Value;

        incompatibilities.UnionWith(candidate.Manifest.IncompatibleModules);
        foreach (var depUrn in candidate.Manifest.Dependencies)
        {
            if (!URN.TryParse(depUrn, out var resolvedUrn) || resolvedUrn == URN.Empty) continue;
            VisitAndCheck(new URN(depUrn), startOrder, visiting, incompatibilities);
        }

        visiting.Remove(urn);
        startOrder.Add(urn);
    }

}