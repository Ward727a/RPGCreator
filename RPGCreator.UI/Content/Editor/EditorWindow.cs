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
using Avalonia.Interactivity;
using Microsoft.Xna.Framework;
using RPGCreator.Core;
using Semi.Avalonia;
using Semi.Avalonia.Tokens.Palette;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using RPGCreator.RTP;

namespace RPGCreator.UI.Content.Editor
{
    internal class EditorWindow : Window
    {

        private static EditorWindow? _instance;

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
        }

        public static EditorWindow Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new EditorWindow();
                }
                return _instance;
            }
        }

        #region EventsHandlers
        private void OnClosing(object? sender, WindowClosingEventArgs e)
        {
            e.Cancel = true; // Prevent the window from closing

            // TODO: Add cleanup and save logic here
            EngineCore.Instance.Events.OnUIEditorClosed(new());

            Closing -= OnClosing; // Unsubscribe from the event to avoid looping issues
            Close(); // Close the window programmatically
        }
        private void OnOpening(object? sender, EventArgs e)
        {
            EngineCore.Instance.Events.OnUIEditorOpened(new());
        }
        #endregion
    }
}
