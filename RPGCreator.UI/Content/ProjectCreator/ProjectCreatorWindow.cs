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
using Avalonia;
using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.UI.Content.ProjectCreator
{
    public class ProjectCreatorWindow : Window
    {
        public ProjectCreatorWindowControl Control;
        public ProjectCreatorWindow()
        {
            // Set the window properties
            Width = 600;
            Height = 400;
            Title = "Project Creator";
            Icon = new WindowIcon("Assets/rpgc-logo.ico");
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            // Set the content of the window to the ProjectCreatorWindowControl
            Control = new ProjectCreatorWindowControl();
            Content = Control;
            // Initialize components if needed
        }
    }
}
