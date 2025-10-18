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
using RPGCreator.Core.Events;
using RPGCreator.Core.Type.Assets.BaseAssetsPack.EventsArgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using RPGCreator.Core.Managers.AssetsManager;
using RPGCreator.Core.Managers.AssetsManager.Registries;
using RPGCreator.Core.Type.Assets.Characters.Stats;
using RPGCreator.Core.Type.Assets.Skills;
using RPGCreator.Core.Type.Assets.Tilesets;
using RPGCreator.Core.Type.Internal;
using Serilog;

namespace RPGCreator.Core.Type.Assets.BaseAssetsPack
{
    
    /*
     * BaseAssetsPack.cs
     * =================
     * This class represents a pack of assets in RPG Creator.
     *
     * DevNote:
     * Right now, the assetsfolder variable is totally fucked, it should be set to the path of the assets folder,
     * but when creating a new pack, it is set successfully, but when loading an existing pack, it is not set (or set to a wrong path / random ID).
     * I NEED to fix this ASAP, because it is a major issue.
     * [Ward727, 26/07/2025]
     */
    
    public class BaseAssetsPack : ISerializable, IDeserializable
    {

        // For now it's quite useless, but it will be used in the future to handle errors when loading assets packs.
        public bool ErrorOnLoad { get; private set; } = false;

        public Dictionary<string, BaseAsset> AssetsCache = [];
        public Dictionary<string, string> AssetsPaths = [];
        public readonly BaseAssetsPackEvents Events = new();

        public string ConfigPath;
        public string AssetsFolder { get; private set; }
        [Obsolete("Need to remove this property, use AssetsCache instead.")]
        public XDocument ConfigDocument;

        // Will probably be removed in the future, as it is not really used anymore.
        public enum PACK_TYPE
        {
            UNKNOWN,
            PROJECT,
            PACK
        }

        public string Name;
        public string? Description;
        public PACK_TYPE Type;
        public Ulid Id = Ulid.NewUlid();

        public BaseAssetsPack()
        {
            ConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RPG Creator", "AssetsPacks",$"{Id}.xml");
            AssetsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RPG Creator", "AssetsPacks", Id.ToString());
        }

        // Constructor used when loading an existing assets pack from a file. Used by the BaseProject class.
        public BaseAssetsPack(string configPath)
        {
            ConfigPath = configPath;
            try
            {
                LoadFromFile();
            }
            catch(Exception ex)
            {
                ErrorOnLoad = true;
                Log.Error($"Error loading assets pack from path {ConfigPath}: {ex.Message}");
                return;
            }

            if(ErrorOnLoad)
            {
                Log.Error($"Error loading assets pack from path {ConfigPath}.");
                return;
            }
        }

        private void LoadFromFile()
        {
            if(string.IsNullOrEmpty(ConfigPath))
            {
                ErrorOnLoad = true;
                throw new Exception("Config path is null.");
            }

            if (!File.Exists(ConfigPath))
            {
                ErrorOnLoad = true;
                throw new Exception($"Config file at path {ConfigPath} doesn't exist.");
            }

            EngineSerializer.Instance.Deserialize(File.ReadAllText(ConfigPath), out object? pack, out System.Type? type);
            if (type != typeof(BaseAssetsPack))
            {
                ErrorOnLoad = true;
                throw new Exception($"Loaded object is not a BaseAssetsPack, but {type?.Name ?? "null"}.");
            }
            if (pack is not BaseAssetsPack assetsPack)
            {
                ErrorOnLoad = true;
                throw new Exception("Loaded object is not a BaseAssetsPack.");
            }
            foreach (var pathPair in assetsPack.AssetsPaths)
            {
                var assetID = pathPair.Key;
                var assetPath = pathPair.Value;
                
                if (string.IsNullOrEmpty(assetID) || string.IsNullOrEmpty(assetPath))
                {
                    ErrorOnLoad = true;
                    throw new Exception($"Asset ID or path is null or empty. ID: {assetID}, Path: {assetPath}");
                }
                if (AssetsCache.ContainsKey(assetID))
                {
                    ErrorOnLoad = true;
                    throw new Exception($"Asset at path {assetPath} already exists in the pack.");
                }

                if (!File.Exists(assetPath))
                {
                    ErrorOnLoad = true;
                    throw new Exception($"Asset file at path {assetPath} doesn't exist.");
                }
                
                EngineSerializer.Instance.Deserialize(File.ReadAllText(assetPath), out var asset, out var assetType);

                if (assetType != null && asset != null)
                {
                    
                    var trueType = assetType switch
                    {
                        _ when assetType == typeof(StatDefinition) => typeof(IStatDef),
                        _ when assetType == typeof(SkillDef) => typeof(ISkillDef),
                        _ when assetType == typeof(GraphSkillEffect) => typeof(ISkillEffect),
                        _ => assetType
                    };
                    
                    if (!AssetsManager.AssetMapping.ContainsKey(trueType))
                    {
                        
                        Log.Warning("No asset mapping found for type {AssetType}. Skipping asset at path {AssetPath}.", trueType.Name, assetPath);
                        continue;
                    }
                    AssetsManager.AssetMapping[trueType](asset);
                    AssetsPaths[assetID] = assetPath;
                }
                else
                {
                    Log.Error("Failed to deserialize asset at path {AssetPath}. Type or asset is null.", assetPath);
                }
                //
                // if (!typeof(BaseAsset).IsAssignableFrom(assetType))
                // {
                //     ErrorOnLoad = true;
                //     throw new Exception($"Loaded asset is not a valid asset, but {assetType?.Name}.");
                // }
                //
                // if(asset is BaseAsset baseAsset)
                // {
                //     AssetsCache[assetID] = baseAsset;
                //     AssetsPaths[assetID] = assetPath;
                //     Console.WriteLine($"Loaded asset of type {baseAsset.Type} with ID {baseAsset.Unique} from path {assetPath}.");
                //     baseAsset.Pack = this;
                // }
                // else
                // {
                //     ErrorOnLoad = true;
                //     throw new Exception($"Loaded asset is not a BaseAsset, but {assetType.Name} (weird, should not happen here?).");
                // }
            }
            Name = assetsPack.Name;
            Description = assetsPack.Description;
            Type = assetsPack.Type;
            Id = assetsPack.Id;
            ConfigPath = assetsPack.ConfigPath;
            AssetsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RPG Creator", "AssetsPacks", Id.ToString());
            if (Directory.Exists(AssetsFolder)) return;
            
            try
            {
                Directory.CreateDirectory(AssetsFolder);
            } catch (Exception ex)
            {
                ErrorOnLoad = true;
                throw new Exception($"Error creating assets folder at path {AssetsFolder}.", ex);
            }
        }

