#region LICENSE

//
// RPG Creator - Open-source RPG Engine.
// (c) 2025 Ward
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
// 
// 

#endregion

using System.Diagnostics;
using Fluxor;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Mediator.Messages;
using RPGCreator.SDK.Modules;
using RPGCreator.SDK.Serializer;
using RPGCreator.SDK.Services.EngineService;
using RPGCreator.SDK.Services.ModuleServices;
using RPGCreator.SDK.States;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Enums;
using RPGCreator.Shared.Types;

// ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract

namespace RPGCreator.EngineLib.Services.Modules
{
    [EngineService(typeof(IModuleService))]
    public class ModuleService : IModuleService
    {
        private readonly IState<EngineState> _engineState;
        private readonly IMediator _mediator;
        
        private readonly IEngineConfig _engineConfig;
        private readonly ISerializerService _serializerService;
        
        private readonly IModuleRegistry _moduleRegistry;
        private readonly ModuleShadowCopyManager _moduleShadowCopyManager;
        private readonly ModuleCertificationService _moduleCertificationService;
        private readonly ModuleLifetimeService _moduleLifetimeService;
        
        private readonly ScopedLogger _logger = Logger.ForContext<ModuleService>();

        internal ModuleService(IState<EngineState> engineState, IEngineConfig engineConfig,
            ISerializerService serializerService, IMediator mediator,
            IModuleRegistry moduleRegistry, ModuleShadowCopyManager moduleShadowCopyManager,
            ModuleCertificationService moduleCertificationService,
            ModuleLifetimeService moduleLifetimeService)
        {
            _engineState = engineState;
            _engineConfig = engineConfig;

            _serializerService = serializerService;
            _mediator = mediator;

            _moduleRegistry = moduleRegistry;
            _moduleShadowCopyManager = moduleShadowCopyManager;

            _moduleCertificationService = moduleCertificationService;
            
            _moduleLifetimeService = moduleLifetimeService;
        }

        public bool IsModuleLoaded(URN moduleUrn) => _moduleRegistry.HasUrn(moduleUrn);

        #region Getters

        public Result<SDK.Modules.Module> GetLoadedModule(URN moduleUrn) => _moduleRegistry.GetModule(moduleUrn);

        public IEnumerable<SDK.Modules.Module> GetAllLoadedModules(EngineSecurityToken token) => _moduleRegistry.GetAllModules();

        #endregion
        
        public void Initialize()
        {
            _logger.Info("Initializing ModulesManager.");
            if (_engineState.Value.Mode == EngineModeFlags.Player)
            {
                _logger.Debug("Player mode detected, skipping module loading.");
                _logger.Info("ModulesManager initialized.");
                return;
            }

            PrepareModuleDirectories();
            LoadAllModules();
        }


        private void PrepareModuleDirectories()
        {
            string modulesPath = _engineConfig.Global.Paths.ModulesPath;
            if (!Directory.Exists(modulesPath))
            {
                _logger.Info("No modules directory found. Creating one at '{ModulesPath}'.", args: modulesPath);
                Directory.CreateDirectory(modulesPath);
            }

            _moduleShadowCopyManager.ClearShadows(new EngineSecurityToken());
        }

        private void LoadAllModules()
        {
            string modulesPath = _engineConfig.Global.Paths.ModulesPath;

            var securityToken = new EngineSecurityToken();

            foreach (var moduleDirectory in Directory.GetDirectories(modulesPath))
            {
                IsValidModuleDir(moduleDirectory)
                    .OnSuccess(() => TryLoadModuleFromDir(moduleDirectory, securityToken)
                        .OnSuccess(() => _logger.Info($"Found and loaded module in directory {moduleDirectory}"))
                        .OnFailure((ex) =>
                            _logger.Error(
                                $"Found module in directory {moduleDirectory} but couldn't load it due to error: {ex}")))
                    .OnFailure((ex) =>
                        _logger.Error($"Error when trying to load module in directory {moduleDirectory}. Error: {ex}"));
            }
        }

        public Result IsValidModuleDir(string directory)
        {
            if (!Directory.Exists(directory))
                return Result.Fail($"Directory not found: {directory}");

            if (!File.Exists(Path.Combine(directory, "plugin.meta.json")))
                return Result.Fail($"Directory {directory} doesn't contain a plugin.meta.json file");

            if (!Directory.EnumerateFiles(directory, "*.dll").Any())
                return Result.Fail($"Directory {directory} doesn't contain any .dll files");

            return Result.Success();
        }

