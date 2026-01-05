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

using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Diagnostics;
using RPGCreator.Core.Managers.AssetsManager.Factories;
using RPGCreator.Core.Managers.AssetsManager.Registries;
using RPGCreator.Core.Types.Assets.BaseAssetsPack;
using RPGCreator.Core.Types.Map;
using RPGCreator.Core.Types.Map.Layers;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets;
using RPGCreator.SDK.Assets.Definitions.Maps;
using RPGCreator.SDK.Types;
using RPGCreator.SDK.Types.Collections;
using RPGCreator.SDK.Types.Interfaces;
using RPGCreator.SDK.Types.Internals;
using RPGCreator.SDK.Types.Records;
using Serilog;

namespace RPGCreator.Core.Managers.AssetsManager
{
    internal class AssetsManager : IAssetsManager
    {

        private struct AssetLocation
        {
            public IAssetsPack Pack;
            public string RelativePath;
            public string TypeName;
        }
        
        private Dictionary<Ulid, AssetLocation> _assetLocations = new();

        readonly Dictionary<Ulid, IAssetsPack> AssetsPacks = [];
        readonly Dictionary<string, Ulid> AssetsPacksMapping = [];

        public AssetsManagerEvent Event;
        
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

        public event Action<IAssetDef>? OnAssetRegistered;
        public event Action<IAssetDef>? OnAssetUnregistered;

        public void RegisterRegistry(IAssetRegistry registry)
        {
            _registries[registry.ModuleName] = registry;

            foreach (var supportedType in registry.SupportedTypes)
            {
                _registryTypeToName[supportedType] = registry.ModuleName;
            }
        }

        public void RegisterAsset(object asset)
        {
            var type = asset.GetType();
            if (TryResolveRegistry(type, out var assetRegistry))
            {
                assetRegistry.RegisterUntyped((IHasUniqueId)asset, true);
                Guard.IsAssignableToType(asset, typeof(IAssetDef));
                OnAssetRegistered?.Invoke((IAssetDef)asset);
                Log.Information("Registered asset of type {AssetType} in registry {RegistryName}", type.FullName, assetRegistry.ModuleName);
                return;
            }
            Log.Warning("No registry found for asset type {AssetType}", type.FullName);
        }
        
        public void UnregisterAsset(object asset)
        {
            var type = asset.GetType();
            if (TryResolveRegistry(type, out var assetRegistry))
            {
                assetRegistry.UnregisterUntyped((IHasUniqueId)asset);
                Guard.IsAssignableToType(asset, typeof(IAssetDef));
                OnAssetUnregistered?.Invoke((IAssetDef)asset);
                Log.Information("Unregistered asset of type {AssetType} from registry {RegistryName}", type.FullName, assetRegistry.ModuleName);
                return;
            }
            Log.Warning("No registry found for asset type {AssetType}", type.FullName);
        }
        
        public bool TryResolveRegistry(string ModuleName, [NotNullWhen(true)] out IAssetRegistry? registry)
        {
            return _registries.TryGetValue(ModuleName, out registry);
        }
        
