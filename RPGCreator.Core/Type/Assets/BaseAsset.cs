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
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static RPGCreator.Core.Type.Assets.AssetCategoryAttribute;
using static RPGCreator.Core.Type.Assets.BaseAsset;

namespace RPGCreator.Core.Type.Assets
{

    [AttributeUsage(AttributeTargets.Field)]
    public class AssetCategoryAttribute : Attribute
    {
        public enum AssetCategoryType
        {
            Unknown,
            Image,
            Audio,
            Script
        }
        public AssetCategoryType Category { get; }

        public AssetCategoryAttribute(AssetCategoryType category)
        {
            Category = category;
        }
    }

    // This class and the one above should be probably moved to a more appropriate location
    public static class AssetsTypeHelper
    {
        public static AssetCategoryType GetCategory(this TYPE type)
        {
            var member = typeof(TYPE).GetMember(type.ToString()).FirstOrDefault();
            var attr = member?.GetCustomAttributes(typeof(AssetCategoryAttribute), false).FirstOrDefault() as AssetCategoryAttribute;
            return attr?.Category ?? AssetCategoryType.Unknown;
        }

        public static bool IsImageCompatible(this TYPE type)
        {
            return type.GetCategory() == AssetCategoryType.Image;
        }
    }

    public class BaseAsset : ObservableObject
    {
        /// <summary>
        /// Indicates if the asset should be cached or not.<br/>
        /// True if the asset should be cached, false otherwise.<br/>
        /// </summary>
        public virtual bool ShouldBeCached => false;

        public bool IsCached { get; internal set; } = false;

        public enum TYPE
        {
            [AssetCategory(AssetCategoryType.Unknown)]
            UNKNOWN,
            [AssetCategory(AssetCategoryType.Image)]
            IMAGE,
            [AssetCategory(AssetCategoryType.Image)]
            TILESETS,
        }

        public Guid Unique { get; protected set; }

        public string PackName;
        public string PackPath;

        public XElement AssetData;

        public string Name { get; set; }
        public TYPE Type;

        public BaseAsset()
        {
            Unique = Guid.NewGuid();
            Name = "UNKNOWN";
            Type = TYPE.UNKNOWN;
        }
        public BaseAsset(string Name)
        {
            Unique = Guid.NewGuid();
            this.Name = Name;
        }

        public virtual void InitAsset()
        {
        }

        public virtual void FromBase()
        { }

        static public T CreateFromFile<T>(XElement asset_elem, TYPE type) where T : BaseAsset, new()
        {
            //string type = asset_elem.Element("type")?.Value ?? "UNKNOWN";
            string name = asset_elem.Element("name")?.Value ?? "UNKNOWN";

            //if (string.IsNullOrEmpty(type) || type == "UNKNOWN")
            //{
            //    throw new Exception($"Asset type is null or empty.");
            //}

            if (string.IsNullOrEmpty(name) || name == "UNKNOWN")
            {
                throw new Exception($"Asset name is null or empty.");
            }

            T asset = new()
            {
                Unique = Guid.NewGuid(),
                Name = name,
                AssetData = asset_elem,
                Type = type,
                PackPath = asset_elem.Element("pack_path")?.Value ?? $"tilesets/{name}"
            };

            //TYPE type = (TYPE)Enum.Parse(typeof(TYPE), type);

            if (string.IsNullOrEmpty(asset.PackPath))
            {
                asset.PackPath = $"tilesets/{name}";
            }

            asset.FromBase();

            // TODO: Add the conversion from BaseAsset to Tileset
            // Need file path, dimension, image size, hardlink or softlink, etc...
            return asset;
        }
    }
}
