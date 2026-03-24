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

using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Modules.Definition;
using RPGCreator.SDK.Serializer;

namespace RPGCreator.SDK.EngineService;

[SerializingType("BaseConfig")]
public class BaseConfig : IConfig
{
    public event Action<string>? KeyChanged;
    public event Action? ConfigSaved;
    public event Action? ConfigLoaded;
    public event Action? ConfigChanged;
    
    public bool IsDirty { get; protected set; } = false;
    
    private static readonly ScopedLogger Logger = SDK.Logging.Logger.ForContext<BaseConfig>();

    public virtual string ConfigPath
    {
        get => field;
        set { field = value; Logger.Debug("Config path set to: {Path}", args: field ?? "null"); }
    } = string.Empty;

    protected CustomData Data = new();

    public virtual CustomData GetDefaultConfig()
    {
        return new CustomData();
    }

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
        return Data.GetAsOrDefault(key, defaultValue);
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
        Data.Set(key, value);
        IsDirty = true;
        ConfigChanged?.Invoke();
        KeyChanged?.Invoke(key);
    }

    public bool HasString(string key)
    {
        return Data.Has(key);
    }

    public bool HasInt(string key)
    {
        return Data.Has(key);
    }

    public bool HasBool(string key)
    {
        return Data.Has(key);
    }

    public bool HasFloat(string key)
    {
        return Data.Has(key);
    }

    public bool HasDouble(string key)
    {
        return Data.Has(key);
    }

    public bool Has<T>(string key)
    {
        return Data.Has(key);
    }


    public bool SaveConfig()
    {
        return SaveConfigAt(ConfigPath);
    }

    public bool SaveConfigAt(string path)
    {
        OnSavedConfig();
        EngineServices.Serializer.Serialize(this, out var stringData);
        
        if (string.IsNullOrEmpty(stringData))
            return false;

        try
        {
            File.WriteAllText(path, stringData);
            IsDirty = false;
            return true;
        } catch (Exception ex)
        {
            Logger.Error(ex, "Failed to save config to path: {Path}", args: path);
            return false;
        }
    }

    public SerializationInfo GetObjectData()
    {
        var info = new SerializationInfo(typeof(BaseConfig));
        info.AddValue("settings", Data);
        return info;
    }

    public List<Ulid> GetReferencedAssetIds()
    {
        return [];
    }

    public void SetObjectData(DeserializationInfo info)
    {
        info.TryGetValue("settings", out CustomData? savedData);
        
        Data = savedData ?? new CustomData();
    }

    public void OnLoadedConfig()
    {
        _OnLoadedConfig();
        ConfigLoaded?.Invoke();
    }

    public void OnSavedConfig()
    {
        _OnSavedConfig();
        ConfigSaved?.Invoke();
    }
    
    protected virtual void _OnLoadedConfig(){}
    protected virtual void _OnSavedConfig(){}
}