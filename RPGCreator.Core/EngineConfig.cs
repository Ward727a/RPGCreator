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
using RPGCreator.SDK.EngineService;
using RPGCreator.SDK.Modules.Definition;
using RPGCreator.SDK.Serializer;
using RPGCreator.SDK.Types;
using RPGCreator.UI.Content.Editor;

namespace RPGCreator.Core;

[SerializingType("EngineConfig")]
public class EngineConfig : IEngineConfig
{
    
    public string ConfigPath => GetString("configPath", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json"));
    public List<URN> Shortcuts  => Get("shortcuts", new List<URN>());
    public List<URN> ToolsShortcuts => Get("toolsShortcuts", new List<URN>());
    
    public EngineConfig()
    {
        _data.Set("configPath", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json"));
    }
    
    private CustomData? _data = new CustomData();

    public string GetString(string key, string defaultValue = "")
    {
        return Get(key, defaultValue)!;
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
        return _data!.GetAsOrDefault(key, defaultValue);
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
        _data?.Set(key, value);
    }
    

    public SerializationInfo GetObjectData()
    {
        var info = new SerializationInfo(typeof(EngineConfig));
        info.AddValue("data", _data);
        return info;
    }

    public List<Ulid> GetReferencedAssetIds()
    {
        return [];
    }

    public void SetObjectData(DeserializationInfo info)
    {
        info.TryGetValue("data", out _data);

        _data ??= new CustomData();
    }
}