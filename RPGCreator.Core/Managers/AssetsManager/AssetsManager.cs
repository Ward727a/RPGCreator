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
using RPGCreator.Core.Managers.AssetsManager.EventsArgs;
using RPGCreator.Core.Managers.AssetsManager.Factories;
using RPGCreator.Core.Managers.AssetsManager.Registries;
using RPGCreator.Core.Managers.ProjectsManager.Events;
using RPGCreator.Core.Types.Assets;
using RPGCreator.Core.Types.Assets.BaseAssetsPack;
using RPGCreator.Core.Types.Internal;
using RPGCreator.Core.Types.Map;
using Serilog;

namespace RPGCreator.Core.Managers.AssetsManager
{
    public class AssetsManager
    {
        private Dictionary<Ulid, BaseAsset> _cachedAssets = [];

        readonly Dictionary<Ulid, BaseAssetsPack> AssetsPacks = [];
        readonly Dictionary<string, Ulid> AssetsPacksMapping = [];

        public AssetsManagerEvent Event;
        
        #region Registries
        
        private readonly Dictionary<string, IAssetRegistry> _registries = new();
        private readonly Dictionary<System.Type, string> _registryTypeToName = new();
        
        #endregion
        
        #region Factories
        
        public GenericPooledFactory<TileLayerInstance, TileLayerDefinition> TileLayerFactory = new();
        public GenericCachedFactory<MapInstance, MapDefinition> MapFactory = new();
        public TilesetFactory TilesetFactory { get; } = new();
        public TileFactory TileFactory { get; } = new();
        public StatFactory StatFactory { get; } = new();
        
        #endregion
        
        #region RegistryHelpers

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
                Log.Information("Registered asset of type {AssetType} in registry {RegistryName}", type.FullName, assetRegistry.ModuleName);
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
            return false;
        }

        public T CreateAsset<T>() where T : IAssetDef, new()
        {
            var newAsset = new T();
            
            RegisterAsset(newAsset);
            
            newAsset.IsDirty = true;
            
            return newAsset;
        }

        public AssetScope CreateAssetScope(string name = "Unnamed Asset Scope")
        {
            return new AssetScope(this, name);
        }
        
        public T CreateTransientAsset<T>(AssetScope? scope = null) where T : IAssetDef, new()
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

            if (TryResolveRegistry(asset.GetType(), out var assetRegistry))
            {
                assetRegistry.UnregisterUntyped(asset);
                Log.Information("Destroyed transient asset of type {AssetType} with ID {AssetID}",
                    typeof(T).FullName, asset.Unique);
            }
            else
            {
                Log.Warning("No registry found for asset type {AssetType}", typeof(T).FullName);
            }
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
            
