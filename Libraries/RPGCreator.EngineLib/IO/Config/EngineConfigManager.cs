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

using System.Text.Json;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using RPGCreator.SDK;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.IO.Config;
using RPGCreator.SDK.Mediator.Messages;
using RPGCreator.SDK.Services.EngineService;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Enums;

namespace RPGCreator.EngineLib.IO.Config;

[EngineService(typeof(IEngineConfig), ServiceLifetime.Singleton, EngineModeFlags.All)]
public class EngineConfigManager : IEngineConfig
{
    private readonly string _globalConfigPath = Path.Combine(RpgEnv.Path.ConfigFolder, "config.json");
    private string? _localConfigPath;
    
    private readonly IScheduler _scheduler;
    private readonly IMediator _mediator;
    
    private Ulid _autoSaveTaskId = Ulid.Empty;
    
    public EngineConfigData Global { get; private set; } = new();
    public LocalConfigData? Local { get; private set; }
    
    public bool IsGlobalDirty { get; set; }
    public bool IsLocalDirty { get; set; }

    public EngineConfigManager(IScheduler scheduler, IMediator mediator)
    {
        _scheduler = scheduler;
        _mediator = mediator;
    }

    public async Task LoadGlobalAsync()
    {
        if (!File.Exists(_globalConfigPath))
        {
            Global = new EngineConfigData();
            await SaveGlobalAsync();
            return;
        }

        try
        {
            var json = await File.ReadAllTextAsync(_globalConfigPath);
            Global = JsonSerializer.Deserialize<EngineConfigData>(json) ?? new();
            IsGlobalDirty = false;
        }
        catch (Exception e)
        {
            Global = new EngineConfigData();
            Logger.Error(e, "Failed to load global config, using default values.");
        }
    }

    public async Task SaveGlobalAsync()
    {
        var json = JsonSerializer.Serialize(Global, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_globalConfigPath, json);
        IsGlobalDirty = false;
    }

    public async Task LoadProjectAsync(string projectDirectory)
    {
        _localConfigPath = Path.Combine(projectDirectory, "project.config.json");

        if (!File.Exists(_localConfigPath))
        {
            Local = new LocalConfigData();
            await SaveProjectAsync();
        }
        else
        {
            try
            {
                var json = await File.ReadAllTextAsync(_localConfigPath);
                Local = JsonSerializer.Deserialize<LocalConfigData>(json) ?? new();
            }
            catch (Exception)
            {
                Local = new LocalConfigData();
            }
        }
        
        IsLocalDirty = false;
        StartAutoSaveLoop();
        
        await _mediator.Publish(new ConfigMessage.ConfigLoaded());
    }

    public async Task SaveProjectAsync()
    {
        if (Local == null || _localConfigPath == null) return;

        var json = JsonSerializer.Serialize(Local, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_localConfigPath, json);
        IsLocalDirty = false;
        
        await _mediator.Publish(new ConfigMessage.ConfigSaved());
    }

    public void UnloadProject()
    {
        if (_autoSaveTaskId != Ulid.Empty)
        {
            _scheduler.CancelTask(_autoSaveTaskId);
        }
        
        Local = null;
        _localConfigPath = null;
    }

    // --- AUTOSAVE ---

    private void StartAutoSaveLoop()
    {
        if (_autoSaveTaskId != Ulid.Empty) _scheduler.CancelTask(_autoSaveTaskId);
        if (Local == null) return;

        _autoSaveTaskId = _scheduler.WaitSecond(Convert.ToSingle(Local.AutoSave.Interval), async void () =>
        {
            try
            {
                if (!Local.AutoSave.Enabled) return;

                if (IsLocalDirty) await SaveProjectAsync();
                if (IsGlobalDirty) await SaveGlobalAsync();
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to autosave project config.");
            }
        }, true);
    }
    
    // --- PLUGIN API ---
    
    public async Task<T> GetPluginConfigAsync<T>(string pluginKey, T defaultValue, IEngineConfig.ConfigScope scope)
    {
        if (scope == IEngineConfig.ConfigScope.Local && Local != null)
        {
            if (Local.PluginData.TryGetValue(pluginKey, out var json))
                return json.Deserialize<T>() ?? defaultValue;
        }
        else if (scope == IEngineConfig.ConfigScope.Global)
        {
            if (Global.PluginData.TryGetValue(pluginKey, out var json))
                return json.Deserialize<T>() ?? defaultValue;
        }

        return defaultValue;
    }

    public async Task SetPluginConfigAsync<T>(string pluginKey, T pluginConfig, IEngineConfig.ConfigScope scope)
    {
        if (scope == IEngineConfig.ConfigScope.Local && Local != null)
        {
            Local.PluginData[pluginKey] = JsonSerializer.SerializeToElement(pluginConfig);
            IsLocalDirty = true;
        }
        else if (scope == IEngineConfig.ConfigScope.Global)
        {
            Global.PluginData[pluginKey] = JsonSerializer.SerializeToElement(pluginConfig);
            IsGlobalDirty = true;
        }

        await _mediator.Publish(new ConfigMessage.PluginConfigSaved(pluginKey));
    }
}