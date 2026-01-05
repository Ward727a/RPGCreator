#region LICENSE
//
// RPG Creator - Open-source RPG Engine.
// (c) 2025 Ward
// 
// This file is part of RPG Creator and is distributed under the MIT License.
// You are free to use, modify, and distribute this file under the terms of the MIT License.
// See LICENSE for details.
// 
// ---
// 
// Ce fichier fait partie de RPG Creator et est distribué sous licence MIT.
// Vous êtes libre de l'utiliser, de le modifier et de le distribuer sous les termes de la licence MIT.
// Voir LICENSE pour plus de détails.
// 
// Contact:
// => Mail: Ward727a@gmail.com
//    Please use this object: "RPG Creator [YourObject]"
// => Discord: ward727
// 
// For urgent inquiries, sending both an email and a message on Discord is highly recommended for a quicker response.
// 
// 
#endregion

using RPGCreator.Core.Types.Internal;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets;
using RPGCreator.SDK.Serializer;
using RPGCreator.SDK.Types.Interfaces;
using RPGCreator.SDK.Types.Internals;
using Serilog;

namespace RPGCreator.Core.Types.Assets.BaseAssetsPack
{
    
    /*
     * BaseAssetsPack.cs
     * =================
     * This class represents a pack of assets in RPG Creator.
     */
    public class BaseAssetsPack : IAssetsPack, ISerializable, IDeserializable, IDisposable
    {

    public BaseAssetsPack()
    {
    }

    public Ulid Id { get; set; } = Ulid.NewUlid();
    public string Name { get; set; }
    public string? Description { get; set; }

    public string DbFilePath { get; private set; }
    public string RootFolder { get; private set; }

    private Ulid _dbId = Ulid.Empty;
    private const string INDEX_COLLECTION = "asset_index";

    public BaseAssetsPack(string dbPath)
    {
        DbFilePath = dbPath;
        var dbName = Path.GetFileNameWithoutExtension(dbPath);
        RootFolder = Path.Combine(Path.GetDirectoryName(dbPath) ?? string.Empty, $"assets_{dbName}");

        if (string.Equals(RootFolder, $"assets_{dbName}", StringComparison.OrdinalIgnoreCase))
        {
            throw new IOException("Invalid database path. Root folder cannot be 'assets' directly.");
        }

        LoadIndex();
    }

    private void LoadIndex()
    {
        _dbId = EngineDB.OpenDB(DbFilePath);

        if (_dbId == Ulid.Empty)
        {
            throw new IOException("Failed to open database at path " + DbFilePath);
        }

        var meta = EngineDB.GetDBMetaData(_dbId);

        if (meta != null)
        {
            this.Id = meta.ObjectId;
            this.Name = meta.Name;
        }

        var db = EngineDB.GetDB(_dbId);
        if (db == null)
        {
            throw new IOException("Failed to get database instance for DB ID " + _dbId);
        }

        var indexCollection = db.GetCollection<EngineDB.AssetIndexRecord>(INDEX_COLLECTION);
        if (indexCollection == null)
        {
            throw new IOException("Failed to get or create index collection in database.");
        }
    }

    public object LoadAsset(Ulid assetId)
    {
        var db = EngineDB.GetDB(_dbId);
        if (db == null)
        {
            throw new IOException("Database not found for DB ID " + _dbId);
        }

        var indexCollection = db.GetCollection<EngineDB.AssetIndexRecord>(INDEX_COLLECTION);
        var index = indexCollection.FindById(assetId.ToString());

        if (index == null)
        {
            throw new KeyNotFoundException("Asset ID " + assetId + " not found in index.");
        }

        string fullPath = Path.Combine(RootFolder, index.RelativePath);

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("Asset file not found at path " + fullPath);
        }

        object? loadedAsset = null;