        public bool TryResolveRegistry(System.Type type, [NotNullWhen(true)] out IAssetRegistry? registry)
        {
            registry = null;
            if (_registryTypeToName.TryGetValue(type, out var registryName))
            {
                return _registries.TryGetValue(registryName, out registry);
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
        
        [Obsolete("Use 'AssetScope.Load()' instead for better scope management.", false)]
        public bool TryResolveAsset<T>(Ulid uniqueId, [NotNullWhen(true)] out T? result) where T : class, IHasUniqueId
        {
            result = null;
            if (_registryTypeToName.TryGetValue(typeof(T), out var registryName))
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

            if (_assetLocations.TryGetValue(uniqueId, out AssetLocation location))
            {
                try
                {
                    object loadedObject = location.Pack.LoadAsset(uniqueId);

                    RegisterAsset(loadedObject);

                    if (loadedObject is T typedAsset)
                    {
                        result = typedAsset;
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to load asset with ID {AssetID} from pack {PackName}", uniqueId,
                        location.Pack.Name);
                }
            }
            return false;
        }

        public T CreateAsset<T>() where T : IAssetDef, new()
        {
            var newAsset = new T();
            
            RegisterAsset(newAsset);
            
            newAsset.IsDirty = true;
            
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
        
        public T CreateTransientAsset<T>(IAssetScope? scope = null) where T : IAssetDef, new()
        {
            var newAsset = new T();
            
            newAsset.IsTransient = true;
            
            RegisterAsset(newAsset);

            scope?.Track(newAsset);

            Log.Information("Created transient asset of type {AssetType} with ID {AssetID}", typeof(T).FullName, newAsset.Unique);
            
            return newAsset;
        }

        public void DestroyTransientAsset<T>(T asset) where T : IAssetDef
        {
            if (!asset.IsTransient)
            {
                Log.Warning("Attempted to destroy a non-transient asset of type {AssetType} with ID {AssetID}",
                    typeof(T).FullName, asset.Unique);
                return;
            }

            UnregisterAsset(asset);
        }

        public void CommitAsset(IAssetDef asset, string packName, AssetScope? fromScope = null)
        {
            if (!asset.IsTransient)
            {
                Log.Warning("Attempted to commit a non-transient asset of type {AssetType} with ID {AssetID}",
                    asset.GetType().FullName, asset.Unique);
                return;
            }
            
            fromScope?.Untrack(asset);
            
            if (TryGetPack(packName, out var pack))
            {
                asset.IsTransient = false;
                pack.AddOrUpdateAsset(asset);
                Log.Information("Committed transient asset of type {AssetType} with ID {AssetID} to pack {PackName}",
                    asset.GetType().FullName, asset.Unique, packName);
            }
            else
            {
                Log.Warning("No assets pack found with name {PackName}", packName);
            }
        }
        internal object RetainAsset(Ulid id)
        {
            if (_assetLocations.TryGetValue(id, out var location))
            {
                
                Type type = Type.GetType(location.TypeName)!;
                
                if (TryResolveRegistry(type, out var registry))
                {
                    if (registry.TryRetainUntyped(id, out var cachedAsset))
                    {
                        return cachedAsset!;
                    }

                    Log.Information($"Asset {id} not in RAM. Loading from Pack...");
            
                    var loadedAsset = location.Pack.LoadAsset(id);
                    
                    RegisterAsset(loadedAsset); 

                    return loadedAsset;
                }
            }

            throw new Exception($"Asset {id} not found anywhere (RAM or Disk).");
        }

        internal void ReleaseAsset(Ulid id)
        {
            if (_assetLocations.TryGetValue(id, out var location))
            {
                Type type = Type.GetType(location.TypeName)!;
                if (TryResolveRegistry(type, out var registry))
                {
                    registry.ReleaseUntyped(id);
                }
            }
        }
        #endregion
        
        public AssetsManager()
        {
            Event = new();
            
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
            
            EngineState.ProjectState.PropertyChanged += (object? sender, System.ComponentModel.PropertyChangedEventArgs e) =>
            {
                if (e.PropertyName == nameof(IProjectState.CurrentProject))
                {
                    if (EngineState.ProjectState.CurrentProject == null)
                    {
                        var copyPacks = AssetsPacks.ToArray();
                        foreach (var pack in copyPacks)
                        {
                            pack.Value.Dispose();
                        }
                        AssetsPacks.Clear();
                        AssetsPacksMapping.Clear();
                        _assetLocations.Clear();
                        Log.Information("Unloaded all assets packs due to project change.");
                    }
                    else
                    {
                        var loadedProject = EngineState.ProjectState.CurrentProject;
                        // Loading handled in LoadedProject event
                        foreach (string packPath in loadedProject.AssetsPackPath)
                        {
                            try
                            {
                                BaseAssetsPack pack = new(packPath);

                                // RegisterPack(pack, false, false);
                                AssetsPacks[pack.Id] = pack;
                                AssetsPacksMapping[pack.Name] = pack.Id;

                                foreach (var record in pack.EnumerateIndexOnly())
                                {
                                    _assetLocations[record.Id] = new AssetLocation
                                    {
                                        Pack = pack,
                                        RelativePath = record.RelativePath
                                    };
                                }
                            
                                Log.Information("Loaded assets pack from path: {packPath}", packPath);
                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex, "Failed to load assets pack from path: {packPath}", packPath);
                                return;
                            }
                        }
                    }
                }
            };
            Log.Information("AssetsManager initialized.");
        }
        
        #region AssetsPackManagement

        public void AddPack(string dbPath)
        {
            BaseAssetsPack pack = new(dbPath);

            AssetsPacks[pack.Id] = pack;
            AssetsPacksMapping[pack.Name] = pack.Id;

            foreach (var record in pack.EnumerateIndexOnly())
            {
                _assetLocations[record.Id] = new AssetLocation
                {
                    Pack = pack,
                    RelativePath = record.RelativePath,
                    TypeName = record.TypeName
                };
            }
                            
            Log.Information("Loaded assets pack from path: {packPath}", pack.DbFilePath);
        }

        public void RegisterPack(IAssetsPack pack)
        {
            if (AssetsPacks.ContainsKey(pack.Id))
            {
                Log.Warning("Assets pack with ID {PackID} is already registered.", pack.Id);
                return;
            }
            
            AssetsPacks[pack.Id] = pack;

            if (!AssetsPacksMapping.ContainsKey(pack.Name))
            {
                AssetsPacksMapping[pack.Name] = pack.Id;
            }
            else
            {
                Log.Warning("Assets pack with name {PackName} is already registered.", pack.Name);
            }
            
            Log.Information("Pack {PackName} with ID {PackID} registered.", pack.Name, pack.Id);
        }
        
        public void UnregisterPack(Ulid packId)
        {
            if (AssetsPacks.TryGetValue(packId, out IAssetsPack? pack))
            {
                AssetsPacks.Remove(packId);
                AssetsPacksMapping.Remove(pack.Name);
                
                pack.Dispose();
                
                Log.Information("Pack {PackName} with ID {PackID} unregistered.", pack.Name, pack.Id);
            }
            else
            {
                Log.Warning("No assets pack found with ID: {PackID}", packId);
            }
        }
        
        public bool TryGetPack(string? packName, [NotNullWhen(true)] out IAssetsPack? pack)
        {
            pack = null;
            if(packName == null)
                return false;
            if (AssetsPacksMapping.TryGetValue(packName, out Ulid packId))
            {
                return AssetsPacks.TryGetValue(packId, out pack);
            }
            return false;
        }
        
        public bool TryGetPack(Ulid packId, [NotNullWhen(true)] out IAssetsPack? pack)
        {
            return AssetsPacks.TryGetValue(packId, out pack);
        }

        public IAssetsPack GetPack(Ulid packId)
        {
            if (AssetsPacks.TryGetValue(packId, out IAssetsPack? pack))
            {
                return pack;
            }
            throw new KeyNotFoundException($"No assets pack found with ID: {packId}");
        }


        public void AddNewAssetLocation(Ulid assetId, IAssetsPack pack, string relativePath, string typeName)
        {
            _assetLocations[assetId] = new AssetLocation
            {
                Pack = pack,
                RelativePath = relativePath,
                TypeName = typeName
            };
        }

        public List<IAssetsPack> GetLoadedPacks()
        {
            return AssetsPacks.Values.ToList();
        }

        
        /// <summary>
        /// Search all packs for assets of type T.
        /// </summary>
        /// <typeparam name="T"> Type of asset to search for.</typeparam>
        /// <returns> <see cref="IEnumerable{t}"/> of <see cref="PackSearchResult"/> containing the found assets.</returns>
        public IEnumerable<PackSearchResult> SearchAllPacks<T>()
        {
            var targetType = typeof(T);
            foreach (var pack in AssetsPacks.Values)
            {
                foreach (var asset in pack.SearchIndexByType(targetType))
                {
                    yield return new PackSearchResult(asset.Id, pack.Id, asset.TypeName, asset.RelativePath);
                }
            }
        }
        
        #endregion
    }
}
