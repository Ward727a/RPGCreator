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
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Microsoft.Extensions.DependencyInjection;
using RPGCreator.Core.Services;
using RPGCreator.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.Core.Helpers
{
    public interface IFolderManager
    {
        string GetBaseFolder();
        string GetContentFolder();
        string GetConfigFolder();
        string GetLogsFolder();
        string GetProjectsFolder();
    }
    public class FolderManager : IFolderManager
    {

        Dictionary<string, string> _configuration;

        public FolderManager()
        {
            //ConfigurationService_old service = App.Services.GetService<ConfigurationService_old>();
        }

        protected bool CheckFolders()
        {
            // Base Folder
            if (!CheckFolder(GetBaseFolder()))
                return false;
            if (!CheckFolder(GetContentFolder()))
                return false;
            if (!CheckFolder(GetConfigFolder()))
                return false;
            if (!CheckFolder(GetLogsFolder()))
                return false;
            if (!CheckFolder(GetProjectsFolder()))
                return false;
            return true;
        }

        protected bool CheckFolder(string path)
        {

            if (path.IndexOfAny(Path.GetInvalidPathChars()) != -1)
            {
                return false;
            }

            Directory.CreateDirectory(path);
            return true;
        }

        protected string FormatPath(string unformatted_path)
        {
            return unformatted_path.Replace("%BASE_FOLDER%", GetBaseFolder());
        }

        public string GetBaseFolder()
        {
            return _configuration?["Folders:Base"] ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RPG Creator");
        }

        public string GetConfigFolder()
        {
            return FormatPath(_configuration?["Folders:Config"] ?? Path.Combine(GetBaseFolder(), "Config"));
        }

        public string GetContentFolder()
        {
            return FormatPath(_configuration?["Folders:Content"] ?? Path.Combine(GetBaseFolder(), "Content"));
        }

        public string GetLogsFolder()
        {
            return FormatPath(_configuration?["Folders:Logs"] ?? Path.Combine(GetBaseFolder(), "Logs"));
        }

        public string GetProjectsFolder()
        {
            return FormatPath(_configuration?["Folders:Projects"] ?? Path.Combine(GetBaseFolder(), "Projects"));
        }
    }
}