        try
        {
            string fileContent = File.ReadAllText(fullPath);
            EngineServices.SerializerService.Deserialize(fileContent, out loadedAsset, out System.Type? _);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "[Pack {PackName}] Failed to deserialize asset at path {FilePath}.", Name, fullPath);
            throw;
        }

        if (loadedAsset is IHasUniqueId idAsset && idAsset.Unique != index.Id)
        {
            Log.Warning(
                "[Pack {PackName}] Loaded asset ID {LoadedId} does not match index ID {IndexId} for file at path {FilePath}.",
                Name, idAsset.Unique, index.Id, fullPath);
        }

        return loadedAsset ?? throw new InvalidOperationException("Failed to load asset from file " + fullPath);
    }

    public IEnumerable<object> EnumerateAssets()
    {
        var db = EngineDB.GetDB(_dbId);
        if (db == null)
        {
            yield break;
        }

        var indexCollection = db.GetCollection<EngineDB.AssetIndexRecord>(INDEX_COLLECTION);
        var allIndexed = indexCollection.FindAll();

        foreach (var index in allIndexed)
        {
            string fullPath = Path.Combine(RootFolder, index.RelativePath);

            if (!File.Exists(fullPath))
            {
                Log.Warning("[Pack {PackName}] Index points to missing file at path {FilePath}. Skipping.", Name,
                    fullPath);
                continue;
            }

            object? loadedAsset = null;

            try
            {
                string fileContent = File.ReadAllText(fullPath);
                EngineServices.SerializerService.Deserialize(fileContent, out loadedAsset, out System.Type? _);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[Pack {PackName}] Failed to deserialize asset at path {FilePath}.", Name, fullPath);
                continue;
            }

            if (loadedAsset != null)
            {
                if (loadedAsset is IHasUniqueId idAsset && idAsset.Unique != index.Id)
                {
                    Log.Warning(
                        "[Pack {PackName}] Loaded asset ID {LoadedId} does not match index ID {IndexId} for file at path {FilePath}.",
                        Name, idAsset.Unique, index.Id, fullPath);
                }

                yield return loadedAsset;
            }
        }
    }

    public IEnumerable<EngineDB.AssetIndexRecord> EnumerateIndexOnly()
    {
        var db = EngineDB.GetDB(_dbId);
        if (db == null)
        {
            yield break;
        }

        var indexCollection = db.GetCollection<EngineDB.AssetIndexRecord>(INDEX_COLLECTION);
        var allIndexed = indexCollection.FindAll();

        foreach (var index in allIndexed)
        {
            yield return index;
        }
    }

    public void AddOrUpdateAsset(object asset, string relativeFolderPath = "")
    {
        if (asset is not IHasUniqueId idAsset) return;
        if (asset is not IHasSavePath savePathAsset) return;
        if (asset is not ISerializable serializableAsset) return;

        var db = EngineDB.GetDB(_dbId);
        if (db == null) return;

        if (string.IsNullOrEmpty(savePathAsset.SavePath))
        {
            Log.Warning("[Pack {PackName}] Asset {AssetId} has no save path defined. Generating one.", Name,
                idAsset.Unique);
            savePathAsset.SavePath = $"{idAsset.Unique}.asset";

            Log.Warning("[Pack {PackName}] Asset {AssetId} generated save path: {SavePath}.", Name,
                idAsset.Unique, savePathAsset.SavePath);
        }

        string fileName = Path.GetFileName(savePathAsset.SavePath);
        string relativePath = Path.Combine(relativeFolderPath, fileName);
        string fullPath = Path.Combine(RootFolder, relativePath);

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath) ?? string.Empty);

        EngineServices.SerializerService.Serialize(serializableAsset, out string serializedData);
        File.WriteAllText(fullPath, serializedData);

        var indexCollection = db.GetCollection<EngineDB.AssetIndexRecord>(INDEX_COLLECTION);

        var record = new EngineDB.AssetIndexRecord()
        {
            Id = idAsset.Unique,
            RelativePath = relativePath,
            TypeName = asset.GetType().FullName ?? "Unknown",
            LastIndexed = DateTime.UtcNow,
        };

        indexCollection.Upsert(record);

        EngineCore.Instance.Managers.Assets.AddNewAssetLocation(idAsset.Unique, this, relativePath,
            record.TypeName);

        Log.Information("[Pack {PackName}] Asset {AssetId} saved to path {FilePath} and indexed.", Name,
            idAsset.Unique, fullPath);
    }

    public void RemoveAsset(Ulid assetId)
    {
        var db = EngineDB.GetDB(_dbId);
        if (db == null) return;

        var indexCollection = db.GetCollection<EngineDB.AssetIndexRecord>(INDEX_COLLECTION);
        var record = indexCollection.FindById(assetId.ToString());

        if (record != null)
        {
            string fullPath = Path.Combine(RootFolder, record.RelativePath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                Log.Information("[Pack {PackName}] Asset file at path {FilePath} deleted.", Name, fullPath);
            }

            indexCollection.Delete(assetId.ToString());
            Log.Information("[Pack {PackName}] Asset {AssetId} removed from index.", Name, assetId);
        }
    }

    public IEnumerable<IAssetIndexRecord> SearchIndex(Func<IAssetIndexRecord, bool> predicate)
    {
        var db = EngineDB.GetDB(_dbId);
        if (db == null)
        {
            yield break;
        }

        var indexCollection = db.GetCollection<EngineDB.AssetIndexRecord>(INDEX_COLLECTION);
        var allIndexed = indexCollection.FindAll();

        foreach (var index in allIndexed.Where(predicate))
        {
            yield return index;
        }
    }

    public IEnumerable<IAssetIndexRecord> SearchIndexByType(Type type)
    {
        var db = EngineDB.GetDB(_dbId);
        if (db == null)
        {
            yield break;
        }

        var indexCollection = db.GetCollection<EngineDB.AssetIndexRecord>(INDEX_COLLECTION);

        var validNames = Common.TypeUtil.GetInheritance(type);

        var allIndexed = indexCollection.Find(x => validNames.Contains(x.TypeName));

        foreach (var index in allIndexed)
        {
            yield return index;
        }
    }

    public void Dispose()
    {
        if (_dbId != Ulid.Empty)
        {
            EngineDB.CloseDB(_dbId);
            _dbId = Ulid.Empty;
        }
    }

    public void Save()
    {
        if (_dbId != Ulid.Empty)
        {
            if (EngineDB.SaveDB(_dbId) is EngineDB.ESaveDbStatus status && status != EngineDB.ESaveDbStatus.Success)
            {
                Log.Error("[Pack {PackName}] Failed to save database. Status: {Status}", Name, status);
                return;
            }

            Log.Information("[Pack {PackName}] Database saved successfully.", Name);
        }
    }

    public SerializationInfo GetObjectData()
    {
        var info = new SerializationInfo(typeof(BaseAssetsPack));

        info.AddValue(nameof(Id), Id)
            .AddValue(nameof(Name), Name)
            .AddValue(nameof(Description), Description)
            .AddValue(nameof(DbFilePath), DbFilePath)
            .AddValue(nameof(RootFolder), RootFolder);

        return info;
    }

    public void SetObjectData(DeserializationInfo info)
    {
        if (info == null)
        {
            throw new ArgumentNullException(nameof(info), "DeserializationInfo cannot be null.");
        }

        info.TryGetValue(nameof(Id), out Ulid id, Ulid.NewUlid());
        info.TryGetValue(nameof(Name), out string name, string.Empty);
        info.TryGetValue(nameof(Description), out string? description, null);
        info.TryGetValue(nameof(DbFilePath), out string dbFilePath, string.Empty);
        info.TryGetValue(nameof(RootFolder), out string rootFolder, string.Empty);

        Id = id;
        Name = name;
        Description = description;
        DbFilePath = dbFilePath;
        RootFolder = rootFolder;
    }
    }
}
