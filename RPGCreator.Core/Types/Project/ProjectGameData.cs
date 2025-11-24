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
using RPGCreator.Core.Types.Assets;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RPGCreator.Core.Types.Assets.BaseAssetsPack;
using RPGCreator.Core.Types.Assets.Characters.Stats;
using RPGCreator.Core.Types.Map;

namespace RPGCreator.Core.Types.Project
{
    /// <summary>
    /// This class should contains all the data related to the game.<br/>
    /// Like but not limited to: <br/>
    /// - Game Variables <br/>
    /// - Quests <br/>
    /// - Maps <br/>
    /// - NPC <br/>
    /// - Items <br/>
    /// - And more... <br/>
    /// </summary>
    public partial class ProjectGameData : ISerializable, IDeserializable
    {

        private BaseProject Project;

        public ObservableCollection<MapDefinition> Maps = [];
        public string GamePackPath;

        public ProjectGameData()
        {
        }

        public ProjectGameData(BaseProject project)
        {
            Project = project;
        }

        public SerializationInfo GetObjectData()
        {
            SerializationInfo info = new SerializationInfo(typeof(ProjectGameData));
            info.AddValue(nameof(Maps), Maps);
            info.AddValue(nameof(GamePackPath), GamePackPath);
            // Add other properties as needed
            return info;
        }

        public void SetObjectData(Serializer.DeserializationInfo info)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));

            info.TryGetList("Maps", out List<MapDefinition> maps);
            
            if(maps == null)
            {
                maps = new List<MapDefinition>();
            }
            // Set other properties as needed
            Maps = new ObservableCollection<MapDefinition>(maps);
            
        }
    }
}
