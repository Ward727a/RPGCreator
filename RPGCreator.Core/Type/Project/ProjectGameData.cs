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
using RPGCreator.Core.Type.Assets;
using RPGCreator.Core.Type.Map;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.Core.Type.Project
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
            info.AddValue("Maps", Maps);
            // Add other properties as needed
            return info;
        }

        public void SetObjectData(DeserializationInfo info)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));

            info.TryGetList("Maps", out List<MapInstance> maps, [], "Could not load Maps from project game data.");
            // Set other properties as needed
            if (Maps == null)
            {
                Maps = new ObservableCollection<MapDefinition>();
            }
        }
    }
}
