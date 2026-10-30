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

using System.Diagnostics;
using System.Reflection;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Mediator.Messages;
using RPGCreator.SDK.Modules;
using RPGCreator.SDK.Services.EngineService;
using RPGCreator.SDK.Services.ModuleServices;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Types;

namespace RPGCreator.EngineLib.Services.Modules;

[EngineService]
public class ModuleLifetimeService
{
    private readonly ScopedLogger _logger = Logger.ForContext<ModuleLifetimeService>();

    // Services
    private readonly IEngineConfig _engineConfig;
    private readonly IModuleRegistry _moduleRegistry;
    private readonly IMediator _mediator;
    private readonly IServiceProvider _serviceProvider;

    // Data
    private readonly Dictionary<URN, BaseModule> _startedModulesByUrn = new();
    private readonly List<Assembly> _moduleAssemblies = new();

    public ModuleLifetimeService(IModuleRegistry moduleRegistry, IEngineConfig engineConfig, IMediator mediator,
        IServiceProvider serviceProvider)
    {
        _moduleRegistry = moduleRegistry;
        _engineConfig = engineConfig;
        _mediator = mediator;
        _serviceProvider = serviceProvider;
        
        SetupGlobalExceptionHandling();
    }
    
    #region Error Handling
    
    private void SetupGlobalExceptionHandling()
    {
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var ex = (Exception)e.ExceptionObject;

        var assembly = ex.TargetSite?.DeclaringType?.Assembly;

        if (assembly != null && _moduleAssemblies.Contains(assembly))
        {
            Logger.Critical("Unhandled exception in module assembly '{Assembly}': {Exception}",
                args: [assembly.FullName, ex]);
        }
    }

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        foreach (var inner in e.Exception.InnerExceptions)
        {
            if (inner.TargetSite != null && inner.TargetSite.DeclaringType != null &&
                inner.TargetSite.DeclaringType == typeof(ModuleService))
            {
                _logger.Critical("Unobserved task exception in a module: {Exception}", args: inner);

                if (inner is UnauthorizedAccessException UAE)
                {
                    _logger.Critical("UnauthorizedAccessException detected: {Message}", args: UAE.Message);
                    _logger.Critical("This may indicate a security violation within the module!!!");
                }
            }
        }
    }
    
    #endregion

    public (bool isIncompatible, IEnumerable<URN> incompatibilitesFound) IsModuleIncompatible(URN moduleUrn)
    {
        var contextResult = _moduleRegistry.GetModule(moduleUrn);
        if (contextResult.IsFailure)
            return (false, []);
        var context = contextResult.Value;
        var list = _startedModulesByUrn.Where(kp => context.Manifest.IncompatibleModules.Contains(kp.Key))
            .Select(kp => kp.Key);
        var incompatibilitiesFound = list as URN[] ?? list.ToArray();
        return (incompatibilitiesFound.Length != 0, incompatibilitiesFound);
    }

    public IEnumerable<IEngineModuleInfo> GetAllStartedModules()
    {
        return _startedModulesByUrn.Values;
    }

    public bool IsModuleStarted(URN moduleUrn)
    {
        return _startedModulesByUrn.ContainsKey(moduleUrn);
    }

    public Result StartModule(URN moduleUrn, EngineSecurityToken token)
    {
        if (token == null)
        {
            StackTrace stackTrace = new StackTrace();
            var callingMethod = stackTrace.GetFrame(1)?.GetMethod();
            throw new UnauthorizedAccessException(
                $"TryLoadModule method can only be called by the engine. Unauthorized call from method: {callingMethod?.DeclaringType?.FullName}.{callingMethod?.Name} in assembly {callingMethod?.DeclaringType?.Assembly.FullName} estimed path: {callingMethod?.DeclaringType?.Assembly.Location}");
        }

        if (!_moduleRegistry.HasUrn(moduleUrn))
            return Result.Fail("Module is  not loaded");

        if (IsModuleStarted(moduleUrn))
            return Result.Success(); // Already started

        var (isIncompatible, incompatibilitesFound) = IsModuleIncompatible(moduleUrn);
        if (isIncompatible)
        {
            return Result.Fail(
                $"Module {moduleUrn} is incompatible with the following module: {string.Join(", ", incompatibilitesFound)}");
        }

        var moduleResult = _moduleRegistry.GetModule(moduleUrn);
        if (moduleResult.IsFailure)
            return Result.Fail("Module is not loaded");

        var moduleType = moduleResult.Value.ModuleType;
        if (moduleType == null)
            return Result.Fail("Failed to determine module type"); // Should not happen, but just in case
        BaseModule? module;

        try
        {
            module = (BaseModule?)ActivatorUtilities.CreateInstance(_serviceProvider, moduleType);
        }
        catch (Exception ex)
        {
            _logger.Error(
                "Failed to create instance of module type '{ModuleType}' for module '{ModuleUrn}'. Exception: {Exception}",
                args: [moduleType.FullName, moduleUrn, ex]);
            return Result.Fail("Failed to create module instance");
        }

        if (module == null)
            return Result.Fail("Failed to create module instance"); // Failed to create instance

        try
        {
            module.Initialize(token);
            _mediator.Publish(new ModuleMessage.ModuleStarted(moduleResult.Value));
            SetConfigStartContext(moduleResult.Value, true);
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to initialize module '{ModuleUrn}'. Exception: {Exception}",
                args: [moduleUrn, ex]);
            return Result.Fail("Failed to initialize module");
        }

        _startedModulesByUrn[moduleUrn] = module;
        _moduleAssemblies.Add(moduleType.Assembly);
        return Result.Success();
    }

    public bool StopModule(URN moduleUrn, EngineSecurityToken token)
    {
        if (token == null)
        {
            StackTrace stackTrace = new StackTrace();
            var callingMethod = stackTrace.GetFrame(1)?.GetMethod();
            throw new UnauthorizedAccessException(
                $"TryLoadModule method can only be called by the engine. Unauthorized call from method: {callingMethod?.DeclaringType?.FullName}.{callingMethod?.Name} in assembly {callingMethod?.DeclaringType?.Assembly.FullName} estimed path: {callingMethod?.DeclaringType?.Assembly.Location}");
        }

        if (!IsModuleStarted(moduleUrn))
            return true; // Already stopped

        var candidate = _moduleRegistry.GetModule(moduleUrn);
        var module = GetModuleInternal(moduleUrn, token);

        if (module == null || candidate.IsFailure)
            return true; // Not even loaded, consider it stopped

        SetConfigStartContext(candidate.Value, false);

        try
        {
            module.Shutdown(token);
            _mediator.Publish(new ModuleMessage.ModuleStopped(candidate.Value));
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to shutdown module '{ModuleUrn}'. Exception: {Exception}",
                args: [moduleUrn, ex]);
            return false;
        }

        _startedModulesByUrn.Remove(moduleUrn);
        _moduleAssemblies.Remove(module.GetType().Assembly);

        return true;
    }
    
    #region Internal

    private void SetConfigStartContext(SDK.Modules.Module candidate, bool isStarted)
    {
        var urn = candidate.Manifest.ModuleUrn;
        var hash = candidate.ModuleHash;
        var cert = candidate.Certified;

        var data = _engineConfig.Global.Modules.KnownList.GetValueOrDefault(urn, new ModuleStartContext());
        if (data.IsStarted != isStarted)
        {
            data.IsStarted = isStarted;
        }

        data.ModuleCertification = cert;
        data.ModuleHash = hash;
        data.ModuleUrn = urn;
        _engineConfig.SaveGlobalAsync();
    }
    
    private ModuleStartContext GetModuleStartContext(URN moduleUrn)
    {
        return _engineConfig.Global.Modules.KnownList.GetValueOrDefault(moduleUrn, new ModuleStartContext());
    }

    private BaseModule? GetModuleInternal(URN moduleUrn, EngineSecurityToken token)
    {
        return _startedModulesByUrn.GetValueOrDefault(moduleUrn);
    }
    
    #endregion

    public void RequestAutoStart(SDK.Modules.Module module, EngineSecurityToken token)
    {
        var manifest = module.Manifest;
        var urn = manifest.ModuleUrn;
        var startContext = GetModuleStartContext(urn);

        if (!startContext.IsStarted) return;
        
        if (startContext.ModuleUrn != manifest.ModuleUrn)
        {
            _logger.Error(
                "Couldn't automatically start module '{ModuleName}' v{ModuleVersion} by {ModuleAuthor} because it was started with a different URN than the current one.",
                args: [manifest.ModuleName, manifest.ModuleVersion, manifest.Authors]);
        }
        else if (startContext.ModuleCertification != module.Certified)
        {
            _logger.Error(
                "Couldn't automatically start module '{ModuleName}' v{ModuleVersion} by {ModuleAuthor} because it was started with a different certification than the current one.",
                args: [manifest.ModuleName, manifest.ModuleVersion, manifest.Authors]);
        }
        else if (startContext.ModuleHash != module.ModuleHash)
        {
            _logger.Error(
                "Couldn't automatically start module '{ModuleName}' v{ModuleVersion} by {ModuleAuthor} because it was started with a different hash than the current one.",
                args: [manifest.ModuleName, manifest.ModuleVersion, manifest.Authors]);
        }
        else
        {
            StartModule(urn, new EngineSecurityToken()).OnSuccess(() =>
            {
                _mediator.Publish(new ModuleMessage.ModuleAutoStarted(module));
            });
        }
    }
}