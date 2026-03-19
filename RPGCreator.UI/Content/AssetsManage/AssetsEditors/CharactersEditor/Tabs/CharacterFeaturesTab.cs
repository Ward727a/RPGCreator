using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using CommunityToolkit.Diagnostics;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Characters;
using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.Commands;
using RPGCreator.SDK.EditorUiService;
using RPGCreator.SDK.EngineService;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Modules.Features.Entity;
using RPGCreator.SDK.Modules.UIModule;
using RPGCreator.SDK.Types;
using RPGCreator.UI.Common.Modal.Browser;
using RPGCreator.UI.Contexts;
using Ursa.Controls;
using Thickness = Avalonia.Thickness;

namespace RPGCreator.UI.Content.AssetsManage.AssetsEditors.CharactersEditor.Tabs;

public class CharacterFeaturesTab : UserControl
{
    #region Events

    #endregion

    #region Properties

    public CharacterData Data;

    #endregion

    #region Components

    private Grid Body { get; set; } = null!;
    private Button AddFeatureButton { get; set; } = null!;
    private StackPanel FeaturesList { get; set; } = null!;

    #endregion

    #region Constructors

    public CharacterFeaturesTab(CharacterData data)
    {
        Data = data;
        Name = "Features"; // Define the name of the tab
        CreateComponents();
        RegisterEvents();
        Content = Body;
        LoadFeatures();
    }

    #endregion

    #region Methods

    private void CreateComponents()
    {
        Body = new Grid()
        {
            RowDefinitions = new RowDefinitions("Auto, *"),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Margin = new Thickness(10)
        };

        AddFeatureButton = new Button
        {
            Content = "Add Feature",
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(0, 0, 0, 10)
        };

        Body.Children.Add(AddFeatureButton);


        var scrollContainer = new ScrollViewer()
        {
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
        };

        FeaturesList = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Margin = new Thickness(10)
        };
        scrollContainer.Content = FeaturesList;

