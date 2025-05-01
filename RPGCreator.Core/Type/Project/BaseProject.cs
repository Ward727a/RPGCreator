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
using RPGCreator.Core.Type.Assets;
using RPGCreator.Core.Type.Assets.BaseAssetsPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.Core.Type.Project
{
    public class BaseProject
    {

        public string? Name { get; set; } = "";
        public string? Description { get; set; } = "";
        public string? Path  { get; set; } = "";
        public Version? Version  { get; set; } = new Version(0, 0, 0, 0);
        public Version? EditorVersion  { get; set; } = new Version(0, 0, 0, 0);
        public bool IsArchived  { get; set; }
        public bool IsFavorite  { get; set; }
        public string? Copyright { get; set; } = "";
        public List<string> Authors { get; set; } = [];
        public List<string> AssetsPackPath = [];

        public List<object>? Maps;

        public ProjectGameData GameData;

        ProjectEvent Event;

        public BaseProject(string name)
        {
            Name = name;
            Event = new ProjectEvent();
            GameData = new ProjectGameData(this);
        }

        public void Load()
        {
            EngineCore.Instance.Managers.AssetsPack.ClearAssetsPacks();

            foreach (string packPath in AssetsPackPath)
            {
                EngineCore.Instance.Managers.AssetsPack.LoadAssetsPack(packPath);
            }
        }

        public void Unload()
        {
            EngineCore.Instance.Managers.AssetsPack.ClearAssetsPacks();
        }

        private string[] FormatString(string unformatedString)
        {
            return unformatedString.Split([':'], StringSplitOptions.TrimEntries);
        }

        public BaseAsset? GetAsset(string path)
        {
            var pathParts = FormatString(path);

            if(pathParts.Length != 2)
                return null;

            string AssetPack = pathParts[0];
            string AssetPath = pathParts[1];

            if(!EngineCore.Instance.Managers.AssetsPack.HasAssetsPack(AssetPack))
            {
                return null;
            }

            BaseAsset? asset = EngineCore.Instance.Managers.Assets.GetAsset(path);

            if(asset == null)
            {
                return null;
            }

            return asset;
        }

        public List<BaseAsset> GetAssetsType(BaseAsset.TYPE type)
        {
            List<BaseAsset> assets_found = [];

            foreach (BaseAssetsPack pack in EngineCore.Instance.Managers.Assets.GetAssetsPacks())
            {
                if (pack.Assets.Values.Any(x => x.Type == type))
                {
                    assets_found.AddRange(pack.Assets.Values.Where(x => x.Type == type).ToList());
                }
            }

            return assets_found;
        }
        public List<T> GetAssetsType<T>(BaseAsset.TYPE type) where T:BaseAsset
        {
            List<T> assets_found = [];

            foreach (BaseAssetsPack pack in EngineCore.Instance.Managers.Assets.GetAssetsPacks())
            {
                if (pack.Assets.Values.Any(x => x.Type == type))
                {
                    assets_found.AddRange(
                        pack.Assets.Values.Where(x => x.Type == type && x is T).Cast<T>().ToList()
                        );
                }
            }

            return assets_found;
        }

    }
}
