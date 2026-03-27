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

using System.Reflection;
using RPGCreator.SDK.Logging;

namespace RPGCreator.SDK;

public static class RpgEnv
{

    public static string ExecutableFolder
    {
        get
        {
            if (field == "")
            {
                if (Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) is { } path &&
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
                field = Path.Combine(ExecutableFolder, "assets");
            }

            return field;
        }
    } = "";
    
    public static string ApplicationData
    {
        get
        {
            if (field == "")
            {
                field = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "RPGCreator");
                if (!Directory.Exists(field))
                {
                    Directory.CreateDirectory(field);
                }
            }

            return field;
        }
    } = "";

    public static string Config
    {
        get
        {
            if (field == "")
            {
                field = Path.Combine(ApplicationData, "config");
                if (!Directory.Exists(field))
                {
                    Directory.CreateDirectory(field);
                }
            }

            return field;
        }
    } = "";

    public static string Modules
    {
        get
        {
            if (field == "")
            {
                field = Path.Combine(ApplicationData, "modules");
                if (!Directory.Exists(field))
                {
                    Directory.CreateDirectory(field);
                    File.WriteAllText(Path.Combine(field, "__SECURITY WARNING - PLEASE READ!!!.txt"),
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
                field = Path.Combine(ApplicationData, "_runningModules");
                if (!Directory.Exists(field))
                {
                    Directory.CreateDirectory(field);
                    File.WriteAllText(Path.Combine(field, "__DO NOT TOUCH HERE!!!.txt"),
                        "DO NOT TOUCH THIS FOLDER! Otherwise the engine could crash!");
                }
            }

            return field;
        }
    } = "";
}