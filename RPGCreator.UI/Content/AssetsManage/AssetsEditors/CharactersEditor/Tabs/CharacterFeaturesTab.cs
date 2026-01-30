using System;
using System.Collections.Generic;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Layout;
using Avalonia.Media.Imaging;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Characters;
using RPGCreator.SDK.Commands;
using RPGCreator.SDK.ECS.Features;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Modules.UIModule;
using RPGCreator.SDK.Types;
using RPGCreator.SDK.UiService;
using Ursa.Controls;
using NumericUpDown = Avalonia.Controls.NumericUpDown;

namespace RPGCreator.UI.Content.AssetsManage.AssetsEditors.CharactersEditor.Tabs;

public class CharacterFeaturesTab : UserControl
{
    
    #region Events
    #endregion

    #region Properties

    public CharacterData Data;
    
    #endregion
    
    #region Components
    
    private Grid Body { get; set; }
    private Button AddFeatureButton { get; set; }
    private StackPanel FeaturesList { get; set; }
    
    #endregion
    
    #region Constructors
    public CharacterFeaturesTab(CharacterData data)
    {
        Data = data;
        Name = "Features"; // Define the name of the tab
        CreateComponents();
        RegisterEvents();
        Content = Body;
    }
    #endregion
    
    #region Methods

    private void CreateComponents()
    {
        Body = new Grid()
        {
            RowDefinitions = new RowDefinitions("Auto, *"),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            Margin = new Avalonia.Thickness(10)
        };
        
        AddFeatureButton = new Button
        {
            Content = "Add Feature",
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            Margin = new Avalonia.Thickness(0, 0, 0, 10)
        };
        
        Body.Children.Add(AddFeatureButton);
        

        var scrollContainer = new ScrollViewer()
        {
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
        };
        
        FeaturesList = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            Margin = new Avalonia.Thickness(10)
        };
        scrollContainer.Content = FeaturesList;
        
        Body.Children.Add(scrollContainer);
        Grid.SetRow(scrollContainer, 1);
        
    }

    private void RegisterEvents()
    {
        AddFeatureButton.Click += OnAddFeatureButtonClick;
    }
    
    #endregion
    
    #region Events Handlers
    
    /// <summary>
    /// A command representing the addition of a feature to the character.<br/>
    /// So that it can be undone/redone.
    /// </summary>
    private class FeatureAddedCommand : ICommand
    {
        public event Action? Executed;
        public event Action? Undone;
        private readonly IEntityFeature _feature;
        private readonly StackPanel _featuresList; 
        private FeatureItemControl? _featureControl;
        public string Name { get; }

        public FeatureAddedCommand(IEntityFeature feature, StackPanel featuresList)
        {
            _feature = feature;
            _featuresList = featuresList;
            
            var displayableName = string.IsNullOrWhiteSpace(_feature.FeatureName) ? _feature.FeatureUrn.ToString() : _feature.FeatureName;
            
            if(displayableName.Length > 100)
                displayableName = string.Concat(displayableName.AsSpan(0, 97), "...");

            Name = $"Add Feature: {displayableName}";
        }
        
        public void Execute()
        {
            _featureControl ??= new FeatureItemControl(_feature);
            _featuresList.Children.Add(_featureControl);
            Executed?.Invoke();
        }

        public void Undo()
        {
            if (_featureControl != null)
            {
                _featuresList.Children.Remove(_featureControl);
                Undone?.Invoke();
            }
            else
                Logger.Error("Cannot undo feature addition, feature control is null");
        }

    }
    
    /// <summary>
    /// Just a test list to keep track of added features in this session.<br/>
    /// This will not be kept in the final implementation.
    /// </summary>
    private readonly List<IEntityFeature> _addedFeatures = new();
    
    async void OnAddFeatureButtonClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        try
        {
            Logger.Debug("Add Feature button clicked");

            var dialog = new FeatureLibraryExplorerDialog();
            var result = await UiServices.DialogService.ConfirmAsync("Add Feature", dialog, new DialogStyle(Height: 400, CanResize: true));

            if (result && dialog.HasFeature)
            {
                var feature = dialog.GetSelectedFeature();
                Logger.Debug("Feature selected: {featureName}", feature.FeatureName);
                var cmd = new FeatureAddedCommand(feature, FeaturesList);
                cmd.Executed += () =>
                {
                    _addedFeatures.Add(feature);
                };
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
    /// It will just log warnings and show notifications for missing dependencies!! DO NOT USE FOR THE RELEASE!!!
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
                    Logger.Warning("Feature {feature} is missing dependency {dependency}", entityFeature.FeatureUrn, dependency);
                    // Here you could also notify the user via UI
                    UiServices.NotificationService.Error("Feature Dependency Missing", 
                        $"The feature '{entityFeature.FeatureName}' is missing the required dependency feature '{dependency}'.", new NotificationOptions(30000));
                }
            }
        }
    }
    
    #endregion
}

