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

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Diagnostics;
using RPGCreator.Core.Managers.AssetsManager.Factories;
using RPGCreator.Core.Managers.AssetsManager.Registries;
using RPGCreator.Core.Types.Assets.BaseAssetsPack;
using RPGCreator.Core.Types.Map;
using RPGCreator.Core.Types.Map.Layers;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets;
using RPGCreator.SDK.Assets.Definitions;
using RPGCreator.SDK.Assets.Definitions.Maps;
using RPGCreator.SDK.Assets.Definitions.Maps.Layers;
using RPGCreator.SDK.EngineService;
using RPGCreator.SDK.Exceptions;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Types;
using RPGCreator.SDK.Types.Collections;
using RPGCreator.SDK.Types.Internals;
using RPGCreator.SDK.Types.Records;

namespace RPGCreator.Core.Managers.AssetsManager
{
    internal class AssetsManager : IAssetsManager
    {

        private static readonly ScopedLogger Logger = SDK.Logging.Logger.ForContext<AssetsManager>();
        
        private struct AssetLocation()
        {
            public IAssetsPack? Pack;
            public string RelativePath;
            public string TypeName;

            /// <summary>
            /// Determines whether the asset is transient (not saved to disk).
            /// </summary>
            public bool IsTransient = false;
        }
        
        private readonly Dictionary<Ulid, AssetLocation> _assetLocations = new();

        readonly Dictionary<Ulid, IAssetsPack> _assetsPacks = [];
        private readonly Dictionary<string, Ulid> _assetsPacksMapping = [];

        
        #region Registries
        
        private readonly Dictionary<string, IAssetRegistry> _registries = new();
        private readonly Dictionary<System.Type, string> _registryTypeToName = new();
        
        #endregion
        
        #region Factories
        
        public GenericPooledFactory<TileLayerInstance, TileLayerDefinition> TileLayerFactory = new();
        public GenericCachedFactory<MapInstance, IMapDef> MapFactory = new();
        public TilesetFactory TilesetFactory { get; } = new();
        public TileFactory TileFactory { get; } = new();
        public StatFactory StatFactory { get; } = new();
        
        #endregion
        
        #region RegistryHelpers

        public event Action<IBaseAssetDef>? OnAssetRegistered;
        public event Action<IBaseAssetDef>? OnAssetUnregistered;

        public void RegisterRegistry(IAssetRegistry registry)
        {
            _registries[registry.ModuleName] = registry;

            foreach (var supportedType in registry.SupportedTypes)
            {
                _registryTypeToName[supportedType] = registry.ModuleName;
            }
            
            Logger.Debug("Registered asset registry {RegistryName} for types: {SupportedTypes}", args: [registry.ModuleName, string.Join(", ", registry.SupportedTypes.Select(t => t.FullName))]);
        }

        public void RegisterAsset(object asset)
        {
            Guard.IsAssignableToType(asset, typeof(IHasUniqueId));
            Guard.IsAssignableToType(asset, typeof(IBaseAssetDef));

            if (asset is IHasUniqueId uniqueIdAsset)
            {

                if(uniqueIdAsset.Unique == Ulid.Empty)
                {
                    uniqueIdAsset.Init(Ulid.NewUlid());
                    Logger.Warning("Registered asset of type {AssetType} had an empty Unique ID. A new ID has been generated: {NewID}", args: [asset.GetType().FullName, uniqueIdAsset.Unique]);
                }
            }
            
            var type = asset.GetType();
            if (TryResolveRegistry(type, out var assetRegistry))
            {
                assetRegistry.RegisterUntyped((IHasUniqueId)asset, true);
                Guard.IsAssignableToType(asset, typeof(IBaseAssetDef));
                OnAssetRegistered?.Invoke((IBaseAssetDef)asset);
                Logger.Info("Registered asset {unique} ({URN}) of type {AssetType} in registry {RegistryName}",  args: [((IHasUniqueId)asset).Unique, ((IHasUniqueId)asset).Urn, type.FullName, assetRegistry.ModuleName]);
                return;
            }
            
            // Check if the asset has a inheritance relationship with any of the supported types of the registries
            var inherited = type.BaseType;

            if (inherited != null)
            {
                if (TryResolveRegistry(inherited, out var inheritedRegistry))
                {
                    Guard.IsAssignableToType(asset, typeof(IHasUniqueId));
                    inheritedRegistry.RegisterUntyped((IHasUniqueId)asset, true);
                    Guard.IsAssignableToType(asset, typeof(IBaseAssetDef));
                    OnAssetRegistered?.Invoke((IBaseAssetDef)asset);
                    Logger.Info("Registered asset of type {AssetType} in registry {RegistryName} with inherited type {inherited}",  args: [type.FullName, inheritedRegistry.ModuleName, inherited.FullName]);
                    return;
                }
            }
            
            Logger.Warning("No registry found for asset type {AssetType} - inherited: {inherited}", args: [type.FullName, inherited?.FullName ?? "NONE"]);
        }
        
