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
using Microsoft.Xna.Framework;
using RPGCreator.Core.Managers.AssetsPackManager.EventsArgs;
using RPGCreator.Core.Type.Assets;
using RPGCreator.Core.Type.Assets.BaseAssetsPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RPGCreator.Core.Managers.AssetsPackManager
{
    [Obsolete("AssetsPackManager is deprecated. Use AssetsManager instead.")]
    public class AssetsPackManager
    {

        readonly Dictionary<string, BaseAssetsPack> AssetsPacks = [];
        readonly AssetsPackManagerEvent Event = new();

        [Obsolete("Need to switch to AssetsManager for this!")]
        public void NewAssetsPack(string name, BaseAssetsPack.PACK_TYPE type)
        {

            var PreArgs = new AssetsPackManagerAddingPackArgs(name, type);
            Event.OnAddingPack(PreArgs);

            if(PreArgs.Cancel)
            {
                return;
            }

            BaseAssetsPack? created_pack;

            switch (type)
            {
                case BaseAssetsPack.PACK_TYPE.PACK:
                    created_pack = new ExternAssetsPack() { Name = name };
                    created_pack.CreateXMLDocument();
                    AssetsPacks.Add(name, created_pack);
                    EngineCore.Instance.Managers.Assets.RegisterPack(AssetsPacks[name]);
                    Event.OnAddedPack(PreArgs.ToPost());
                    break;
                case BaseAssetsPack.PACK_TYPE.PROJECT:
                    created_pack = new ProjectAssetsPack() { Name = name };
                    created_pack.CreateXMLDocument();
                    AssetsPacks.Add(name, created_pack);
                    EngineCore.Instance.Managers.Assets.RegisterPack(AssetsPacks[name]);
                    Event.OnAddedPack(PreArgs.ToPost());
                    break;
                default:
                    Event.OnAddedPack(PreArgs.ToPost().SetError(true, $"Pack type {type} is not supported."));
                    break;
            }
        }

        [Obsolete("Need to switch to AssetsManager for this!")]
        public void LoadAssetsPack(string path)
        {
            if (File.Exists(path))
            {
                try
                {
                    XDocument doc = XDocument.Load(path);

                    var metaElement = doc.Root?.Element("meta");

                    string packName = metaElement?.Element("name")?.Value ?? Path.GetFileNameWithoutExtension(path);

                    if(HasAssetsPack(packName))
                    {
                        //Event.OnLoadedPack(new AssetsPackManagerLoadingPackArgs(packName, path).SetError(true, $"Pack with name {packName} already exist."));
                        return;
                    }

                    BaseAssetsPack.PACK_TYPE packType = metaElement?.Element("type")?.Value switch
                    {
                        "PACK" => BaseAssetsPack.PACK_TYPE.PACK,
                        "PROJECT" => BaseAssetsPack.PACK_TYPE.PROJECT,
                        _ => BaseAssetsPack.PACK_TYPE.UNKNOWN
                    };

                    BaseAssetsPack? pack = null;

                    switch (packType)
                    {
                        case BaseAssetsPack.PACK_TYPE.PACK:
                            pack = new ExternAssetsPack(doc);
                            AssetsPacks.Add(packName, new ExternAssetsPack(doc));
                            EngineCore.Instance.Managers.Assets.RegisterPack(pack);
                            break;
                        case BaseAssetsPack.PACK_TYPE.PROJECT:
                            pack = new ProjectAssetsPack(path, doc);
                            AssetsPacks.Add(packName, new ProjectAssetsPack(path, doc));
                            EngineCore.Instance.Managers.Assets.RegisterPack(pack);
                            break;
                        default:
                            //Event.OnLoadedPack(new AssetsPackManagerLoadingPackArgs(name, path).SetError(true, $"Pack type {packType} is not supported."));
                            return;
                    }
                }
                catch (Exception e)
                {
                    //Event.OnLoadedPack(new AssetsPackManagerLoadingPackArgs(name, path).SetError(true, $"Failed to load pack {name} at path {path}: {e.Message}"));
                    return;
                }
            }
        }

        [Obsolete("Need to switch to AssetsManager for this!")]
        public void ClearAssetsPacks()
        {
            foreach (var pack in AssetsPacks)
            {
                //RemoveAssetsPack(pack.Key);
            }
            AssetsPacks.Clear();
        }
        [Obsolete("Need to switch to AssetsManager for this!")]
        public bool TryGetAssetsPack(string name, out BaseAssetsPack? pack)
        {
            return AssetsPacks.TryGetValue(name, out pack);
        }
        [Obsolete("Need to switch to AssetsManager for this!")]
        public bool TryGetAssetsPack<T>(string name, out T? pack) where T : BaseAssetsPack
        {
            if (AssetsPacks.TryGetValue(name, out BaseAssetsPack? _pack))
            {
                if (_pack is T typedPack)
                {
                    pack = typedPack;
                    return true;
                }
                else
                {
                    pack = default;
                    return false;
                }
            }
            else
            {
                pack = default;
                return false;
            }
        }

        [Obsolete("Need to switch to AssetsManager for this!")]
        public bool HasAssetsPack(string name)
        {
            return AssetsPacks.ContainsKey(name);
        }

        [Obsolete("Need to switch to AssetsManager for this!")]
        public BaseAssetsPack GetAssetsPack(string name)
        {
            if (AssetsPacks.ContainsKey(name))
            {
                return AssetsPacks[name];
            }
            else
            {
                EngineCore.Instance.Events.OnCoreError(new());
                throw new KeyNotFoundException($"Assets pack with name {name} not found.");
            }
        }

        [Obsolete("Need to switch to AssetsManager for this!")]
        public T GetAssetsPack<T>(string name) where T : BaseAssetsPack
        {
            if (AssetsPacks.ContainsKey(name))
            {
                if(AssetsPacks[name] is T)
                    return (T)AssetsPacks[name];
                else
                {
                    EngineCore.Instance.Events.OnCoreError(new());
                    throw new InvalidCastException($"Assets pack with name {name} is not of type {typeof(T).Name}.");
                }
            }
            else
            {
                return null;
            }
        }

        [Obsolete("Need to switch to AssetsManager for this!")]
        public List<string> GetAssetsPacksNames()
        {
            return AssetsPacks.Keys.ToList();
        }

        [Obsolete("Need to switch to AssetsManager for this!")]
        public void RemoveAssetsPack(string name)
        {

            var preArgs = new AssetsPackManagerRemovingPackArgs(name);
            Event.OnRemovingPack(preArgs);

            if (AssetsPacks.ContainsKey(name))
            {
                AssetsPacks.Remove(name);
                Event.OnRemovedPack(preArgs.ToPost());
            }
            else
            {
                Event.OnRemovedPack(preArgs.ToPost().SetError(true, $"Pack with name {name} not found."));
            }
        }
    }
}
