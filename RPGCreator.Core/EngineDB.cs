using LiteDB;
using Serilog;

namespace RPGCreator.Core;

public class DataBaseMetaData()
{
    public int Id; // DB Related ID
    public Ulid ObjectId; // DataBase ID
    public string Name; // DataBase Name
}


public class EngineDB
{
    
    private enum ERegisterDbStatus
    {
        UnexpectedError,
        NoIdFound,
        IdAlreadyRegistered,
        PathAlreadyRegistered,
        Success,
    }

    public enum ECloseDbStatus
    {
        UnexpectedError,
        DbNotFound,
        Success,
    }
    
    private static readonly ILogger Logger = Log.ForContext<EngineDB>();
    
    private static Dictionary<Ulid, LiteDatabase>  Databases { get; } = new Dictionary<Ulid, LiteDatabase>();
    private static Dictionary<string, Ulid> DatabaseIds { get; } = new Dictionary<string, Ulid>();

    public static Ulid OpenDB(string dbFilePath)
    {

        if (File.Exists(dbFilePath))
        {
            var db = new LiteDatabase(dbFilePath);
            
            var metadataCollection = db.GetCollection<DataBaseMetaData>("metadata");

            if (metadataCollection.Count() == 0)
            {
                Logger.Error($"No metadata found for database at {dbFilePath}");
                return Ulid.Empty;
            }

            var metadata = metadataCollection.Query().ToList()[0];

            if (metadata.ObjectId == Ulid.Empty)
            {
                Logger.Error("Found metadata but no id found for database {dbFilePath}({dbMetaName})", dbFilePath, string.IsNullOrWhiteSpace(metadata.Name) ? "NO NAME" : metadata.Name);
                return Ulid.Empty;
            }

            if (RegisterDB(dbFilePath, metadata, db) is var status && status  != ERegisterDbStatus.Success)
            {
                Logger.Error("Failed to register database at {dbFilePath} with code {errorCode}", dbFilePath, status);
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
                Logger.Error("Failed to register database at {dbFilePath} with code {errorCode}", dbFilePath, status);
                return Ulid.Empty;
            }
            
            return metadata.ObjectId;
        }
    }

    public static ECloseDbStatus CloseDB(Ulid dbId)
    {

        var kvp = DatabaseIds.FirstOrDefault(x => x.Value == dbId);
        
        if(kvp.Equals(default(KeyValuePair<string, Ulid>)))
        {
            Logger.Warning("Tried to close database with id {dbId} but it was not found", dbId);
            return ECloseDbStatus.DbNotFound;
        }
        
        if(Databases.ContainsKey(dbId))
        {
            Databases[dbId].Dispose();
            Databases.Remove(dbId);
            DatabaseIds.Remove(kvp.Key);
            return ECloseDbStatus.Success;
        }
        else
        {
            Logger.Warning("Tried to close database with id {dbId} but it was not found", dbId);
            return ECloseDbStatus.DbNotFound;
        }
    }
    
    // HELPERS

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
    
}