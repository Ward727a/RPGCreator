#region LICENSE
//
// RPG Creator - Open-source RPG Engine.
// (c) 2025 Ward
// 
// This file is part of RPG Creator and is distributed under the Apache 2.0 License.
// You are free to use, modify, and distribute this file under the terms of the Apache 2.0 License.
// See LICENSE for details.
// 
// ---
// 
// Ce fichier fait partie de RPG Creator et est distribué sous licence Apache 2.0.
// Vous êtes libre de l'utiliser, de le modifier et de le distribuer sous les termes de la licence Apache 2.0.
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
using RPGCreator.SDK.Assets.Definitions.Maps;
using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.Serializer;
using RPGCreator.SDK.Types.Interfaces;

namespace RPGCreator.SDK.Projects
{
    /// <summary>
    /// This class should contains all the data related to the game.<br/>
    /// Like but not Limited to: <br/>
    /// - Game Variables <br/>
    /// - Quests <br/>
    /// - Maps <br/>
    /// - NPC <br/>
    /// - Items <br/>
    /// - And more... <br/>
    /// </summary>
    [SerializingType("ProjectGameData")]
    public partial class ProjectGameData : ISerializable, IDeserializable
    {

        private IBaseProject Project;

        public ProjectGameData()
        {
        }

        public ProjectGameData(IBaseProject project)
        {
            Project = project;
        }

        public SerializationInfo GetObjectData()
        {
            SerializationInfo info = new SerializationInfo(typeof(ProjectGameData));
            // Add other properties as needed
            return info;
        }

        public List<Ulid> GetReferencedAssetIds()
        {
            List<Ulid> referencedIds = new List<Ulid>();
            // Add logic to collect referenced asset IDs from properties
            return referencedIds;
        }

        public void SetObjectData(DeserializationInfo info)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));
            // Set other properties as needed
            
        }
    }
}
