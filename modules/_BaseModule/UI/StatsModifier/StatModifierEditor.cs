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
using _BaseModule.Features.Entity;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Stats;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Modules.UIModule;
using RPGCreator.SDK.Types;
using RPGCreator.UI.Common;
using RPGCreator.UI.Contexts;
using RPGCreator.UI.Extensions;
using Ursa.Controls;
using AutoCompleteBox = Avalonia.Controls.AutoCompleteBox;
using Thickness = Avalonia.Thickness;

namespace _BaseModule.UI.StatsModifier;

public class StatModifierEditor : UserControl
{

    
    #region Components
    private Grid _editorGrid = null!;
    private ScrollViewer _editorScrollViewer = null!;
    private StackPanel _editorPanel = null!;
    
    private TextBox _nameTextBox = null!;
    private TextBox _descriptionTextBox = null!;
    
    #region Stat Selection Controls
    private Grid _selectStatPanel = null!;
    private Button _selectStatForModifier = null!;
    private TextBlock _selectedStatTextBlock = null!;
    #endregion
    
    #region Modifier Type Controls
    private Grid _modifierTypePanel = null!;
    private Label _modifierTypeLabel = null!;
    private ComboBox _modifierTypeComboBox = null!;
    #endregion
    
    #region Stacking Type Controls
    
    private Grid _stackingTypePanel = null!;
    private Label _stackingTypeLabel = null!;
    private ComboBox _stackingTypeComboBox = null!;

    #endregion
    
    private NumericFloatUpDown _valueUpDown = null!;

    #region Duration Controls
    private Grid _durationPanel = null!;
    
    private TextBlock _durationLabel = null!;

    private Label _durationHoursLabel = null!;
    private NumericDoubleUpDown _durationHoursUpDown = null!;
    
    private Label _durationMinutesLabel = null!;
    private NumericDoubleUpDown _durationMinutesUpDown = null!;
    
    private Label _durationSecondsLabel = null!;
    private NumericDoubleUpDown _durationSecondsUpDown = null!;
    
    private Label _durationMillisecondsLabel = null!;
    private NumericDoubleUpDown _durationMillisecondsUpDown = null!;
    #endregion
    
    #region Buttons
    
    private StackPanel _buttonsPanel = null!;
    private Button _saveButton = null!;
    private Button _cancelButton = null!;
    private HelpButton _helpButton = null!;
    
    #endregion
    
    #endregion
    
    #region Properties
    
    private AssetsManagerMenuContext _context;
    private StatModifierDefinition _definition;
    private Ulid _selectedStatId;
    
    double dur_hours, dur_minutes, dur_seconds, dur_milliseconds;
    
    #endregion
    
