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
using RPGCreator.Core;
using RPGCreator.UI.Content.Editor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.UI.Content.Launcher
{
    public class LauncherWindow : Window
    {
        public LauncherWindow()
        {
            Closing += OnClosing;
            Opened += OnOpening;

            Width = 1000;
            Height = 600;
            Title = "Test Launcher Window";
            // For now we will use the default avalonia icon, but you can replace it with your own icon.
            Icon = new WindowIcon("Assets/avalonia-logo.ico");
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            Content = new LauncherWindowControl();

            this.Show(); // Show the window immediately
        }

        #region EventsHandlers
        private void OnClosing(object? sender, WindowClosingEventArgs e)
        {
            e.Cancel = true; // Prevent the window from closing

            // TODO: Add cleanup and save logic here
            EngineCore.Instance.Events.OnUILauncherClosed(new());

            Closing -= OnClosing; // Unsubscribe from the event to avoid looping issues
            Close(); // Close the window programmatically
        }
        private void OnOpening(object? sender, EventArgs e)
        {
            EngineCore.Instance.Events.OnUILauncherOpened(new());
        }
        #endregion
    }
}