        public Result TryLoadModuleFromDir(string moduleDirectoryPath, EngineSecurityToken token)
        {
            if (token == null)
            {
                StackTrace stackTrace = new StackTrace();
                var callingMethod = stackTrace.GetFrame(1)?.GetMethod();
                throw new UnauthorizedAccessException(
                    $"TryLoadModuleFromDir method can only be called by the engine. Unauthorized call from method: {callingMethod?.DeclaringType?.FullName}.{callingMethod?.Name} in assembly {callingMethod?.DeclaringType?.Assembly.FullName} estimed path: {callingMethod?.DeclaringType?.Assembly.Location}");
            }

            _logger.Debug("Trying to load module from directory '{ModuleDirectoryPath}'", args: moduleDirectoryPath);

            if (!Directory.Exists(moduleDirectoryPath))
            {
                return Result.Failure("Module directory does not exist");
            }

            var manifest = GetManifest(out var loadResult);

            if (loadResult.IsFailure)
            {
                return loadResult;
            }

            _logger.Debug("Got manifest: {moduleDirectory}/plugin.meta.json", args: moduleDirectoryPath);

            return TryLoadModuleFromManifest(manifest, moduleDirectoryPath, token);

            ModuleManifest GetManifest(out Result result)
            {
                result = Result.Fail();

                const string manifestFilename = "plugin.meta.json";
                var manifestPath = Path.Combine(moduleDirectoryPath, manifestFilename);
                if (!File.Exists(manifestPath))
                {
                    result = Result.Failure("Module directory does not contain a plugin.meta.json");
                    return default;
                }

                var manifestResult = ModuleManifest.CreateManifest(manifestPath, _serializerService);

                if (manifestResult.IsFailure)
                {
                    result = Result.Failure(manifestResult.Error);
                    return default;
                }

                return manifestResult.Value;
            }
        }

        public Result TryLoadModuleFromDll(string dllPath, EngineSecurityToken token)
        {
            if (token == null)
            {
                StackTrace stackTrace = new StackTrace();
                var callingMethod = stackTrace.GetFrame(1)?.GetMethod();
                throw new UnauthorizedAccessException(
                    $"TryLoadModuleFromDir method can only be called by the engine. Unauthorized call from method: {callingMethod?.DeclaringType?.FullName}.{callingMethod?.Name} in assembly {callingMethod?.DeclaringType?.Assembly.FullName} estimed path: {callingMethod?.DeclaringType?.Assembly.Location}");
            }

            if (!File.Exists(dllPath))
            {
                return Result.Failure($"DLL file at path '{dllPath}' does not exist");
            }

            var moduleDirectoryPath = Path.GetDirectoryName(dllPath);

            if (moduleDirectoryPath == null)
            {
                return Result.Failure("Invalid DLL path");
            }

            if (!Directory.Exists(moduleDirectoryPath))
            {
                return Result.Failure("Module directory does not exist");
            }

            var manifest = GetManifest(out var loadResult);

            if (loadResult.IsFailure)
            {
                return loadResult;
            }

            if (!manifest.EntryDll.Equals(Path.GetFileName(dllPath)))
            {
                return Result.Fail("The provided DLL does not match the entry DLL specified in the module manifest.");
            }

            _logger.Debug("Got manifest: {moduleDirectory}/plugin.meta.json", args: moduleDirectoryPath);

            return TryLoadModuleFromManifest(manifest, moduleDirectoryPath, token);


            ModuleManifest GetManifest(out Result result)
            {
                result = Result.Fail();

                const string manifestFilename = "plugin.meta.json";
                var manifestPath = Path.Combine(moduleDirectoryPath, manifestFilename);
                if (!File.Exists(manifestPath))
                {
                    result = Result.Failure("Module directory does not contain a plugin.meta.json");
                    return default;
                }

                var manifestResult = ModuleManifest.CreateManifest(manifestPath, _serializerService);

                if (manifestResult.IsFailure)
                {
                    result = Result.Failure(manifestResult.Error);
                    return default;
                }

                return manifestResult.Value;
            }
        }

