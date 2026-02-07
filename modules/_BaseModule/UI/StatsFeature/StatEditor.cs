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

using _BaseModule.AssetDefinitions.BaseStats;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Stats;
using RPGCreator.SDK.Modules.UIModule;
using RPGCreator.UI.Contexts;
using RPGCreator.UI.Extensions;
using Ursa.Controls;

namespace _BaseModule.UI.StatsFeature;

public class StatEditor : UserControl
{
    
    #region Components
    
    private Grid _editorGrid = null!;
    
    private ScrollViewer _formScroller = null!;
    private StackPanel _formPanel = null!;
    private TextBox _nameTextBox = null!;
    private TextBox _descriptionTextBox = null!;
    private NumericDoubleUpDown _defaultValueNumeric = null!;
    private NumericDoubleUpDown _minValueNumeric = null!;
    
    private StackPanel _buttonsPanel = null!;
    private Button _saveButton = null!;
    private Button _cancelButton = null!;
    #endregion
    
    #region Properties
    
    private readonly AssetsManagerMenuContext _context;
    private BaseStatDefinition _stat;
    private string _currentStatus = "Editing ???";
    
    #endregion

    public StatEditor(AssetsManagerMenuContext context, BaseStatDefinition? stat = null)
    {
        _context = context;

        if (stat == null)
        {
            _currentStatus = "Creating new Stat";
            _stat = new StatDefinition();
        }
        else
        {
            _currentStatus = $"Editing Stat: {stat.DisplayName}";
            _stat = stat;
        }
        
        
        CreateComponents();
        RegisterEvents();
        
        var config = new StatEditorContext.Config()
        {
            GetEditorGrid = () => _editorGrid,
        };
        
        UiServices.ExtensionManager.ApplyExtensions(new UIRegion("BaseModule.StatEditor"), this, new StatEditorContext(config));
    }

    private void CreateComponents()
    {
        _editorGrid = new Grid
        {
            RowDefinitions = new RowDefinitions("*, Auto"),
            Margin = new Thickness(10),
        };
        this.Content = _editorGrid;
        
        _formScroller = new ScrollViewer
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };
        _editorGrid.Children.Add(_formScroller);
        
        _formPanel = new StackPanel
        {
            Spacing = 10,
        };
        _formScroller.Content = _formPanel;
        
        _nameTextBox = new TextBox
        {
            InnerLeftContent = "Name",
            Text = _stat.Name,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        _formPanel.Children.Add(_nameTextBox);
        ToolTip.SetTip(_nameTextBox, "The name of the stat.\n" +
                                    "For example, you could name a health stat 'Health' or 'HP', and a mana stat 'Mana' or 'MP'.\n" +
                                    "Names are just for display purposes, so you can use the same name for different stats if you want, but it is recommended to use unique names to avoid confusion.");
        
        _descriptionTextBox = new TextBox
        {
            InnerLeftContent = "Description",
            Text = _stat.Description,
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        _formPanel.Children.Add(_descriptionTextBox);
        ToolTip.SetTip(_descriptionTextBox, "A description for this stat.\n" +
                                            "This is optional and can be left empty, but it can be useful for providing additional information about the stat, such as its purpose or how it should be used.");
        
        _defaultValueNumeric = new NumericDoubleUpDown
        {
            InnerLeftContent = "Default Value",
            Value = _stat.DefaultValue,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        _formPanel.Children.Add(_defaultValueNumeric);
        ToolTip.SetTip(_defaultValueNumeric, "The default value for this stat.\n" +
                                             "This is the value that will be used when a player start a new game, or a new entity is spawned in the map.\n" +
                                             "This is still editable for each entity when they are created, but this is the default value that will be used if no other value is specified.");
        
        _minValueNumeric = new NumericDoubleUpDown
        {
            InnerLeftContent = "Min Value",
            Value = _stat.MinValue,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        _formPanel.Children.Add(_minValueNumeric);
        ToolTip.SetTip(_minValueNumeric, "The minimum value for this stat.\n" +
                                         "This is used for clamping the stat value and for validation purposes.");
        
        _buttonsPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 10,
            Margin = new Thickness(0, 10, 0, 0)
        };
        _editorGrid.Children.Add(_buttonsPanel);
        Grid.SetRow(_buttonsPanel, 1);
        
        _saveButton = new Button
        {
            Content = "Save",
            Padding = new Thickness(10, 5, 10, 5)
        };
        _buttonsPanel.Children.Add(_saveButton);
        
        _cancelButton = new Button
        {
            Content = "Cancel",
            Padding = new Thickness(10, 5, 10, 5)
        };
        _buttonsPanel.Children.Add(_cancelButton);
    }

    private void RegisterEvents()
    {
        
        _saveButton.Click += OnSave;
        
        _cancelButton.Click += OnCancel;
        
        AttachedToVisualTree += OnAttachedToVisualTree;
    }

    private void OnSave(object? sender, RoutedEventArgs e)
    {
        _stat.Name = _nameTextBox.Text ?? "";
        _stat.Description = _descriptionTextBox.Text ?? string.Empty;
        _stat.DefaultValue = _defaultValueNumeric.Value ?? 0;
        _stat.MinValue = _minValueNumeric.Value ?? 0;

        EngineServices.AssetsManager.GetDefaultPack().AddOrUpdateAsset(_stat);
        
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is Window window)
        {
            window.Title = "Assets Management";
        }
        
        _context.ShowAssetsPanel("Stats");
    }

    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is Window window)
        {
            window.Title = "Assets Management - " + _currentStatus;
        }
    }

    private void OnCancel(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is Window window)
        {
            window.Title = "Assets Management";
        }
        _context.ShowAssetsPanel("Stats");
    }
}