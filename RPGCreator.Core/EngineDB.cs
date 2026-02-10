using LiteDB;
using RPGCreator.SDK.Assets;
using RPGCreator.SDK.Logging;

namespace RPGCreator.Core;

public class DataBaseMetaData()
{
    public int Id { get; set; } // DB Related ID
    public Ulid ObjectId { get; set; } = Ulid.NewUlid() ; // DataBase ID
    public string Name { get; set; } // DataBase Name
}

public class EngineDB
{

    static EngineDB()
    {
        BsonMapper.Global.RegisterType(
            serialize: (ulid) => ulid.ToString(),
            deserialize: (bson) => 
            {
                if (bson.IsString) return Ulid.Parse(bson.AsString);
                
                if (bson.IsDocument)
                {
                    var idValue = bson.AsDocument["_id"] ?? bson.AsDocument["$value"];
                    if (idValue != null && idValue.IsString)
                        return Ulid.Parse(idValue.AsString);
                }
                
                return Ulid.Parse(bson.ToString().Trim('"'));
            }
        );
    }
    
    public class DatabaseFileData()
    {
        public int Id { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public DateTime LastModified { get; set; }
        public Dictionary<string, string> MetaDatas { get; set; } = new Dictionary<string, string>();
    }

    public class AssetRefrenceIdsRecord()
    {
        [BsonId]
        public Ulid AssetId { get; set; }
        public List<Ulid> RefrencedIds { get; set; } = new List<Ulid>();
    }

    public class AssetIndexRecord() : IAssetIndexRecord
    {
        /// <summary>
        /// The unique identifier of the asset.
        /// </summary>
        [BsonId]
        public Ulid Id { get; set; }
        public string RelativePath { get; set; } = string.Empty;
        public string TypeName { get; set; } = string.Empty;
        public DateTime LastIndexed { get; set; }
    }

    public class ModuleAuthRecord()
    {
        [BsonId]
        public Ulid ModuleKey { get; set; }
        public string ModuleHash { get; set; } = string.Empty;
        
        public string ModuleUrn { get; set; } = string.Empty;
        public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUsedAt { get; set; }
        public bool IsAllowed { get; set; } = false;
    }
    
    const string DbHashExtension = ".dbhash";

    public const string EngineSettingsDbKey = "@settings";
    public const string EngineProjectsDbKey = "@projects";

    private static readonly Dictionary<string, string> EngineReservedDatabase  = new Dictionary<string, string>()
    {
        { "@settings", "engine_settings.db" },
        { "@projects", "engine_projects.db" },
        { "@auth", "engine_auth.db" },
    };
    
    private enum ERegisterDbStatus
    {
        UnexpectedError,
        NoIdFound,
        IdAlreadyRegistered,
        PathAlreadyRegistered,
        Success,
    }
    
    private enum ECreateDbHashStatus
    {
        UnexpectedError,
        FileNotFound,
        Success,
    }
    
    private enum ECheckDbHashStatus
    {
        UnexpectedError,
        FileNotFound,
        HashMismatch,
        HashMatch,
    }

    public enum ECloseDbStatus
    {
        UnexpectedError,
        DbNotFound,
        Success,
    }

    public enum EFileInsertStatus
    {
        UnexpectedError,
        DbNotFound,
        Success,
    }
    
    public enum ESaveDbStatus
    {
        UnexpectedError,
        DbNotFound,
        Success,
    }
    
    private static readonly ScopedLogger _logger = Logger.ForContext<EngineDB>();
    
    private static Dictionary<Ulid, LiteDatabase>  Databases { get; } = new Dictionary<Ulid, LiteDatabase>();
    private static Dictionary<string, Ulid> DatabaseIds { get; } = new Dictionary<string, Ulid>();