        Body.Children.Add(scrollContainer);
        Grid.SetRow(scrollContainer, 1);
    }

    private void RegisterEvents()
    {
        AddFeatureButton.Click += OnAddFeatureButtonClick;
    }

    private void LoadFeatures()
    {
        Guard.IsNotNull(Data);
        foreach (var featureData in Data.Features)
        {
            if (featureData.IsSubFeature) continue;
            var featureControl = new FeatureItemControl(featureData.ToEntityFeature(), featureData.InstanceId);
            FeaturesList.Children.Add(featureControl);
            featureControl.FeatureRemoved += OnFeatureRemoved;
        }
    }

    private void OnFeatureRemoved(Ulid instanceId, IEntityFeature feature, FeatureItemControl control)
    {
        var removeCmd = new FeatureRemovedCommand(instanceId, feature, FeaturesList, Data, control).WhenExecuted((_) =>
        {
            control.FeatureRemoved -= OnFeatureRemoved;
        }).WhenUndone((_) =>
        {
            control.FeatureRemoved += OnFeatureRemoved;
        });
        EngineServices.UndoRedoService.ExecuteCommand(removeCmd);
    }

    #endregion

    #region Events Handlers

    /// <summary>
    /// A command representing the addition of a feature to the character.<br/>
    /// So that it can be undone/redone.
    /// </summary>
    private class FeatureAddedCommand : BaseCommand
    {
        /// <summary>
        /// The instance id of the feature added.<br/>
        /// This is used to identify the feature instance in the character data.<br/>
        /// It is generated when the feature is added.
        /// </summary>
        public Ulid FeatureInstanceId { get; private set; }
        
        private readonly IEntityFeature _feature;
        private readonly StackPanel _featuresList;
        private readonly CharacterData _characterData;
        public FeatureItemControl? FeatureControl { get; private set; }
        public override string Name { get; }

        public FeatureAddedCommand(IEntityFeature feature, StackPanel featuresList, CharacterData characterData)
        {
            _feature = feature;
            _featuresList = featuresList;
            _characterData = characterData;

            var displayableName = string.IsNullOrWhiteSpace(_feature.FeatureName)
                ? _feature.FeatureUrn.ToString()
                : _feature.FeatureName;

            if (displayableName.Length > 100)
                displayableName = string.Concat(displayableName.AsSpan(0, 97), "...");

            Name = $"Add Feature: {displayableName}";
        }

        protected override void OnExecute()
        {
            FeatureInstanceId = _characterData.AddFeatureConfig(_feature);
            FeatureControl ??= new FeatureItemControl(_feature, FeatureInstanceId);
            _feature.OnAddingToDefinition(_characterData, FeatureControl.Context);
            _featuresList.Children.Add(FeatureControl);
            _feature.OnAddedToDefinition(_characterData);
        }

        protected override void OnUndo()
        {
            if (FeatureControl != null)
            {
                _featuresList.Children.Remove(FeatureControl);
                if (_characterData.RemoveFeatureConfig(FeatureInstanceId))
                {
                    FeatureInstanceId = Ulid.Empty;
                }

                _feature.OnRemovedFromDefinition(_characterData);
            }
            else
                Logger.Error("Cannot undo feature addition, feature control is null");
        }
    }

    private class FeatureRemovedCommand : BaseCommand
    {
        public override string Name { get; }

        private Ulid _featureInstanceId;
        private readonly IEntityFeature _feature;
        private readonly StackPanel _featuresList;
        private readonly CharacterData _characterData;
        private FeatureItemControl _featureControl;

        public FeatureRemovedCommand(Ulid featureInstanceIdInstanceId, IEntityFeature feature, StackPanel featuresList,
            CharacterData characterData, FeatureItemControl featureControl)
        {
            _featureInstanceId = featureInstanceIdInstanceId;
            _feature = feature;
            _featuresList = featuresList;
            _characterData = characterData;
            _featureControl = featureControl;

            var displayableName = string.IsNullOrWhiteSpace(feature.FeatureName)
                ? feature.FeatureUrn.ToString()
                : feature.FeatureName;

            if (displayableName.Length > 100)
                displayableName = string.Concat(displayableName.AsSpan(0, 97), "...");

            Name = $"Remove Feature: {displayableName}";
        }

        protected override void OnExecute()
        {
            Guard.IsNotNull(_featureControl);
            Guard.IsNotEqualTo(_featureInstanceId, Ulid.Empty);
            
            _featuresList.Children.Remove(_featureControl);
            
            _feature.Configuration.DisposeEvents();
            if (_characterData.RemoveFeatureConfig(_featureInstanceId))
            {
                _featureInstanceId = Ulid.Empty;
            }

            _feature.OnRemovedFromDefinition(_characterData);
        }

        protected override void OnUndo()
        {
            _feature.OnAddingToDefinition(_characterData, _featureControl.Context);
            _featuresList.Children.Add(_featureControl);
            _featureInstanceId = _characterData.AddFeatureConfig(_feature);
            _feature.OnAddedToDefinition(_characterData);
        }
    }

    /// <summary>
    /// Just a test list to keep track of added features in this session.<br/>
    /// This will not be kept in the final implementation.
    /// </summary>
    private readonly List<IEntityFeature> _addedFeatures = new();

    async void OnAddFeatureButtonClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            Logger.Debug("Add Feature button clicked");

            var dialog = new FeatureLibraryExplorerDialog();
            var result = await EditorUiServices.DialogService.ConfirmAsync("Add Feature", dialog,
                new DialogStyle(Height: 450, Width: 500, CanResize: true, SizeToContent: DialogSizeToContent.None));

            if (result && dialog.HasFeature)
            {
                var feature = dialog.GetSelectedFeature();
                Logger.Debug("Feature selected: {featureName}", feature.FeatureName);
                var cmd = new FeatureAddedCommand(feature, FeaturesList, Data).WhenExecuted((cmd) =>
                {
                    if(cmd is not FeatureAddedCommand addedCmd)
                    {
                        Logger.Error("Executed command is not of type FeatureAddedCommand");
                        return;
                    }
                    _addedFeatures.Add(feature);
                    addedCmd.FeatureControl?.FeatureRemoved += OnFeatureRemoved;
                    
                    EditorUiServices.NotificationService.Success("Feature Added",
                        $"The feature '{feature.FeatureName}' has been added.",
                        new NotificationOptions(5000));
                }).WhenUndone((cmd) =>
                {
                    if(cmd is not FeatureAddedCommand addedCmd)
                    {
                        Logger.Error("Undone command is not of type FeatureAddedCommand");
                        return;
                    }
                    _addedFeatures.Remove(feature);
                    addedCmd.FeatureControl?.FeatureRemoved -= OnFeatureRemoved;
                    
                    EditorUiServices.NotificationService.Info("Feature Removed",
                        $"The feature '{feature.FeatureName}' has been removed.",
                        new NotificationOptions(5000));
                });
                EngineServices.UndoRedoService.ExecuteCommand(cmd);
            }
        }
        catch (Exception error)
        {
            Logger.Error("Error while adding feature: {error}", error.Message);
        }

        CheckDependencies();
    }

    /// <summary>
    /// DO NOT USE!!!!<br/>
    /// This is just a "dev" method, this is right now not the final implementation of dependency checking.<br/>
    /// It will just log warnings and show notifications for missing dependencies!!!<br/>
    /// DO NOT USE FOR THE RELEASE!!!
    /// </summary>
    public void CheckDependencies()
    {
        var presentFeatures = new HashSet<URN>();
        foreach (var entityFeature in _addedFeatures)
        {
            presentFeatures.Add(entityFeature.FeatureUrn);
        }

        foreach (var entityFeature in _addedFeatures)
        {
            var dependencies = entityFeature.DependentFeatures;
            foreach (var dependency in dependencies)
            {
                if (!presentFeatures.Contains(dependency))
                {
                    Logger.Warning("Feature {feature} is missing dependency {dependency}", entityFeature.FeatureUrn,
                        dependency);
                    
                    // Here you could also notify the user via UI
                    EditorUiServices.NotificationService.Error("Feature Dependency Missing",
                        $"The feature '{entityFeature.FeatureName}' is missing the required dependency feature '{dependency}'.",
                        new NotificationOptions(10000));
                }
            }
        }
    }

    #endregion
}