            EngineCore.Instance.Managers.Projects.Events.LoadedProject += (object? sender, ProjectsManagerLoadedProjectArgs e) =>
            {
                if (e.LoadedProject != null && !e.HasError)
                {
                    foreach (string packPath in e.LoadedProject.AssetsPackPath)
                    {
                        try
                        {
                            BaseAssetsPack pack = new(packPath);

                            // RegisterPack(pack, false, false);
                            AssetsPacks[pack.Id] = pack;
                            AssetsPacksMapping[pack.Name] = pack.Id;
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "Failed to load assets pack from path: {packPath}", packPath);
                            return;
                        }
                    }
                }
            };

            EngineCore.Instance.Managers.Projects.Events.UnloadedProject += (object? sender, ProjectsManagerUnloadedProjectArgs e) =>
            {
                var copyPacks = AssetsPacks.ToArray();
                foreach (var pack in copyPacks)
                {
                    pack.Value.Dispose();
                }
                AssetsPacks.Clear();
                AssetsPacksMapping.Clear();
            };
            Log.Information("AssetsManager initialized.");
        }
        
        #region AssetsPackManagement

        public void AddPack(string dbPath)
        {
            BaseAssetsPack pack = new(dbPath);

            AssetsPacks[pack.Id] = pack;
            AssetsPacksMapping[pack.Name] = pack.Id;
        }

        public void RegisterPack(BaseAssetsPack pack)
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
            if (AssetsPacks.TryGetValue(packId, out BaseAssetsPack? pack))
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
        
        public bool TryGetPack(string packName, [NotNullWhen(true)] out BaseAssetsPack? pack)
        {
            pack = null;
            if (AssetsPacksMapping.TryGetValue(packName, out Ulid packId))
            {
                return AssetsPacks.TryGetValue(packId, out pack);
            }
            return false;
        }
        
        public bool TryGetPack(Ulid packId, [NotNullWhen(true)] out BaseAssetsPack? pack)
        {
            return AssetsPacks.TryGetValue(packId, out pack);
        }

        public BaseAssetsPack GetPack(Ulid packId)
        {
            if (AssetsPacks.TryGetValue(packId, out BaseAssetsPack? pack))
            {
                return pack;
            }
            throw new KeyNotFoundException($"No assets pack found with ID: {packId}");
        }

        public List<BaseAssetsPack> GetLoadedPacks()
        {
            return AssetsPacks.Values.ToList();
        }

        public IEnumerable<(Ulid id, Ulid packId, string typeName, string path)> SearchAllPacks<T>()
        {
            var targetType = typeof(T);
            foreach (var pack in AssetsPacks.Values)
            {
                foreach (var asset in pack.SearchIndex((record) => record.TypeName == targetType.FullName))
                {
                    yield return (asset.Id, pack.Id, asset.TypeName, asset.RelativePath);
                }
            }
        }
        
        #endregion
        
        //
        // public bool CreateAssetsPack(string packName, BaseAssetsPack.PACK_TYPE type)
        // {
        //     if (string.IsNullOrEmpty(packName))
        //     {
        //         throw new ArgumentException("Pack name cannot be null or empty.", nameof(packName));
        //     }
        //     if (AssetsPacksMapping.ContainsKey(packName)) // Need to check how to allow same name for different packs.
        //     {
        //         throw new InvalidOperationException($"Assets pack with name {packName} already exists.");
        //     }
        //
        //     BaseAssetsPack pack = new()
        //     {
        //         Name = packName,
        //         Type = type
        //     };
        //     pack.CreateXMLDocument();
        //     RegisterPack(pack);
        //
        //     return true;
        // }
        //
        // public bool HasAssetsPack(string pack_name)
        // {
        //     if (string.IsNullOrEmpty(pack_name))
        //     {
        //         throw new ArgumentException("Pack name cannot be null or empty.", nameof(pack_name));
        //     }
        //     return AssetsPacksMapping.ContainsKey(pack_name);
        // }
        //
        // public List<string> GetAssetsPackNames()
        // {
        //     return AssetsPacksMapping.Keys.ToList();
        // }
        //
        // /// <summary>
        // /// This should not be used in other part than Core.<br/>
        // /// Use the AssetsPackManager for this!
        // /// </summary>
        // /// <param name="pack"></param>
        // public void RegisterPack(BaseAssetsPack pack, bool shouldSaveConfig = true, bool shouldSaveInProject = true)
        // {
        //     Event.OnUpdatingAsset();
        //     var args = new AssetsManagerAddingPackArgs(pack);
        //     Event.OnAddingPack(args);
        //
        //     if(args.Cancel)
        //     {
        //         return;
        //     }
        //
        //     AssetsPacks[pack.Id] = pack;
        //     AssetsPacksMapping[pack.Name] = pack.Id;
        //     
        //     foreach (var type in Enum.GetValues(typeof(BaseAsset.TYPE)))
        //     {
        //         if (pack.HasAssetOfType((BaseAsset.TYPE)type))
        //         {
        //             Event.OnUpdatedAsset(new AssetsManagerUpdatedAssetArgs((BaseAsset.TYPE)type, null));
        //         }
        //     }
        //
        //     if(shouldSaveInProject)
        //         EngineCore.Instance.Data.EditedProject?.AssetsPackPath.Add(pack.ConfigPath);
        //
        //     if(shouldSaveConfig)
        //         EngineCore.Instance.Data.EditedProject?.Save();
        //
        //     Event.OnAddedPack(args.ToAddedArgs());
        // }
        //
        // public void LoadPack(string packPath)
        // {
        //
        //     if(string.IsNullOrEmpty(packPath))
        //     {
        //         throw new ArgumentException("Pack path cannot be null or empty.", nameof(packPath));
        //     }
        //
        //     if(!File.Exists(packPath))
        //     {
        //         throw new FileNotFoundException($"Assets pack file not found at path: {packPath}");
        //     }
        //
        //     BaseAssetsPack pack = new(packPath);
        //
        //     if(pack.ErrorOnLoad)
        //     {
        //         throw new InvalidOperationException($"Failed to load assets pack from path: {packPath}.");
        //     }
        //
        //     RegisterPack(pack, false, false);
        // }
        //
        // public void UnregisterPack(Ulid packId, string pack_name = "")
        // {
        //     Event.OnUpdatingAsset();
        //     var PreArgs = new AssetsManagerRemovingPackArgs(packId);
        //     Event.OnRemovingPack(PreArgs);
        //
        //     if (PreArgs.Cancel)
        //     {
        //         return;
        //     }
        //
        //     if (AssetsPacks.TryGetValue(PreArgs.PackID, out BaseAssetsPack? pack))
        //     {
        //         var PostArgs = PreArgs.ToPost(true);
        //
        //         foreach (var type in Enum.GetValues(typeof(BaseAsset.TYPE)))
        //         {
        //             if (pack.HasAssetOfType((BaseAsset.TYPE)type))
        //             {
        //                 Event.OnUpdatedAsset(new AssetsManagerUpdatedAssetArgs((BaseAsset.TYPE)type, null));
        //             }
        //         }
        //
        //         pack.Save();
        //
        //         if(string.IsNullOrEmpty(pack_name))
        //         {
        //             pack_name = pack.Name;
        //         }
        //
        //         AssetsPacks.Remove(PreArgs.PackID);
        //         AssetsPacksMapping.Remove(pack_name);
        //
        //         Event.OnRemovedPack(PostArgs);
        //     }
        //     else
        //     {
        //         AssetsManagerRemovedPackArgs PostArgs = PreArgs.ToPost(false).SetError(true);
        //         Event.OnRemovedPack(PostArgs);
        //     }
        //     Event.OnUpdatedAsset();
        // }
        //
        // /// <summary>
        // /// This should not be used in other part than Core.<br/>
        // /// Use the AssetsPackManager for this!
        // /// </summary>
        // /// <param name="pack"></param>
        // public void UnregisterPack(string packName)
        // {
        //
        //     if(string.IsNullOrEmpty(packName))
        //     {
        //         throw new ArgumentException("Pack name cannot be null or empty.", nameof(packName));
        //     }
        //
        //     if(AssetsPacksMapping.TryGetValue(packName, out Ulid packId))
        //     {
        //         UnregisterPack(packId, packName);
        //     }
        //     else
        //     {
        //         throw new KeyNotFoundException($"No assets pack found with name: {packName}");
        //     }
        //
        // }
        //
        // public void ClearAssetsPacks()
        // {
        //     var copyPacks = AssetsPacks.ToArray();
        //     foreach (var pack in copyPacks)
        //     {
        //         UnregisterPack(pack.Key);
        //     }
        // }
        //
        // public BaseAssetsPack[] GetAssetsPacks()
        // {
        //     return [.. AssetsPacks.Values];
        // }
        //
        // public bool TryGetAssetsPack(Ulid packId, out BaseAssetsPack? pack)
        // {
        //     return AssetsPacks.TryGetValue(packId, out pack);
        // }
        //
        // public bool TryGetAssetsPack(string packName, out BaseAssetsPack? pack)
        // {
        //     if (AssetsPacksMapping.TryGetValue(packName, out Ulid packId))
        //     {
        //         return AssetsPacks.TryGetValue(packId, out pack);
        //     }
        //     else
        //     {
        //         pack = null;
        //         return false;
        //     }
        // }
        //
        // public BaseAsset? GetCachedAsset(Ulid ulid)
        // {
        //     if (_cachedAssets.TryGetValue(ulid, out BaseAsset? asset))
        //     {
        //         return asset;
        //     }
        //     else
        //     {
        //         return null;
        //     }
        // }
    }
}