public class FeatureItemControl : UserControl
{
    public IEntityFeature Feature { get; set; }
    
    public Expander PropExpander { get; set; }
    
    public StackPanel ExpanderContent { get; set; }

    public FeatureItemControl(IEntityFeature feature)
    {
        Feature = feature;
        CreateComponents();
        LoadProperties();
        RegisterEvents();
        
        UIExtensionManager.ApplyExtensions(UIRegion.CharacterFeaturesEditorFeatureItem, this, Feature);
    }

    private void CreateComponents()
    {
        PropExpander = new Expander
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
            Margin = new Avalonia.Thickness(0, 0, 0, 5)
        };
        var body = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Margin = new Avalonia.Thickness(5)
        };

        if (File.Exists(Feature.FeatureIcon))
        {
            var icon = new Image
            {
                Source = EngineServices.ResourcesService.Load<Bitmap>(Feature.FeatureIcon),
                Width = 32,
                Height = 32,
                Margin = new Avalonia.Thickness(0, 0, 10, 0)
            };
            body.Children.Add(icon);
        }
        
        var nameLabel = new TextBlock
        {
            Text = Feature.FeatureName,
            FontWeight = Avalonia.Media.FontWeight.Bold
        };
        
        body.Children.Add(nameLabel);

        PropExpander.Header = body;
        
        ExpanderContent = new StackPanel()
        {
            Orientation = Orientation.Vertical,
            Margin = new Thickness(5),
            HorizontalAlignment = HorizontalAlignment.Left
        };
        
        var description = new TextBlock()
        {
            Inlines = new InlineCollection()
        };
        description.Inlines.Add(new Run {Text = "Description: ", Foreground = Avalonia.Media.Brushes.Gray, FontWeight = Avalonia.Media.FontWeight.Bold});
        description.Inlines.Add(new Run { Text = Feature.FeatureDescription });
        ExpanderContent.Children.Add(description);
        PropExpander.Content = ExpanderContent;
        
        Content = PropExpander;
    }
    
    private void RegisterEvents()
    {
        
    }

    private void LoadProperties()
    {
        Dictionary<string, Expander> propCategories = new();
        var propertiesList = EngineServices.ECS.GetEditableProperties(Feature.FeatureUrn);

        foreach (var propertyMetadata in propertiesList)
        {
            var categoryPanel = GetOrCreateCategory(propertyMetadata.Attribute.Category, propCategories);
            var propText = new TextBlock()
            {
                Inlines = new InlineCollection()
            };
            
            propText.Inlines.Add(new Run {Text = "Property: ", Foreground = Avalonia.Media.Brushes.Gray, FontWeight = Avalonia.Media.FontWeight.Bold});
            propText.Inlines.Add(new Run { Text = propertyMetadata.Attribute.Name });
            categoryPanel.Children.Add(propText);
            var inputControl = CreateValidInput(propertyMetadata);
            categoryPanel.Children.Add(inputControl);
        }
        
    }

    /// <summary>
    /// Gets or creates a category panel based on the category path.
    /// </summary>
    /// <param name="categoryPath">The category path, separated by '/'.</param>
    /// <param name="propCategories">The dictionary of existing categories.</param>
    /// <returns>The StackPanel for the category.</returns>
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
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
                    Margin = new Avalonia.Thickness(0, 5, 0, 5)
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

            if(expander.Content is not StackPanel)
            {
                Logger.Error("Expander content is not a StackPanel for category {category}", currentPath);
                ExpanderContent.Children.Add(new TextBlock(){Text = "[ERROR] Invalid category content."});
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
    private Control CreateValidInput(EntityFeaturePropertyMetadata propertyMetadata)
    {
        var propType = propertyMetadata.PropertyType;

        if (propType is not { } notNullType)
        {
            Logger.Error("Property type is null for property {propertyName} in feature {featureUrn}", propertyMetadata.Attribute.Name, Feature.FeatureUrn);
            #if DEBUG // Only dump in debug mode to avoid performance hit in release
            Logger.Dump(propertyMetadata);
            #endif
            return new TextBlock { Text = "[ERROR] Property type is null." };
        }

        switch (notNullType)
        {
            case Type t when t == typeof(int):
                {
                    var min = int.MinValue;
                    var max = int.MaxValue;
                    if(propertyMetadata.Attribute.MinValue is int minProp)
                        min = minProp;
                    if(propertyMetadata.Attribute.MaxValue is int maxProp)
                        max = maxProp;
                    
                    var numericUpDown = new NumericIntUpDown()
                    {
                        Minimum = min,
                        Maximum = max,
                        Value = (int)(propertyMetadata.PropertyInfo.GetValue(Feature) ?? 0),
                        Width = 100
                    };
                    numericUpDown.ValueChanged += (s, e) =>
                    {
                        propertyMetadata.PropertyInfo.SetValue(Feature, (int)numericUpDown.Value);
                    };
                    if (propertyMetadata.Attribute.IsShared)
                    {
                        Feature.SharedMemoryConfiguration.OnDataChanged += (propName) =>
                        {
                            if (propName == propertyMetadata.PropertyInfo.Name)
                            {
                                var newValue = Feature.SharedMemoryConfiguration.Get<int>(propName);
                                if(numericUpDown.Value != newValue)
                                    numericUpDown.Value = newValue;
                            }
                        };
                    }
                    return numericUpDown;
                }
            case Type t when t == typeof(float):
                {
                    var min = float.MinValue;
                    var max = float.MaxValue;
                    if(propertyMetadata.Attribute.MinValue is float minProp)
                        min = minProp;
                    if(propertyMetadata.Attribute.MaxValue is float maxProp)
                        max = maxProp;
                    
                    var numericUpDown = new NumericFloatUpDown()
                    {
                        Minimum = min,
                        Maximum = max,
                        Value = (float)(propertyMetadata.PropertyInfo.GetValue(Feature) ?? 0f),
                        Width = 100
                    };
                    numericUpDown.ValueChanged += (s, e) =>
                    {
                        propertyMetadata.PropertyInfo.SetValue(Feature, (float)numericUpDown.Value);
                    };
                    if (propertyMetadata.Attribute.IsShared)
                    {
                        Feature.SharedMemoryConfiguration.OnDataChanged += (propName) =>
                        {
                            if (propName == propertyMetadata.PropertyInfo.Name)
                            {
                                var newValue = Feature.SharedMemoryConfiguration.Get<float>(propName);
                                if(numericUpDown.Value != newValue)
                                    numericUpDown.Value = newValue;
                            }
                        };
                    }
                    return numericUpDown;
                }
            case Type t when t == typeof(string):
                {
                    var textBox = new TextBox
                    {
                        Text = (string)(propertyMetadata.PropertyInfo.GetValue(Feature) ?? string.Empty),
                        Width = 200
                    };
                    textBox.TextChanged += (s, e) =>
                    {
                        propertyMetadata.PropertyInfo.SetValue(Feature, textBox.Text);
                    };
                    if (propertyMetadata.Attribute.IsShared)
                    {
                        Feature.SharedMemoryConfiguration.OnDataChanged += (propName) =>
                        {
                            if (propName == propertyMetadata.PropertyInfo.Name)
                            {
                                var newValue = Feature.SharedMemoryConfiguration.Get<string>(propName);
                                if (textBox.Text != newValue)
                                    textBox.Text = newValue;
                            }
                        };
                    }
                    return textBox;
                }
            case Type t when t == typeof(bool):
                {
                    var checkBox = new CheckBox
                    {
                        IsChecked = (bool)(propertyMetadata.PropertyInfo.GetValue(Feature) ?? false),
                    };
                    checkBox.Checked += (s, e) =>
                    {
                        propertyMetadata.PropertyInfo.SetValue(Feature, true);
                    };
                    checkBox.Unchecked += (s, e) =>
                    {
                        propertyMetadata.PropertyInfo.SetValue(Feature, false);
                    };
                    if (propertyMetadata.Attribute.IsShared)
                    {
                        Feature.SharedMemoryConfiguration.OnDataChanged += (propName) =>
                        {
                            if (propName == propertyMetadata.PropertyInfo.Name)
                            {
                                var newValue = Feature.SharedMemoryConfiguration.Get<bool>(propName);
                                if(checkBox.IsChecked != newValue)
                                    checkBox.IsChecked = newValue;
                            }
                        };
                    }
                    return checkBox;
                }
            default:
                return new TextBlock { Text = $"[Unsupported Type: {notNullType.Name}]" };
        }
    }
}

