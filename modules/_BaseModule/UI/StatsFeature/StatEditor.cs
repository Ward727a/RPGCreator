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
using _BaseModule.UI.SimpleEvents;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Stats;
using RPGCreator.SDK.EditorUiService;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Modules.UIModule;
using RPGCreator.SDK.Types;
using RPGCreator.UI.Common;
using RPGCreator.UI.Contexts;
using RPGCreator.UI.Extensions;
using Ursa.Controls;
using Thickness = Avalonia.Thickness;

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
    // Define if the stat value can be negative or not (for example, a "Health" stat usually can't be negative, but a "Money" stat can be negative if the player is in debt).
    private ToggleSwitch _statCanBeNegativeToggle = null!;
    // Define if the stat is visible or not inside the UI (in-game!)
    private ToggleSwitch _statIsVisibleInUiToggle = null!;
    private ComboBox _statTypeKindComboBox = null!;
    
    private Expander _statCapSettingsExpander = null!;
    
    private Grid _statCapSettingsGrid = null!;
    private StackPanel _statCapSettingsPanel = null!;
    
    private ComboBox _statCapTypeComboBox = null!;
    // If the cap type is "ByValue"
    private NumericDoubleUpDown _statCapValueNumeric = null!;
    // If the cap type is "ByStat"
    private StackPanel _statCapStatSelectPanel = null!;
    private Button _statCapStatUniqueIdButton = null!;
    private TextBlock _statCapStatNameTextBlock = null!;
    
    private StackPanel _buttonsPanel = null!;
    private Button _saveButton = null!;
    private Button _cancelButton = null!;
    private HelpButton _statHelpButton = null!;
    #endregion
    
    #region Properties
    
    private readonly AssetsManagerMenuContext _context;
    private BaseStatDefinition _stat;
    private string _currentStatus = "Editing ???";
    private Ulid _statCapUnique = Ulid.Empty;
    
    #endregion

    public StatEditor(AssetsManagerMenuContext context, BaseStatDefinition? stat = null)
    {
        _context = context;

        if (stat == null)
        {
            _currentStatus = "Creating new Stat";
            _stat = EngineServices.AssetsManager.CreateAsset<StatDefinition>();
        }
        else
        {
            _currentStatus = $"Editing Stat: {stat.DisplayName}";
            _stat = stat;
            _statCapUnique = stat.CapSettings.CapStatUnique;

        }
        
        
        CreateComponents();
        
        if (_statCapUnique != Ulid.Empty)
        {
            if(EngineServices.AssetsManager.TryResolveAsset(_statCapUnique, out BaseStatDefinition? capStat))
            {
                _statCapStatNameTextBlock.Text = capStat.DisplayName;
            }
            else
            {
                _statCapStatNameTextBlock.Text = "Selected stat not found (it may have been deleted)";
            }
        }

        _statCapValueNumeric.IsVisible = _stat.CapSettings.CapType == EStatTypeCap.ByValue;
        _statCapStatSelectPanel.IsVisible = _stat.CapSettings.CapType == EStatTypeCap.ByStat;
        
        RegisterEvents();
        
        var config = new StatEditorContext.Config()
        {
            GetEditorGrid = () => _editorGrid,
        };
        
        EditorUiServices.ExtensionManager.ApplyExtensions(new UIRegion("BaseModule.StatEditor"), this, new StatEditorContext(config));
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
        
        _statCanBeNegativeToggle = new ToggleSwitch
        {
            Content = "Can be Negative",
            IsChecked = _stat.CanBeNegative,
            HorizontalAlignment = HorizontalAlignment.Left
        };
        _formPanel.Children.Add(_statCanBeNegativeToggle);
        ToolTip.SetTip(_statCanBeNegativeToggle, "Whether this stat can have negative values or not.\n" +
                                                  "For example, a 'Health' stat usually can't be negative, but a 'Money' stat can be negative if the player is in debt.");
        
        _statIsVisibleInUiToggle = new ToggleSwitch
        {
            Content = "Is Visible in UI",
            IsChecked = _stat.IsVisible,
            HorizontalAlignment = HorizontalAlignment.Left
        };
        _formPanel.Children.Add(_statIsVisibleInUiToggle);
        ToolTip.SetTip(_statIsVisibleInUiToggle, "Whether this stat is visible in the game UI or not.\n" +
                                                  "For example, a 'Health' stat is usually visible in the UI, while a 'Stealth' stat might not be visible in the UI and only used for calculations.");
        _statTypeKindComboBox = new ComboBox
        {
            ItemsSource = Enum.GetValues<EStatTypeKind>(),
            SelectedItem = _stat.TypeKind,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        _formPanel.Children.Add(_statTypeKindComboBox);
        ToolTip.SetTip(_statTypeKindComboBox, "The type kind of this stat, used to categorize the stat and determine how it is used in the game.");
        
        _statCapSettingsExpander = new Expander
        {
            Header = "Stat Cap Settings",
            IsExpanded = false,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        _formPanel.Children.Add(_statCapSettingsExpander);
        
        _statCapSettingsGrid = new Grid
        {
            RowDefinitions = new RowDefinitions("*"),
            Margin = new Thickness(10)
        };
        _statCapSettingsExpander.Content = _statCapSettingsGrid;
        
        _statCapSettingsPanel = new StackPanel
        {
            Spacing = 10
        };
        _statCapSettingsGrid.Children.Add(_statCapSettingsPanel);
        
        _statCapTypeComboBox = new ComboBox
        {
            ItemsSource = Enum.GetValues<EStatTypeCap>(),
            SelectedItem = _stat.CapSettings.CapType,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        _statCapSettingsPanel.Children.Add(_statCapTypeComboBox);
        ToolTip.SetTip(_statCapTypeComboBox,
            "The cap type for this stat, used to determine how the stat is capped, either by a fixed value or by another stat.");
        
        _statCapValueNumeric = new NumericDoubleUpDown
        {
            InnerLeftContent = "Cap Value",
            Value = _stat.CapSettings.CapValue,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        _statCapSettingsPanel.Children.Add(_statCapValueNumeric);
        
        _statCapStatSelectPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            IsVisible = false
        };
        _statCapSettingsPanel.Children.Add(_statCapStatSelectPanel);
        
        _statCapStatUniqueIdButton = new Button
        {
            Content = "Select Stat",
            Margin = new Thickness(0, 0, 5, 0)
        };
        _statCapStatSelectPanel.Children.Add(_statCapStatUniqueIdButton);
        
        _statCapStatNameTextBlock = new TextBlock
        {
            Text = "No stat selected",
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        _statCapStatSelectPanel.Children.Add(_statCapStatNameTextBlock);
        
        _buttonsPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 5,
        };
        _editorGrid.Children.Add(_buttonsPanel);
        Grid.SetRow(_buttonsPanel, 1);
        
        _saveButton = new Button
        {
            Content = "Save",
        };
        _buttonsPanel.Children.Add(_saveButton);
        
        _cancelButton = new Button
        {
            Content = "Cancel",
        };
        _buttonsPanel.Children.Add(_cancelButton);
        
        _statHelpButton = new HelpButton(new URN("rpgc", "docs", "stat_editor"));
        _buttonsPanel.Children.Add(_statHelpButton);

        var testButtonEventEditor = new Button()
        {
            Content = "Test Stat Events Editor",
        };
        _buttonsPanel.Children.Add(testButtonEventEditor);
        testButtonEventEditor.Click += (s, e) =>
        {
            _context.OpenCustom(new SimpleEventEditor());
        };
    }

    private void RegisterEvents()
    {
        
        _saveButton.Click += OnSave;
        _cancelButton.Click += OnCancel;
        _statCapStatUniqueIdButton.Click += SelectCapStat;
        _statCapTypeComboBox.SelectionChanged += SetStatCapType;
        
        AttachedToVisualTree += OnAttachedToVisualTree;
    }

    private void SetStatCapType(object? sender, SelectionChangedEventArgs e)
    {
        if (_statCapTypeComboBox.SelectedItem is EStatTypeCap selectedCapType)
        {
            _statCapValueNumeric.IsVisible = selectedCapType == EStatTypeCap.ByValue;
            _statCapStatSelectPanel.IsVisible = selectedCapType == EStatTypeCap.ByStat;
        }
    }

    private async void SelectCapStat(object? sender, RoutedEventArgs e)
    {
        var stats = EngineServices.AssetsManager.GetAssetsOfType<BaseStatDefinition>();
        
        // Sort stats to remove itself, and all stats that have a cap stat that is this one (to avoid circular references)
        stats = GetAvailableCapStats(_stat, stats.ToList());

        var selectedStat = await EditorUiServices.DialogService.ShowSelectAsync(
            "Select Stat for Cap",
            "Select the stat that will be used as a cap for this stat. The value of the capped stat will not be able to exceed the value of the cap stat.\n" +
            "\n" +
            "If you don't see the stat you want to use as a cap in the list make sure it doesn't have as a cap stat the stat you are currently editing.",
            stats,
            (stat) => stat.DisplayName,
            new DialogStyle(800, 500, CanResize:false, SizeToContent:DialogSizeToContent.None));
        
        if (selectedStat != null)
        {
            _statCapStatNameTextBlock.Text = selectedStat.DisplayName;
            _statCapUnique = selectedStat.Unique;
        }
    }
    
    public List<BaseStatDefinition> GetAvailableCapStats(BaseStatDefinition currentStat, List<BaseStatDefinition> allStats)
    {
        // On retire la stat elle-même (on ne peut pas se caper sur soi-même)
        // Et on retire toutes celles qui créeraient un cycle
        return allStats.Where(s => 
            s.Unique != currentStat.Unique && 
            !WillCreateCycle(currentStat, s, allStats)
        ).ToList();
    }

    private bool WillCreateCycle(BaseStatDefinition statBeingEdited, BaseStatDefinition potentialCap, List<BaseStatDefinition> allStats)
    {
        var lookup = allStats.ToDictionary(s => s.Unique);
        var current = potentialCap;
    
        while (current != null && current.CapSettings.CapType == EStatTypeCap.ByStat)
        {
            if (current.CapSettings.CapStatUnique == statBeingEdited.Unique)
                return true;

            if (!lookup.TryGetValue(current.CapSettings.CapStatUnique, out current))
                break;
        }

        return false;
    }

    private void OnSave(object? sender, RoutedEventArgs e)
    {
        _stat.Name = _nameTextBox.Text ?? "";
        _stat.Description = _descriptionTextBox.Text ?? string.Empty;
        _stat.DefaultValue = _defaultValueNumeric.Value ?? 0;
        _stat.MinValue = _minValueNumeric.Value ?? 0;
        _stat.CanBeNegative = _statCanBeNegativeToggle.IsChecked ?? false;
        _stat.IsVisible = _statIsVisibleInUiToggle.IsChecked ?? false;
        _stat.TypeKind = _statTypeKindComboBox.SelectedItem is EStatTypeKind selectedTypeKind ? selectedTypeKind : EStatTypeKind.Resource;
        _stat.CapSettings = new StatCapSettings()
        {
            CapType = _statCapTypeComboBox.SelectedItem is EStatTypeCap selectedCapType
                ? selectedCapType
                : EStatTypeCap.ByValue,
            CapValue = _statCapValueNumeric.Value ?? 0,
            CapStatUnique = _statCapUnique
        };
        
        if(_stat.CapSettings.CapType == EStatTypeCap.ByStat && _stat.CapSettings.CapStatUnique == Ulid.Empty)
        {
            EditorUiServices.NotificationService.Error("Invalid Cap Stat", "You have selected 'ByStat' as the cap type, but you haven't selected a stat to use as a cap.\n" +
                                                                     "Please select a stat to use as a cap or change the cap type to 'ByValue'.", new NotificationOptions(10000));
            return;
        }

        EngineServices.AssetsManager.GetDefaultPack().AddOrUpdateAsset(_stat);
        var pointsTest = EngineServices.AssetsManager.GetDefaultPack().GetWhoPointsToAsset(_stat.Unique);
        
        Logger.Debug("DEBUG REFERENCES: {reference}", args: pointsTest);
        
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