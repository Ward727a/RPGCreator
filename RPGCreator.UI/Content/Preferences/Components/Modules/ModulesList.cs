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

using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Media;
using ExCSS;
using Projektanker.Icons.Avalonia;
using RPGCreator.SDK;
using RPGCreator.SDK.Modules;
using Color = Avalonia.Media.Color;
using FontWeight = Avalonia.Media.FontWeight;
using HorizontalAlignment = Avalonia.Layout.HorizontalAlignment;
using VerticalAlignment = Avalonia.Layout.VerticalAlignment;

namespace RPGCreator.UI.Content.Preferences.Components.Modules;

public class ModulesList : UserControl
{

    private class ModuleItem : UserControl
    {
        private readonly ModuleCandidate _candidate;
        
        private readonly Grid _itemGrid = new Grid();
        private readonly TextBlock _moduleName, _moduleAuthor, _moduleVersion, _moduleDescription = new TextBlock();

        private readonly Border _certifiedBorder = new Border();
        private readonly Icon _certifiedIcon = new Icon();
        
        private readonly StackPanel _tagsPanel = new StackPanel();
        
        public ModuleItem(ModuleCandidate candidate)
        {
            _moduleName = MakeTextBlock();
            _moduleAuthor = MakeTextBlock();
            _moduleVersion = MakeTextBlock();
            
            _candidate = candidate;

            ClipToBounds = false;
            CreateComponents();
            RegisterEvents();
            Content = _itemGrid;
        }

        private void CreateComponents()
        {
            _itemGrid.ColumnDefinitions = new ColumnDefinitions("Auto, *");
            _itemGrid.RowDefinitions = new RowDefinitions("Auto, Auto, Auto, Auto");
            _itemGrid.RowSpacing = 4;
            _itemGrid.ColumnSpacing = 4;
            
            _moduleName.Inlines = MakeInlines("Name: ", _candidate.Name);
            AddToGrid(_moduleName);
            
            _moduleAuthor.Inlines = MakeInlines("Author: ", _candidate.Author);
            AddToGrid(_moduleAuthor, 1);
            
            _moduleVersion.Inlines = MakeInlines("Version: ", _candidate.Version);
            AddToGrid(_moduleVersion, 2);
            
            _moduleDescription.Inlines = MakeInlines("Description: ", _candidate.Description);
            _moduleDescription.Inlines.Insert(1, new LineBreak());
            _moduleDescription.VerticalAlignment = VerticalAlignment.Top;
            _moduleDescription.TextWrapping = TextWrapping.Wrap;
            AddToGrid(_moduleDescription, 0, 1);
            Grid.SetRowSpan(_moduleDescription, 3);

            
            if (_candidate.Certified)
            {
                _certifiedIcon.Value = "mdi-shield-check-outline";
                _certifiedIcon.Foreground = Brushes.Green;
                ToolTip.SetTip(_certifiedBorder, "This module has been certified by the RPG Creator team.");
            }
            else
            {
                _certifiedIcon.Value = "mdi-shield-alert-outline";
                _certifiedIcon.Foreground = Brushes.Red;
                ToolTip.SetTip(_certifiedBorder, "This module is not certified and as such it may not be safe to use.");
            }
            
            _certifiedBorder.Background = Brushes.Transparent; // For tooltip to show
            _certifiedBorder.BorderThickness = new Thickness(0);
            _certifiedBorder.Child = _certifiedIcon;
            
            _certifiedIcon.IsHitTestVisible = false;
            _certifiedIcon.FontSize = 26;
            _certifiedBorder.HorizontalAlignment = HorizontalAlignment.Right;
            _certifiedBorder.VerticalAlignment = VerticalAlignment.Top;
            _certifiedBorder.Margin = new Thickness(0, -8, -15, 0);
            AddToGrid(_certifiedBorder, 0, 1);

            _tagsPanel.Orientation = Orientation.Horizontal;
            _tagsPanel.HorizontalAlignment = HorizontalAlignment.Right;
            _tagsPanel.Spacing = 4;
            AddToGrid(_tagsPanel, 3);
            Grid.SetColumnSpan(_tagsPanel, 2);

            if(EngineServices.ModuleManager.IsModuleStarted(_candidate.ModuleUrn))
            {
                AddActivatedTag();
            }
            else
            {
                AddDeactivatedTag();
            }

            Content = _itemGrid;
        }

        private void RegisterEvents()
        {
        }

        private void AddToGrid(Control control, int row = 0, int column = 0)
        {
            _itemGrid.Children.Add(control);
            Grid.SetRow(control, row);
            Grid.SetColumn(control, column);
        }

        private TextBlock MakeTextBlock()
        {
            return new TextBlock()
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            };
        }
        
        private InlineCollection MakeInlines(string title, string value)
        {
            var inlines = new InlineCollection();
            
            inlines.Add(new Run(title)
            {
                Foreground = Brushes.Gray
            });
            inlines.Add(new Run(value)
            {
            });
            return inlines;
        }

        private Border MakeTagBorder(Color color)
        {
            var borderRpg = new SDK.Types.Color(color.R, color.G, color.B, color.A).Darken(.2f);
            var borderColor = new Color(borderRpg.A, borderRpg.R, borderRpg.G, borderRpg.B);

            return new Border()
            {
                BorderThickness = new Thickness(4),
                CornerRadius = new CornerRadius(20),
                BorderBrush = new SolidColorBrush(borderColor),
                Background = new SolidColorBrush(color),
                Padding = new Thickness(20, 2)
            };
        }
        
        private void AddActivatedTag()
        {
            var border = MakeTagBorder(Color.FromUInt32(0x6400AA00));
            _tagsPanel.Children.Add(border);
            border.Child = new TextBlock()
            {
                Text = "Activated",
                Foreground = Brushes.White,
                FontSize = 12,
                FontWeight = FontWeight.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            ToolTip.SetTip(border, "This module is currently activated.");
        }
        
        private void AddDeactivatedTag()
        {
            var border = MakeTagBorder(Color.FromUInt32(0x64FF0000));
            _tagsPanel.Children.Add(border);
            border.Child = new TextBlock()
            {
                Text = "Deactivated",
                Foreground = Brushes.White,
                FontSize = 12,
                FontWeight = FontWeight.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            ToolTip.SetTip(border, "This module is currently deactivated.");
        }
    }
    
    private Grid _bodyGrid = new Grid();

    private ScrollViewer _scrollViewer = new ScrollViewer();
    private ListBox _modulesList = new ListBox();

    private List<ModuleCandidate> _moduleCandidates;
    
    public ModulesList()
    {
        LoadModules();
        CreateComponents();
        RegisterEvents();
        Content = _bodyGrid;
    }
    
    private void CreateComponents()
    {
        _bodyGrid.ColumnDefinitions = new ColumnDefinitions("*, Auto");
        _bodyGrid.RowDefinitions = new RowDefinitions("Auto, *, Auto");
        
        _scrollViewer = new ScrollViewer();
        _bodyGrid.Children.Add(_scrollViewer);
        Grid.SetRow(_scrollViewer, 1);
        
        _modulesList = new ListBox();
        _scrollViewer.Content = _modulesList;

        _modulesList.ItemsSource = _moduleCandidates;
        _modulesList.ItemTemplate = new FuncDataTemplate<ModuleCandidate>((mc, _) => new ModuleItem(mc));
    }

    private void LoadModules()
    {
        _moduleCandidates = EngineServices.ModuleManager.GetAllLoadedModules(new EngineSecurityToken()).ToList();
    }

    private void RegisterEvents()
    {
    }
}