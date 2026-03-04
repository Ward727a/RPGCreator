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

using RPGCreator.SDK.Assets.MetaData;

namespace RPGCreator.SDK.Registry;

/// <summary>
/// A registry for assets metadata, which are simplified and light representations of the assets, containing only essential info.
/// </summary>
public interface IAssetsMetaDataRegistry : IService
{
    
    public void RegisterIfNotExists<T>(T data) where T : BaseMetaData;
    
    public void RegisterMetaData<T>(T data) where T : BaseMetaData;
    public void UpdateMetaData<T>(T data) where T : BaseMetaData;
    public void UnregisterMetaData(Ulid uniqueId);
    
    public T GetMetaData<T>(Ulid uniqueId) where T : BaseMetaData;
    public bool TryGetMetaData<T>(Ulid uniqueId, out T? data) where T : BaseMetaData;
    
    public IEnumerable<Ulid> GetAllMetaData();
    public IEnumerable<T> GetAllMetaDataOfType<T>() where T : BaseMetaData, new();

    public bool ContainsMetaData(Ulid uniqueId);
    
    public IEnumerable<Ulid> GetRelatedMetaDataIds(Ulid uniqueId);
}