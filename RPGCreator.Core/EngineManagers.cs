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
using RPGCreator.Core.Managers.AssetsManager;
using RPGCreator.Core.Managers.AssetsPackManager;
using RPGCreator.Core.Managers.ProjectsManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.Core
{
    public class EngineManagers
    {
        public AssetsPackManager AssetsPack { get; private set; }
        public AssetsManager Assets { get; private set; }
        public ProjectsManager Projects { get; private set; }

        internal EngineManagers()
        {
            Projects = new ProjectsManager();
            AssetsPack = new AssetsPackManager();
            Assets = new AssetsManager();

            EngineCore.Instance.Events.OnCoreManagersReady(new());
        }

        internal void Init()
        {
            Assets.Init();
        }

    }
}