    /// <summary>
    /// Opening a database file, if it does not exist it will be created.
    /// </summary>
    /// <param name="dbFilePath">The file path to the database file.</param>
    /// <returns>The Ulid of the opened database, or Ulid.Empty if an error occurred.</returns>
    public static Ulid OpenDB(string dbFilePath)
    {
        dbFilePath = CheckReservedPath(dbFilePath);
        
        if (DatabaseIds.TryGetValue(dbFilePath, out var existingId))
        {
            _logger.Info("Database at {dbFilePath} is already opened with id {dbId}", args: [dbFilePath, existingId]);
            return existingId;
        }

        if (File.Exists(dbFilePath))
        {
            var hashStatus = ECheckDbHashStatus.HashMatch;
            if (hashStatus != ECheckDbHashStatus.HashMatch)
            {
                if (hashStatus == ECheckDbHashStatus.HashMismatch)
                {
                    _logger.Error("Database file at {dbFilePath} failed integrity check.", args: dbFilePath);
                    return Ulid.Empty;
                }

                if (hashStatus == ECheckDbHashStatus.FileNotFound)
                {
                    _logger.Warning("No hash file found for database at {dbFilePath}, creating new hash file.", args: dbFilePath);
                    var createHashStatus = CreateDbHash(dbFilePath);
                    if (createHashStatus != ECreateDbHashStatus.Success)
                    {
                        _logger.Error("Failed to create hash file for database at {dbFilePath} with code {errorCode}",
                            args: [dbFilePath, createHashStatus]);
                        return Ulid.Empty;
                    }
                }
                else
                {
                    _logger.Error("Failed to check hash for database at {dbFilePath} with code {errorCode}",
                        args: [dbFilePath, hashStatus]);
                    return Ulid.Empty;
                }
            }

            var db = new LiteDatabase(dbFilePath);
            
            var metadataCollection = db.GetCollection<DataBaseMetaData>("metadata");

            if (metadataCollection.Count() == 0)
            {
                _logger.Error($"No metadata found for database at {dbFilePath}");
                return Ulid.Empty;
            }

            var metadata = metadataCollection.Query().ToList()[0];

            if (metadata.ObjectId == Ulid.Empty)
            {
                _logger.Error("Found metadata but no id found for database {dbFilePath}({dbMetaName})", args: [dbFilePath, string.IsNullOrWhiteSpace(metadata.Name) ? "NO NAME" : metadata.Name]);
                return Ulid.Empty;
            }

            if (RegisterDB(dbFilePath, metadata, db) is var status && status  != ERegisterDbStatus.Success)
            {
                _logger.Error("Failed to register database at {dbFilePath} with code {errorCode}", args: [dbFilePath, status]);
                return Ulid.Empty;
            }

            return metadata.ObjectId;
        }
        else
        {
            var db = new LiteDatabase(dbFilePath);
            
            var metadataCollection = db.GetCollection<DataBaseMetaData>("metadata");
            var metadata = new DataBaseMetaData()
            {
                Name = Path.GetFileNameWithoutExtension(dbFilePath),
            };
            
            metadataCollection.Insert(metadata);
            
            if (RegisterDB(dbFilePath, metadata, db) is var status && status  != ERegisterDbStatus.Success)
            {
                _logger.Error("Failed to register database at {dbFilePath} with code {errorCode}", args: [dbFilePath, status]);
                return Ulid.Empty;
            }
            
            _logger.Debug("Created new database at {dbFilePath} with id {dbId}", args: [dbFilePath, metadata.ObjectId]);
            
            return metadata.ObjectId;
        }
    }

    public static bool IsDBOpen(string dbFilePath)
    {
        dbFilePath = CheckReservedPath(dbFilePath);
        
        return DatabaseIds.ContainsKey(dbFilePath);
    }
    
    public static LiteDatabase? GetDB(string dbFilePath)
    {
        dbFilePath = CheckReservedPath(dbFilePath);
        
        if (DatabaseIds.TryGetValue(dbFilePath, out var dbId))
        {
            return GetDB(dbId);
        }
        _logger.Warning("Tried to get database at path {dbFilePath} but it was not found", args: dbFilePath);
        return null;
    }
    