public class FeatureItemControl : UserControl
{
    public event Action<Ulid, IEntityFeature, FeatureItemControl>? FeatureRemoved;

    [ExposePropToPlugin("CharacterFeaturesEditor.FeatureItem")]
    public IEntityFeature Feature { get; set; } = null!;
    
    [ExposePropToPlugin("CharacterFeaturesEditor.FeatureItem")]
    public Ulid FeatureInstanceId { get; set; } = Ulid.Empty;

    [ExposePropToPlugin("CharacterFeaturesEditor.FeatureItem")]
    public Expander PropExpander { get; set; } = null!;

    [ExposePropToPlugin("CharacterFeaturesEditor.FeatureItem")]
    public StackPanel ExpanderContent { get; set; } = null!;

    [ExposePropToPlugin("CharacterFeaturesEditor.FeatureItem")]
    private Grid ExpanderBodyGrid { get; set; } = null!;

    [ExposePropToPlugin("CharacterFeaturesEditor.FeatureItem")]
    private StackPanel LeftExpanderContentPanel { get; set; } = null!;

    [ExposePropToPlugin("CharacterFeaturesEditor.FeatureItem")]
    private Image? FeatureIconImage { get; set; }

    [ExposePropToPlugin("CharacterFeaturesEditor.FeatureItem")]
    private TextBlock FeatureNameLabel { get; set; } = null!;

    [ExposePropToPlugin("CharacterFeaturesEditor.FeatureItem")]
    private Button FeatureDeleteButton { get; set; } = null!;

    [ExposePropToPlugin("CharacterFeaturesEditor.FeatureItem")]
    private TextBlock FeatureDescription { get; set; } = null!;

    public readonly CharacterFeaturesEditorFeatureItemContext Context;

