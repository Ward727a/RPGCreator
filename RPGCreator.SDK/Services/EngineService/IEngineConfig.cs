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
using RPGCreator.SDK.Modules.Definition;
using RPGCreator.SDK.Serializer;
using RPGCreator.SDK.Types;
using RPGCreator.Shared.Types;

namespace RPGCreator.SDK.Services.EngineService;

public interface IConfig : ISerializable, IDeserializable
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

public interface IEngineConfig : IConfig, IService, ISerializable, IDeserializable
{
    event Action? AutoSaveStarted;

    public ObservableCollection<URN> Shortcuts { get; }
    public ObservableCollection<URN> ToolsShortcuts { get; }

    
    /// <summary>
    /// Try to open a config from its name.
    /// </summary>
    /// <param name="configName">The name of the config to open.</param>
    /// <param name="isGlobal">If the config is a global config (i.e. located in the app config folder) or a project config (i.e. located in the current project config folder).</param>
    /// <param name="config">The config if found, null otherwise.</param>
    /// <returns>
    /// True if the config was found, false otherwise.
    /// </returns>
    /// <remarks>
    /// For example, check <see cref="From"/>
    /// </remarks>
    public bool TryFrom(string configName, bool isGlobal, [NotNullWhen(true)] out IConfig? config);
    
    /// <summary>
    /// Try to open a config from its name.
    /// </summary>
    /// <param name="configName">The name of the config to open.</param>   
    /// <param name="isGlobal">If the config is a global config (i.e. located in the app config folder) or a project config (i.e. located in the current project config folder).</param>  
    /// <param name="config">The config if found, null otherwise.</param> 
    /// <typeparam name="T">The type of the config to open.</typeparam>
    /// <returns>
    /// True if the config was found, false otherwise.
    /// </returns>
    /// <remarks>
    /// For example, check <see cref="From"/>
    /// </remarks>
    public bool TryFrom<T>(string configName, bool isGlobal, [NotNullWhen(true)] out T? config) where T : class, IConfig;
    
    /// <summary>
    /// Allow opening another config from its name.<br/>
    /// E.g. EngineConfig.From("MyConfig") will look for a config named "MyConfig" in the project config folder.<br/>
    /// E.g. EngineConfig.From("MyConfig", true) will look for a config named "MyConfig" in the global config folder of the app.
    /// </summary>
    /// <param name="configName">The name of the config to open.</param>
    /// <param name="isGlobal">Whether the config is a global config (i.e. located in the app config folder) or a project config (i.e. located in the current project config folder).</param>
    /// <returns>
    /// Return the config if found, null otherwise.
    /// </returns>
    public IConfig? From(string configName, bool isGlobal = false);
    
    /// <summary>
    /// Allow creating a new config from config data.<br/>
    /// E.g. EngineConfig.CreateConfig("MyConfig", new MyConfig()) will create a new config named "MyConfig" with the data of MyConfig.<br/>
    /// E.g. EngineConfig.CreateConfig("MyConfig", new MyConfig(), true) will create a new global config named "MyConfig" with the data of MyConfig.
    /// </summary>
    /// <param name="configName">The name of the config to create.</param>
    /// <param name="configData">The data of the config to create. This data will be serialized and saved in the config file.</param>
    /// <param name="isGlobal">Whether the config is a global config (i.e. located in the app config folder) or a project config (i.e. located in the current project config folder).</param>
    /// <returns>
    /// Return true if the config was created successfully, false otherwise.
    /// </returns>
    public bool CreateConfig(string configName, IConfig configData, bool isGlobal = false);
    
    public bool LoadConfig();
    public bool LoadConfigFrom(string path);
}