        private Result TryLoadModuleFromManifest(ModuleManifest manifest, string directory, EngineSecurityToken token)
        {
            if (token == null)
            {
                StackTrace stackTrace = new StackTrace();
                var callingMethod = stackTrace.GetFrame(1)?.GetMethod();
                throw new UnauthorizedAccessException(
                    $"TryLoadModuleFromManifest method can only be called by the engine. Unauthorized call from method: {callingMethod?.DeclaringType?.FullName}.{callingMethod?.Name} in assembly {callingMethod?.DeclaringType?.Assembly.FullName} estimed path: {callingMethod?.DeclaringType?.Assembly.Location}");
            }

            _logger.Debug("Trying to load module from manifest '{directory}/plugin.meta.json'", args: directory);

            if (!manifest.EntryDll.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
            {
                return Result.Failure("Module manifest entry_dll must be a .dll file");
            }

            var entryDllPath = Path.Combine(directory, manifest.EntryDll);

            if (!File.Exists(entryDllPath))
            {
                return Result.Failure(
                    $"Module directory does not contain the entry dll referenced in the manifest (no {manifest.EntryDll} file found in {directory})");
            }

            _logger.Debug("Got entry_dll '{directory}/{manifest.EntryDll}'", args: [directory, entryDllPath]);

            var certifiedData = _moduleCertificationService.CheckCertification(entryDllPath);

            if (certifiedData.cert == SDK.Modules.Module.CertificationState.Certified)
            {
                _logger.Info("Module '{directory}/{manifest.EntryDll}' is certified.", args: [directory, entryDllPath]);
            }
            else
            {
                _logger.Warning("Module '{directory}/{manifest.EntryDll}' is not certified.",
                    args: [directory, entryDllPath]);
            }

            var shadowResult = _moduleShadowCopyManager.CreateShadow(entryDllPath);

            if (shadowResult.IsFailure)
            {
                _logger.Error(
                    "Failed to create shadow copy for module '{directory}/{manifest.EntryDll}': {Exception}",
                    args: [directory, manifest.EntryDll, shadowResult.Error]);
                return Result.Fail(shadowResult.Error);
            }

            var shadow = shadowResult.Value;

            _logger.Debug("Created shadow copy for module '{directory}/{manifest.EntryDll}' at '{copy}'",
                args: [directory, manifest.EntryDll, shadow]);

            var context = new ModuleContext(shadow);
            var assembly = context.LoadFromAssemblyPath(shadow);

            var moduleType = assembly.GetLoadableTypes().FirstOrDefault(t =>
                typeof(BaseModule).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            if (moduleType == null)
            {
                context.Unload();
                _logger.Debug("No valid module type found in assembly '{shadow}'. Unloading module.", args: shadow);
                return Result.Failure("No valid module type found in assembly");
            }

            _contexts[manifest.ModuleUrn] = context;

            var module =
                new SDK.Modules.Module(manifest, moduleType, shadow, certifiedData.fileHash, certifiedData.cert);
            _moduleRegistry.RegisterModule(module);

            _logger.Info(
                "Module '{ModuleName}'(path: {entryDllPath}) version {moduleVersion} by @{moduleAuthors} loaded successfully.",
                args: [manifest.ModuleName, entryDllPath, manifest.ModuleVersion, manifest.Authors]);

            _mediator.Publish(new ModuleMessage.ModuleLoaded(module));

            _moduleLifetimeService.RequestAutoStart(module, token);

            return Result.Success();
        }


        private readonly Dictionary<URN, ModuleContext> _contexts = new();

        public void UnloadModule(URN moduleUrn)
        {
            if (_moduleLifetimeService.IsModuleStarted(moduleUrn))
                return;

            var moduleResult = GetLoadedModule(moduleUrn);

            if (moduleResult.IsFailure || !_contexts.ContainsKey(moduleUrn))
                return;

            var module = moduleResult.Value;
            _moduleRegistry.UnregisterModule(module);
            var context = _contexts.GetValueOrDefault(moduleUrn);
            _contexts.Remove(moduleUrn);

            context?.Unload();

            _mediator.Publish(new ModuleMessage.ModuleUnloaded(module));
        }

        public void ForceUnloadModule(URN moduleUrn, EngineSecurityToken token)
        {
            if (token == null)
            {
                StackTrace stackTrace = new StackTrace();
                var callingMethod = stackTrace.GetFrame(1)?.GetMethod();
                throw new UnauthorizedAccessException(
                    $"TryLoadModule method can only be called by the engine. Unauthorized call from method: {callingMethod?.DeclaringType?.FullName}.{callingMethod?.Name} in assembly {callingMethod?.DeclaringType?.Assembly.FullName} estimed path: {callingMethod?.DeclaringType?.Assembly.Location}");
            }


            var moduleResult = GetLoadedModule(moduleUrn);
            if (moduleResult.IsFailure)
                return;
            var module = moduleResult.Value;

            _moduleLifetimeService.StopModule(moduleUrn, token);

            _moduleRegistry.UnregisterModule(module);

            var context = _contexts.GetValueOrDefault(moduleUrn);
            _contexts.Remove(moduleUrn);

            context?.Unload();

            _mediator.Publish(new ModuleMessage.ModuleUnloaded(module));
            _mediator.Publish(new ModuleMessage.ModuleForceUnloaded(module));

            _logger.Critical(
                "Forcefully unloaded module {ModuleUrn}. This may lead to instability if the module was in use.",
                args: moduleUrn);
        }
    }
}