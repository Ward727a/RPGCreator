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

using System.Diagnostics.CodeAnalysis;
using RPGCreator.SDK.Modules;
using RPGCreator.SDK.Types;

namespace RPGCreator.SDK.EngineService;

/// <summary>
/// The global path data interface provides features and module the possibility to store ULID<br/>
/// via a URN path, and also to retrieve the URN path from a tag system.<br/>
/// <remarks>
/// This is a one-way system, meaning that you can only use tag to retrieve paths, but not the opposite.<br/>
/// And you can only use paths to retrieve values, but not the opposite.<br/>
/// This is because the main use of this system is to allow features and modules to store data that can be retrieved by other features and modules without having to reference them directly.<br/>
/// and without having to worry about the order of initialization as long as the "declaring" feature or module is set inside the "dependency" of the "dependent" feature.<br/>
/// The engine will take care of the rest, and will make sure that the data is available when the dependent feature or module is initialized.
/// </remarks>
/// <br/>
/// <b>PLEASE NOTE:</b><br/>
/// It's important to note that this system is not meant to be used in a loop or in a performance-critical code, as it is not optimized for that.<br/>
/// You should use this system to store, and/or retrieve data during the initialization of features and modules, or in other non-performance-critical code.<br/>
/// Or else you risk to cause performance issues!!!<br/>
/// <example>
/// <code>
/// public class MySuperFeature : BaseEntityFeature
/// {
///     public override string FeatureName => "My Super Feature";
///     public override string FeatureDescription => "A super feature that does super things.";
///     public override URN FeatureUrn => new URN("rpgc", FeatureUrnModule, "my_super_feature");
///     
///     public override void OnSetup()
///     {
///         EngineServices.OnceServiceReady((IGlobalPathData globalPathData) =>
///         {
///             var myStatTag = new URN("rpgc", "tags", "stats");
///             globalPathData.RegisterCategory(myStatTag);
///             globalPathData.RegisterPath(new URN("rpgc", "stats", "health"), new Ulid(), myStatTag);
///         });
///     }
/// }
/// // Here we declare another feature (in the same module or not, as long as we set the DependentFeatures).
/// public class MyOtherSuperFeature : BaseEntityFeature
/// {
///     public override string FeatureName => "My Other Super Feature";
///     public override string FeatureDescription => "Another super feature that does other super things.";
///     public override URN FeatureUrn => new URN("rpgc", FeatureUrnModule, "my_other_super_feature");
///     public override URN[] DependentFeatures => [new URN("rpgc", FeatureUrnModule, "my_super_feature")];
///     // Here, we can use the path registered by MySuperFeature without worrying about the order of initialization,
///     // because we declared the dependency on MySuperFeature, so the engine will make sure that MySuperFeature is
///     //initialized before MyOtherSuperFeature, and therefore that the path is registered before we try to retrieve it.
///     public override void OnSetup()
///     {
///         var myStatTag = new URN("rpgc", "tags", "stats");
///         if(globalPathData.TryGetPaths(myStatTag, out var paths))
///         {
///             var healthPath = paths.FirstOrDefault(p => p == new URN("rpgc", "stats", "health"));
///             var healthId = globalPathData.TryGetValue(healthPath, out var healthId) ? healthId : Ulid.Empty;
///         }
///     }
/// }
/// </code>
/// </example>
/// </summary>
public interface IGlobalPathData : IService
{
    /// <summary>
    /// Register a path to a value, with an optional tag to group it with other paths.
    /// </summary>
    /// <param name="pathToValue">The URN path to the value.</param>
    /// <param name="idValue">The ULID value to be associated with the path.</param>
    /// <param name="tag">An optional URN tag to group this path with other paths. This allows for easier retrieval of related paths.</param>
    public void RegisterPath(URN pathToValue, Ulid idValue, URN? tag = null);
    
    /// <summary>
    /// Register multiple paths to values, with an optional tag to group them together.<br/>
    /// This is a more efficient way to register multiple paths at once, especially if they share the same tag.
    /// </summary>
    /// <param name="pathsToValues">A list of tuples, where each tuple contains a URN path and its associated ULID value.</param>
    /// <param name="tag">An optional URN tag to group these paths together. This allows for easier retrieval of related paths.</param>
    public void RegisterPaths(List<(URN pathToValue, Ulid idValue)> pathsToValues, URN? tag = null);
    
    /// <summary>
    /// Try to retrieve the list of URN paths associated with a given tag.
    /// </summary>
    /// <param name="tag">The URN tag for which to retrieve the associated paths. This tag should have been used when registering paths.</param>
    /// <param name="path">The paths associated with the given tag</param>
    /// <returns>
    /// True if the path exists and has an associated value.<br/>
    /// False otherwise.
    /// </returns>
    public bool TryGetValues(URN tag, [NotNullWhen(true)] out IEnumerable<Ulid>? path);
    