        public void UnregisterAsset(object asset)
        {
            var type = asset.GetType();
            if (TryResolveRegistry(type, out var assetRegistry))
            {
                assetRegistry.UnregisterUntyped((IHasUniqueId)asset);
                Guard.IsAssignableToType(asset, typeof(IBaseAssetDef));
                OnAssetUnregistered?.Invoke((IBaseAssetDef)asset);
                Logger.Info("Unregistered asset of type {AssetType} from registry {RegistryName}", args:[type.FullName, assetRegistry.ModuleName]);
                return;
            }
            Logger.Warning("No registry found for asset type {AssetType}", args: type.FullName);
        }

        public bool TryResolveRegistry(string moduleName, [NotNullWhen(true)] out IAssetRegistry? registry)
        {
            return _registries.TryGetValue(moduleName, out registry);
        }
        
        public bool TryResolveRegistry(Type type, [NotNullWhen(true)] out IAssetRegistry? registry)
        {
            registry = null;
            if (type == null)
            {
                Logger.Critical("TryResolveRegistry called with null type.");
                return false;
            }
            
            if (_registryTypeToName.TryGetValue(type, out var registryName))
            {
                return _registries.TryGetValue(registryName, out registry);
            }
            return false;
        }

        public bool TryResolveRegistry<T>(string moduleName, [NotNullWhen(true)] out T? registry) where T : IAssetRegistry
        {
            registry = default;
            if (_registries.TryGetValue(moduleName, out var _registry) && _registry is T typedRegistry)
            {
                registry = typedRegistry;
                return true;
            }
            return false;
        }
        
        public bool TryResolveRegistry<T>(System.Type type, [NotNullWhen(true)] out T? registry) where T : IAssetRegistry
        {
            registry = default;

            if (type == null)
            {
                Logger.Critical("TryResolveRegistry called with null type.");
                return false;
            }
            
            if (_registryTypeToName.TryGetValue(type, out var registryName))
            {
                if (_registries.TryGetValue(registryName, out var _registry) && _registry is T typedRegistry)
                {
                    registry = typedRegistry;
                    return true;
                }
            }
            return false;
        }
        
        [Obsolete("Use 'AssetScope.Load()' instead for better scope management.")]
        public bool TryResolveAsset<T>(URN urn, [NotNullWhen(true)] out T? result) where T : class, IHasUniqueId
        {
            result = null;
            if (_registryTypeToName.TryGetValue(typeof(T), out var registryName))
            {
                if (_registries.TryGetValue(registryName, out var registry))
                {
                    if (registry.TryResolveUrnUntyped(urn, out var asset))
                    {
                        result = asset as T;
                        return result != null;
                    }
                }
            }
            return false;
        }

        public string GetRegistryFromTypeOrInherited(Type type)
        {
            if (_registryTypeToName.TryGetValue(type, out var registryName))
            {
                return registryName;
            }
            
            var baseType = type.BaseType;
            
            if(baseType != null)
            {
                if (_registryTypeToName.TryGetValue(baseType, out registryName))
                {
                    return registryName;
                }
            }

            return "";
        }
        
        [Obsolete("Use 'AssetScope.Load()' instead for better scope management.", false)]
        public bool TryResolveAsset<T>(Ulid uniqueId, [NotNullWhen(true)] out T? result) where T : class, IHasUniqueId
        {
            result = null;
            var registryName = GetRegistryFromTypeOrInherited(typeof(T));
            if (!string.IsNullOrEmpty(registryName))
            {
                if (_registries.TryGetValue(registryName, out var registry))
                {
                    if (registry.TryGetUntyped(uniqueId, out var asset))
                    {
                        result = asset as T;
                        return result != null;
                    }
                }
            }
            else
            {
                Logger.Error("No registry found for asset type {AssetType} or its direct inherited type.", args: typeof(T).FullName);
            }

            if (_assetLocations.TryGetValue(uniqueId, out AssetLocation location))
            {
                try
                {
                    object? loadedObject = location.Pack?.LoadAssetDirect(location.RelativePath);

                    if(loadedObject == null)
                    {
                        Logger.Error("Failed to load asset with ID {AssetID} from pack {PackName}: LoadAssetDirect returned null.", args:[uniqueId,
                            location.Pack.Name]);
                        return false;
                    }
                    RegisterAsset(loadedObject);

                    if (loadedObject is T typedAsset)
                    {
                        result = typedAsset;
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Failed to load asset with ID {AssetID} from pack {PackName}", args:[uniqueId,
                        location.Pack.Name]);
                }
            }
            return false;
        }

