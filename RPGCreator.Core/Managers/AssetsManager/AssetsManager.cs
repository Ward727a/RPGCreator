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

using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using RPGCreator.Core.Managers.AssetsManager.EventsArgs;
using RPGCreator.Core.Managers.AssetsManager.Factories;
using RPGCreator.Core.Managers.AssetsManager.Registries;
using RPGCreator.Core.Managers.ProjectsManager.Events;
using RPGCreator.Core.Type.Assets;
using RPGCreator.Core.Type.Assets.BaseAssetsPack;
using RPGCreator.Core.Type.Assets.Characters.Stats;
using RPGCreator.Core.Type.Assets.Skills;
using RPGCreator.Core.Type.Assets.Tilesets;
using RPGCreator.Core.Type.Internal;
using RPGCreator.Core.Type.Map;
using Serilog;

namespace RPGCreator.Core.Managers.AssetsManager
{
    public class AssetsManager
    {
        private Dictionary<Ulid, BaseAsset> _cachedAssets = [];

        public static readonly IReadOnlyDictionary<System.Type, Action<object>> AssetMapping =
            new ReadOnlyDictionary<System.Type, Action<object>>(
                new Dictionary<System.Type, Action<object>>
                {
                    { typeof(MapDefinition),obj => { if (obj is MapDefinition map)   EngineCore.Instance.Managers.Assets.MapRegistry.Register(map); } },
                    { typeof(TilesetDef), obj => { if (obj is TilesetDef tileset) EngineCore.Instance.Managers.Assets.TilesetRegistry.Register(tileset); } },
                    { typeof(IStatDef),   obj => { if (obj is IStatDef stat)      EngineCore.Instance.Managers.Assets.StatsRegistry.Register(stat, true); } },
                    { typeof(ISkillDef), obj => { if (obj is ISkillDef skill)    EngineCore.Instance.Managers.Assets.SkillRegistry.Register(skill, true); } },
                }
            );

        readonly Dictionary<Ulid, BaseAssetsPack> AssetsPacks = [];
        readonly Dictionary<string, Ulid> AssetsPacksMapping = [];

        public AssetsManagerEvent Event;
        
        #region Registries

        public StatsRegistry StatsRegistry { get; } = new();
        public MapRegistry MapRegistry { get; } = new();
        public TilesetRegistry TilesetRegistry { get; } = new();
        public SkillsRegistry SkillRegistry { get; } = new();
        public SkillEffectsRegistry SkillEffectsRegistry { get; } = new();
        
        #endregion
        
        #region Factories
        
        public GenericPooledFactory<TileLayerInstance, TileLayerDefinition> TileLayerFactory = new();
        public GenericCachedFactory<MapInstance, MapDefinition> MapFactory = new();
        public TilesetFactory TilesetFactory { get; } = new();
        public TileFactory TileFactory { get; } = new();
        public StatFactory StatFactory { get; } = new();
        
        #endregion
        
        public AssetsManager()
        {
            Event = new();
        }
        
        public bool TryResolveStats(Ulid statId, out IStatDef? stat)
        {
            if (StatsRegistry.TryGet(statId, out stat))
            {
                return true;
            }
            else
            {
                Log.Error("Stat with ID {statId} not found in the registry.", statId);
                stat = null;
                return false;
            }
        }
        public bool TryResolveStats(URN statUrn, out IStatDef? stat)
        {
            if (StatsRegistry.TryGetUrn(statUrn, out stat))
            {
                return true;
            }
            else
            {
                Log.Error("Stat with URN {statUrn} not found in the registry.", statUrn);
                stat = null;
                return false;
            }
        }

