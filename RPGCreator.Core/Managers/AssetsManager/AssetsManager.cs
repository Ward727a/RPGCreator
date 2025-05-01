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
using RPGCreator.Core.Managers.AssetsManager.EventsArgs;
using RPGCreator.Core.Managers.ProjectsManager.Events;
using RPGCreator.Core.Type.Assets;
using RPGCreator.Core.Type.Assets.BaseAssetsPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.Core.Managers.AssetsManager
{
    public class AssetsManager
    {
        private Dictionary<string, BaseAssetsPack> _loadedPacks = new();
        private Dictionary<Guid, BaseAsset> _cachedAssets = [];

        public AssetsManagerEvent Event;

        public AssetsManager()
        {
            Event = new();

        }

        internal void Init()
        {
            EngineCore.Instance.Managers.Projects.Events.LoadedProject += (object? sender, ProjectsManagerLoadedProjectArgs e) =>
            {
                if (e.LoadedProject != null && !e.HasError)
                {
                    foreach (string packPath in e.LoadedProject.AssetsPackPath)
                    {
                        BaseAssetsPack pack = new(packPath);
                        RegisterPack(pack);
                    }

                }
            };

            EngineCore.Instance.Managers.Projects.Events.UnloadedProject += (object? sender, ProjectsManagerUnloadedProjectArgs e) =>
            {
                var copyPacks = _loadedPacks.ToArray();
                foreach (var pack in copyPacks)
                {
                    UnregisterPack(pack.Key);
                }
            };
        }
        /// <summary>
        /// This should not be used in other part than Core.<br/>
        /// Use the AssetsPackManager for this!
        /// </summary>
        /// <param name="pack"></param>
        public virtual void RegisterPack(BaseAssetsPack pack)
        {
            Event.OnUpdatingAsset();
            var args = new AssetsManagerAddingPackArgs(pack);
            Event.OnAddingPack(args);

            if(args.Cancel)
            {
                return;
            }

            _loadedPacks[pack.Name] = pack;
            foreach (var type in Enum.GetValues(typeof(BaseAsset.TYPE)))
            {
                if (pack.HasAssetOfType((BaseAsset.TYPE)type))
                {
                    Event.OnUpdatedAsset(new AssetsManagerUpdatedAssetArgs((BaseAsset.TYPE)type));
                }
            }
            Event.OnAddedPack(args.ToAddedArgs());
        }

        /// <summary>
        /// This should not be used in other part than Core.<br/>
        /// Use the AssetsPackManager for this!
        /// </summary>
        /// <param name="pack"></param>
        public virtual void UnregisterPack(string packName)
        {

            Event.OnUpdatingAsset();
            var PreArgs = new AssetsManagerRemovingPackArgs(packName);
            Event.OnRemovingPack(PreArgs);

            if (PreArgs.Cancel)
            {
                return;
            }


            if (_loadedPacks.ContainsKey(PreArgs.PackName))
            {
                var PostArgs = PreArgs.ToPost(true);

                foreach (var type in Enum.GetValues(typeof(BaseAsset.TYPE)))
                {
                    if (_loadedPacks[PreArgs.PackName].HasAssetOfType((BaseAsset.TYPE)type))
                    {
                        Event.OnUpdatedAsset(new AssetsManagerUpdatedAssetArgs((BaseAsset.TYPE)type));
                    }
                }
                _loadedPacks.Remove(PreArgs.PackName);
                Event.OnRemovedPack(PostArgs);
            }
            else
            {
                AssetsManagerRemovedPackArgs PostArgs = PreArgs.ToPost(false).SetError(true);
                Event.OnRemovedPack(PostArgs);
            }
            Event.OnUpdatedAsset();
        }

        public virtual BaseAssetsPack[] GetAssetsPacks()
        {
            return [.. _loadedPacks.Values];
        }

        public virtual BaseAsset? GetCachedAsset(Guid guid)
        {
            if (_cachedAssets.TryGetValue(guid, out BaseAsset? asset))
            {
                return asset;
            }
            else
            {
                return null;
            }
        }
        public virtual bool TryGetCachedAsset(Guid guid, out BaseAsset? asset)
        {
            return _cachedAssets.TryGetValue(guid, out asset);
        }

        public virtual void AddCachedAsset(BaseAsset asset)
        {
            if (asset.ShouldBeCached)
            {
                if (!_cachedAssets.ContainsKey(asset.Unique))
                {
                    _cachedAssets.Add(asset.Unique, asset);
                } else
                {
                    throw new InvalidOperationException($"Asset {asset.Name} is already cached.");
                }
            }
            else
            {
                throw new InvalidOperationException($"Asset {asset.Name} is not cacheable.");
            }
        }

        public virtual BaseAsset? GetAsset(string fullPath) // TODO: Add a caching system for the assets?
        {

            AssetsManagerGettingAssetArgs PreArgs = new(fullPath);

            Event.OnGettingAsset(PreArgs);

            if(PreArgs.Cancel)
            {
                return null;
            }

            string[] parts = PreArgs.Fullpath.Split(":"); // Example: "packname:assetpath"
            if (parts.Length != 2)
            {
                Event.OnGotAsset(PreArgs.ToPost().SetError(true, $"Invalid asset path {PreArgs.Fullpath}"));
                return null;
            }

            string packName = parts[0];
            string assetPath = parts[1];

            if (EngineCore.Instance.Managers.AssetsPack.TryGetAssetsPack(packName, out BaseAssetsPack? pack))
            {

                BaseAsset asset = pack!.GetAsset(assetPath);

                if (asset != null)
                {
                    Event.OnGotAsset(PreArgs.ToPost(asset));
                    return asset;
                }
                else
                {
                    Event.OnGotAsset(PreArgs.ToPost().SetError(true, $"Asset {PreArgs.Fullpath} not found in pack {packName}"));
                    return null;
                }
            }
            else
            {
                Event.OnGotAsset(PreArgs.ToPost().SetError(true, $"Pack {packName} not found"));
                return null;
            }
        }

        public virtual void AddAsset(string packname, BaseAsset asset)
        {
            AssetsManagerUpdatingAssetArgs preUpdateArgs = new(asset.Type);
            Event.OnUpdatingAsset(preUpdateArgs);
            AssetsManagerAddingAssetArgs PreArgs = new(packname, asset);
            Event.OnAddingAsset(PreArgs);

            if (EngineCore.Instance.Managers.AssetsPack.TryGetAssetsPack(packname, out BaseAssetsPack? pack))
            {
                if (pack == null)
                {
                    Event.OnAddedAsset(PreArgs.ToPost(false).SetError(true));
                    return;
                }
                pack.AddAsset(asset);
                Event.OnUpdatedAsset(preUpdateArgs.ToPost());
                Event.OnAddedAsset(PreArgs.ToPost(true));
            }
            else
            {
                Event.OnAddedAsset(PreArgs.ToPost(false).SetError(true));
                Event.OnUpdatedAsset(preUpdateArgs.ToPost().SetError(true));
            }
        }

        public virtual void MoveAsset(string oldPath, string newPath)
        {
            AssetsManagerMovingAssetArgs PreArgs = new(oldPath, newPath);
            Event.OnMovingAsset(PreArgs);
            if (PreArgs.Cancel)
                return;
            string[] parts = PreArgs.OldPath.Split(":"); // Example: "packname:assetpath"
            if (parts.Length != 2)
            {
                Event.OnMovedAsset(PreArgs.ToPost(false).SetError(true, $"Invalid asset path {PreArgs.OldPath}"));
                return;
            }
            string packName = parts[0];
            string assetPath = parts[1];
            if (EngineCore.Instance.Managers.AssetsPack.TryGetAssetsPack(packName, out BaseAssetsPack? pack))
            {
                AssetsManagerUpdatingAssetArgs PreUpdateArgs = new(pack.GetAsset(assetPath)?.Type ?? BaseAsset.TYPE.UNKNOWN);
                Event.OnUpdatingAsset(PreUpdateArgs);
                bool result = pack.MoveAsset(assetPath, newPath);
                Event.OnMovedAsset(PreArgs.ToPost(result));
                Event.OnUpdatedAsset(PreUpdateArgs.ToPost().SetError(result));
            }
            else
            {
                Event.OnMovedAsset(PreArgs.ToPost(false).SetError(true, $"No asset pack with name {packName} could be found."));
            }
        }

        public virtual void RemoveAsset(string fullPath)
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
            if (EngineCore.Instance.Managers.AssetsPack.TryGetAssetsPack(packName, out BaseAssetsPack? pack))
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

        public virtual void SwitchAsset(string currentFullPath, string newFullPath)
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

            if (EngineCore.Instance.Managers.AssetsPack.TryGetAssetsPack(currentPackName, out BaseAssetsPack? currentPack) &&
                EngineCore.Instance.Managers.AssetsPack.TryGetAssetsPack(newPackName, out BaseAssetsPack? newPack))
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