        public BaseAsset? GetAsset(string path)
        {
            var preArgs = new BaseAssetsPackGettingArgs(path);
            Events.OnAssetGetting(preArgs);

            if(preArgs.Cancel)
                return null;

            if(AssetsCache.TryGetValue(path, out BaseAsset? asset))
            {
                Events.OnAssetGot(preArgs.ToPost(asset));
                return asset;
            }

            Events.OnAssetGot(preArgs.ToPost(null).SetError(true, $"Asset at path {path} doesnt' exist."));

            return null;
        }

        public bool HasAssetOfType(BaseAsset.TYPE type)
        {
            return AssetsCache.Values.Any(asset => asset.Type == type);
        }

        public void UpdateAsset<T>(T asset) where T : BaseAsset
        {
            if (asset == null)
            {
                throw new ArgumentNullException(nameof(asset), "Asset cannot be null.");
            }

            asset.Save();
        }

        /// <summary>
        /// Old system, need to be updated or removed depending on usage.
        /// </summary>
        /// <param name="oldPath"></param>
        /// <param name="newPath"></param>
        /// <returns></returns>
        public bool MoveAsset(string oldPath, string newPath)
        {

            var preArgs = new BaseAssetsPackMovingArgs(oldPath, newPath);
            Events.OnAssetMoving(preArgs);
            if (preArgs.Cancel)
                return false;
            if (!AssetsCache.TryGetValue(oldPath, out BaseAsset? asset))
            {
                Events.OnAssetMoved(preArgs.ToPost(asset).SetError(true, $"Asset at path {oldPath} doesn't exist."));
                return false;
            }
            AssetsCache.Remove(oldPath);
            asset.PackPath = newPath;
            AssetsCache.Add(newPath, asset);
            Events.OnAssetMoved(preArgs.ToPost(asset));
            return true;
        }

        public bool RemoveAsset(string path)
        {
            var preArgs = new BaseAssetsPackRemovingArgs(path);
            Events.OnAssetRemoving(preArgs);
            if (preArgs.Cancel)
                return false;
            if (!AssetsCache.Remove(path, out BaseAsset? asset))
            {
                Events.OnAssetRemoved(preArgs.ToPost().SetError(true, $"Asset at path {path} doesn't exist."));
                return false;
            }
            Events.OnAssetRemoved(preArgs.ToPost(asset));
            return true;
        }

        public bool RemoveAsset(BaseAsset asset)
        {
            var preArgs = new BaseAssetsPackRemovingArgs(asset.PackPath);

            Events.OnAssetRemoving(preArgs);
            if (preArgs.Cancel)
                return false;
            if (!AssetsCache.Remove(asset.PackPath, out BaseAsset? _))
            {
                Events.OnAssetRemoved(preArgs.ToPost().SetError(true, $"Asset at path {asset.PackPath} doesn't exist."));
                return false;
            }
            Save();
            EngineCore.Instance.Managers.Assets.Event.OnRemovedAsset(new(asset, true));
            Events.OnAssetRemoved(preArgs.ToPost(asset));
            return true;
        }

