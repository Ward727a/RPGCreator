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
using RPGCreator.Core.Managers.AssetsManager;
using RPGCreator.Core.Managers.ProjectsManager;
using RPGCreator.Core.Managers.RTP.BrushManagers;
using RPGCreator.SDK;
using RPGCreator.SDK.Commands;
using RPGCreator.SDK.Logging;
using Serilog;

namespace RPGCreator.Core
{
    internal class EngineManagers
    {
        private readonly ScopedLogger _logger = Logger.ForContext<EngineManagers>();
        public AssetsManager Assets { get; private set; }
        public GameFactory GameFactory { get; private set; }
        public ProjectsManager Projects { get; private set; }
        public BrushManager Brush { get; private set; }
        public ImageCache ImageCache { get; private set; }
        public CommandManager Commands { get; private set; }
        
        public FeaturesRulesManager FeaturesRules { get; private set; }

        internal EngineManagers()
        {
            Assets = new AssetsManager();
            Projects = new ProjectsManager();
            GameFactory = new GameFactory();
            Brush = new BrushManager();
            ImageCache = new ImageCache();
            FeaturesRules = new FeaturesRulesManager();
            Commands = new CommandManager();
            
            EngineServices.AssetsManager = Assets;
            EngineServices.GameFactory = GameFactory;
            EngineServices.ProjectsManager = Projects;
            EngineServices.BrushManager = Brush;

            EngineCore.Instance.Events.OnCoreManagersReady(new());
            
            _logger.Info($"EngineManagers initialized.");
        }

        internal void Init()
        {
            Assets.Init();
        }

    }
}
