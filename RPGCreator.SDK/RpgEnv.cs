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
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Types;
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
                    field = SysPath.Combine(ExecutableFolder, "assets");
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
        private static string? DbFolder = null;
        public static Result<string> GetDbPath(string dbName)
        {
            if (DbFolder == null)
            {
                if (GlobalStates.ProjectState.CurrentProject is not { } currentProject)
                {
                    return Result.Fail("No project is currently open");
                }
                
                DbFolder = SysPath.Combine(currentProject.MetaData.Directory, "_db");
                
                if (!Directory.Exists(DbFolder))
                    Directory.CreateDirectory(DbFolder);
                
                GlobalStates.ProjectState.PropertyChanged -= OnPropertyChanged;
                GlobalStates.ProjectState.PropertyChanged += OnPropertyChanged;
            }
            
            if(!dbName.EndsWith(".db"))
                dbName += ".db";
            
            return SysPath.Combine(DbFolder, dbName);
            
            void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == nameof(GlobalStates.ProjectState.CurrentProject))
                {
                    DbFolder = null;
                }
            }
        }
        private static string? AssetFolder = null;
        public static Result<string> GetAssetsPath()
        {
            if (AssetFolder == null)
            {
                if (GlobalStates.ProjectState.CurrentProject is not { } currentProject)
                {
                    return Result.Fail("No project is currently open");
                }
                
                AssetFolder = SysPath.Combine(currentProject.MetaData.Directory, "assets_data");
                
                if (!Directory.Exists(AssetFolder))
                    Directory.CreateDirectory(AssetFolder);
                
                GlobalStates.ProjectState.PropertyChanged -= OnPropertyChanged;
                GlobalStates.ProjectState.PropertyChanged += OnPropertyChanged;
            }
            
            return AssetFolder;
            
            void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == nameof(GlobalStates.ProjectState.CurrentProject))
                {
                    AssetFolder = null;
                }
            }
        }
        
        private static FilePath? ImportedFiles = null;
        public static Result<FilePath> GetImportedFilesPath(string additionalFolder = "")
        {
            if (ImportedFiles == null)
            {
                if (GlobalStates.ProjectState.CurrentProject is not { } currentProject)
                {
                    return Result.Fail("No project is currently open");
                }
                
                ImportedFiles = SysPath.Combine("#project#", "imported_files");
                
                if (!Directory.Exists(ImportedFiles))
                    Directory.CreateDirectory(ImportedFiles);
                
                GlobalStates.ProjectState.PropertyChanged -= OnPropertyChanged;
                GlobalStates.ProjectState.PropertyChanged += OnPropertyChanged;
            }
            
            if(string.IsNullOrWhiteSpace(additionalFolder))
                return Result<FilePath>.Success(ImportedFiles.Value);
            
            var path = new FilePath(SysPath.Combine(ImportedFiles.Value.PureText, additionalFolder));
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return Result<FilePath>.Success(path);

            void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == nameof(GlobalStates.ProjectState.CurrentProject))
                {
                    ImportedFiles = null;
                }
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

    public static class TypesDiscriminator
    {
        private static readonly IReadOnlyDictionary<string, Type> StringToTypes = new Dictionary<string, Type>()
        {
            { "string", typeof(string) },
            { "int", typeof(int) },
            { "float", typeof(float) },
            { "bool", typeof(bool) },
            { "double", typeof(double) },
            { "long", typeof(long) },
            { "short", typeof(short) },
            { "byte", typeof(byte) },
            { "decimal", typeof(decimal) },
            { "sbyte", typeof(sbyte) },
            { "uint", typeof(uint) },
            { "ulong", typeof(ulong) },
            { "ushort", typeof(ushort) },
            { "char", typeof(char) },
            { "object", typeof(object) },
            { "array", typeof(Array) },
            { "OCollection", typeof(ObservableCollection<>)}
        };

        private static readonly IReadOnlyDictionary<Type, string> TypeToStrings = StringToTypes.ToDictionary(x => x.Value, x => x.Key);

        public static string TypeToString(Type? type)
        {
            if(type is null)
                return "object";
            
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ObservableCollection<>))
            {
                var genericType = type.GetGenericArguments()[0];
                return $"OCollection<{TypeToString(genericType)}>";
            }

            if (type.IsArray)
            {
                return $"{TypeToString(type.GetElementType())}[]";
            }

            if (!TypeToStrings.TryGetValue(type, out var stringType))
            {
                Logger.Error("Type {0} is not supported", type);
                return $"NotSupportedTypeDiscriminator({type.Name})";
            }
    
            return stringType;
        }

        private const int OCollectionSizeString = 12; // Size for "OCollection<"
        private const int OCollectionSizeStringWithBrackets = 13; // Size for "OCollection<>"
        
        public static Type StringToType(string type)
        {
            var span = type.AsSpan();

            if (span.StartsWith("OCollection<") && span.EndsWith(">"))
            {
                var innerContent = span.Slice(OCollectionSizeString, span.Length - OCollectionSizeStringWithBrackets);
                var innerType = StringToType(innerContent.ToString());
        
                return typeof(ObservableCollection<>).MakeGenericType(innerType);
            }
            
            if (!StringToTypes.TryGetValue(type, out var stringType))
            {
                Logger.Error("Type {0} is not supported", type);
                return typeof(object);
            }
            
            return stringType;
        }
    }
}