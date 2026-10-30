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

using RPGCreator.EngineLib.Managers.Assets;

namespace RPGCreator.SDK.Services.Assets;

public interface IAssetDependencyManager
{
    public void RegisterType(Type assetType);
    public void UnregisterType(Type assetType);
    public void AutoFillDependencies(IAssetDto asset);
    public void RegisterAssetDependencies(Ulid assetId, List<Ulid> dependencies);
    public void ClearAssetDependencies(Ulid assetId);
    public void LoadHeader(AssetHeader assetHeader);
    

    /// <summary>
    /// Determines whether a specified dependency exists for the given asset.
    /// </summary>
    /// <param name="assetId">The unique identifier of the asset for which the dependency is being checked.</param>
    /// <param name="dependencyId">The unique identifier of the dependency to check for.</param>
    /// <returns>Returns <c>true</c> if the asset contains the specified dependency; otherwise, <c>false</c>.</returns>
    bool HasDependency(Ulid assetId, Ulid dependencyId);

    /// <summary>
    /// Determines whether the specified asset is being used by another asset.
    /// </summary>
    /// <param name="assetId">The unique identifier of the asset that might be used by another asset.</param>
    /// <param name="dependentId">The unique identifier of the asset that might be using the specified asset.</param>
    /// <returns>Returns <c>true</c> if the specified asset is being used by the dependent asset; otherwise, <c>false</c>.</returns>
    bool IsUsedBy(Ulid assetId, Ulid dependentId);

    /// <summary>
    /// Retrieves the dependencies associated with a specified asset.
    /// </summary>
    /// <param name="assetId">The unique identifier of the asset for which the dependencies are being retrieved.</param>
    /// <returns>Returns a read-only collection of unique identifiers representing the dependencies of the specified asset.</returns>
    IReadOnlyCollection<Ulid> GetDependencies(Ulid assetId);

    /// <summary>
    /// Retrieves the assets that depend on the specified asset.
    /// </summary>
    /// <param name="assetId">The unique identifier of the asset for which the dependent assets are being retrieved.</param>
    /// <returns>Returns a read-only collection of unique identifiers representing the assets that depend on the specified asset.</returns>
    IReadOnlyCollection<Ulid> GetDependents(Ulid assetId);
}