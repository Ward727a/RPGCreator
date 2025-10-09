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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace RPGCreator.Core
{
    public class EngineModules
    {

        // This is the SHA256 checksum of the module DLL file to ensure integrity.
        // Those should be updated with each new module version. (even for small changes!)
        private readonly List<string> CHECKSUM_INTERNAL_MODULES = new(
            [
                "37dd0521d9dc2796ba8c67020f8f596d9196aaeec68891f9517dc669c45ba691", // TestModule.dll
                ]
            );

        private readonly string MODULES_PATH = $"{AppContext.BaseDirectory}Assets/Modules/";
        
        internal EngineModules()
        {
            
            Log.Information($"EngineModules initialized.");

            foreach (var directory in Directory.GetDirectories(MODULES_PATH))
            {
                var files = Directory.GetFiles(directory, "*.dll");
                foreach (var file in files)
                {
                    try
                    {
                        // Calculate the SHA256 checksum of the file.
                        using var sha256 = System.Security.Cryptography.SHA256.Create();
                        using var stream = File.OpenRead(file);
                        var hash = sha256.ComputeHash(stream);
                        var hashString = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                        if (CHECKSUM_INTERNAL_MODULES.Contains(hashString))
                        {
                            var assembly = System.Reflection.Assembly.LoadFrom(file);
                            var types = assembly.GetTypes().Where(t =>
                                typeof(ModuleSDK.IEngineModule).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
                            foreach (var type in types)
                            {
                                var module = (ModuleSDK.IEngineModule)Activator.CreateInstance(type)!;
                                module.Initialize();
                                Log.Information(
                                    $"Module '{module.Name}' v{module.Version} by {module.Author} initialized.");
                            }
                        }
                        else
                        {
                            Log.Warning($"Module file '{file}' failed integrity check and will not be loaded.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, $"Failed to load module from file '{file}'.");
                    }
                }
            }
            
        }

    }
}
