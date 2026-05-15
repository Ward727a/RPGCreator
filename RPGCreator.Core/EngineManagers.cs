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
using RPGCreator.Core.Module;
using RPGCreator.Core.Registry;
using RPGCreator.Core.Services;
using RPGCreator.Core.Services.StorageService;
using RPGCreator.SDK;
using RPGCreator.SDK.Commands;
using RPGCreator.SDK.Logging;

namespace RPGCreator.Core
{
    internal class EngineManagers
    {
        private readonly ScopedLogger _logger = Logger.ForContext<EngineManagers>();
        public AssetsManagerRevamp Assets { get; private set; }
        public ProjectsManager Projects { get; private set; }
        public ToolService Brush { get; private set; }
        public UndoRedoService Commands { get; private set; }
        
        internal EngineManagers()
        {
            Assets = new AssetsManagerRevamp(JsonFileStorageService.Shared);
            EngineServices.AssetsManager = Assets;
            Projects = new ProjectsManager();
            // Created because this allow tool to interact with the engine canvas.
            // Do not delete it, even if you think it's doesn't used.. It is.
            Brush = new ToolService(); 
            Commands = new UndoRedoService();
            
            EngineServices.ProjectsManager = Projects;
            EngineServices.UndoRedoService = Commands;
            EngineServices.FeaturesManager = new FeatureManager();

            Projects.OnProjectOpened += (_) =>
            {
                RegistryServices.AssetsMetaDataRegistry = new AssetsMetadataRegistry();
                var ids = RegistryServices.AssetsMetaDataRegistry.GetAllMetaData();
                
                Logger.Debug($"AssetsMetadataRegistry: Loaded {ids.Count()} metadata entries for the opened project.");
            };

            _logger.Info($"EngineManagers initialized.");
        }

        internal void Init()
        {
        }

    }
}