    public StatModifierEditor(AssetsManagerMenuContext context, StatModifierDefinition? definition = null)
    {
        _context = context;
        _definition = definition ?? EngineServices.AssetsManager.CreateAsset<StatModifierDefinition>();
        
        double totalMilliseconds = _definition.Duration.TotalMilliseconds;
        dur_hours = Math.Floor(totalMilliseconds / 3600000);
        totalMilliseconds -= dur_hours * 3600000;
        dur_minutes = Math.Floor(totalMilliseconds / 60000);
        totalMilliseconds -= dur_minutes * 60000;
        dur_seconds = Math.Floor(totalMilliseconds / 1000);
        totalMilliseconds -= dur_seconds * 1000;
        dur_milliseconds = totalMilliseconds;
        CreateComponents();
        RegisterEvents();
        

        if (_definition.StatId != Ulid.Empty)
        {
            if(EngineServices.AssetsManager.TryResolveAsset<BaseStatDefinition>(_definition.StatId, out var stat))
            {
                _selectedStatId = stat.Unique;
                _selectedStatTextBlock.Text = stat.DisplayName;
            }
            else
            {
                Logger.Warning($"Stat with ID {_definition.StatId} not found for stat modifier {_definition.Name}.");
                _selectedStatTextBlock.Text = "The stat associated with this modifier couldn't be found. It might have been deleted.";
            }
        }

        var config = new StatModifierEditorContext.Config()
        {
            GetEditorGrid = () => _editorGrid,
            GetEditorScrollViewer = () => _editorScrollViewer,
            GetEditorPanel = () => _editorPanel,
            GetNameTextBox = () => _nameTextBox,
            GetDescriptionTextBox = () => _descriptionTextBox,
            GetSelectStatPanel = () => _selectStatPanel,
            GetSelectStatForModifier = () => _selectStatForModifier,
            GetSelectedStatTextBlock = () => _selectedStatTextBlock,
            GetModifierTypePanel = () => _modifierTypePanel,
            GetModifierTypeLabel = () => _modifierTypeLabel,
            GetModifierTypeComboBox = () => _modifierTypeComboBox,
            GetStackingTypePanel = () => _stackingTypePanel,
            GetStackingTypeLabel = () => _stackingTypeLabel,
            GetStackingTypeComboBox = () => _stackingTypeComboBox,
            GetValueUpDown = () => _valueUpDown,
            GetDurationPanel = () => _durationPanel,
            GetDurationHoursLabel = () => _durationHoursLabel,
            GetDurationHoursUpDown = () => _durationHoursUpDown,
            GetDurationMinutesLabel = () => _durationMinutesLabel,
            GetDurationMinutesUpDown = () => _durationMinutesUpDown,
            GetDurationSecondsLabel = () => _durationSecondsLabel,
            GetDurationSecondsUpDown = () => _durationSecondsUpDown,
            GetDurationMillisecondsLabel = () => _durationMillisecondsLabel,
            GetDurationMillisecondsUpDown = () => _durationMillisecondsUpDown,
            GetButtonsPanel = () => _buttonsPanel,
            GetSaveButton = () => _saveButton,
            GetCancelButton = () => _cancelButton,
            GetHelpButton = () => _helpButton,
            GetStatModifier = () => _definition
        };
        
        EditorUiServices.ExtensionManager.ApplyExtensions(new UIRegion("BaseModule.StatModifierEditor"), this, new StatModifierEditorContext(config));
    }

