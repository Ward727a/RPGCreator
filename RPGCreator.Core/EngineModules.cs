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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;
using RPGCreator.Core.Common;
using RPGCreator.SDK;
using RPGCreator.SDK.Logging;
using Serilog;

namespace RPGCreator.Core
{

    public class ModuleContext : AssemblyLoadContext
    {
        private readonly AssemblyDependencyResolver _resolver;

        public ModuleContext(string modulePath) : base(isCollectible: true)
        {
            _resolver = new AssemblyDependencyResolver(modulePath);
        }

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            string? assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
            return assemblyPath != null ? LoadFromAssemblyPath(assemblyPath) : null;
        }
    }
    
    public class EngineModules
    {

        private readonly ScopedLogger _logger = Logger.ForContext<EngineModules>();
        
        // This is the SHA256 checksum of the module DLL file to ensure integrity.
        // Those should be updated with each new module version. (even for small changes!)
        private readonly List<string> CHECKSUM_INTERNAL_MODULES =
        [ 
            // "f3886692656072a8702c0d0faf32d956fefa6078f47087d4a5113b1927d5b6eb", // TestModule.dll - For now disabled so it doesn't load automatically
        ];

        private readonly string MODULES_PATH = $"{AppContext.BaseDirectory}Assets/Modules/";
        
        internal EngineModules()
        {                   
            
            _logger.Info($"EngineModules initialized.");

            if (!Directory.Exists(MODULES_PATH))
            {
                _logger.Error("Engine modules directory not found.");
                return;
            }
            foreach (var directory in Directory.GetDirectories(MODULES_PATH))
            {
                var files = Directory.GetFiles(directory, "*.dll");
                foreach (var file in files)
                {
                    try
                    {
                        // Calculate the SHA256 checksum of the file.
                        var hashString = ShaUtil.ComputeSha256(file);
                        if (CHECKSUM_INTERNAL_MODULES.Contains(hashString))
                        {
                            var context = new ModuleContext(file);
                            var assembly = context.LoadFromAssemblyPath(file);
                            
                            var types = assembly.GetTypes().Where(t =>
                                typeof(IEngineModule).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
                            foreach (var type in types)
                            {
                                var module = (IEngineModule)Activator.CreateInstance(type)!;
                                module.Initialize();
                                _logger.Info(
                                    $"Module '{module.Name}' v{module.Version} by {module.Author} initialized.");
                            }
                        }
                        else
                        {
                            _logger.Warning($"Module file '{file}' failed integrity check and will not be loaded.");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Failed to load module from file '{file}'. Exception: {ex}");
                    }
                }
            }
            
        }

    }
}