    public FeatureItemControl(IEntityFeature feature, Ulid featureInstanceId)
    {
        Feature = feature;
        FeatureInstanceId = featureInstanceId;
        CreateComponents();
        LoadProperties();
        RegisterEvents();

        var config = new CharacterFeaturesEditorFeatureItemContext.Config()
        {
            GetFeature = () => Feature,
            GetPropExpander = () => PropExpander,
            GetExpanderContent = () => ExpanderContent,
            GetExpanderBodyGrid = () => ExpanderBodyGrid,
            GetLeftExpanderContentPanel = () => LeftExpanderContentPanel,
            GetFeatureIconImage = () => FeatureIconImage,
            GetFeatureNameLabel = () => FeatureNameLabel,
            GetFeatureDeleteButton = () => FeatureDeleteButton,
            GetFeatureDescription = () => FeatureDescription,
            CreatePropertyControl = CreatePropertyControl,
            GetOrCreateCategory = GetOrCreateCategory,
            CreateValidInput = CreateValidInput,
            LoadProperties = LoadProperties
        };

        Context = new CharacterFeaturesEditorFeatureItemContext(config);

        EditorUiServices.ExtensionManager.ApplyExtensions(UIRegion.CharacterFeaturesEditorFeatureItem, this,
            new CharacterFeaturesEditorFeatureItemContext(config));
    }

    private void CreateComponents()
    {
        PropExpander = new Expander
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 0, 0, 5)
        };

        ExpanderBodyGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto, *, Auto"),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5),
        };
        PropExpander.Header = ExpanderBodyGrid;

        LeftExpanderContentPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center
        };
        ExpanderBodyGrid.Children.Add(LeftExpanderContentPanel);

        if (File.Exists(Feature.FeatureIcon))
        {
            FeatureIconImage = new Image
            {
                Source = EngineServices.ResourcesService.Load<Bitmap>(Feature.FeatureIcon),
                Width = 32,
                Height = 32,
                Margin = new Thickness(0, 0, 10, 0)
            };
            LeftExpanderContentPanel.Children.Add(FeatureIconImage);
        }

        FeatureNameLabel = new TextBlock
        {
            Text = Feature.FeatureName,
            FontWeight = FontWeight.Bold,
            FontSize = 18,
        };
        LeftExpanderContentPanel.Children.Add(FeatureNameLabel);

        FeatureDeleteButton = new Button
        {
            Content = "Remove",
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5, 0, 0, 0)
        };
        ExpanderBodyGrid.Children.Add(FeatureDeleteButton);
        Grid.SetColumn(FeatureDeleteButton, 2);

        ExpanderContent = new StackPanel()
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(5),
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        PropExpander.Content = ExpanderContent;

        FeatureDescription = new TextBlock()
        {
            Inlines = new InlineCollection()
        };
        FeatureDescription.Inlines.Add(new Run
            { Text = "Description: ", Foreground = Brushes.Gray, FontWeight = FontWeight.Bold });
        FeatureDescription.Inlines.Add(new Run { Text = Feature.FeatureDescription });
        ExpanderContent.Children.Add(FeatureDescription);

        Content = PropExpander;

#if DEBUG
        if (Feature is BaseMacroEntityFeature baseMacroEntityFeature)
        {
            var debugSubFeature = new Expander()
            {
                Header = "[DEBUG] Sub-Features",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 5, 0, 5)
            };

            var debugSubFeaturePanel = new StackPanel()
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            debugSubFeature.Content = debugSubFeaturePanel;
            ExpanderContent.Children.Add(debugSubFeature);
            debugSubFeaturePanel.Children.Add(new TextBlock()
            {
                Text = "This group add the following sub-features:"
            });
            foreach (var subFeature in baseMacroEntityFeature.RequiredFeatures)
            {
                debugSubFeaturePanel.Children.Add(new TextBlock()
                {
                    Text = $"{subFeature.FeatureName} ({subFeature.FeatureUrn})",
                    Margin = new Thickness(0, 2, 0, 2)
                });
            }
        }