    public static LiteDatabase? GetDB(Ulid dbId)
    {
        if (Databases.TryGetValue(dbId, out var db))
        {
            return db;
        }
        _logger.Warning("Tried to get database with id {dbId} but it was not found", args: dbId);
        return null;
    }
    
    /// <summary>
    /// Closes the database with the given id.
    /// </summary>
    /// <param name="dbId">The Ulid of the database to close.</param>
    /// <returns>Status of the close operation. <br/>(<see cref="ECloseDbStatus.Success"/> if successful)</returns>
    public static ECloseDbStatus CloseDB(Ulid dbId)
    {
        try
        {
            var kvp = DatabaseIds.FirstOrDefault(x => x.Value == dbId);

            if (kvp.Equals(default(KeyValuePair<string, Ulid>)))
            {
                _logger.Warning("Tried to close database with id {dbId} but it was not found", args: dbId);
                return ECloseDbStatus.DbNotFound;
            }

            if (Databases.ContainsKey(dbId))
            {
                
                var createDbHashStatus = CreateDbHash(kvp.Key);
                
                if(createDbHashStatus != ECreateDbHashStatus.Success)
                {
                    _logger.Error("Failed to create hash for database at {dbFilePath} while closing with code {errorCode}", args: [kvp.Key, createDbHashStatus]);
                    return ECloseDbStatus.UnexpectedError;
                }
                
                Databases[dbId].Dispose();
                Databases.Remove(dbId);
                DatabaseIds.Remove(kvp.Key);
                return ECloseDbStatus.Success;
            }
            else
            {
                _logger.Warning("Tried to close database with id {dbId} but it was not found", args:dbId);
                return ECloseDbStatus.DbNotFound;
            }
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to close database with id {dbId}: {errorMessage}", args: [dbId, ex.Message]);
            return ECloseDbStatus.UnexpectedError;
        }
    }
    
    public static ESaveDbStatus SaveDB(Ulid dbId)
    {
        var db = GetDB(dbId);
        if (db == null)
        {
            _logger.Error("Failed to save database with id {dbId}: Database not found", args: dbId);
            return ESaveDbStatus.DbNotFound;
        }

        db.Checkpoint();
        return ESaveDbStatus.Success;
    }
    
    public static DataBaseMetaData? GetDBMetaData(Ulid dbId)
    {
        var db = GetDB(dbId);
        if (db == null)
        {
            _logger.Error("Failed to get metadata for database with id {dbId}: Database not found", args: dbId);
            return null;
        }
        
        var metadataCollection = db.GetCollection<DataBaseMetaData>("metadata");
        var metadata = metadataCollection.Query().ToList().FirstOrDefault();
        return metadata;
    }

    public static EFileInsertStatus AddConfig(DatabaseFileData data)
    {
        if (!IsDBOpen(EngineSettingsDbKey))
        {
            var dbId = OpenDB(EngineSettingsDbKey);
            if (dbId == Ulid.Empty)
            {
                _logger.Error("Failed to open engine settings database to insert config file.");
                return EFileInsertStatus.DbNotFound;
            }
        }
        
        var db = GetDB(EngineSettingsDbKey);
        
        if(db == null)
        {
            _logger.Error("Failed to insert config file to engine settings database: Database not found");
            return EFileInsertStatus.DbNotFound;
        }
        
        var collection = db.GetCollection<DatabaseFileData>();
        
        var existing = collection.FindOne(x => x.FilePath == data.FilePath);
        if (existing != null)
        {
            data.Id = existing.Id;
            collection.Update(data);
            return EFileInsertStatus.Success;
        }
        
        db.GetCollection<DatabaseFileData>().Insert(data);
        
        return EFileInsertStatus.Success;
    }

