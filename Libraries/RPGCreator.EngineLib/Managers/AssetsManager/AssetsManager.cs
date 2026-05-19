#region LICENSE

//
// RPG Creator - Open-source RPG Engine.
// (c) 2025 Ward
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
// 
// 

#endregion

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using LiteDB;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets;
using RPGCreator.SDK.Common.Debug;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.Services.EngineService;
using RPGCreator.SDK.Types;
using RPGCreator.SDK.Types.Internals;
using RPGCreator.Shared.Types;

namespace RPGCreator.EngineLib.Managers.AssetsManager
{
    internal class AssetsManager : IAssetsManager
    {
        public class AssetsManagerDb
        {
            private Ulid _currentProjectDb = Ulid.Empty;
            private const string DbName = "AssetsManager.db";
            
            private LiteDatabase? _db = null;
            
            private ILiteCollection<AssetRelation>? _assetRelations = null;
            private ILiteCollection<AssetIndex>? _assetIndexes = null;
            private ILiteCollection<ClassChildren>? _classChildren = null;

            [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
            private class AssetRelation
            {
                [BsonId]
                public Ulid Id { get; set; }
                public List<Ulid> References { get; set; } = new List<Ulid>();
                public List<Ulid> BackRefs { get; set; } = new List<Ulid>();
                
                public AssetRelation() { }
                public AssetRelation(Ulid id) => Id = id;
            }
                
            [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
            private class AssetIndex
            {
                [BsonId]
                public Ulid Id { get; set; }
                public URN ClassUrn { get; set; }
                
                public AssetIndex() { }
                public AssetIndex(Ulid id, URN classUrn) 
                { 
                    Id = id; 
                    ClassUrn = classUrn; 
                }
            }

            private class ClassChildren(URN parentUrn)
            {
                public URN ParentUrn { get; set; } = parentUrn;
                public List<URN> ChildrenUrns { get; set; } = new List<URN>();
            }

            public class AssetsIndexes : IAssetsManager.IAssetIndexes
            {
                private AssetsManagerDb _assetsManagerDb;
                private ILiteCollection<AssetIndex>? AssetIndexes => _assetsManagerDb._assetIndexes;
                private bool IsReady => AssetIndexes != null;
                
                [Obsolete("Do not use this constructor directly, use the AssetsManagerDb created variable for this!")]
                internal AssetsIndexes(AssetsManagerDb assetsManagerDb)
                {
                    _assetsManagerDb = assetsManagerDb;
                }

                public bool Has(Ulid id)
                {
                    if(!IsReady)
                        return false;
                
                    return AssetIndexes!.Exists(x => x.Id == id);
                }

                public Result<List<Ulid>> GetAssetsOfClass(URN classUrn)
                {
                    if(!IsReady)
                        return Result<List<Ulid>>.Failure("Database is not ready");
                    
                    var search = classUrn.ToString();

                    return Result<List<Ulid>>.Success(AssetIndexes!
                        .Find(Query.Or(
                            Query.EQ("ClassUrn", classUrn.ToString()), 
                            Query.StartsWith("ClassUrn", search)
                        ))
                        .Select(x => x.Id)
                        .ToList());
                }

                public Result<URN> GetClassUrn(Ulid id)
                {
                    if(!IsReady)
                        return Result<URN>.Failure("Database is not ready");
                    
                    if(!Has(id))
                        return Result<URN>.Fail($"Asset with ID '{id}' not found in database");
                
                    var assetIndex = AssetIndexes!.FindOne(x => x.Id == id);
                
                    return Result<URN>.Success(assetIndex.ClassUrn);
                }

                public Result AddAsset(IEngineClass asset)
                {
                    if(!IsReady)
                        return Result.Failure("Database is not ready");
                
                    var assetId = asset.Unique;
                    var assetClassUrn = asset.ClassUrn;
                
                    if(Has(assetId))
                        return Result.Fail($"Asset with ID '{assetId}' already exists in database");
                
                    var assetIndex = new AssetIndex(assetId, assetClassUrn);
                    AssetIndexes!.Insert(assetIndex);
                
                    return Result.Success();
                }

                public Result RemoveAsset(Ulid id)
                {
                    if(!IsReady)
                        return Result.Failure("Database is not ready");
                
                    if(!Has(id))
                        return Result.Fail($"Asset with ID '{id}' not found in database");

                    var count = AssetIndexes!.Count(x => x.Id == id);
                
                    if(count > 1)
                        return Result.Fail($"More than one asset with ID '{id}' found in database");
                
                    var deleted = AssetIndexes!.DeleteMany(x => x.Id == id);
                
                    if(deleted <= 0)
                        return Result.Fail($"Failed to delete asset with ID '{id}' from database");

                    return Result.Success();
                }
            }

            public class AssetsRelations : IAssetsManager.IAssetRelations
            {
                private readonly AssetsManagerDb _assetsManagerDb;
                private ILiteCollection<AssetRelation>? AssetRelations => _assetsManagerDb._assetRelations;
                private bool IsReady => AssetRelations != null;
                
                [Obsolete("Do not use this constructor directly, use the AssetsManagerDb created variable for this!")]
                internal AssetsRelations(AssetsManagerDb assetsManagerDb)
                {
                    _assetsManagerDb = assetsManagerDb;
                }
                
                public bool HasRelationData(Ulid id)
                {
                    if (!IsReady) return false;
                    
                    return AssetRelations!.Exists(x => x.Id == id);
                }

                private void _createRelationData(Ulid id)
                {
                    if (HasRelationData(id))
                        return;
                    
                    var relationData = new AssetRelation(id);
                    AssetRelations!.Insert(relationData);
                }

                private AssetRelation GetRelationData(Ulid id)
                {
                    return AssetRelations.Find(x => x.Id == id).FirstOrDefault();
                }

                public List<Ulid> GetReferences(Ulid id)
                {
                    return GetRelationData(id).References;
                }

                public List<Ulid> GetBackRefs(Ulid id)
                {
                    return GetRelationData(id).BackRefs;
                }

                public RelationKind GetRelationBetween(Ulid firstId, Ulid secondId)
                {
                    if (!IsReady)
                        return RelationKind.InternalError;
                    
                    if (firstId == secondId || !HasRelationData(firstId) || !HasRelationData(secondId))
                        return RelationKind.NoRelation;

                    var firstRelation = GetRelationData(firstId);
                    var secondRelation = GetRelationData(secondId);

                    if (firstRelation.References.Contains(secondId) || secondRelation.References.Contains(firstId))
                    {
                        if (firstRelation.References.Contains(secondId) && secondRelation.References.Contains(firstId))
                            return RelationKind.CircularReference;

                        if (firstRelation.References.Contains(secondId))
                            return secondRelation.BackRefs.Contains(firstId)
                                ? RelationKind.FirstReferenceSecond
                                : RelationKind.UnexpectedRelation;

                        if (secondRelation.References.Contains(firstId))
                            return firstRelation.BackRefs.Contains(secondId)
                                ? RelationKind.SecondReferenceFirst
                                : RelationKind.UnexpectedRelation;
                    }

                    return RelationKind.NoRelation;
                }
                
                public Result AddRelation(Ulid id, Ulid referencedAssetId)
                {
                    if(!IsReady)
                        return Result.Failure("Database is not ready");
                    
                    if(!HasRelationData(id))
                        _createRelationData(id);

                    if(!HasRelationData(referencedAssetId))
                        _createRelationData(referencedAssetId);
                    
                    var parentData = GetRelationData(id);
                    var refData = GetRelationData(referencedAssetId);
                    
                    if(parentData.BackRefs.Contains(referencedAssetId))
                        return Result.Failure($"Asset with ID '{id}' already has a back-reference to asset with ID '{referencedAssetId}' - Circular References!");
                    
                    if(parentData.References.Contains(referencedAssetId))
                        return Result.Failure($"Asset with ID '{id}' already has a reference to asset with ID '{referencedAssetId}'");
                    
                    parentData.References.Add(referencedAssetId);
                    refData.BackRefs.Add(id);
                    
                    AssetRelations!.Update(parentData);
                    AssetRelations!.Update(refData);
                    
                    return Result.Success();
                }

                public Result RemoveRelation(Ulid id, Ulid referencedAssetId)
                {
                    if(!IsReady)
                        return Result.Failure("Database is not ready");
                    
                    if(!HasRelationData(id))
                        return Result.Failure($"Asset with ID '{id}' does not exist in database");
                    
                    if(!HasRelationData(referencedAssetId))
                        return Result.Failure($"Asset with ID '{referencedAssetId}' does not exist in database");
                    
                    var parentData = GetRelationData(id);
                    var refData = GetRelationData(referencedAssetId);
                    
                    parentData.References.Remove(referencedAssetId);
                    refData.BackRefs.Remove(id);
                    AssetRelations!.Update(parentData);
                    AssetRelations!.Update(refData);
                    
                    return Result.Success();
                }

                public Result ClearRelations(Ulid id)
                {
                    if(!IsReady)
                        return Result.Failure("Database is not ready");

                    if (!HasRelationData(id))
                        return Result.Ok();

                    var data = GetRelationData(id);

                    var references = data.References.ToList();
                    var backRefs = data.BackRefs.ToList();

                    foreach (var reference in references)
                    {
                        RemoveRelation(id, reference);
                    }

                    foreach (var backRef in backRefs)
                    {
                        RemoveRelation(backRef, id);
                    }
                    
                    return Result.Ok();
                }

                public bool HasRelation(Ulid id, Ulid referencedAssetId) => GetRelationBetween(id, referencedAssetId) is RelationKind.FirstReferenceSecond or RelationKind.SecondReferenceFirst;
            }

            public AssetsIndexes Indexes;
            public AssetsRelations Relations;

            public AssetsManagerDb()
            {
#pragma warning disable CS0618 // Type or member is obsolete
                Indexes = new AssetsIndexes(this);
                Relations = new AssetsRelations(this);
#pragma warning restore CS0618 // Type or member is obsolete
            }
            
            public Result CheckDb()
            {
                if(_currentProjectDb == GlobalStates.ProjectState.CurrentProject?.MetaData.Unique && _db != null)
                {
                    return Result.Success();
                }

                if (GlobalStates.ProjectState.CurrentProject != null &&
                    GlobalStates.ProjectState.CurrentProject.MetaData.Unique != Ulid.Empty)
                {
                    return InitializeDb();
                }
                
                return Result.Failure("Database is not initialized for current project");
            }

            public Result InitializeDb()
            {
                if(GlobalStates.ProjectState.CurrentProject == null)
                    return Result.Failure("No project is currently open");

                var currentProjectData = GlobalStates.ProjectState.CurrentProject.MetaData;
                var projectId = currentProjectData.Unique;
                
                if(projectId == Ulid.Empty)
                    return Result.Failure("Project ID is empty");

                var folder = currentProjectData.Directory;
                
                if(!Directory.Exists(folder))
                    return Result.Failure($"Project folder '{folder}' does not exist");

                var dbPath = RpgEnv.PathFormat.GetDbPath(DbName);

                if (dbPath.IsFailure)
                {
                    return Result.Fail($"Error while trying to get database path for project: {dbPath.Error}");
                }

                if (EngineDB.IsDBOpen(dbPath.Value))
                {
                    _db = EngineDB.GetDB(dbPath.Value);
                }
                else
                {
                    var dbId = EngineDB.OpenDB(dbPath.Value);
                    _db = EngineDB.GetDB(dbId);
                }
                
                _assetIndexes = _db!.GetCollection<AssetIndex>("AssetIndexes");
                _assetRelations = _db!.GetCollection<AssetRelation>("AssetRelations");
                _classChildren = _db!.GetCollection<ClassChildren>("ClassChildren");
                _currentProjectDb = projectId;
                return Result.Success();
            }
        }
        
        private static readonly ScopedLogger Logger = SDK.Common.Logging.Logger.ForContext<AssetsManager>();

        private readonly Dictionary<Ulid, IEngineClass> _loadedAssets = new();
        public IReadOnlyCollection<Ulid> LoadedUids => _loadedAssets.Keys;
        public IReadOnlyCollection<IEngineClass> LoadedAssets => _loadedAssets.Values;
        public IFileStorageService FileStorageService { get; }
        private AssetsManagerDb ManagerDb { get; set; }
        
        public IAssetsManager.IAssetIndexes AssetIndexes => ManagerDb.Indexes;
        public IAssetsManager.IAssetRelations AssetRelations => ManagerDb.Relations;

        public AssetsManager(IFileStorageService storageService)
        {
            FileStorageService = storageService;
            ManagerDb = new AssetsManagerDb();
            ManagerDb.InitializeDb();
        }
        
        public void RefreshAssets()
        {
            _loadedAssets.Clear();
            ManagerDb.InitializeDb();
        }
        
        public override string ToString()
        {
            return $"AssetsManager: {LoadedUids.Count} assets loaded. Storage Type used: {FileStorageService.StorageType}";
        }

        public record struct DeserializationArgument(Ulid Unique, URN ClassUrn, object? Argument);

        public Result<T> Create<T>(URN classUrn, object? argument) where T : class, IEngineClass
        {
            if (!ClassesRegistry.HasUrn(classUrn))
                return Result<T>.Fail($"Class with URN '{classUrn}' not found in registry");

            var engineClass = ClassesRegistry.GetEngineClass(classUrn);

            return engineClass
                .Bind(@class =>
                {
                    if (argument is DeserializationArgument deserializationArg)
                    {
                        return deserializationArg.Argument is not null ? @class.Factory.CreateInstance(deserializationArg.Argument) : @class.Factory.DefaultConstructor();
                    }
                    
                    return argument is null
                        ? @class.Factory.DefaultConstructor()
                        : @class.Factory.CreateInstance(argument);
                })
                .Bind<T>(value =>
                {
                    if (value is T typedValue)
                    {
                        if(argument is DeserializationArgument deserializationArg)
                        {
                            typedValue.Unique = deserializationArg.Unique;
                            typedValue.ClassUrn = deserializationArg.ClassUrn;
                        }
                        else
                        {
                            typedValue.ClassUrn = classUrn;
                            typedValue.Unique = Ulid.NewUlid();
                        }
                        
                        return Result<T>.Success(typedValue);
                    }

                    return Result<T>.Fail(
                        $"Value is not of the correct type! Expected: {typeof(T).FullName}, Got: {value?.GetType().FullName ?? "NULL"}");
                }).OnSuccess(value =>
                {
                    if(value is not IConfig)
                        AssetIndexes.AddAsset(value);
                    AddToCache(value.Unique, value);
                });
        }

        public Result Save<T>(T instance) where T : class, IEngineClass
        {
            if (!ClassesRegistry.HasUrn(instance.ClassUrn))
                return Result.Fail($"Class with URN '{instance.ClassUrn}' not found in registry");

            return ClassesRegistry.GetEngineClass(instance.ClassUrn).Bind(@class =>
            {
                var projectFolderResult = RpgEnv.PathFormat.GetAssetsPath();
                if(projectFolderResult.IsFailure)
                    return Result.Fail($"Error while trying to get project assets folder: {projectFolderResult.Error}");
                var folder = Path.Combine(projectFolderResult.Value, @class.SerializationFolder);
                
                        
                if(!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                return FileStorageService.FormatFilename(instance.Unique.ToString())
                    .Bind(fileName => FileStorageService.MakeValidPath(folder, fileName))
                    .Bind(filePath =>
                    {
                        DebugMemory.Set("filepath", filePath);
                        
                        return FileStorageService.Save(filePath, instance);
                    }).OnSuccess(() =>
                    {
                        Logger.Debug("Asset saved at {path}", args: DebugMemory.Get<string>("filepath") ?? "NONE FILE FOUND");
                        DebugMemory.Unset("filepath");
                        SyncRelation(instance);
                    });
            });
        }

        private void SyncRelation<T>(T instance) where T : class, IEngineClass
        {
            AssetRelations.ClearRelations(instance.Unique);
            
            var type = typeof(T);
            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;

            var props = type.GetProperties(flags)
                .Where(p => IsAssetReference(p.PropertyType));
                
            foreach (var prop in props)
                ProcessRef(prop.GetValue(instance), instance.Unique);
            
            var fields = type.GetFields(flags)
                .Where(f => IsAssetReference(f.FieldType));
            
            foreach (var field in fields)
                ProcessRef(field.GetValue(instance), instance.Unique);

            return;
            
            bool IsAssetReference(Type t) => 
                t.IsGenericType && t.GetGenericTypeDefinition() == typeof(AssetReference<>);

            void ProcessRef(object? value, Ulid sourceId)
            {
                if (value == null) return;
                
                var idProp = value.GetType().GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);
                var targetId = (Ulid)idProp!.GetValue(value)!;

                if (targetId != Ulid.Empty)
                {
                    AssetRelations.AddRelation(sourceId, targetId);
                }
            }
        }

        public Result<T> Load<T>(Ulid id) where T : class, IEngineClass
        {
            if (HasInCache(id))
                return GetFromCache<T>(id);

            var classUrnResult = AssetIndexes.GetClassUrn(id);
            
            if(classUrnResult.IsFailure)
                return Result<T>.Fail(classUrnResult.Error);
            
            var classUrn = classUrnResult.Value;
            
            if(!ClassesRegistry.HasUrn(classUrn))
                return Result<T>.Fail($"Class with type '{classUrn}' not found in registry");
            
            return ClassesRegistry.GetEngineClass(classUrn).Bind<T>(@class =>
            {
                
                var folder = Path.Combine(RpgEnv.PathFormat.GetAssetsPath().Value, @class.SerializationFolder);
                
                return FileStorageService.FormatFilename(id.ToString())
                    .Bind(fileName => FileStorageService.MakeValidPath(folder, fileName))
                    .Bind(filePath => FileStorageService.Load<T>(filePath)).OnSuccess((typedValue) =>
                    {
                        AssetIndexes.AddAsset(typedValue);
                        AddToCache(id, typedValue);
                    });
            });
        }

        public Result<List<Ulid>> GetAssetsOfClass(URN classUrn) => AssetIndexes.GetAssetsOfClass(classUrn);

        /// <summary>
        /// Delete the asset from the folder, indexes, and relations.<br/>
        /// Warning: This operation is irreversible and will PERMANENTLY delete the asset from the disk!<br/>
        /// This does not check if the asset is referenced by any other asset, it will delete the asset regardless of references!<br/>
        /// Please check if the asset is referenced by any other asset before calling this method!
        /// </summary>
        /// <param name="id">The ID of the asset to delete.</param>
        /// <returns>
        /// A <see cref="Result"/> indicating whether the asset was successfully deleted.
        /// </returns>
        public Result Delete(Ulid id)
        {
            if(!AssetIndexes.Has(id))
                return Result.Fail($"Asset with ID '{id}' not found in indexes");
            
            var classUrnResult = AssetIndexes.GetClassUrn(id);
            
            if(classUrnResult.IsFailure)
                return Result.Fail(classUrnResult.Error);
            
            var classUrn = classUrnResult.Value;
            
            return ClassesRegistry.GetEngineClass(classUrn).Bind(@class =>
            {
                var folder = @class.SerializationFolder;
                return FileStorageService.FormatFilename(id.ToString())
                    .Bind(fileName => FileStorageService.MakeValidPath(folder, fileName))
                    .Bind(path => FileStorageService.Delete(path))
                    .OnSuccess(() =>
                    {
                        AssetIndexes.RemoveAsset(id);
                        AssetRelations.ClearRelations(id);
                        RemoveFromCache(id);
                    });
            });
        }
        
        public Result CanDelete(Ulid id)
        {
            if (!AssetRelations.HasRelationData(id)) 
                return Result.Success();

            var relations = AssetRelations.GetBackRefs(id);
            if (relations.Count > 0)
            {
                return Result.Failure($"This asset is used by {relations.Count} other assets.");
            }

            return Result.Success();
        }
        
        public bool Has(Ulid id) => AssetIndexes.Has(id);
        
        private bool HasInCache(Ulid id) => _loadedAssets.ContainsKey(id);
        
        private void RemoveFromCache(Ulid id)
        {
            _loadedAssets.Remove(id);
        }

        private void AddToCache(Ulid id, IEngineClass value)
        {
            _loadedAssets[id] = value;
        }

        private Result<T> GetFromCache<T>(Ulid id) where T : class, IEngineClass
        {
            if(!_loadedAssets.TryGetValue(id, out var value))
                return Result<T>.Fail($"Asset with ID '{id}' not found in cache");
            
            if(value is T typedValue)
                return Result<T>.Success(typedValue);
            
            return Result<T>.Fail($"Asset with ID '{id}' is not of type '{typeof(T).FullName}'");
        }
    }
}