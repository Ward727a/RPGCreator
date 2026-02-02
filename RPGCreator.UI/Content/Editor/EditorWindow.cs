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
using Avalonia.Input;
using RPGCreator.RTP;
using RPGCreator.SDK;
using RPGCreator.SDK.UiService;

namespace RPGCreator.UI.Content.Editor
{
    internal class EditorWindow : Window
    {
        private EditorWindow() : base()
        {
            new EditorGame();
            Closing += OnClosing;
            Opened += OnOpening;

            Width = 1500;
            Height = 900;
            Title = "RPGCreator - Editor";
            // For now we will use the default avalonia icon, but you can replace it with your own icon.
            Icon = new WindowIcon("Assets/rpgc-logo.ico");
            WindowStartupLocation = WindowStartupLocation.Manual;
                Position = Position.WithX(this.Screens.Primary.WorkingArea.Center.X-1500/2)
                .WithY(this.Screens.Primary.WorkingArea.Center.Y-900/2);

            Content = new EditorWindowControl();
            //InitializeIfNeeded();
            this.Show(); // Show the window immediately
            
            GotFocus += OnGotFocus;
        }

        private void OnGotFocus(object? sender, GotFocusEventArgs e)
        {
            
            UiServices.NotificationService.Warn(
                "Welcome to RPG Creator Editor!",
                "This is a pre-release (and alpha) version of the editor, there might be bugs, and there is still a lot of features to implement." +
                "\nThank you for testing and helping us improve RPG Creator!" +
                "\nDon't worry, this message will disappear in 20 seconds!", new NotificationOptions(20000));
            
            GotFocus -= OnGotFocus;
        }

        public static EditorWindow Instance
        {
            get
            {
                field ??= new EditorWindow();
                return field;
            }
        }

        #region EventsHandlers
        private void OnClosing(object? sender, WindowClosingEventArgs e)
        {
            e.Cancel = true; // Prevent the window from closing

            // TODO: Add cleanup and save logic here

            Closing -= OnClosing; // Unsubscribe from the event to avoid looping issues
            Close(); // Close the window programmatically
        }
        private void OnOpening(object? sender, EventArgs e)
        {
        }
        #endregion
    }
}
