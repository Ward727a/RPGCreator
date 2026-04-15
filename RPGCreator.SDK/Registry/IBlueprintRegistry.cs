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

using RPGCreator.SDK.Assets.Definitions.Blueprints;
using RPGCreator.SDK.Graph;
using RPGCreator.SDK.Types;

namespace RPGCreator.SDK.Registry;

public interface IBlueprintRegistry : IService
{
    int BlueprintCount { get; }

    Result RegisterBlueprintLogic(Ulid id, BaseBpCompiledLogic logic);
    
    /// <summary>
    /// Register a blueprint.
    /// </summary>
    /// <param name="blueprintData">The blueprint data to register.</param>
    /// <returns>
    /// Return a <see cref="Result"/> object indicating the success or failure of the operation.<br/>
    /// In case of failure, the <see cref="Result.Error"/> property will contain the error message.
    /// </returns>
    Result RegisterBlueprint(BlueprintData blueprintData);
    
    /// <summary>
    /// Unregister a blueprint by its ID.
    /// </summary>
    /// <param name="id">The ID of the blueprint to unregister.</param>
    /// <returns>
    /// Return a <see cref="Result"/> object indicating the success or failure of the operation.<br/>
    /// In case of failure, the <see cref="Result.Error"/> property will contain the error message.
    /// </returns>
    Result UnregisterBlueprint(Ulid id);
    
    /// <summary>
    /// Get a blueprint by its ID.
    /// </summary>
    /// <param name="id">The ID of the blueprint to get.</param>   
    /// <returns>
    /// Return a <see cref="Result{T}"/> object containing the blueprint data if found; otherwise, an error result.
    /// </returns>
    Result<BlueprintData> GetBlueprint(Ulid id);

    /// <summary>
    /// Return all registered blueprints.
    /// </summary>
    /// <returns>
    /// An enumerable of <see cref="BlueprintData"/> containing all registered blueprints.
    /// </returns>
    IEnumerable<BlueprintData> GetAllBlueprints();
    
    /// <summary>
    /// Check if a blueprint with the given ID is registered.
    /// </summary>
    /// <param name="id">
    /// The ID of the blueprint to check.
    /// </param>  
    /// <returns>
    /// True if the blueprint is registered; otherwise, false.
    /// </returns>
    bool HasBlueprint(Ulid id);

    /// <summary>
    /// Compile all registered blueprints in a GlobalBlueprints.dll saved in the specified folder, or in the default folder if not specified.<br/>
    /// If the GlobalBlueprints.dll already exists, it will be overwritten if: <br/>
    /// - any blueprint has been changed.<br/>
    /// - or if forceCompilation is true.
    /// </summary>
    /// <param name="toFolder">The folder where the GlobalBlueprints.dll will be saved. If null, the default folder will be used.</param>
    /// <param name="forceCompilation">If true, the compilation will be forced even if no blueprint has been changed.</param>
    /// <param name="skipBlueprints">A collection of blueprint IDs to skip during compilation. If null, no blueprints will be skipped.</param>
    /// <returns>
    /// Return a <see cref="Result"/> object indicating the success or failure of the operation.<br/>
    /// In case of failure, the <see cref="Result.Error"/> property will contain the error message.
    /// </returns>
    Result<byte[]> CompileAllBlueprints(string? blueprintFolder = null, string? toFolder = null, bool forceCompilation = false,
        IEnumerable<Ulid>? skipBlueprints = null);


    /// <summary>
    /// Build a specified blueprint, creating a [blueprint.Id].cs file in the specified folder, or in the default folder if not specified.<br/>
    /// If the file already exists, it will be overwritten if:<br/>
    /// - the blueprint has changed.<br/>
    /// - or if forceBuild is true.
    /// </summary>
    /// <param name="id">The ID of the blueprint to build.</param>
    /// <param name="toFolder">The folder where the [blueprint.Id].cs file will be saved. If null, the default folder will be used.</param>
    /// <param name="forceBuild">If true, the build will be forced even if the blueprint has not changed.</param>
    /// <returns>
    /// Return a <see cref="Result{T}"/> object containing the content of the [blueprint.Id].cs file if successful; otherwise, an error result.
    /// </returns>
    Result<string> BuildBlueprint(Ulid id, string? toFolder = null, bool forceBuild = false);
    
    /// <summary>
    /// Build a specified blueprint, creating a [blueprint.Id].cs file in the specified folder, or in the default folder if not specified.<br/>
    /// If the file already exists, it will be overwritten if:<br/>
    /// - the blueprint has changed.<br/>
    /// - or if forceBuild is true.
    /// </summary>
    /// <param name="blueprintData">The blueprint data to build.</param>
    /// <param name="toFolder">The folder where the [blueprint.Id].cs file will be saved. If null, the default folder will be used.</param>
    /// <param name="forceBuild">If true, the build will be forced even if the blueprint has not changed.</param>   
    /// <remarks>
    /// Using this method is not recommended, because it does not register the given blueprint in the registry, and as such, will not compile it when calling <see cref="CompileAllBlueprints(string?, bool)"/>.
    /// </remarks>
    /// <returns>
    /// Return a <see cref="Result{T}"/> object containing the content of the [blueprint.Id].cs file if successful; otherwise, an error result.
    /// </returns>
    Result<string> BuildBlueprint(BlueprintData blueprintData, string? toFolder = null, bool forceBuild = false);
}