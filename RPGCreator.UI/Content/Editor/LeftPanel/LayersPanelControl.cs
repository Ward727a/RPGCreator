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

using Avalonia.Controls;

namespace RPGCreator.UI.Content.Editor.LeftPanel;

public class LayersPanelControl : UserControl
{

    private Expander _expander;
    
    private Grid _grid;
    
    private ScrollViewer _scrollViewer;
    
    private ListBox _scrollContent;
    
    public LayersPanelControl()
    {
        CreateComponents();
        RegisterEvents();
        LinkToExtension();
    }

    private void CreateComponents()
    {
        _expander = new Expander
        {
            Header = "Layers",
            IsExpanded = true,
            ExpandDirection = ExpandDirection.Up,
            MaxHeight = 350,
            Content = new TextBlock { Text = "Layers content goes here..." }
        };

        _grid = new Grid()
        {
            RowDefinitions = new RowDefinitions("*")
        };
        _expander.Content = _grid;
    }

    private void RegisterEvents()
    {
    }

    private void LinkToExtension()
    {
    }
}