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

using LiteDB;
using RPGCreator.Core.Types.Assets.BaseAssetsPack;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.MetaData;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Registry;

namespace RPGCreator.Core.Registry;

public record struct RegistryEntry()
{
    [BsonId]
    public Ulid Id { get; set; }
    public string DbTableKey { get; set; }
}

public enum MetaDataChangeType
{
    Registered,
    Updated,
    Unregistered
}

public class MetaDataChangedArgs : EventArgs
{
    public Ulid UniqueId { get; }
    public string DbTableKey { get; }
    public MetaDataChangeType ChangeType { get; }

    public MetaDataChangedArgs(Ulid uniqueId, string dbTableKey, MetaDataChangeType changeType)
    {
        UniqueId = uniqueId;
        DbTableKey = dbTableKey;
        ChangeType = changeType;
    }
}

public class AssetsMetadataRegistry : IAssetsMetaDataRegistry
{
    public event Action<MetaDataChangedArgs>? OnMetaDataChanged;
    
    private BaseAssetsPack _defaultPack;
    
    private static ILiteCollection<RegistryEntry> _registryCache;
    private static Dictionary<string, object> _detailsDbCache = new Dictionary<string, object>();
    private static Dictionary<Type, string> _typeToDbTableKeyCache = new Dictionary<Type, string>();

    private void EnsureRegistry()
    {
        if (_registryCache != null) return;
        var db = GetDatabase() ?? throw new InvalidOperationException("The database is not available. This should never happen, as the database should be available when the registry is initialized.");
        _registryCache = db.GetCollection<RegistryEntry>("AssetsMetadata");
        _registryCache.EnsureIndex(x => x.Id);
        Logger.Debug("AssetsMetadataRegistry: Registry initialized.");
    }
    
    private ILiteCollection<T> GetDetailsCollection<T>(string dbTableKey) where T : BaseMetaData
    {
        if (_detailsDbCache.TryGetValue(dbTableKey, out var cachedCollection) && cachedCollection is ILiteCollection<T> detailsCollection)
        {
            return detailsCollection;
        }
        
        var db = GetDatabase() ?? throw new InvalidOperationException("The database is not available. This should never happen, as the database should be available when the registry is initialized.");
        var detailsDb = db.GetCollection<T>(dbTableKey);
        detailsDb.EnsureIndex(x => x.UniqueId);
        _detailsDbCache[dbTableKey] = detailsDb;
        Logger.Debug($"AssetsMetadataRegistry: Details collection for table key '{dbTableKey}' initialized.");
        return detailsDb;
    }
    
    public AssetsMetadataRegistry()
    {
        var pack = EngineServices.AssetsManager.GetDefaultPack();
        
        if(pack == null)
            throw new InvalidOperationException("The default assets pack is not loaded. This should never happen, as the default pack should be loaded before the registry.");
        
        if(pack is not BaseAssetsPack assetsPack)
            throw new InvalidOperationException("The default assets pack is not of type BaseAssetsPack. This should never happen, as the default pack should be of type BaseAssetsPack.");
        
        _defaultPack = assetsPack;
    }

    public void RegisterIfNotExists<T>(T data) where T : BaseMetaData
    {
        EnsureRegistry();
        
        if(ContainsMetaData(data.UniqueId))
            return;
        
        RegisterMetaData(data);
    }

    public void RegisterMetaData<T>(T data) where T : BaseMetaData
    {
        EnsureRegistry();
        
        _registryCache.Upsert(new RegistryEntry()
        {
            Id = data.UniqueId,
            DbTableKey = data.DbKey
        });
        
        var detailsDb = GetDetailsCollection<T>(data.DbKey);
        detailsDb.Upsert(data);
        OnMetaDataChanged?.Invoke(new MetaDataChangedArgs(data.UniqueId, data.DbKey, MetaDataChangeType.Registered));
        Logger.Debug($"AssetsMetadataRegistry: Registered metadata with unique id {data.UniqueId} in the registry.");
    }
    
