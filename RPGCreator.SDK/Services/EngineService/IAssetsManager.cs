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

using RPGCreator.SDK.Types;
using RPGCreator.SDK.Types.Internals;

namespace RPGCreator.SDK.Services.EngineService;

public enum RelationKind
{
    NoRelation,
    /// <summary>
    /// The first asset references the second asset.<br/>
    /// We can also say that the second asset is the parent of the first asset.
    /// </summary>
    FirstReferenceSecond,
    /// <summary>
    /// The second asset references the first asset.<br/>
    /// We can also say that the first asset is the parent of the second asset.
    /// </summary>
    SecondReferenceFirst,
    /// <summary>
    /// The engine did not expect the relation.<br/>
    /// Meaning that the relation between the two assets is not valid, like First references Second, but Second doesn't contains a back-reference to First.
    /// </summary>
    UnexpectedRelation,
    /// <summary>
    /// The engine detected a circular reference between the two assets.<br/>
    /// Meaning that the relation between the two assets is not valid, like First references Second, and Second references First.
    /// </summary>
    CircularReference,
    /// <summary>
    /// An internal error occured while trying to get the relation between the two assets.<br/>
    /// This can be caused because the DB is still not ready to process the relation.
    /// </summary>
    InternalError
}

public interface IAssetsManager : IService
{
    public interface IAssetIndexes
    {
        bool Has(Ulid id);
        Result<List<Ulid>> GetAssetsOfClass(URN classUrn);
        Result<URN> GetClassUrn(Ulid id);
        Result AddAsset(IEngineClass asset);
        Result RemoveAsset(Ulid id);

    }

    public interface IAssetRelations
    {
        bool HasRelationData(Ulid id);
        List<Ulid> GetReferences(Ulid id);
        List<Ulid> GetBackRefs(Ulid id);
        RelationKind GetRelationBetween(Ulid firstId, Ulid secondId);
        Result AddRelation(Ulid id, Ulid referencedAssetId);
        Result RemoveRelation(Ulid id, Ulid referencedAssetId);
        Result ClearRelations(Ulid id);
        bool HasRelation(Ulid id, Ulid referencedAssetId);
    }
    
    
    public IReadOnlyCollection<Ulid> LoadedUids { get; }
    public IReadOnlyCollection<IEngineClass> LoadedAssets { get; }
    public IAssetIndexes AssetIndexes { get; }
    public IAssetRelations AssetRelations { get; }
    
    void RefreshAssets();
    Result<T> Create<T>(URN classUrn, object? args = null) where T : class, IEngineClass;
    Result Save<T>(T instance) where T : class, IEngineClass;
    Result<T> Load<T>(Ulid id) where T : class, IEngineClass;
    Result<List<Ulid>> GetAssetsOfClass(URN classUrn);
    Result Delete(Ulid id);
    Result CanDelete(Ulid id);
    bool Has(Ulid id);
}