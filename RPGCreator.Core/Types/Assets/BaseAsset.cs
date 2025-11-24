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
using RPGCreator.Core.Types.Internal;
using static RPGCreator.Core.Types.Assets.AssetCategoryAttribute;
using static RPGCreator.Core.Types.Assets.BaseAsset;

// Devnote:
// This class should be removed or at least refactored to be more generic.
// It should be designed for the new definition system.

namespace RPGCreator.Core.Types.Assets
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
        public static AssetCategoryAttribute.AssetCategoryType GetCategory(this BaseAsset.TYPE type)
        {
            var member = typeof(BaseAsset.TYPE).GetMember(type.ToString()).FirstOrDefault();
            var attr = member?.GetCustomAttributes(typeof(AssetCategoryAttribute), false).FirstOrDefault() as AssetCategoryAttribute;
            return attr?.Category ?? AssetCategoryAttribute.AssetCategoryType.Unknown;
        }

        public static bool IsImageCompatible(this BaseAsset.TYPE type)
        {
            return type.GetCategory() == AssetCategoryAttribute.AssetCategoryType.Image;
        }
    }

    public class BaseAsset : ObservableObject, IHasSavePath
    {
        

        public string SavePath { get; set; }
        #region Events
        
        public event EventHandler<string>? NameChanged;
        
        #endregion
        
        /// <summary>
        /// Indicates if the asset should be cached or not.<br/>
        /// True if the asset should be cached, false otherwise.<br/>
        /// </summary>
        public virtual bool ShouldBeCached => false;

        public bool IsCached { get; internal set; } = false;
        public BaseAssetsPack.BaseAssetsPack Pack { get; set; } = null!; // This should be set by the pack manager when the asset is loaded.

        public enum TYPE
        {
            [AssetCategory(AssetCategoryAttribute.AssetCategoryType.Unknown)]
            UNKNOWN,
            [AssetCategory(AssetCategoryAttribute.AssetCategoryType.Image)]
            IMAGE,
            [AssetCategory(AssetCategoryAttribute.AssetCategoryType.Image)]
            TILESETS,
            [AssetCategory(AssetCategoryAttribute.AssetCategoryType.Image)]
            AUTOTILES,
            CHARACTER_DATA
        }

        public Ulid Unique { get; protected set; }

        public string PackName;
        public string PackPath;

        public XElement AssetData;

        private string _name = "UNKNOWN";
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    NameChanged?.Invoke(this, value);
                }
            }
        }
        public TYPE Type;

        public BaseAsset()
        {
            Unique = Ulid.NewUlid();
            Name = "UNKNOWN";
            Type = TYPE.UNKNOWN;
        }
        public BaseAsset(string Name)
        {
            Unique = Ulid.NewUlid();
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
            Ulid unique = Ulid.Parse(asset_elem.Element("unique")?.Value ?? Guid.NewGuid().ToString());
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
                Unique = unique,
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

        public void Save()
        {
            if (this is ISerializable serializable)
            {

                if(string.IsNullOrEmpty(SavePath))
                {

                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Saved asset {Name} of type {Type} with unique ID {Unique} to {SavePath}.");
                Console.ResetColor();
                return;
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Couldn't save asset {Name} of type {Type} with unique ID {Unique} because this is not an ISerializable object!.");
            Console.ResetColor();
        }

        protected void AddBaseSerialization(SerializationInfo info)
        {
            info.AddValue("unique", Unique);
            info.AddValue("name", Name);
            info.AddValue("type", Type);
        }

    }
}
