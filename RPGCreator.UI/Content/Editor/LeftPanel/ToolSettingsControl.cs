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
using Avalonia.Controls.Primitives;

namespace RPGCreator.UI.Content.Editor.LeftPanel;

public class ToolSettingsControl : UserControl
{

    private Expander _content;
    private Grid? _body;
    private ScrollViewer? _scroll;
    private StackPanel? _settingsPanel;
    
    public ToolSettingsControl()
    {
        CreateComponents();
        RegisterEvents();
        this.Content = _content;
    }

    private void CreateComponents()
    {
        _content = new Expander()
        {
            IsExpanded = false,
            Header = "Tool Settings",
        };
        _body = new Grid()
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            RowDefinitions = new RowDefinitions("*"),
            ColumnDefinitions = new ColumnDefinitions("*"),
        };
        _content.Content = _body;
        _scroll = new ScrollViewer()
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        };
        _body.Children.Add(_scroll);
        _settingsPanel = new StackPanel()
            { };
        _scroll.Content = _settingsPanel;
    }
    
    private void RegisterEvents()
    {
        
    }

}