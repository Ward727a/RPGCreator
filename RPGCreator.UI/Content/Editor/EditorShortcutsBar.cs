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
using Avalonia.Controls.Documents;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Projektanker.Icons.Avalonia;
using RPGCreator.SDK.Editor;
using RPGCreator.SDK.Logging;
using Ursa.Controls;

namespace RPGCreator.UI.Content.Editor;

public class EditorShortcutsBar : UserControl
{
    private Grid _body;
    private ScrollViewer _scroll;
    private StackPanel _menuPanel;
    
    public EditorShortcutsBar()
    {
        CreateComponents();
        RegisterEvents();
    }
    
    private void CreateComponents()
    {
        _body = new Grid()
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            RowDefinitions = new RowDefinitions("*"),
            ColumnDefinitions = new ColumnDefinitions("*"),
        };
        this.Content = _body;
        _scroll = new ScrollViewer()
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
        };
        _body.Children.Add(_scroll);
        _menuPanel = new StackPanel()
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            Spacing = 4
        };
        _scroll.Content = _menuPanel;
        
        AddCustomButton("Add custom shortcut","Add a custom shortcut to the bar.","mdi-plus",
            () => { Logger.Info("Add custom shortcut clicked."); }
        );

        AddSeparator();
    }
    
    private void RegisterEvents()
    {
        
    }

    private Button CreateValidButton()
    {
        return new Button()
        {
            MaxHeight = 32,
            MinHeight = 32,
            MinWidth = 32,
            MaxWidth = 32,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
        };
    }

    private TextBlock CreateButtonTip(string name, string description = "")
    {
        var tip = new TextBlock()
        {
            Inlines = new InlineCollection()
        };
        
        var nameRun = new Run(name)
        {
            FontWeight = Avalonia.Media.FontWeight.Bold,
        };
        tip.Inlines.Add(nameRun);
        
        tip.Inlines.Add(new LineBreak());
        
        var descRun = new Run(!string.IsNullOrWhiteSpace(description) ? description : "No description provided.")
        {
            FontStyle = Avalonia.Media.FontStyle.Italic,
        };
        tip.Inlines.Add(descRun);
        
        return tip;
    }
    
    public void AddShortcutButton(ShortcutButtonInfo info)
    {
        var button = CreateValidButton();

        var icon = new Icon()
        {
            Value = info.Icon,
            Width = 32,
            Height = 32,
        };
        
        button.Content = icon;
        button.FontSize = 24;
        button.Click += (s, e) => info.GetAction()?.Invoke([]);
        
        _menuPanel.Children.Add(button);
        
        ToolTip.SetTip(button, CreateButtonTip(info.Name, info.Description));
    }

    public void AddSeparator()
    {
        var separator = new Divider()
        {
            Orientation = Orientation.Vertical
        };
        _menuPanel.Children.Add(separator);
    }
    
    public void AddCustomButton(string name, string description, string icon, Action action)
    {
        var button = CreateValidButton();

        var iconControl = new Icon()
        {
            Value = icon,
            Width = 32,
            Height = 32,
        };
        
        button.Content = iconControl;
        button.FontSize = 24;
        button.Click += (s, e) => action.Invoke();
        
        _menuPanel.Children.Add(button);
        
        ToolTip.SetTip(button, CreateButtonTip(name, description));
    }
    
}