    public static EFileInsertStatus AddFile(Ulid dbId, DatabaseFileData data)
    {
        var db = GetDB(dbId);
        
        if(db == null)
        {
            _logger.Error("Failed to insert file to database with id {dbId}: Database not found", args: dbId);
            return EFileInsertStatus.DbNotFound;
        }
        
        db.GetCollection<DatabaseFileData>().Insert(data);

        return EFileInsertStatus.Success;
    }
    
    internal static ECloseDbStatus CloseAllDBs()
    {
        try
        {
            foreach (var db in Databases.Values)
            {
                db.Dispose();
            }
            Databases.Clear();
            DatabaseIds.Clear();
            return ECloseDbStatus.Success;
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to close all databases: {errorMessage}", args: ex.Message);
            return ECloseDbStatus.UnexpectedError;
        }
    }
    
    // HELPERS

    /// <summary>
    /// Registers a database with the given metadata.
    /// </summary>
    /// <param name="dbFilePath"></param>
    /// <param name="metaData"></param>
    /// <param name="db"></param>
    /// <returns></returns>
    private static ERegisterDbStatus RegisterDB(string dbFilePath, DataBaseMetaData metaData, LiteDatabase db)
    {
        var id = metaData.ObjectId;

        if (id == Ulid.Empty)
            return ERegisterDbStatus.NoIdFound;

        if (!Databases.TryAdd(id, db))
            return ERegisterDbStatus.IdAlreadyRegistered;

        if (!DatabaseIds.TryAdd(dbFilePath, id))
            return ERegisterDbStatus.PathAlreadyRegistered;

        return ERegisterDbStatus.Success;
    }

    private static ECreateDbHashStatus CreateDbHash(string dbFilePath)
    {
        var hashFilePath = dbFilePath + DbHashExtension;
        var hash = string.Empty;

        if (!File.Exists(dbFilePath))
            return ECreateDbHashStatus.FileNotFound;

        try
        {

            // Compute hash of database file
            using (var stream = File.OpenRead(dbFilePath))
            {
                using (var sha256 = System.Security.Cryptography.SHA256.Create())
                {
                    var hashBytes = sha256.ComputeHash(stream);
                    hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                }
            }

            File.WriteAllText(hashFilePath, hash);
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to create DB hash for {dbFilePath}: {errorMessage}", args: [dbFilePath, ex.Message]);
            return ECreateDbHashStatus.UnexpectedError;
        }
        return ECreateDbHashStatus.Success;
    }
    
    private static ECheckDbHashStatus CheckDbHash(string dbFilePath)
    {
        var hashFilePath = dbFilePath + DbHashExtension;
        var existingHash = string.Empty;
        var currentHash = string.Empty;

        if (!File.Exists(dbFilePath) || !File.Exists(hashFilePath))
            return ECheckDbHashStatus.FileNotFound;

        try
        {
            existingHash = new string(File.ReadAllText(hashFilePath).Where(c => !char.IsControl(c)).ToArray()).ToLowerInvariant();

            // Compute current hash of database file
            using (var stream = File.OpenRead(dbFilePath))
            {
                using (var sha256 = System.Security.Cryptography.SHA256.Create())
                {
                    var hashBytes = sha256.ComputeHash(stream);
                    currentHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                }
            }

            // Compare hashes
            return existingHash == currentHash ? ECheckDbHashStatus.HashMatch : ECheckDbHashStatus.HashMismatch;
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to check DB hash for {dbFilePath}: {errorMessage}", args: [dbFilePath, ex.Message]);
            return ECheckDbHashStatus.UnexpectedError;
        }
    }
    
    private static string CheckReservedPath(string dbFilePath)
    {
        if(EngineReservedDatabase.TryGetValue(dbFilePath, out var reservedPath))
        {
            var engineDbPath = Path.Combine(AppContext.BaseDirectory, reservedPath);
            return engineDbPath;
        }
        return dbFilePath;
    }
    
}