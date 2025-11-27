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
using RPGCreator.Core.Managers.ProjectsManager;
using RPGCreator.Core.Managers.RTP.BrushManagers;
using Serilog;

namespace RPGCreator.Core
{
    public class EngineManagers
    {
        public AssetsManager Assets { get; private set; }
        public GameFactory GameFactory { get; private set; }
        public ProjectsManager Projects { get; private set; }
        public BrushManager Brush { get; private set; }
        public ImageCache ImageCache { get; private set; }
        
        public FeaturesRulesManager FeaturesRules { get; private set; }

        internal EngineManagers()
        {
            Projects = new ProjectsManager();
            GameFactory = new GameFactory();
            Assets = new AssetsManager();
            Brush = new BrushManager();
            ImageCache = new ImageCache();
            FeaturesRules = new FeaturesRulesManager();

            EngineCore.Instance.Events.OnCoreManagersReady(new());
            
            Log.Information($"EngineManagers initialized.");
        }

        internal void Init()
        {
            Assets.Init();
        }

    }
}
