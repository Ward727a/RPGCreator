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
using System.Collections.Generic;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Projektanker.Icons.Avalonia;
using RPGCreator.SDK.Editor;
using RPGCreator.SDK.Logging;
using Ursa.Controls;

namespace RPGCreator.UI.Content.Editor;

public class EditorToolsBar : UserControl
{
    private Grid _body;
    private ScrollViewer _scroll;
    private StackPanel _menuPanel;
    private List<ToggleButton> _group = new();
    
    public EditorToolsBar()
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
        
        AddToolButton(new ToolButtonInfo()
        {
            Name = "Simple pen",
            Description = "A simple pen tool for drawing.",
            Icon = "mdi-pencil",
            OnCheckedChanged = (_, state) => Logger.Info($"Simple pen {
                (state ? "selected" : "unselected")
            }!")
        });
        
        AddToolButton(new ToolButtonInfo()
        {
            Name = "Eraser",
            Description = "A simple eraser tool for erasing.",
            Icon = "mdi-eraser",
            OnCheckedChanged = (_, state) => Logger.Info($"Eraser {
                (state ? "selected" : "unselected")
            }!")
        });
        AddToolButton(new ToolButtonInfo()
        {
            Name = "Tile picker",
            Description = "A tool for picking tiles from the map.",
            Icon = "mdi-eyedropper",
            OnCheckedChanged = (_, state) => Logger.Info($"Tile picker {
                (state ? "selected" : "unselected")
            }!")
        });
        
        AddToolSeparator();
        
        AddToolButton(new ToolButtonInfo()
        {
            Name = "Character placer",
            Description = "A tool for placing characters on the map.",
            Icon = "mdi-account-plus",
            OnCheckedChanged = (_, state) => Logger.Info($"Character placer {
                (state ? "selected" : "unselected")
            }!")
        });
        AddToolButton(new ToolButtonInfo()
        {
            Name = "Object placer",
            Description = "A tool for placing objects on the map.\nLike doors, chests, etc.",
            Icon = "mdi-cube",
            OnCheckedChanged = (_, state) => Logger.Info($"Object placer {
                (state ? "selected" : "unselected")
            }!")
        });
        
        AddToolButton(new ToolButtonInfo()
        {
            Name = "Event placer",
            Description = "A tool for placing events on the map.\nLike areas, etc.",
            Icon = "mdi-flag",
            OnCheckedChanged = (_, state) => Logger.Info($"Event placer {
                (state ? "selected" : "unselected")
            }!")
        });
        
        AddToolButton(new ToolButtonInfo()
        {
            Name = "Path tool",
            Description = "A tool for creating paths on the map.\nLike for patrolling characters, etc.",
            Icon = "mdi-chart-timeline-variant-shimmer",
            OnCheckedChanged = (_, state) => Logger.Info($"Path tool {
                (state ? "selected" : "unselected")
             }!")
        });
        
        AddToolSeparator();
        
        AddToolButton(new ToolButtonInfo()
        {
            Name = "Manage tools",
            Description = "Open the tools management window.",
            Icon = "mdi-cog",
            OnCheckedChanged = (obj, state) =>
            {
                if(obj is not ToggleButton button)
                    return;
                if (!state) return;
                Logger.Info($"Manage tools {
                    (state ? "selected" : "unselected")
                }!");
                button.IsChecked = false;
            }
        });
    }
    
    private void RegisterEvents()
    {
        
    }

    private ToggleButton CreateValidButton()
    {
        return new ToggleButton()
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
    
    private void RegisterButtonToGroup(ToggleButton button)
    {
        _group.Add(button);
        button.IsCheckedChanged += (s, e) =>
        {
            if (button.IsChecked != true)
                return;
            foreach (var btn in _group)
            {
                if (btn.Tag != button.Tag)
                    btn.IsChecked = false;
            }
        };
    }
    
    public void AddToolButton(ToolButtonInfo info)
    {
        var button = CreateValidButton();
        button.Tag = Ulid.NewUlid();
        var icon = new Icon()
        {
            Value = info.Icon,
            Width = 32,
            Height = 32,
        };
        
        button.Content = icon;
        button.FontSize = 24;
        
        button.IsCheckedChanged += (s, e) => info.OnCheckedChanged?.Invoke(button, button.IsChecked == true);
        RegisterButtonToGroup(button);
        
        _menuPanel.Children.Add(button);
        
        ToolTip.SetTip(button, CreateButtonTip(info.Name, info.Description));
    }
    
    public void AddToolSeparator()
    {
        var separator = new Divider()
        {
            Orientation = Orientation.Vertical
        };
        _menuPanel.Children.Add(separator);
    }
}