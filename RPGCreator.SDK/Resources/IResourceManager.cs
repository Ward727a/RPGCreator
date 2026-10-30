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
using Newtonsoft.Json.Converters;
using RPGCreator.Shared.Types;
using SDL3;

namespace RPGCreator.SDK.Resources;

/// <summary>
/// Defines functionality for managing resources in a structured way,
/// including operations for importing, exporting, loading, retrieving,
/// and classifying resources based on their types and file formats.
/// </summary>
public interface IResourceManager
{
    public enum ResourceType
    {
        Unknown,
        Image,
        Audio,
        Video,
        Text,
    }

    public static IReadOnlyDictionary<StringName, ResourceType> ResourceTypes => new Dictionary<StringName, ResourceType>()
    {
        { ".png", ResourceType.Image },
        { ".jpg", ResourceType.Image },
        { ".jpeg", ResourceType.Image },
        { ".bmp", ResourceType.Image },
        { ".gif", ResourceType.Image },
        { ".tiff", ResourceType.Image },
        { ".mp3", ResourceType.Audio },
        { ".wav", ResourceType.Audio },
        { ".ogg", ResourceType.Audio },
        { ".flac", ResourceType.Audio },
        { ".mp4", ResourceType.Video },
        { ".avi", ResourceType.Video },
        { ".mkv", ResourceType.Video },
        { ".mov", ResourceType.Video },
        { ".txt", ResourceType.Text },
        { ".json", ResourceType.Text },
        { ".xml", ResourceType.Text }
    };
    
    public static IReadOnlyDictionary<ResourceType, List<StringName>> ResourceFormats = new Dictionary<ResourceType, List<StringName>>()
    {
        { ResourceType.Image, new List<StringName>() { ".png", ".jpg", ".jpeg" } },
        { ResourceType.Audio, new List<StringName>() { ".mp3", ".wav", ".ogg", ".flac" } },
        { ResourceType.Video, new List<StringName>() { ".mp4", ".avi", ".mkv", ".mov" } },
        { ResourceType.Text, new List<StringName>() { ".txt", ".json", ".xml" } }
    };

    public static IReadOnlyDictionary<ResourceType, string> ResourceIcons = new Dictionary<ResourceType, string>()
    {
        { ResourceType.Image, "\uf000" },
        { ResourceType.Audio, "\uf00d" },
        { ResourceType.Video, "\uf002" },
        { ResourceType.Text, "\uf00e" },
        { ResourceType.Unknown, "\uf015" }
    };
    
    public class ResourceObject
    {
        [JsonPropertyName("Id")]
        public Ulid ResourceId { get; set; }
        
        [JsonPropertyName("Name")]
        public string ResourceName { get; set; }

        /// <summary>
        /// The original path of the resource on the operating system.
        /// </summary>
        [JsonPropertyName("SourcePath")]
        public string ResourceSourcePath { get; set; }
        [JsonPropertyName("LocalPath")]
        public string ResourceLocalPath { get; set; }
        
        [JsonPropertyName("Format")]
        public StringName ResourceFormat { get; set; }
        
        [JsonPropertyName("Type")]
        [JsonConverter(typeof(JsonStringEnumConverter<ResourceType>))]
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public ResourceType ResourceType { get; set; }
        
        [JsonPropertyName("Meta")]
        public JsonElement ResourceMeta { get; init; }

        [JsonPropertyName("MetaType")]
        public URN? ResourceMetaTypeUrn { get; init; }
        
        [JsonIgnore]
        public object? ResourceMetaObject { get; set; }

        public Result<TMeta> GetMeta<TMeta>(IResourceManager resourceManager) where TMeta : class
        {
            return resourceManager.GetResourceMeta<TMeta>(this);
        }
    }

    public Result<ReadOnlyMemory<byte>> GetResource(string resourceLocalPath);
    public Result<ReadOnlyMemory<byte>> GetResource(Ulid resourceId);
    
    public Result<ResourceObject> GetResourceObject(string resourceLocalPath);
    public Result<ResourceObject> GetResourceObject(Ulid resourceId);

    public Result ExportResource(string resourceLocalPath, string destination);
    public Result ExportResource(Ulid resourceId, string destination);
    public Result ExportResource(ResourceObject resource, string destination);

    public bool HasResource(string localPath);
    public bool HasResource(Ulid resourceId);
    
    /// <summary>
    /// Imports a resource from the specified source path into the resource manager's internal cache.
    /// </summary>
    /// <param name="resourceSourcePath">
    /// The source path of the resource to import.
    /// </param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the imported <see cref="IResourceManager.ResourceObject"/>
    /// if the operation succeeds, or an error message if it fails.
    /// </returns>
    public Result<ResourceObject> ImportResource(string resourceSourcePath);

    /// <summary>
    /// Loads all resources from disk into the resource manager's internal cache.
    /// </summary>
    /// <returns>
    /// A <see cref="Result"/> indicating the success or failure of the operation.
    /// </returns>
    public Result LoadAllResourcesFromDisk();

    public Result<TMeta> GetResourceMeta<TMeta>(Ulid resourceId) where TMeta : class;
    public Result<TMeta> GetResourceMeta<TMeta>(string resourcePath) where TMeta : class;
    public Result<TMeta> GetResourceMeta<TMeta>(IResourceManager.ResourceObject @object) where TMeta : class;

    /// <summary>
    /// Retrieves all resources currently managed by the resource manager.
    /// </summary>
    /// <returns>
    /// A collection of <see cref="IResourceManager.ResourceObject"/> instances representing
    /// all resources stored in the resource manager's cache.
    /// </returns>
    public IEnumerable<ResourceObject> GetAllResources();

    /// <summary>
    /// Retrieves all resources of a specified type managed by the resource manager.
    /// </summary>
    /// <param name="resourceType">
    /// The type of resources to retrieve, specified as a value of <see cref="IResourceManager.ResourceType"/>.
    /// </param>
    /// <returns>
    /// A collection of <see cref="IResourceManager.ResourceObject"/> instances representing
    /// all resources of the specified type stored in the resource manager's cache.
    /// </returns>
    public IEnumerable<ResourceObject> GetAllResourcesOfType(ResourceType resourceType);


}