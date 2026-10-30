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

using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using RPGCreator.SDK.IO.Config;
using RPGCreator.SDK.Modules.Definition;
using RPGCreator.SDK.Serializer;
using RPGCreator.Shared.Types;

namespace RPGCreator.SDK.Services.EngineService;

public interface IConfig
{
    public event Action<string>? KeyChanged;
    event Action? ConfigSaved;
    event Action? ConfigLoaded;
    event Action? ConfigChanged;
    [JsonIgnore]
    public bool IsDirty { get; }    
    [JsonIgnore]
    public string ConfigPath { get; set; }

    public CustomData GetDefaultConfig();

    public Version? GetVersion(string key, Version? defaultValue = null);
    public string GetString(string key, string defaultValue = "");
    public int GetInt(string key, int defaultValue = 0);
    public bool GetBool(string key, bool defaultValue = false);
    public double GetDouble(string key, double defaultValue = 0.0);
    public T Get<T>(string key, T defaultValue);

    public IConfig SetVersion(string key, Version value);   
    public IConfig SetString(string key, string value);
    public IConfig SetInt(string key, int value);
    public IConfig SetBool(string key, bool value);
    public IConfig SetDouble(string key, double value);
    public IConfig Set<T>(string key, T value);
    
    public bool HasVersion(string key);  
    public bool HasString(string key);
    public bool HasInt(string key);
    public bool HasBool(string key);
    public bool HasDouble(string key);
    public bool Has<T>(string key);
    
    public bool SaveConfig();
    public bool SaveConfigAt(string path);

    public void OnLoadedConfig();
    public void OnSavedConfig();
}

public interface IEngineConfig
{
    public enum ConfigScope { Global, Local }
    
    public EngineConfigData Global { get; }
    public LocalConfigData? Local { get; }
    
    
    Task LoadGlobalAsync();
    Task SaveGlobalAsync();
    Task LoadProjectAsync(string projectDirectory);
    Task SaveProjectAsync();
    void UnloadProject();
    Task<T> GetPluginConfigAsync<T>(string pluginKey, T defaultValue, ConfigScope scope);
    Task SetPluginConfigAsync<T>(string pluginKey, T pluginConfig, ConfigScope scope);
    
}