    public void UpdateMetaData<T>(T data) where T : BaseMetaData
    {
        EnsureRegistry();
        
        if(!ContainsMetaData(data.UniqueId))
            throw new InvalidOperationException($"The metadata with the unique id {data.UniqueId} is not registered in the registry.");
        
        var detailsDb = GetDetailsCollection<T>(data.DbKey);
        detailsDb.Update(data);
        OnMetaDataChanged?.Invoke(new MetaDataChangedArgs(data.UniqueId, data.DbKey, MetaDataChangeType.Updated));
        
    }

    public void UnregisterMetaData(Ulid uniqueId)
    {
        EnsureRegistry();
        var db = GetDatabase() ?? throw new InvalidOperationException("The database is not available. This should never happen, as the database should be available when the registry is initialized.");
        var validId = new BsonValue(uniqueId.ToString());
        var entry = _registryCache.FindById(validId);

        if(entry == default)
            throw new InvalidOperationException($"The metadata with the unique id {uniqueId} is not registered in the registry.");
        
        _registryCache.Delete(validId);
        
        var detailsDb = db.GetCollection(entry.DbTableKey);
        detailsDb.Delete(validId);
        OnMetaDataChanged?.Invoke(new MetaDataChangedArgs(uniqueId, entry.DbTableKey, MetaDataChangeType.Unregistered));
    }

    public T GetMetaData<T>(Ulid uniqueId) where T : BaseMetaData
    {
        EnsureRegistry();
        
        var validId = new BsonValue(uniqueId.ToString());
        var entry = _registryCache.FindById(validId);

        if(entry == default)
            throw new InvalidOperationException($"The metadata with the unique id {uniqueId} is not registered in the registry.");
        
        var data = GetDetailsCollection<T>(entry.DbTableKey).FindById(validId);

        if(data == default)
            throw new InvalidOperationException($"The metadata with the unique id {uniqueId} is not registered in the registry.");
        
        return data;
    }

    public bool TryGetMetaData<T>(Ulid uniqueId, out T? data) where T : BaseMetaData
    {
        EnsureRegistry();
        
        var validId = new BsonValue(uniqueId.ToString());
        var entry = _registryCache.FindById(validId);

        if(entry == default)
        {
            data = null;
            return false;
        }
        
        var foundData = GetDetailsCollection<T>(entry.DbTableKey).FindById(validId);

        if(foundData == null)
        {
            data = null;
            return false;
        }
        
        data = foundData;
        return true;
    }

    public IEnumerable<Ulid> GetAllMetaData()
    {
        EnsureRegistry();
        return _registryCache.FindAll().Select(entry => entry.Id);
    }

    public IEnumerable<T> GetAllMetaDataOfType<T>() where T : BaseMetaData, new()
    {
        var detailsDb = GetDetailsCollection<T>(GetDbTableKey<T>());
        return detailsDb.FindAll();
    }

    public bool ContainsMetaData(Ulid uniqueId)
    {
        EnsureRegistry();
        var validId = new BsonValue(uniqueId.ToString());
        var entry = _registryCache.FindById(validId);

        return entry != default;
    }

    public IEnumerable<Ulid> GetRelatedMetaDataIds(Ulid uniqueId)
    {
        EnsureRegistry();
        var db = GetDatabase() ?? throw new InvalidOperationException("The database is not available. This should never happen, as the database should be available when the registry is initialized.");
        var validId = new BsonValue(uniqueId.ToString());
        var entry = _registryCache.FindById(validId);

        if(entry == default)
            throw new InvalidOperationException($"The metadata with the unique id {uniqueId} is not registered in the registry.");
        
        var detailsDb = db.GetCollection<BaseMetaData>(entry.DbTableKey);
        return detailsDb.Find(x => x.HasReferenceTo(uniqueId)).Select(x => x.UniqueId);
    }
    
    private LiteDatabase? GetDatabase()
    {
        return EngineDB.GetDB(_defaultPack.GetDbId());
    }
    
    private string GetDbTableKey<T>() where T : BaseMetaData, new()
    {
        if(_typeToDbTableKeyCache.TryGetValue(typeof(T), out var cachedKey))
            return cachedKey;
        
        var dbTableKey = new T().DbKey;
        _typeToDbTableKeyCache[typeof(T)] = dbTableKey;
        return dbTableKey;
    }
}