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
using System.Text.Json.Serialization;
using RPGCreator.SDK;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Modules.Editor.Ui;
using RPGCreator.SDK.Serializer;
using RPGCreator.Shared.Attributes;
using RPGCreator.Shared.Types;

namespace RPGCreator.EngineLib.Services.Modules.Features.UiFeatureProvider;

[EngineService(typeof(IEditorUiStore))]
public sealed class EditorUiStore : IEditorUiStore
{
    private readonly ScopedLogger _logger = Logger.ForContext<EditorUiStore>();

    [JsonConstructor]
    public EditorUiStore()
    {
    }

    [JsonInclude]
    private Dictionary<URN, EditorUiStoreContext> Contexts { get; set; } = [];
    private readonly ISerializerService _serializerService;

    public EditorUiStore(ISerializerService serializerService)
    {
        _serializerService = serializerService;
        EditorUiStoreContext.SerializerOption = serializerService.JsonOptions;
    }

    private const string Filename = "editor_store.json";
    
    public IEditorUiStoreContext ForContext(URN urnView)
    {
        if (urnView == URN.Empty)
        {
            _logger.Error("Empty URN provided, returning empty, throwaway, context");
            return new EditorUiStoreContext();
        }

        if (Contexts.TryGetValue(urnView, out var context)) return context;
        
        context = new EditorUiStoreContext();
        Contexts.Add(urnView, context);

        return context;
    }

    public void SaveToDisk()
    {
        var cacheDirectory = RpgEnv.PathFormat.GetCacheFolder();
        
        var path = Path.Combine(cacheDirectory, "ui");
        
        if(!Directory.Exists(path))
            Directory.CreateDirectory(path);

        var filePath = Path.Combine(path, Filename);

        _serializerService.SerializeTo(this, filePath);
    }

    public void LoadFromDisk()
    {
        var cacheDirectory = RpgEnv.PathFormat.GetCacheFolder();
        
        var path = Path.Combine(cacheDirectory, "ui");
        
        if(!Directory.Exists(path))
            Directory.CreateDirectory(path);

        var filePath = Path.Combine(path, Filename);

        _serializerService.DeserializeFrom<EditorUiStore>(filePath, out var editorUiStore);

        Contexts = editorUiStore?.Contexts ?? [];
    }
}

public sealed class EditorUiStoreContext : IEditorUiStoreContext
{
    internal static JsonSerializerOptions SerializerOption { get; set; }

    [JsonInclude]
    private Dictionary<string, object> Values { get; set; } = [];

    public bool Has(string key) => Values.ContainsKey(key);

    public bool TryGet<T>(string key, out T? value)
    {
        if (Values.TryGetValue(key, out var obj))
        {
            value = (T?)obj;
            return true;
        }

        value = default;
        return false;
    }

    public T GetOrDefault<T>(string key, T defaultValue)
    {
        if (!Values.TryGetValue(key, out var obj))
        {
            return defaultValue;
        }
        
        if (obj is T typedObj)
        {
            return typedObj;
        }
        
        if (obj is JsonElement jsonElement)
        {
            var deserialized = jsonElement.Deserialize<T>(SerializerOption);

            if (deserialized != null)
            {
                Values[key] = deserialized;
                return deserialized;
            }
                
            return defaultValue;
        }
        
        try 
        {
            var converted = (T)Convert.ChangeType(obj, typeof(T));
            Values[key] = converted;
            return converted;
        }
        catch
        {
            return defaultValue;
        }
    }

    public void AddOrEdit<T>(string key, T value)
    {
        if(value != null)
        {
            Values[key] = value;
        }
        else
        {
            Values.Remove(key);
        }
    }

    public void Remove(string key) => Values.Remove(key);
}