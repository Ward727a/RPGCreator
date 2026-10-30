// RPG Creator - Open-source RPG Engine.
// (c) 2026 Ward
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

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using Fluxor;
using Microsoft.Extensions.DependencyInjection;
using RPGCreator.SDK.Common.Logging;
using RPGCreator.SDK.States;
using RPGCreator.SDK.Types;
using RPGCreator.Shared.Types;
using SysPath = System.IO.Path;

namespace RPGCreator.SDK;

public static class RpgEnv
{
    public static class Path
    {
        public static string ExecutableFolder
        {
            get
            {
                if (field == "")
                {
                    if (SysPath.GetDirectoryName(Assembly.GetExecutingAssembly().Location) is { } path &&
                        Directory.Exists(path))
                    {
                        field = path;
                    }
                    else
                    {
                        Logger.Error("Could not find the executable folder.");
                        throw new Exception("Could not find the executable folder.");
                    }
                }

                return field;
            }
        } = "";
    
        public static string ExeAssetsFolder
        {
            get
            {
                if (field == "")
                {
                    field = SysPath.Combine(ExecutableFolder, "Content");
                }

                return field;
            }
        } = "";
        
        public static string ApplicationDataFolder
        {
            get
            {
                if (field == "")
                {
                    field = SysPath.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "RPGCreator");
                    if (!Directory.Exists(field))
                    {
                        Directory.CreateDirectory(field);
                    }
                }

                return field;
            }
        } = "";

        public static string TempFolder
        {
            get
            {
                if(field == "")
                {
                    field = SysPath.Combine(SysPath.GetTempPath(), "RPGCreator");
                    if (!Directory.Exists(field))
                    {
                        Directory.CreateDirectory(field);
                    }
                }
                return field;
            }
        }
        
        public static string ConfigFolder
        {
            get
            {
                if (field == "")
                {
                    field = SysPath.Combine(ApplicationDataFolder, "config");
                    if (!Directory.Exists(field))
                    {
                        Directory.CreateDirectory(field);
                    }
                }

                return field;
            }
        } = "";
        
        public static string ModulesFolder
        {
            get
            {
                if (field == "")
                {
                    field = SysPath.Combine(ApplicationDataFolder, "modules");
                    if (!Directory.Exists(field))
                    {
                        Directory.CreateDirectory(field);
                        File.WriteAllText(SysPath.Combine(field, "__SECURITY WARNING - PLEASE READ!!!.txt"),
                            "If you are here, it could mean one of the following:\n" +
                            "1. You have created your own module, then it's all good, continue!\n" +
                            "2. You have DOWNLOADED a module from the internet, please make sure to read this!\n\n" +
                            "=== IF YOU HAVE DOWNLOADED A MODULE FROM THE INTERNET -- READ PLEASE ===\n\n" +
                            "The modules you download on the internet and move in this folder could be potentially harmful for your computer.\n" +
                            "The engine (RPG Creator) has no way of verifying the code inside those modules, and as such will only disable it by default.\n" +
                            "You can enable the module by going to the \"Modules\" tab in the project settings, BUT BE AWARE that once the module is enabled,\n" +
                            "the engine has NO WAY of knowing what the module does, and as such, can't stop any harmful code from being executed on your computer.\n" +
                            "By enabling the module, you are taking full responsibility for any damage that may occur to your computer or data.\n\n" +
                            "Please make sure to only enable modules from sources you trust!!!");
                    }
                }

                return field;
            }
        } = "";
        