        public void AddAsset<T>(T asset) where T : IHasUniqueId, IHasSavePath
        {
            // Old way, need to be updated or removed depending on if the global events are kept or not.
            // var PreArgs = new BaseAssetsPackAddingArgs(asset.Unique.ToString(), asset);
            // Events.OnAssetAdding(PreArgs);
            
            // if(PreArgs.Cancel)
                // return;

            // if (!AssetsCache.TryAdd(asset.Unique.ToString(), asset))
            // {
            //     // Events.OnAssetAdded(PreArgs.ToPost().SetError(true, $"Asset at path {asset.Unique} already exist."));
            //     return;
            // }

            if (string.IsNullOrEmpty(asset.SavePath))
            {
                asset.SavePath = Path.Combine(AssetsFolder, $"{asset.Unique}.xml");
                Log.Warning("Asset {AssetName} had no save path, setting it to {SavePath}.", asset is IHasUniqueId ba ? ba.Urn : "Unknown", asset.SavePath);
            }
            AssetsPaths[asset.Unique.ToString()] = asset.SavePath;

            // Events.OnAssetAdded(PreArgs.ToPost());
        }

        [Obsolete("This method should not be used. Use AddAsset<T>(T Asset) method instead.")]
        public void AddAsset<T>(string path, T asset) where T : BaseAsset
        {
            var PreArgs = new BaseAssetsPackAddingArgs(path, asset);
            Events.OnAssetAdding(PreArgs);

            if (PreArgs.Cancel)
                return;

            if (string.IsNullOrEmpty(asset.PackName))
            {
                asset.PackName = Name;
            }
            else if (asset.PackName != Name)
            {
                Events.OnAssetAdded(PreArgs.ToPost().SetError(true, $"Asset {asset.Name} doesn't belong to this pack (Belong to: {asset.PackName}."));
                return;
            }

            asset.PackPath = path;

            if (!AssetsCache.TryAdd(path, asset))
            {
                Events.OnAssetAdded(PreArgs.ToPost().SetError(true, $"Asset at path {path} already exist."));
                return;
            }

            asset.Save();

            Events.OnAssetAdded(PreArgs.ToPost());
        }

        [Obsolete("This method should not be used. Use Save() method instead.")]
        public void CreateXMLDocument()
        {
        }

        /// <summary>
        /// Save the assets pack to his config file.<br/>
        /// This method will serialize the pack with <see cref="EngineSerializer"/> and save it to the <see cref="ConfigPath"/>.<br/>
        /// It will also save each asset in the pack to their respective file.
        /// </summary>
        /// <exception cref="InvalidOperationException">If the <see cref="ConfigPath"/> is not set.</exception>
        public void Save()
        {
            EngineSerializer.Instance.Serialize(this, out string packStringData, false);
            if (string.IsNullOrEmpty(ConfigPath))
            {
                throw new InvalidOperationException("Config path is not set. Cannot save assets pack.");
            }
            if (!Directory.Exists(Path.GetDirectoryName(ConfigPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath)!);
            }
            File.WriteAllText(ConfigPath, packStringData);
            
            // Save each asset in the pack
            foreach (var asset in AssetsCache.Values)
            {
                asset.Save();
            }
        }

        public SerializationInfo GetObjectData()
        {
            SerializationInfo info = new SerializationInfo(typeof(BaseAssetsPack));
            info.AddValue("ConfigPath", ConfigPath);
            info.AddValue("Name", Name);
            info.AddValue("Description", Description ?? "");
            info.AddValue("Type", Type);
            info.AddValue("Id", Id);
            info.AddValue("Assets", AssetsPaths);
            info.AddValue("AssetsFolder", AssetsFolder);
            return info;
        }

        public void SetObjectData(DeserializationInfo info)
        {
            info.TryGetValue("ConfigPath", out ConfigPath, "", "Field 'ConfigPath' not found in serialization info.");
            info.TryGetValue("Name", out Name, "", "Field 'Name' not found in serialization info.");
            info.TryGetValue("Description", out Description, "");
            info.TryGetValue("Type", out Type, PACK_TYPE.UNKNOWN, "Field 'Type' not found in serialization info.");
            info.TryGetValue("Id", out Id, Ulid.NewUlid(), "Field 'Id' not found in serialization info.");
            info.TryGetDictionary("Assets", out AssetsPaths);
            info.TryGetValue("AssetsFolder", out string folder,  Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RPG Creator", "AssetsPacks", Id.ToString()), "Field 'AssetsFolder' not found in serialization info.");
            AssetsFolder = folder;
            
            if (Directory.Exists(AssetsFolder)) return;
            
            try
            {
                Directory.CreateDirectory(AssetsFolder);
            } catch (Exception ex)
            {
                ErrorOnLoad = true;
                throw new Exception($"Error creating assets folder at path {AssetsFolder}.", ex);
            }
        }
    }
}