    /// <summary>
    /// Remove a path and its associated value from the global path data.
    /// </summary>
    /// <param name="pathToValue">The URN path to the value that should be removed.</param>
    public void RemovePath(URN pathToValue);
    
    /// <summary>
    /// Register a tag to group paths together. This allows for easier retrieval of related paths.<br/>
    /// If you want to register a path with a tag, you can use the RegisterPath or RegisterPaths method and provide the tag as an argument<br/>
    /// You don't need to use this method to register a tag before using it in RegisterPath or RegisterPaths.<br/>
    /// But you can use it if you want to create a tag without registering any path yet.
    /// </summary>
    /// <param name="tag">The URN tag to be registered. This tag can be used to group related paths together for easier retrieval.</param>
    public void RegisterTag(URN tag);
    
    /// <summary>
    /// Register a tag with a path to group it together. This allows for easier retrieval of related paths.
    /// </summary>
    /// <param name="tag">The URN tag to be registered. This tag can be used to group related paths together for easier retrieval.</param>
    /// <param name="pathToValue">The URN path to the value that should be associated with the tag. This allows for easier retrieval of related paths.</param>
    public void RegisterTag(URN tag, Ulid pathToValue);
    
    /// <summary>
    /// Register a tag with multiple paths to group them together. This allows for easier retrieval of related paths.
    /// </summary>
    /// <param name="tag">The URN tag to be registered. This tag can be used to group related paths together for easier retrieval.</param>
    /// <param name="pathsToValues">A list of URN paths to the values that should be associated with the tag. This allows for easier retrieval of related paths.</param>
    public void RegisterTag(URN tag, List<Ulid> pathsToValues);
    
    /// <summary>
    /// Register multiple tags with their associated paths to group them together. This allows for easier retrieval of related paths.<br/>
    /// This is a more efficient way to register multiple tags at once, especially if they share the same paths.
    /// </summary>
    /// <param name="tagsToPaths">A list of tuples, where each tuple contains a URN tag and a list of URN paths to the values that should be associated with that tag. This allows for easier retrieval of related paths.</param>
    public void RegisterTags(List<(URN tag, List<Ulid> values)> tagsToPaths);
    
    /// <summary>
    /// Remove a tag and its associated paths from the global path data.<br/>
    /// This will not remove the paths themselves, but it will disassociate them from the tag, making them harder to retrieve if you were relying on the tag for that.
    /// </summary>
    /// <param name="tag">The tag to remove.</param>
    public void RemoveTag(URN tag);
    
    /// <summary>
    /// Try to retrieve the ULID value associated with a given URN path.<br/>
    /// This allows you to retrieve the value that was registered with the path, if it exists.<br/>
    /// If the path does not exist, the method will return false and the output value will be set to Ulid.Empty.
    /// </summary>
    /// <param name="path">The URN path for which to retrieve the associated value. This path should have been used when registering a value.</param>
    /// <param name="value">The ULID value associated with the given path, if it exists. If the path does not exist, this will be set to Ulid.Empty.</param>
    /// <returns>
    /// True if the path exists and has an associated value.<br/>
    /// False otherwise.
    /// </returns>
    public bool TryGetValue(URN path, out Ulid value);
    
    /// <summary>
    /// Return all the tags that are currently registered in the global path data.
    /// </summary>
    /// <returns>
    /// An enumerable of URN tags that are currently registered in the global path data. Each tag can be used to retrieve associated paths and values.
    /// </returns>
    public IEnumerable<URN> GetAllTags();
    
    /// <summary>
    /// Check if a given URN tag is registered in the global path data.
    /// </summary>
    /// <param name="tag">The URN tag to check for existence in the global path data. This tag should have been used when registering paths or values.</param>
    /// <returns>
    /// True if the tag is registered.<br/>
    /// False otherwise.
    /// </returns>
    public bool HasTag(URN tag);
    
    /// <summary>
    /// Check if a given URN tag is registered in the global path data and has a specific URN path associated with it.<br/>
    /// This allows you to check if a tag is associated with a specific path.
    /// </summary>
    /// <param name="tag">The URN tag.</param>
    /// <param name="pathToValue">The URN path to check for association with the tag.</param>
    /// <returns></returns>
    public bool TagHasValue(URN tag, Ulid pathToValue);
    
    /// <summary>
    /// Check if a given path is registered in the global path data.
    /// </summary>
    /// <param name="path">The URN path to check for existence in the global path data. This path should have been used when registering a value.</param>
    /// <returns>
    /// True if the path is registered.<br/>
    /// False otherwise.
    /// </returns>
    public bool HasPath(URN path);
}