    private void CreateComponents()
    {
        _editorGrid = new Grid()
        {
            RowDefinitions = new RowDefinitions("*, Auto")
        };
        this.Content = _editorGrid;

        #region Editor Panel
        _editorScrollViewer = new ScrollViewer();
        _editorGrid.Children.Add(_editorScrollViewer);
        
        _editorPanel = new StackPanel()
        {
            Margin = new Thickness(10),
            Spacing = 5
        };
        _editorScrollViewer.Content = _editorPanel;

        _nameTextBox = new TextBox()
        {
            InnerLeftContent = "Name",
            Text = _definition.Name
        };
        _editorPanel.Children.Add(_nameTextBox);
        
        _descriptionTextBox = new TextBox()
        {
            InnerLeftContent = "Description",
            AcceptsReturn = true,
            Height = 100,
            Text = _definition.Description
        };
        _editorPanel.Children.Add(_descriptionTextBox);

        _selectStatPanel = new Grid()
        {
            ColumnDefinitions = new ColumnDefinitions("Auto, *"),
            ColumnSpacing = 5
        };
        _editorPanel.Children.Add(_selectStatPanel);
        
        _selectStatForModifier = new Button()
        {
            Content = "Select Stat"
        };
        _selectStatPanel.Children.Add(_selectStatForModifier);
        
        _selectedStatTextBlock = new TextBlock()
        {
            Text = "No stat selected",
            VerticalAlignment = VerticalAlignment.Center
        };
        _selectStatPanel.Children.Add(_selectedStatTextBlock);
        Grid.SetColumn(_selectedStatTextBlock, 1);

        _modifierTypePanel = new Grid()
        {
            ColumnDefinitions = new ColumnDefinitions("Auto, *"),
            ColumnSpacing = 5
        };
        _editorPanel.Children.Add(_modifierTypePanel);

        _modifierTypeLabel = new Label()
        {
            Target = _modifierTypeComboBox,
            Content = "Modifier Type",
            VerticalAlignment = VerticalAlignment.Center
        };
        _modifierTypePanel.Children.Add(_modifierTypeLabel);
        
        _modifierTypeComboBox = new ComboBox()
        {
            ItemsSource = Enum.GetValues(typeof(StatModifierType)),
            SelectedIndex = (int)_definition.ModifierType,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        _modifierTypePanel.Children.Add(_modifierTypeComboBox);
        Grid.SetColumn(_modifierTypeComboBox, 1);

        _stackingTypePanel = new Grid()
        {
            ColumnDefinitions = new ColumnDefinitions("Auto, *"),
            ColumnSpacing = 5
        };
        _editorPanel.Children.Add(_stackingTypePanel);
        
        _stackingTypeLabel = new Label()
        {
            Target = _stackingTypeComboBox,
            Content = "Stacking Type",
            VerticalAlignment = VerticalAlignment.Center
        };
        _stackingTypePanel.Children.Add(_stackingTypeLabel);
        
        _stackingTypeComboBox = new ComboBox()
        {
            ItemsSource = Enum.GetValues(typeof(StatStackingPolicy)),
            SelectedIndex = (int)_definition.StackingPolicy,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        _stackingTypePanel.Children.Add(_stackingTypeComboBox);
        Grid.SetColumn(_stackingTypeComboBox, 1);

        _valueUpDown = new NumericFloatUpDown()
        {
            InnerLeftContent = "Value",
            Minimum = float.MinValue,
            Maximum = float.MaxValue,
            InnerRightContent = _definition.ModifierType == StatModifierType.Percent ? "%" : _definition.ModifierType == StatModifierType.Multiplier ? "x" : "",
            Value = _definition.Value
        };
        _editorPanel.Children.Add(_valueUpDown);
        
        _durationPanel = new Grid()
        {
            ColumnDefinitions = new ColumnDefinitions("*, *, *, *"),
            RowDefinitions = new RowDefinitions("Auto, Auto"),
            ColumnSpacing = 5,
            RowSpacing = 5
        };
        _editorPanel.Children.Add(_durationPanel);
        
        _durationLabel = new TextBlock()
        {
            Text = "Duration",
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        _durationPanel.Children.Add(_durationLabel);
        Grid.SetColumnSpan(_durationLabel, 4);
        
        _durationHoursLabel = new Label()
        {
            Content = "Hours"
        };
        ToolTip.SetTip(_durationHoursLabel, "Hours");
        
        _durationHoursUpDown = new NumericDoubleUpDown()
        {
            InnerRightContent = _durationHoursLabel,
            Minimum = 0,
            Maximum = int.MaxValue,
            Value = dur_hours
        };
        _durationPanel.Children.Add(_durationHoursUpDown);
        Grid.SetRow(_durationHoursUpDown, 1);
        
        _durationMinutesLabel = new Label()
        {
            Content = "Min"
        };
        ToolTip.SetTip(_durationMinutesLabel, "Minutes");
        
        _durationMinutesUpDown = new NumericDoubleUpDown()
        {
            InnerRightContent = _durationMinutesLabel,
            Minimum = 0,
            Maximum = 59,
            Value = dur_minutes
        };
        _durationPanel.Children.Add(_durationMinutesUpDown);
        Grid.SetColumn(_durationMinutesUpDown, 1);
        Grid.SetRow(_durationMinutesUpDown, 1);
        
        _durationSecondsLabel = new Label()
        {
            Content = "Sec"
        };
        ToolTip.SetTip(_durationSecondsLabel, "Seconds");
        
        _durationSecondsUpDown = new NumericDoubleUpDown()
        {
            InnerRightContent = _durationSecondsLabel,
            Minimum = 0,
            Maximum = 59,
            Value = dur_seconds
        };
        _durationPanel.Children.Add(_durationSecondsUpDown);
        Grid.SetColumn(_durationSecondsUpDown, 2);
        Grid.SetRow(_durationSecondsUpDown, 1);
        
        _durationMillisecondsLabel = new Label()
        {
            Content = "Ms"
        };
        ToolTip.SetTip(_durationMillisecondsLabel, "Milliseconds");
        
        _durationMillisecondsUpDown = new NumericDoubleUpDown()
        {
            InnerRightContent = _durationMillisecondsLabel,
            Minimum = 0,
            Maximum = 999,
            Value = dur_milliseconds
        };
        _durationPanel.Children.Add(_durationMillisecondsUpDown);
        Grid.SetColumn(_durationMillisecondsUpDown, 3);
        Grid.SetRow(_durationMillisecondsUpDown, 1);
        #endregion
        
        #region Buttons panel
        _buttonsPanel = new StackPanel()
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 5,
            Margin = new Thickness(10)
        };
        _editorGrid.Children.Add(_buttonsPanel);
        Grid.SetRow(_buttonsPanel, 1);
        
        _saveButton = new Button()
        {
            Content = "Save",
            HorizontalAlignment = HorizontalAlignment.Right
        };
        _buttonsPanel.Children.Add(_saveButton);
        
        _cancelButton = new Button()
        {
            Content = "Cancel",
            HorizontalAlignment = HorizontalAlignment.Right
        };
        _buttonsPanel.Children.Add(_cancelButton);
        
        _helpButton = new HelpButton(new URN("rpgc", "docs", "stat_modifier_editor"))
        {
            HorizontalAlignment = HorizontalAlignment.Right
        };
        _buttonsPanel.Children.Add(_helpButton);
        ToolTip.SetTip(_helpButton, "Access the documentation for stat modifiers.");
        
        #endregion
    }

    private void RegisterEvents()
    {
        _saveButton.Click += OnSave;
        _cancelButton.Click += OnCancel;
        _selectStatForModifier.Click += OnSelectStat;
        _modifierTypeComboBox.SelectionChanged += OnModifierTypeChanged;
    }

    private void OnModifierTypeChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_modifierTypeComboBox.SelectedItem is StatModifierType selectedType)
        {
            if (selectedType == StatModifierType.Percent)
            {
                _valueUpDown.InnerRightContent = "%";
                _valueUpDown.Watermark = "e.g. 10 for +10% or -5 for -5% that will be applied to the stat.";
            }
            else if (selectedType == StatModifierType.Flat)
            {
                _valueUpDown.InnerRightContent = "";
                _valueUpDown.Watermark = "Flat value to add or subtract from the stat.";
            }
            else if (selectedType == StatModifierType.Multiplier)
            {
                _valueUpDown.InnerRightContent = "x";
                _valueUpDown.Watermark = "e.g. 1.5 for +50% or 0.8 for -20% that will be applied to the stat once percent and flat modifiers have been applied.";
            }
        }
    }

