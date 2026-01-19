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
using Avalonia.Controls;
using System;

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
            Icon = new WindowIcon("Assets/rpgc-logo.ico");
            WindowStartupLocation = WindowStartupLocation.Manual;
            Position = Position.WithX(this.Screens.Primary.WorkingArea.Center.X-(int)Width/2)
                .WithY(this.Screens.Primary.WorkingArea.Center.Y-(int)Height/2);

            Content = new LauncherWindowControl();

            this.Show(); // Show the window immediately
        }

        #region EventsHandlers
        private void OnClosing(object? sender, WindowClosingEventArgs e)
        {
            e.Cancel = true; // Prevent the window from closing
            Closing -= OnClosing; // Unsubscribe from the event to avoid looping issues
            Close(); // Close the window programmatically
        }
        private void OnOpening(object? sender, EventArgs e)
        {
        }
        #endregion
    }
}
