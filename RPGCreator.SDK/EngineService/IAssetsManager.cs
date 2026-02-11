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
using RPGCreator.SDK.Assets;
using RPGCreator.SDK.Assets.Definitions;
using RPGCreator.SDK.Types;
using RPGCreator.SDK.Types.Collections;
using RPGCreator.SDK.Types.Internals;
using RPGCreator.SDK.Types.Records;

namespace RPGCreator.SDK.EngineService;

public interface IAssetsManager : IService
{
    event Action<IBaseAssetDef>? OnAssetRegistered;
    event Action<IBaseAssetDef>? OnAssetUnregistered;
    public void RegisterRegistry(IAssetRegistry registry);
    public void RegisterAsset(object asset);
    public bool TryResolveRegistry<T>(string moduleName, [NotNullWhen(true)] out T? registry) where T : IAssetRegistry;
    public bool TryResolveRegistry<T>(System.Type type, [NotNullWhen(true)] out T? registry) where T : IAssetRegistry;
    public bool TryResolveRegistry(string moduleName, [NotNullWhen(true)] out IAssetRegistry? registry);
    public bool TryResolveRegistry(System.Type type, [NotNullWhen(true)] out IAssetRegistry? registry);
    public bool TryResolveAsset<T>(URN urn, [NotNullWhen(true)] out T? result) where T : class, IHasUniqueId;
    public bool TryResolveAsset<T>(Ulid uniqueId, [NotNullWhen(true)] out T? result) where T : class, IHasUniqueId;
    public T CreateAsset<T>() where T : IBaseAssetDef, IHasUniqueId, new();
    public T CreateTransientAsset<T>(IAssetScope? scope = null) where T : IBaseAssetDef, new();
    public void DestroyTransientAsset<T>(T asset) where T : IBaseAssetDef;
    public IAssetScope CreateAssetScope(string? name = null);
    public void AddPack(string dbPath);
    public void RegisterPack(IAssetsPack pack);
    public void UnregisterPack(Ulid packId);
    public IAssetsPack GetDefaultPack();
    public bool TryGetPack(string? packName, [NotNullWhen(true)] out IAssetsPack? pack);
    public bool TryGetPack(Ulid packId, [NotNullWhen(true)] out IAssetsPack? pack);
    public IAssetsPack GetPack(Ulid packId);
    public void AddNewAssetLocation(Ulid assetId, IAssetsPack? pack, string relativePath, string typeName, bool isTransient = false);
    public List<IAssetsPack> GetLoadedPacks();
    public IEnumerable<PackSearchResult> SearchAllPacks<T>();
    public IEnumerable<T> GetAssetsOfType<T>() where T : class, IBaseAssetDef, IHasUniqueId;
    public IEnumerable<T> GetAssetsOfType<T>(T valueForType) where T : class, IBaseAssetDef, IHasUniqueId;
}