        public T CreateAsset<T>() where T : IBaseAssetDef, IHasUniqueId, new()
        {
            var newAsset = new T();
            
            newAsset.IsDirty = true;
            newAsset.Init(Ulid.NewUlid());
            
            RegisterAsset(newAsset);
            newAsset.ResumeTracking();
            
            Logger.Debug("Created asset of type {AssetType} with ID {AssetID}", args:[typeof(T).FullName, newAsset.Unique]);
            
            return newAsset;
        }

        public IAssetScope CreateAssetScope(string? name = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                name = Ulid.NewUlid().ToString();
            }
            return new AssetScope(this, name);
        }
        
        public T CreateTransientAsset<T>(IAssetScope? scope = null) where T : IBaseAssetDef, new()
        {
            var typeKey = RegistryServices.AssetTypeRegistry.GetKey(typeof(T));
            if(typeKey == null)
            {
                Logger.Error("Cannot create transient asset of type {AssetType} because it is not registered in the AssetTypeRegistry.", args: typeof(T).FullName);
                return new T();
            }
            var newAsset = new T();
            
            newAsset.IsTransient = true;
            
            RegisterAsset(newAsset);
            
            newAsset.IsDirty = true;
            newAsset.Init(Ulid.NewUlid());
            AddNewAssetLocation(newAsset.Unique, null, "", typeKey, true);
            scope?.Track(newAsset);

            Logger.Info("Created transient asset of type {AssetType} with ID {AssetID}", args: [typeof(T).FullName, newAsset.Unique]);
            
            return newAsset;
        }

        public void DestroyTransientAsset<T>(T asset) where T : IBaseAssetDef
        {
            if (!asset.IsTransient)
            {
                Logger.Warning("Attempted to destroy a non-transient asset of type {AssetType} with ID {AssetID}",
                    args:[typeof(T).FullName, asset.Unique]);
                return;
            }

            UnregisterAsset(asset);
        }

        public void CommitAsset(BaseAssetDef baseAsset, string packName, AssetScope? fromScope = null)
        {
            if (!baseAsset.IsTransient)
            {
                Logger.Warning("Attempted to commit a non-transient asset of type {AssetType} with ID {AssetID}",
                    args: [baseAsset.GetType().FullName, baseAsset.Unique]);
                return;
            }
            
            fromScope?.Untrack(baseAsset);
            
            if (TryGetPack(packName, out var pack))
            {
                baseAsset.IsTransient = false;
                pack.AddOrUpdateAsset(baseAsset);
                Logger.Info("Commited transient asset of type {AssetType} with ID {AssetID} to pack {PackName}",
                    args:[baseAsset.GetType().FullName, baseAsset.Unique, packName]);
            }
            else
            {
                Logger.Warning("No assets pack found with name {PackName}", args: packName);
            }
        }
        
        /// <summary>
        /// Retains an asset in memory. If the asset is not already loaded in RAM, it will be loaded from the appropriate Assets Pack.
        /// </summary>
        /// <param name="id">The unique ID of the asset to retain.</param>
        /// <returns>>The retained asset object.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the asset is not found in RAM or on disk.</exception>
        internal object RetainAsset(Ulid id)
        {
            if (_assetLocations.TryGetValue(id, out var location))
            {
                
                Type? type = RegistryServices.AssetTypeRegistry.GetType(location.TypeName);
                if(type == null)
                    type = Type.GetType(location.TypeName)!;
                
                if (TryResolveRegistry(type, out var registry))
                {
                    if (registry.TryRetainUntyped(id, out var cachedAsset))
                    {
                        return cachedAsset!;
                    }

                    if (location.IsTransient)
                    {
                        Logger.Error("Attempted to retain a transient asset with ID {AssetID} which is not loaded in RAM.", args: id);
                        throw new CriticalEngineException($"Transient asset with ID {id} is not loaded in RAM. How did it get inside the _assetLocations without being registered?", _assetLocations);
                    }
                    Logger.Info($"Asset {id} not in RAM. Loading from Pack...");
            
                    var loadedAsset = location.Pack.LoadAsset(id);
                    
                    RegisterAsset(loadedAsset); 

                    return loadedAsset;
                }
                if (!TryResolveRegistry(type, out _))
                {
                    throw new CriticalEngineException($"No registry found for asset type '{type.FullName}'. Did you forget to register the AssetRegistry for this type?", _registries);
                }
                throw new CriticalEngineException($"Asset with ID {id} not found in RAM or on disk. How did it get inside the _assetLocations without being registered?", _assetLocations);
            }
            
            throw new KeyNotFoundException($"No asset found with ID: {id}");
        }

        internal void ReleaseAsset(Ulid id)
        {
            if (_assetLocations.TryGetValue(id, out var location))
            {
                Type? type = RegistryServices.AssetTypeRegistry.GetType(location.TypeName);
                
                if(type == null)
                    type = Type.GetType(location.TypeName);
                
                if(type == null)
                {
                    Logger.Warning("Unable to determine type for asset ID {AssetID} with type name {TypeName}", args: [id, location.TypeName]);
                    return;
                }
                
                if (TryResolveRegistry(type, out var registry))
                {
                    registry.ReleaseUntyped(id);
                }
            }
        }
        #endregion
        
        public AssetsManager()
        {
            
            // Register default registries
            RegisterRegistry(new SkillEffectsRegistry());
            RegisterRegistry(new TilesetRegistry());
            RegisterRegistry(new CharacterRegistry());
            RegisterRegistry(new MapRegistry());
            RegisterRegistry(new SkillsRegistry());
            RegisterRegistry(new StatsRegistry());
            RegisterRegistry(new AnimationRegistry());
            RegisterRegistry(new SpriteSheetRegistry());
        }

        internal void Init()
        {
            if (TryResolveRegistry("skill_effects", out var registry) && registry is SkillEffectsRegistry skillEffectsRegistry)
            {   
                skillEffectsRegistry.ReloadData();
            }
            
            GlobalStates.ProjectState.PropertyChanged += (object? sender, System.ComponentModel.PropertyChangedEventArgs e) =>
            {
                if (e.PropertyName == nameof(IProjectState.CurrentProject))
                {
                    if (GlobalStates.ProjectState.CurrentProject == null)
                    {
                        var copyPacks = _assetsPacks.ToArray();
                        foreach (var pack in copyPacks)
                        {
                            pack.Value.Dispose();
                        }
                        _assetsPacks.Clear();
                        _assetsPacksMapping.Clear();
                        _assetLocations.Clear();
                        Logger.Info("Unloaded all assets packs due to project change.");
                    }
                    else
                    {
                        var loadedProject = GlobalStates.ProjectState.CurrentProject;
                        // Loading handled in LoadedProject event
                        foreach (string packPath in loadedProject.AssetsPackPath)
                        {
                            try
                            {
                                BaseAssetsPack pack = new(packPath);

                                // RegisterPack(pack, false, false);
                                _assetsPacks[pack.Id] = pack;
                                _assetsPacksMapping[pack.Name] = pack.Id;

                                foreach (var record in pack.EnumerateIndexOnly())
                                {
                                    _assetLocations[record.Id] = new AssetLocation
                                    {
                                        Pack = pack,
                                        RelativePath = record.RelativePath
                                    };
                                }
                            
                                Logger.Info("Loaded assets pack from path: {packPath}", args: packPath);
                            }
                            catch (Exception ex)
                            {
                                Logger.Error(ex, "Failed to load assets pack from path: {packPath}", args: packPath);
                                return;
                            }
                        }
                    }
                }
            };
            Logger.Info("AssetsManager initialized.");
        }
        
        #region AssetsPackManagement

        public void AddPack(string dbPath)
        {
            BaseAssetsPack pack = new(dbPath);

            _assetsPacks[pack.Id] = pack;
            _assetsPacksMapping[pack.Name] = pack.Id;

            foreach (var record in pack.EnumerateIndexOnly())
            {
                _assetLocations[record.Id] = new AssetLocation
                {
                    Pack = pack,
                    RelativePath = record.RelativePath,
                    TypeName = record.TypeName
                };
            }
                            
            Logger.Info("Loaded assets pack from path: {packPath}", args: pack.DbFilePath);
        }

        public void RegisterPack(IAssetsPack pack)
        {
            if (_assetsPacks.ContainsKey(pack.Id))
            {
                Logger.Warning("Assets pack with ID {PackID} is already registered.", args: pack.Id);
                return;
            }
            
            _assetsPacks[pack.Id] = pack;

            if (!_assetsPacksMapping.ContainsKey(pack.Name))
            {
                _assetsPacksMapping[pack.Name] = pack.Id;
            }
            else
            {
                Logger.Warning("Assets pack with name {PackName} is already registered.", pack.Name);
            }
            
            Logger.Info("Pack {PackName} with ID {PackID} registered.", args:[pack.Name, pack.Id]);
        }
        
        public void UnregisterPack(Ulid packId)
        {
            if (_assetsPacks.TryGetValue(packId, out IAssetsPack? pack))
            {
                _assetsPacks.Remove(packId);
                _assetsPacksMapping.Remove(pack.Name);
                
                pack.Dispose();
                
                Logger.Info("Pack {PackName} with ID {PackID} unregistered.", args: [pack.Name, pack.Id]);
            }
            else
            {
                Logger.Warning("No assets pack found with ID: {PackID}", args: packId);
            }
        }

        public IAssetsPack GetDefaultPack()
        {
            if(_assetsPacksMapping.TryGetValue("assets_pack", out Ulid packId))
                return _assetsPacks[packId];
            throw new CriticalEngineException("Default assets pack with name 'assets_pack' not found. Make sure it is included in the project and loaded correctly.",
                (_assetsPacksMapping, _assetsPacks));
        }
        
        public bool TryGetPack(string? packName, [NotNullWhen(true)] out IAssetsPack? pack)
        {
            pack = null;
            if(packName == null)
                return false;
            if (_assetsPacksMapping.TryGetValue(packName, out Ulid packId))
            {
                return _assetsPacks.TryGetValue(packId, out pack);
            }
            return false;
        }
        
        public bool TryGetPack(Ulid packId, [NotNullWhen(true)] out IAssetsPack? pack)
        {
            return _assetsPacks.TryGetValue(packId, out pack);
        }

        public IAssetsPack GetPack(Ulid packId)
        {
            if (_assetsPacks.TryGetValue(packId, out IAssetsPack? pack))
            {
                return pack;
            }
            throw new KeyNotFoundException($"No assets pack found with ID: {packId}");
        }


        public void AddNewAssetLocation(Ulid assetId, IAssetsPack? pack, string relativePath, string typeName, bool isTransient = false)
        {
            _assetLocations[assetId] = new AssetLocation
            {
                Pack = pack,
                RelativePath = relativePath,
                TypeName = typeName,
                IsTransient = isTransient
            };
        }

        public List<IAssetsPack> GetLoadedPacks()
        {
            return _assetsPacks.Values.ToList();
        }

        
        /// <summary>
        /// Search all packs for assets of type T.
        /// </summary>
        /// <typeparam name="T"> Type of asset to search for.</typeparam>
        /// <returns> <see cref="IEnumerable{t}"/> of <see cref="PackSearchResult"/> containing the found assets.</returns>
        public IEnumerable<PackSearchResult> SearchAllPacks<T>()
        {
            var targetType = typeof(T);
            foreach (var pack in _assetsPacks.Values)
            {
                foreach (var asset in pack.SearchIndexByType(targetType))
                {
                    yield return new PackSearchResult(asset.Id, pack.Id, asset.TypeName, asset.RelativePath);
                }
            }
        }

        public IEnumerable<T> GetFromAllPacks<T>()
        {
            var list = new List<T>();
            foreach (var pack in _assetsPacks.Values)
            {
                list.AddRange(pack.LoadAssetsByType<T>());
            }
            return list;
        }
        
        public IEnumerable<T> GetAssetsOfType<T>() where T : class, IBaseAssetDef, IHasUniqueId
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            foreach (var result in SearchAllPacks<T>())
            {
                if (TryResolveAsset(result.AssetId, out T? asset))
                {
                    yield return asset;
                }
            }
            stopwatch.Stop();
            Logger.Info("Retrieved all assets of type {AssetType} in {ElapsedMilliseconds} ms", args: [typeof(T).FullName, stopwatch.ElapsedMilliseconds]);
        }

        public IEnumerable<T> GetAssetsOfType<T>(T valueForType) where T : class, IBaseAssetDef, IHasUniqueId
        {
            return GetAssetsOfType<T>();
        }

        #endregion
    }
}
