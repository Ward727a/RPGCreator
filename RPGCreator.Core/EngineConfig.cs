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
using RPGCreator.SDK;
using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.Editor;
using RPGCreator.SDK.EngineService;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Modules.Definition;
using RPGCreator.SDK.Serializer;
using RPGCreator.SDK.Types;

namespace RPGCreator.Core;

[SerializingType("EngineConfig")]
public class EngineConfig : IEngineConfig
{
    public event Action? OnConfigChanged;
    public event Action? OnShortcutsChanged;
    public event Action? OnToolsShortcutsChanged;
    
    public bool IsDirty { get; private set; } = false;
    
    private static readonly ScopedLogger Logger = SDK.Logging.Logger.ForContext<EngineConfig>();
    private readonly string _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");

    public ObservableCollection<URN> Shortcuts { get; private set; } = [];
    public ObservableCollection<URN> ToolsShortcuts { get; private set; } = [];
    
    private CustomData _data = new();

    public string GetString(string key, string defaultValue = "")
    {
        return Get(key, defaultValue);
    }

    public int GetInt(string key, int defaultValue = 0)
    {
        return Get(key, defaultValue);
    }

    public bool GetBool(string key, bool defaultValue = false)
    {
        return Get(key, defaultValue);
    }

    public float GetFloat(string key, float defaultValue = 0)
    {
        return Get(key, defaultValue);
    }

    public double GetDouble(string key, double defaultValue = 0)
    {
        return Get(key, defaultValue);
    }

    public T Get<T>(string key, T defaultValue)
    {
        return _data.GetAsOrDefault(key, defaultValue);
    }

    public void SetString(string key, string value)
    {
        Set(key, value);
    }

    public void SetInt(string key, int value)
    {
        Set(key, value);
    }

    public void SetBool(string key, bool value)
    {
        Set(key, value);
    }

    public void SetFloat(string key, float value)
    {
        Set(key, value);
    }

    public void SetDouble(string key, double value)
    {
        Set(key, value);
    }

    public void Set<T>(string key, T value)
    {
        _data.Set(key, value);
        IsDirty = true;
        OnConfigChanged?.Invoke();
    }

    public bool SaveConfig()
    {
        return SaveConfigAt(_configPath);
    }

    public bool SaveConfigAt(string path)
    {
        EngineServices.SerializerService.Serialize(this, out var stringData);
        
        if (string.IsNullOrEmpty(stringData))
            return false;

        try
        {
            File.WriteAllText(path, stringData);
            return true;
        } catch (Exception ex)
        {
            Logger.Error(ex, "Failed to save engine config to path: {Path}", args: path);
            return false;
        }
    }

    public bool LoadConfig()
    {
        return LoadConfigFrom(_configPath);
    }

    public bool LoadConfigFrom(string path)
    {
        Logger.Info("Attempting to load engine config from path: {Path}", args: path);
        if (!File.Exists(path))
        {
            Logger.Warning("Config file not found at path: {Path}", args: path);
            return false;
        }

        try
        {
            var stringData = File.ReadAllText(path);
            EngineServices.SerializerService.Deserialize<EngineConfig>(stringData, out var config);
            // ReSharper disable once ConvertTypeCheckToNullCheck
            if (config == null || config is not EngineConfig)
            {
                Logger.Error("Failed to deserialize engine config from file: {Path}", args: path);
                return false;
            }

            Logger.Debug("Successfully deserialized engine config from path: {Path}", args: path);
            Logger.Debug("Checking data integrity for loaded config...");

            if (!config.CheckDataIntegrity(out _))
            {
                Logger.Error("Data integrity check failed for loaded config from path: {Path}", args: path);
                Logger.Error("Attempting to fix data integrity issues...");
                if (!config.TryFixDataIntegrity(out var checksResults))
                {
                    Logger.Error(
                        "Data integrity check has failed, and automatic fixing did not work for loaded config from path: {Path}",
                        args: path);
                    Logger.Error("Data integrity check results: {Results} (1: Key Integrity, 2: Data Type Integrity)",
                        args: checksResults);

                    return false;
                }
            }

            _data = config._data;
            Shortcuts = config.Shortcuts;
            ToolsShortcuts = config.ToolsShortcuts;

            Logger.Debug("Successfully loaded engine config from path: {Path}", args: path);
            Logger.Debug("Loaded config data: {@Data}", args: _data);

            OnConfigChanged?.Invoke();

            return true;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to load engine config from path: {Path}", args: path);
            return false;
        }
        finally
        {
            Logger.Info("Finished loading engine config from path: {Path}", args: path);
        }
    }

    private bool CheckDataIntegrity(out byte checksResults, CustomData? data = null)
    {
        checksResults = 0;
        data ??= _data;

        var keyIntrity = data.Has("shortcuts") && data.Has("toolsShortcuts");
        checksResults |= (byte)(keyIntrity ? 1 : 0);
        
        var dataIntegrity = data.GetTypeOf("shortcuts") == typeof(List<ShortcutButtonInfo>) &&
                           data.GetTypeOf("toolsShortcuts") == typeof(List<IToolButtonInfo>);
        checksResults |= (byte)(dataIntegrity ? 2 : 0);
        
        return keyIntrity && dataIntegrity;
    }
    
    private bool TryFixDataIntegrity(out byte checksResults)
    {
        if (CheckDataIntegrity(out checksResults))
            return true;

        if (!_data.Has("shortcuts"))
            _data.Set("shortcuts", new List<ShortcutButtonInfo>());

        if (!_data.Has("toolsShortcuts"))
            _data.Set("toolsShortcuts", new List<IToolButtonInfo>());

        return CheckDataIntegrity(out checksResults);
    }

    public SerializationInfo GetObjectData()
    {
        var info = new SerializationInfo(typeof(EngineConfig));
        info.AddValue("settings", _data);
        return info;
    }

    public List<Ulid> GetReferencedAssetIds()
    {
        return [];
    }

    public void SetObjectData(DeserializationInfo info)
    {
        info.TryGetValue("settings", out CustomData? savedData);
        
        _data = savedData ?? new CustomData();
    }

    /// <summary>
    /// This method allows the engine to attempt to load the config from disk.<br/>
    /// If it is missing, then it will create a new config with default values and save it to disk, so it can be loaded next time.<br/>
    /// This is used for first-time engine launch, or when the config file is deleted.<br/>
    /// Note: This does nothing directly against corrupted files, but use the <see cref="LoadConfig"/> and this one try to fix the corrupted or invalid config.
    /// </summary>
    internal void CreateOrLoadConfig()
    {
        if (File.Exists(_configPath))
        {
            LoadConfig();
            return;
        }

        this.SaveConfig();
    }
}