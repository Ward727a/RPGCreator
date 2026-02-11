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

using System;
using Avalonia.Controls;
using RPGCreator.RTP;

namespace RPGCreator.UI.Content.GameUiEditor;

public sealed class UiEditorWindow : Window
{
    public static bool IsOpen { get; private set; } = false;
    
    public UiEditorWindow()
    {
        IsOpen = true;
        Closing += OnClosing;
        Opened += OnOpening;
        Width = 1500;
        Height = 900;
        
        Title = "RPGCreator - UI Editor";
        
        // For now, we will use the default avalonia icon, but you can replace it with your own icon.
        Icon = new WindowIcon("Assets/rpgc-logo.ico");
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Content = new UiEditorWindowControl(new GameUiPreviewer());
    }
    
    private void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        IsOpen = false;
        e.Cancel = true; // Prevent the window from closing

        // TODO: Add cleanup and save logic here

        Closing -= OnClosing; // Unsubscribe from the event to avoid looping issues
        Close(); // Close the window programmatically
    }
    
    private void OnOpening(object? sender, EventArgs e)
    {
    }
}