        internal void Init()
        {
            
            SkillEffectsRegistry.ReloadData();
            
            EngineCore.Instance.Managers.Projects.Events.LoadedProject += (object? sender, ProjectsManagerLoadedProjectArgs e) =>
            {
                if (e.LoadedProject != null && !e.HasError)
                {
                    foreach (string packPath in e.LoadedProject.AssetsPackPath)
                    {
                        try
                        {
                            LoadPack(packPath);
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
                    UnregisterPack(pack.Key);
                }
            };
            Log.Information("AssetsManager initialized.");
        }

        public bool CreateAssetsPack(string packName, BaseAssetsPack.PACK_TYPE type)
        {
            if (string.IsNullOrEmpty(packName))
            {
                throw new ArgumentException("Pack name cannot be null or empty.", nameof(packName));
            }
            if (AssetsPacksMapping.ContainsKey(packName)) // Need to check how to allow same name for different packs.
            {
                throw new InvalidOperationException($"Assets pack with name {packName} already exists.");
            }

            BaseAssetsPack pack = new()
            {
                Name = packName,
                Type = type
            };
            pack.CreateXMLDocument();
            RegisterPack(pack);

            return true;
        }

        public bool HasAssetsPack(string pack_name)
        {
            if (string.IsNullOrEmpty(pack_name))
            {
                throw new ArgumentException("Pack name cannot be null or empty.", nameof(pack_name));
            }
            return AssetsPacksMapping.ContainsKey(pack_name);
        }

        public List<string> GetAssetsPackNames()
        {
            return AssetsPacksMapping.Keys.ToList();
        }

        /// <summary>
        /// This should not be used in other part than Core.<br/>
        /// Use the AssetsPackManager for this!
        /// </summary>
        /// <param name="pack"></param>
        public void RegisterPack(BaseAssetsPack pack, bool shouldSaveConfig = true, bool shouldSaveInProject = true)
        {
            Event.OnUpdatingAsset();
            var args = new AssetsManagerAddingPackArgs(pack);
            Event.OnAddingPack(args);

            if(args.Cancel)
            {
                return;
            }

            AssetsPacks[pack.Id] = pack;
            AssetsPacksMapping[pack.Name] = pack.Id;
            
            foreach (var type in Enum.GetValues(typeof(BaseAsset.TYPE)))
            {
                if (pack.HasAssetOfType((BaseAsset.TYPE)type))
                {
                    Event.OnUpdatedAsset(new AssetsManagerUpdatedAssetArgs((BaseAsset.TYPE)type, null));
                }
            }

            if(shouldSaveInProject)
                EngineCore.Instance.Data.EditedProject?.AssetsPackPath.Add(pack.ConfigPath);

            if(shouldSaveConfig)
                EngineCore.Instance.Data.EditedProject?.Save();

            Event.OnAddedPack(args.ToAddedArgs());
        }

        public void LoadPack(string packPath)
        {

            if(string.IsNullOrEmpty(packPath))
            {
                throw new ArgumentException("Pack path cannot be null or empty.", nameof(packPath));
            }

            if(!File.Exists(packPath))
            {
                throw new FileNotFoundException($"Assets pack file not found at path: {packPath}");
            }

            BaseAssetsPack pack = new(packPath);

            if(pack.ErrorOnLoad)
            {
                throw new InvalidOperationException($"Failed to load assets pack from path: {packPath}.");
            }

            RegisterPack(pack, false, false);
        }

        public void UnregisterPack(Ulid packId, string pack_name = "")
        {
            Event.OnUpdatingAsset();
            var PreArgs = new AssetsManagerRemovingPackArgs(packId);
            Event.OnRemovingPack(PreArgs);

            if (PreArgs.Cancel)
            {
                return;
            }

            if (AssetsPacks.TryGetValue(PreArgs.PackID, out BaseAssetsPack? pack))
            {
                var PostArgs = PreArgs.ToPost(true);

                foreach (var type in Enum.GetValues(typeof(BaseAsset.TYPE)))
                {
                    if (pack.HasAssetOfType((BaseAsset.TYPE)type))
                    {
                        Event.OnUpdatedAsset(new AssetsManagerUpdatedAssetArgs((BaseAsset.TYPE)type, null));
                    }
                }

                pack.Save();

                if(string.IsNullOrEmpty(pack_name))
                {
                    pack_name = pack.Name;
                }

                AssetsPacks.Remove(PreArgs.PackID);
                AssetsPacksMapping.Remove(pack_name);

                Event.OnRemovedPack(PostArgs);
            }
            else
            {
                AssetsManagerRemovedPackArgs PostArgs = PreArgs.ToPost(false).SetError(true);
                Event.OnRemovedPack(PostArgs);
            }
            Event.OnUpdatedAsset();
        }

        /// <summary>
        /// This should not be used in other part than Core.<br/>
        /// Use the AssetsPackManager for this!
        /// </summary>
        /// <param name="pack"></param>
        public void UnregisterPack(string packName)
        {

            if(string.IsNullOrEmpty(packName))
            {
                throw new ArgumentException("Pack name cannot be null or empty.", nameof(packName));
            }

            if(AssetsPacksMapping.TryGetValue(packName, out Ulid packId))
            {
                UnregisterPack(packId, packName);
            }
            else
            {
                throw new KeyNotFoundException($"No assets pack found with name: {packName}");
            }

        }

        public void ClearAssetsPacks()
        {
            var copyPacks = AssetsPacks.ToArray();
            foreach (var pack in copyPacks)
            {
                UnregisterPack(pack.Key);
            }
        }

        public BaseAssetsPack[] GetAssetsPacks()
        {
            return [.. AssetsPacks.Values];
        }

        public bool TryGetAssetsPack(Ulid packId, out BaseAssetsPack? pack)
        {
            return AssetsPacks.TryGetValue(packId, out pack);
        }

        public bool TryGetAssetsPack(string packName, out BaseAssetsPack? pack)
        {
            if (AssetsPacksMapping.TryGetValue(packName, out Ulid packId))
            {
                return AssetsPacks.TryGetValue(packId, out pack);
            }
            else
            {
                pack = null;
                return false;
            }
        }

        public BaseAsset? GetCachedAsset(Ulid ulid)
        {
            if (_cachedAssets.TryGetValue(ulid, out BaseAsset? asset))
            {
                return asset;
            }
            else
            {
                return null;
            }
        }
        public bool TryGetCachedAsset(Ulid ulid, out BaseAsset? asset)
        {
            return _cachedAssets.TryGetValue(ulid, out asset);
        }

        public bool TryResolveAsset<T>(string urnString, [NotNullWhen(true)] out T? returnValue)
        {

            returnValue = default;
            var hasParsed = URN.TryParse(urnString, out URN? result);
            TryResolveAsset(result.GetValueOrDefault(), out returnValue);
            return hasParsed;
        }

        public bool TryResolveAsset<T>(URN urn, [NotNullWhen(true)] out T? result)
        {
            result = default;
            
            object? obj = urn.Module switch
            {
                "tileset" => TilesetRegistry.GetUrn(urn),
                "map" => MapRegistry.GetUrn(urn),
                "stat" => StatsRegistry.GetUrn(urn),
                _ => null
            };

            if (obj is T typedObj)
            {
                result = typedObj;
                return true;
            }

            return false;
        }

        public bool TryRemoveAsset(string urnString)
        {
            if (URN.TryParse(urnString, out URN? urn))
            {
                return TryRemoveAsset(urn.GetValueOrDefault());
            }
            Log.Error("Invalid URN format: {urnString}", urnString);
            return false;
        }

        public bool TryRemoveAsset(URN urn)
        {
            var resolved= TryResolveAsset<object>(urn, out var value);

            if (resolved)
            {
                switch (urn.Module)
                {
                    case "map":
                        EngineCore.Instance.Managers.Assets.MapRegistry.Unregister(value as IMapDef);
                        return true;
                    case "tileset":
                        EngineCore.Instance.Managers.Assets.TilesetRegistry.Unregister(value as ITilesetDef);
                        return true;
                    case "stat":
                        EngineCore.Instance.Managers.Assets.StatsRegistry.Unregister(value as IStatDef);
                        return true;
                    default:
                        Log.Error("Unsupported asset type: {assetType}", value.GetType());
                        return false;
                }
            }
            Log.Error("Failed to resolve asset with URN: {urn}", urn);
            return false;
        }

        public void RemoveAsset(string fullPath)
        {
            AssetsManagerRemovingAssetArgs PreArgs = new(fullPath);
            Event.OnRemovingAsset(PreArgs);
            if (PreArgs.Cancel)
                return;
            string[] parts = PreArgs.Path.Split(":"); // Example: "packname:assetpath"
            if (parts.Length != 2)
            {
                Event.OnRemovedAsset(PreArgs.ToPost(false).SetError(true, $"Invalid asset path {PreArgs.Path}"));
                return;
            }
            string packName = parts[0];
            string assetPath = parts[1];
            if (TryGetAssetsPack(packName, out BaseAssetsPack? pack))
            {

                AssetsManagerUpdatingAssetArgs PreUpdateArgs = new(pack.GetAsset(assetPath)?.Type ?? BaseAsset.TYPE.UNKNOWN);
                Event.OnUpdatingAsset(PreUpdateArgs);

                bool result = pack.RemoveAsset(assetPath);
                Event.OnRemovedAsset(PreArgs.ToPost(result));

                Event.OnUpdatedAsset(PreUpdateArgs.ToPost().SetError(result));
            }
            else
            {
                Event.OnRemovedAsset(PreArgs.ToPost(false).SetError(true, $"No asset pack with name {packName} could be found."));
            }
        }

        public void SwitchAsset(string currentFullPath, string newFullPath)
        {

            AssetsManagerSwitchingAssetArgs PreArgs = new(currentFullPath, newFullPath);
            Event.OnSwitchingAsset(PreArgs);

            if (PreArgs.Cancel)
                return;

            string[] currentParts = PreArgs.CurrentPath.Split(":"); // Example: "packname:assetpath"
            string[] newParts = PreArgs.NewPath.Split(":"); // Example: "packname:assetpath"

            if (currentParts.Length != 2 || newParts.Length != 2)
            {
                Event.OnSwitchedAsset(PreArgs.ToPost(null).SetError(true, $"Invalid asset path {PreArgs.CurrentPath} or {PreArgs.NewPath}"));
                return;
            }

            string currentPackName = currentParts[0];
            string currentAssetPath = currentParts[1];
            string newPackName = newParts[0];
            string newAssetPath = newParts[1];

            if (TryGetAssetsPack(currentPackName, out BaseAssetsPack? currentPack) &&
                TryGetAssetsPack(newPackName, out BaseAssetsPack? newPack))
            {
                BaseAsset? asset = currentPack!.GetAsset(currentAssetPath);
                if (asset != null)
                {
                    AssetsManagerUpdatingAssetArgs PreUpdateArgs = new(asset.Type);
                    Event.OnUpdatingAsset(PreUpdateArgs);
                    asset.PackName = newPackName;
                    newPack!.AddAsset(newAssetPath, asset);
                    currentPack.RemoveAsset(currentAssetPath);
                    Event.OnSwitchedAsset(PreArgs.ToPost(asset));
                    Event.OnUpdatedAsset(PreUpdateArgs.ToPost());
                }
                else
                {
                    Event.OnSwitchedAsset(PreArgs.ToPost(null).SetError(true, $"Asset {PreArgs.CurrentPath} not found in pack {currentPackName}"));
                    return;
                }
            }
            else
            {
                Event.OnSwitchedAsset(PreArgs.ToPost(null).SetError(true, $"No asset pack with name {currentPackName} or {newPackName} could be found."));
            }
            Event.OnUpdatedAsset();

        }
    }
}