public class FeatureLibraryExplorerDialog : UserControl
{

    private IEntityFeature? SelectedFeature { get; set; }

    public bool HasFeature => SelectedFeature != null;
    
    private StackPanel Body { get; set; } = null!;
    private ListBox FeaturesListBox { get; set; } = null!;

    public FeatureLibraryExplorerDialog()
    {
        CreateComponents();
        RegisterEvents();
    }
    
    private void CreateComponents()
    {
        Body = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            Margin = new Avalonia.Thickness(10)
        };
        
        FeaturesListBox = new ListBox
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            Height = 300
        };
        
        Body.Children.Add(FeaturesListBox);
        
        Content = Body;
    }

    private void RegisterEvents()
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        FeaturesListBox.SelectionChanged += OnFeatureSelected;
    }
    
    private void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        LoadFeatures();
    }
    private void OnUnloaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        FeaturesListBox.Items.Clear();
    }
    
    private void OnFeatureSelected(object? sender, SelectionChangedEventArgs e)
    {
        if(e.AddedItems.Count > 0 && e.AddedItems[0] is FeatureListItem item)
        {
            SelectedFeature = item.Feature;
        }
    }

    private void LoadFeatures()
    {
        var features = EngineServices.ECS.GetAllFeatures();

        if (features.Count == 0)
        {
            FeaturesListBox.Items.Add(new TextBlock
            {
                Text = "No features available.",
                Foreground = Avalonia.Media.Brushes.Gray,
                Margin = new Avalonia.Thickness(5)
            });
            return;
        }
        
        foreach (var feature in features)
        {
            FeaturesListBox.Items.Add(new FeatureListItem(feature));
        }
        
    }
    
    /// <summary>
    /// Returns the selected feature.<br/>
    /// It will do a clone of the feature to avoid modifying the template.<br/>
    /// Throws an exception if no feature is selected.
    /// </summary>
    /// <returns>The selected feature clone.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no feature is selected.</exception>
    public IEntityFeature GetSelectedFeature()
    {
        if (SelectedFeature == null)
            throw new InvalidOperationException("No feature selected.");
        return SelectedFeature.Clone();
    }
    
    private class FeatureListItem : UserControl
    {
        public IEntityFeature Feature { get; set; }
        public URN Urn { get; set; }
        
        public FeatureListItem(IEntityFeature feature)
        {
            Feature = feature;
            Urn = feature.FeatureUrn;
            CreateComponents();
        }
        
        private void CreateComponents()
        {
            var body = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Margin = new Avalonia.Thickness(5)
            };

            if (File.Exists(Feature.FeatureIcon))
            {
                var icon = new Image
                {
                    Source = EngineServices.ResourcesService.Load<Bitmap>(Feature.FeatureIcon),
                    Width = 32,
                    Height = 32,
                    Margin = new Avalonia.Thickness(0, 0, 10, 0)
                };
                body.Children.Add(icon);
            }
            else
            {
                var noIconText = new TextBlock
                {
                    Text = "[No Icon]",
                    Foreground = Avalonia.Media.Brushes.Gray,
                    Width = 32,
                    Height = 32,
                    Margin = new Avalonia.Thickness(0, 0, 10, 0),
                    TextAlignment = Avalonia.Media.TextAlignment.Center,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                };
                body.Children.Add(noIconText);
            }
            
            var nameLabel = new TextBlock
            {
                Text = Feature.FeatureName,
                FontWeight = Avalonia.Media.FontWeight.Bold,
                Margin = new Avalonia.Thickness(0, 0, 10, 0)
            };
            
            var urnLabel = new TextBlock
            {
                Text = Urn.ToString(),
                FontStyle = Avalonia.Media.FontStyle.Italic,
                Foreground = Avalonia.Media.Brushes.Gray
            };
            
            body.Children.Add(nameLabel);
            body.Children.Add(urnLabel);
            
            Content = body;
        }
    }
    
}