#endif
    }

    private void RegisterEvents()
    {
        FeatureDeleteButton.Click += OnDeleteButtonClicked;
    }

    private void OnDeleteButtonClicked(object? sender, RoutedEventArgs e)
    {
        FeatureRemoved?.Invoke(FeatureInstanceId, Feature, this);
    }

    [ExposeToPlugin("CharacterFeaturesEditor.FeatureItem")]
    private void LoadProperties()
    {
        Dictionary<string, Expander> propCategories = new();
        var propertiesList = EngineServices.FeaturesManager.GetEntityProperties(Feature.FeatureUrn);

        var entityFeaturePropertyMetadatas = propertiesList.ToList();

        if (entityFeaturePropertyMetadatas.Count == 0) return;
        
        foreach (var propertyMetadata in entityFeaturePropertyMetadatas)
        {
            CreatePropertyControl(propertyMetadata, propCategories);
        }
    }

    [ExposeToPlugin("CharacterFeaturesEditor.FeatureItem")]
    private void CreatePropertyControl(EntityFeaturePropertyMetadata propertyMetadata,
        Dictionary<string, Expander> propCategories)
    {
        var categoryPanel = GetOrCreateCategory(propertyMetadata.Attribute.Category, propCategories);

        var propGrid = new Grid()
        {
            ColumnDefinitions = new ColumnDefinitions("Auto, *"),
            RowDefinitions = new RowDefinitions("Auto, Auto"),
            ColumnSpacing = 10,
            RowSpacing = 10,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Margin = new Thickness(0, 5, 0, 5)
        };
        categoryPanel.Children.Add(propGrid);

        var propText = new TextBlock()
        {
            Inlines = new InlineCollection(),
            VerticalAlignment = VerticalAlignment.Center
        };

        propText.Inlines.Add(new Run { Text = "Property: ", Foreground = Brushes.Gray, FontWeight = FontWeight.Bold });
        propText.Inlines.Add(new Run { Text = propertyMetadata.Attribute.Name });
        propGrid.Children.Add(propText);

        var propDesc = new TextBlock()
        {
            Inlines = new InlineCollection(),
            TextWrapping = TextWrapping.Wrap,
            VerticalAlignment = VerticalAlignment.Center
        };
        propDesc.Inlines.Add(
            new Run { Text = "Description: ", Foreground = Brushes.Gray, FontWeight = FontWeight.Bold });
        propDesc.Inlines.Add(new Run { Text = propertyMetadata.Attribute.Description });
        propGrid.Children.Add(propDesc);
        Grid.SetRow(propDesc, 1);
        Grid.SetColumnSpan(propDesc, 2);

        var inputControl = CreateValidInput(propertyMetadata);
        inputControl.HorizontalAlignment = HorizontalAlignment.Stretch;
        propGrid.Children.Add(inputControl);
        Grid.SetColumn(inputControl, 1);

        categoryPanel.Children.Add(new Divider
        {
            Margin = new Thickness(10)
        });
    }

    /// <summary>
    /// Gets or creates a category panel based on the category path.
    /// </summary>
    /// <param name="categoryPath">The category path, separated by '/'.</param>
    /// <param name="propCategories">The dictionary of existing categories.</param>
    /// <returns>The StackPanel for the category.</returns>
    [ExposeToPlugin("CharacterFeaturesEditor.FeatureItem")]
    private StackPanel GetOrCreateCategory(string categoryPath, Dictionary<string, Expander> propCategories)
    {
        var categories = categoryPath.Split('/');
        StackPanel currentPanel = ExpanderContent;
        string currentPath = "";

        foreach (var category in categories)
        {
            currentPath = string.IsNullOrEmpty(currentPath) ? category : $"{currentPath}/{category}";
            if (!propCategories.TryGetValue(currentPath, out var expander))
            {
                expander = new Expander
                {
                    Header = category,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(0, 5, 0, 5)
                };
                var panel = new StackPanel()
                {
                    Orientation = Orientation.Vertical,
                    Margin = new Thickness(5),
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };
                expander.Content = panel;
                currentPanel.Children.Add(expander);
                propCategories[currentPath] = expander;
            }

            if (expander.Content is not StackPanel)
            {
                Logger.Error("Expander content is not a StackPanel for category {category}", currentPath);
                ExpanderContent.Children.Add(new TextBlock() { Text = "[ERROR] Invalid category content." });
                return ExpanderContent;
            }

            currentPanel = (StackPanel)expander.Content;
        }

        return currentPanel;
    }

    /// <summary>
    /// Creates a valid input control based on the property type.
    /// </summary>
    /// <param name="propertyMetadata">The property metadata.</param>
    /// <returns>The created input control.</returns>
    [ExposeToPlugin("CharacterFeaturesEditor.FeatureItem")]
    private Control CreateValidInput(EntityFeaturePropertyMetadata propertyMetadata)
    {
        var propType = propertyMetadata.PropertyType;

        if (propType is not { } notNullType)
        {
            Logger.Error("Property type is null for property {propertyName} in feature {featureUrn}",
                propertyMetadata.Attribute.Name, Feature.FeatureUrn);
#if DEBUG // Only dump in debug mode to avoid performance hit in release
            Logger.Dump(propertyMetadata);
#endif
            return new TextBlock { Text = "[ERROR] Property type is null." };
        }

        switch (notNullType)
        {
            case { } t when t == typeof(int):
            {
                return CreateIntInput(propertyMetadata);
            }
            case { } t when t == typeof(float):
            {
                return CreateFloatInput(propertyMetadata);
            }
            case { } t when t == typeof(string):
            {
                return CreateStringInput(propertyMetadata);
            }
            case { } t when t == typeof(bool):
            {
                return CreateBoolInput(propertyMetadata);
            }
            case { } t when t.IsEnum:
            {
                return CreateEnumInput(propertyMetadata, notNullType);
            }
            default:
                return new TextBlock { Text = $"[Unsupported Type: {notNullType.Name}]" };
        }
    }

    private Control CreateIntInput(EntityFeaturePropertyMetadata propertyMetadata)
    {
        var min = int.MinValue;
        var max = int.MaxValue;
        if (propertyMetadata.Attribute.MinValue is int minProp)
            min = minProp;
        if (propertyMetadata.Attribute.MaxValue is int maxProp)
            max = maxProp;

        var numericUpDown = new NumericIntUpDown()
        {
            Minimum = min,
            Maximum = max,
            Value = (int)(propertyMetadata.PropertyInfo.GetValue(Feature) ?? 0),
        };
        numericUpDown.ValueChanged += (_, _) =>
        {
            propertyMetadata.PropertyInfo.SetValue(Feature, (int)numericUpDown.Value);
        };
        
        BindToConfig(numericUpDown, propertyMetadata, (int? newValue) =>
        {
            if (newValue.HasValue && numericUpDown.Value != newValue.Value)
                numericUpDown.Value = newValue.Value;
        });

        return numericUpDown;
    }
    
    private Control CreateFloatInput(EntityFeaturePropertyMetadata propertyMetadata)
    {
        var min = float.MinValue;
        var max = float.MaxValue;
        if (propertyMetadata.Attribute.MinValue is float minProp)
            min = minProp;
        if (propertyMetadata.Attribute.MaxValue is float maxProp)
            max = maxProp;

        var numericUpDown = new NumericFloatUpDown()
        {
            Minimum = min,
            Maximum = max,
            Value = (float)(propertyMetadata.PropertyInfo.GetValue(Feature) ?? 0f),
        };
        numericUpDown.ValueChanged += (_, _) =>
        {
            propertyMetadata.PropertyInfo.SetValue(Feature, (float)numericUpDown.Value);
        };
        
        BindToConfig(numericUpDown, propertyMetadata, (float? newValue) =>
        {
            if (newValue.HasValue)
            {
                var currentValue = (float)numericUpDown.Value;
                if (Math.Abs(currentValue - newValue.Value) > 0.01)
                    numericUpDown.Value = newValue.Value;
            }
        });

        return numericUpDown;
    }
    
    private Control CreateStringInput(EntityFeaturePropertyMetadata propertyMetadata)
    {
        var textBox = new TextBox
        {
            Text = (string)(propertyMetadata.PropertyInfo.GetValue(Feature) ?? string.Empty),
        };
        textBox.TextChanged += (_, _) => { propertyMetadata.PropertyInfo.SetValue(Feature, textBox.Text); };
        
        BindToConfig(textBox, propertyMetadata, (string? newValue) =>
        {
            if (textBox.Text != newValue)
                textBox.Text = newValue ?? string.Empty;
        });

        return textBox;
    }
    
    private Control CreateBoolInput(EntityFeaturePropertyMetadata propertyMetadata)
    {
        var checkBox = new CheckBox
        {
            IsChecked = (bool)(propertyMetadata.PropertyInfo.GetValue(Feature) ?? false),
        };
        
        checkBox.IsCheckedChanged += (_, _) =>
        {
            if (checkBox.IsChecked.HasValue)
            {
                propertyMetadata.PropertyInfo.SetValue(Feature, checkBox.IsChecked.Value);
            }
        };

        BindToConfig(checkBox, propertyMetadata, (bool? newValue) =>
        {
            if (newValue.HasValue && checkBox.IsChecked != newValue.Value)
                checkBox.IsChecked = newValue.Value;
        });

        return checkBox;
    }
    
    private Control CreateEnumInput(EntityFeaturePropertyMetadata propertyMetadata, Type type)
    {
        var comboBox = new ComboBox();

        var enumValues = Enum.GetValues(type);
        
        foreach (Enum value in enumValues)
        {
            var attr = value.GetDescription();
            var comboBoxItem = new ComboBoxItem
            {
                Content = attr.Name,
                Tag = value
            };
            if (!string.IsNullOrEmpty(attr.Description))
            {
                ToolTip.SetTip(comboBoxItem, attr.Description);
                ToolTip.SetPlacement(comboBoxItem, PlacementMode.LeftEdgeAlignedBottom);
            }

            comboBox.Items.Add(comboBoxItem);
        }

        comboBox.SelectedIndex = propertyMetadata.PropertyInfo.GetValue(Feature) is Enum enumValue
            ? Array.IndexOf(enumValues, enumValue)
            : 0;
        
        comboBox.SelectionChanged += (_, _) =>
        {
            if (comboBox.SelectedItem is ComboBoxItem item && item.Tag != null)
            {
                propertyMetadata.PropertyInfo.SetValue(Feature, item.Tag);
            }
        };
        
        BindToConfig(comboBox, propertyMetadata, (string? enumName) =>
        {
            if (!string.IsNullOrEmpty(enumName))
            {
                for (int i = 0; i < comboBox.Items.Count; i++)
                {
                    if (comboBox.Items[i] is ComboBoxItem item && item.Tag?.ToString() == enumName)
                    {
                        if (comboBox.SelectedIndex != i)
                            comboBox.SelectedIndex = i;
                        break;
                    }
                }
            }
        });

        return comboBox;
    }

    private void BindToConfig<T>(Control control, EntityFeaturePropertyMetadata propertyMetadata, Action<T?> updateAction)
    {
        BindToNonSharedConfig(control, propertyMetadata, updateAction);
        if (propertyMetadata.Attribute.IsShared)
        {
            BindToSharedConfig(control, propertyMetadata, updateAction);
        }
    }
    
    private void BindToNonSharedConfig<T>(Control control, EntityFeaturePropertyMetadata propertyMetadata,
        Action<T?> updateAction)
    {
        var propType = propertyMetadata.PropertyType;
        if (propType is not { } notNullType)
        {
            Logger.Error("Property type is null for property {propertyName} in feature {featureUrn}",
                propertyMetadata.Attribute.Name, Feature.FeatureUrn);
        }

        Feature.Configuration.OnDataChanged += (propName) =>
        {
            if (propName == propertyMetadata.PropertyInfo.Name)
            {
                var newValue = Feature.Configuration.GetAs<T>(propName);
                updateAction(newValue);
            }
        };
    }
    
    private void BindToSharedConfig<T>(Control control, EntityFeaturePropertyMetadata propertyMetadata,
        Action<T?> updateAction)
    {
        var propType = propertyMetadata.PropertyType;
        if (propType is not { } notNullType)
        {
            Logger.Error("Property type is null for property {propertyName} in feature {featureUrn}",
                propertyMetadata.Attribute.Name, Feature.FeatureUrn);
        }
        
        Feature.SharedMemoryConfiguration.OnDataChanged += (propName) =>
        {
            if (propName == propertyMetadata.PropertyInfo.Name)
            {
                var newValue = Feature.SharedMemoryConfiguration.GetAs<T>(propName);
                updateAction(newValue);
            }
        };
    }
}