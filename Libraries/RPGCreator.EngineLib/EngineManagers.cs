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

using RPGCreator.EngineLib.Managers.AssetsManager;
using RPGCreator.EngineLib.Managers.ProjectsManager;
using RPGCreator.EngineLib.Module;
using RPGCreator.EngineLib.Registry;
using RPGCreator.EngineLib.Services;
using RPGCreator.EngineLib.Services.StorageService;
using RPGCreator.SDK;
using RPGCreator.SDK.Commands;
using RPGCreator.SDK.Common.Logging;

namespace RPGCreator.EngineLib
{
    internal class EngineManagers
    {
        private readonly ScopedLogger _logger = Logger.ForContext<EngineManagers>();
        
        internal EngineManagers()
        {
            // Created because this allow tool to interact with the engine canvas.
            // Do not delete it, even if you think it's doesn't used.. It is.
            
            // Projects.OnProjectOpened += (_) =>
            // {
            //     RegistryServices.AssetsMetaDataRegistry = new AssetsMetadataRegistry();
            //     var ids = RegistryServices.AssetsMetaDataRegistry.GetAllMetaData();
            //     
            //     Logger.Debug($"AssetsMetadataRegistry: Loaded {ids.Count()} metadata entries for the opened project.");
            // };

            _logger.Info($"EngineManagers initialized.");
        }

        internal void Init()
        {
        }

    }
}