    private async void OnSelectStat(object? sender, RoutedEventArgs e)
    {
        var stats = EngineServices.AssetsManager.GetAssetsOfType<BaseStatDefinition>();
        var selectedStat = await EditorUiServices.DialogService.ShowSelectAsync(
            "Select stat",
            "Select the stat to which this modifier will be applied.",
            stats,
            definition => definition.Name,
            confirmButtonText: "Select",
            cancelButtonText: "Cancel");

        if (selectedStat != null)
        {
            _selectedStatId = selectedStat.Unique;
            _selectedStatTextBlock.Text = selectedStat.DisplayName;
        }
    }

    private void OnSave(object? sender, RoutedEventArgs e)
    {
        _definition.Name = _nameTextBox.Text ?? "";
        _definition.Description = _descriptionTextBox.Text ?? string.Empty;
        _definition.ModifierType = (StatModifierType)_modifierTypeComboBox.SelectedItem!;
        _definition.StackingPolicy = (StatStackingPolicy)_stackingTypeComboBox.SelectedItem!;
        _definition.Value = _valueUpDown.Value ?? 0;
        _definition.Duration = TimeSpan.FromHours(_durationHoursUpDown.Value ?? 0) +
                               TimeSpan.FromMinutes(_durationMinutesUpDown.Value ?? 0) +
                               TimeSpan.FromSeconds(_durationSecondsUpDown.Value ?? 0) +
                               TimeSpan.FromMilliseconds(_durationMillisecondsUpDown.Value ?? 0);
        _definition.StatId = _selectedStatId;
        
        var pack = EngineServices.AssetsManager.GetDefaultPack();

        if (pack == null)
            return;
        
        EngineServices.AssetsManager.RegisterAsset(_definition);
        pack.AddOrUpdateAsset(_definition);
        
        _context.ShowAssetsPanel("Stat Modifiers");
    }
    
    private void OnCancel(object? sender, RoutedEventArgs e)
    {
        _context.ShowAssetsPanel("Stat Modifiers");
    }
}