        public static string RunningModules
        {
            get
            {
                if (field == "")
                {
                    field = SysPath.Combine(ApplicationDataFolder, "_runningModules");
                    if (!Directory.Exists(field))
                    {
                        Directory.CreateDirectory(field);
                        File.WriteAllText(SysPath.Combine(field, "__DO NOT TOUCH HERE!!!.txt"),
                            "DO NOT TOUCH THIS FOLDER! Otherwise the engine could crash!");
                    }
                }

                return field;
            }
        } = "";
    }

    public static class PathFormat
    {
        private static IState<ProjectState> _projectState = null!;
        public static void Boot(IState<ProjectState> projectState)
        {
            _projectState = projectState;
        }

        private static string? _cacheDirectory = null;
        public static string GetCacheFolder()
        {
            if (_cacheDirectory == null)
            {
                _cacheDirectory = SysPath.Combine(GetProjectFolder(), ".cache");
                
                _projectState.StateChanged -= OnPropertyChanged;
                _projectState.StateChanged += OnPropertyChanged;
            }
            
            if(!Directory.Exists(_cacheDirectory))
                Directory.CreateDirectory(_cacheDirectory);
            
            return _cacheDirectory;
            
            void OnPropertyChanged(object? sender, EventArgs e)
            {
                _cacheDirectory = null;
            }
        }

        public static string GetProjectFolder()
        {
            return _projectState.Value.Directory;
        }
        
        private static string? DbFolder = null;
        public static Result<string> GetDbPath(string dbName)
        {
            if (DbFolder == null)
            {
                if (!_projectState.Value.HasProject)
                {
                    return Result.Fail("No project is currently open");
                }
                
                DbFolder = SysPath.Combine(_projectState.Value.Directory, "_db");
                
                if (!Directory.Exists(DbFolder))
                    Directory.CreateDirectory(DbFolder);
                
                _projectState.StateChanged -= OnPropertyChanged;
                _projectState.StateChanged += OnPropertyChanged;
            }
            
            if(!dbName.EndsWith(".db"))
                dbName += ".db";
            
            return SysPath.Combine(DbFolder, dbName);
            
            void OnPropertyChanged(object? sender, EventArgs e)
            {
                DbFolder = null;
            }
        }
        private static string? AssetFolder = null;
        public static Result<string> GetAssetsPath()
        {
            if (AssetFolder == null)
            {
                if (!_projectState.Value.HasProject)
                {
                    return Result.Fail("No project is currently open");
                }

                AssetFolder = SysPath.Combine(_projectState.Value.Directory, "assets_data");
                
                if (!Directory.Exists(AssetFolder))
                    Directory.CreateDirectory(AssetFolder);
                
                _projectState.StateChanged -= OnPropertyChanged;
                _projectState.StateChanged += OnPropertyChanged;
            }
            
            return AssetFolder;
            
            void OnPropertyChanged(object? sender, EventArgs e)
            {
                AssetFolder = null;
            }
        }
        
        private static string? _resourcesFiles = null;
        public static Result<string> GetResourcesPath(string additionalFolder = "")
        {
            if (_resourcesFiles == null)
            {
                if (!_projectState.Value.HasProject)
                {
                    return Result.Fail("No project is currently open");
                }
                
                _resourcesFiles = SysPath.Combine(_projectState.Value.Directory, "resources");
                
                if (!Directory.Exists(_resourcesFiles))
                    Directory.CreateDirectory(_resourcesFiles);
                
                _projectState.StateChanged -= OnPropertyChanged;
                _projectState.StateChanged += OnPropertyChanged;
            }
            
            if(string.IsNullOrWhiteSpace(additionalFolder))
                return Result<string>.Success(_resourcesFiles);
            
            var path = SysPath.Combine(_resourcesFiles, additionalFolder);
            
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            
            return Result<string>.Success(path);
            
            void OnPropertyChanged(object? sender, EventArgs e)
            {
                _resourcesFiles = null;
            }
        }

        private static string? _moduleCache = null;
        public static Result<string> GetModuleCachePath()
        {
            if (_moduleCache == null)
            {
                if (!_projectState.Value.HasProject)
                {
                    return Result.Fail("No project is currently open");
                }
                
                _moduleCache = SysPath.Combine(_projectState.Value.Directory, ".moduleCache");
                
                if (!Directory.Exists(_moduleCache))
                    Directory.CreateDirectory(_moduleCache);
                
                _projectState.StateChanged -= OnPropertyChanged;
                _projectState.StateChanged += OnPropertyChanged;
            }
            
            return Result<string>.Success(_moduleCache);
            
            void OnPropertyChanged(object? sender, EventArgs e)
            {
                _moduleCache = null;
            }
        }
    }

    public static class Versions
    {
        public static Version EngineVersion
        {
            get;
        } = new Version(0, 0, 1);
    
        public static Version SdkVersion
        {
            get;
        } = new Version(0, 0, 1);
    
        public static Version UiVersion
        {
            get;
        } = new Version(0, 0, 1);

        public static Version ConfigBpVersion
        {
            get;
        } = new Version(0, 0, 1);
    }

    public static class FileExtension
    {
        public const string Tileset = ".r.tsets";
        public const string Config = ".r.cfg";
    }
}