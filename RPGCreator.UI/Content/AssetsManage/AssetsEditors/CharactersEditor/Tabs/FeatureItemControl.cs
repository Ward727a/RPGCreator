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
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using RPGCreator.SDK;
using RPGCreator.SDK.Attributes;
using RPGCreator.SDK.EngineService;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Modules.Features.Entity;
using RPGCreator.SDK.Modules.UIModule;
using RPGCreator.UI.Contexts;
using Ursa.Controls;
using Size = RPGCreator.SDK.Types.Size;

namespace RPGCreator.UI.Content.AssetsManage.AssetsEditors.CharactersEditor.Tabs;

public class FeatureItemControl : UserControl
{
    public event Action<Ulid, IEntityFeature, FeatureItemControl>? FeatureRemoved;

    private CharacterEditorWindowControl _parent;
    
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
    
    [ExposePropToPlugin("CharacterFeaturesEditor.FeatureItem")]
    private Dictionary<string, Expander> PropCategories { get; set; } = new();

    public CharacterFeaturesEditorFeatureItemContext Context { get; private set; }

    public FeatureItemControl(IEntityFeature feature, Ulid featureInstanceId, CharacterEditorWindowControl parent)
    {
        _parent = parent;
        Feature = feature;
        FeatureInstanceId = featureInstanceId;
    }

    
    private bool _initialized;
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (_initialized) return;
        _initialized = true;
        
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
            GetPropCategories = () => PropCategories,
            GetFeatureInstanceId = () => FeatureInstanceId,
            CreatePropertyControl = CreatePropertyControl,
            GetOrCreateCategory = GetOrCreateCategory,
            CreateValidInput = CreateValidInput,
            LoadProperties = LoadProperties,
        };

        Context = new CharacterFeaturesEditorFeatureItemContext(config);

        EditorUiServices.ExtensionManager.ApplyExtensions(UIRegion.CharacterFeaturesEditorFeatureItem, this,
            new CharacterFeaturesEditorFeatureItemContext(config));
    }

    public void CallAddedToUi()
    {
        Feature.OnAddedToUi(_parent.Data, Context);
    }

    public void CallRemovedFromUi() => Feature.OnRemovedFromUi(_parent.Data, Context);

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
                Source = EngineServices.Resources.Load<Bitmap>(Feature.FeatureIcon),
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
        var propertiesList = EngineServices.FeaturesManager.GetEntityProperties(Feature.FeatureUrn);

        var entityFeaturePropertyMetadatas = propertiesList.ToList();

        if (entityFeaturePropertyMetadatas.Count == 0) return;
        
        foreach (var propertyMetadata in entityFeaturePropertyMetadatas)
        {
            CreatePropertyControl(propertyMetadata);
        }
    }

    [ExposeToPlugin("CharacterFeaturesEditor.FeatureItem")]
    private void CreatePropertyControl(EntityFeaturePropertyMetadata propertyMetadata)
    {
        var categoryPanel = GetOrCreateCategory(propertyMetadata.Attribute.Category);

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
    private StackPanel GetOrCreateCategory(string categoryPath)
    {
        var categories = categoryPath.Split('/');
        StackPanel currentPanel = ExpanderContent;
        string currentPath = "";

        foreach (var category in categories)
        {
            currentPath = string.IsNullOrEmpty(currentPath) ? category : $"{currentPath}/{category}";
            if (!PropCategories.TryGetValue(currentPath, out var expander))
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
                PropCategories[currentPath] = expander;
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
                return CreateIntInput(propertyMetadata);
            case { } t when t == typeof(float):
                return CreateFloatInput(propertyMetadata);
            case { } t when t == typeof(string):
                return CreateStringInput(propertyMetadata);
            case { } t when t == typeof(bool):
                return CreateBoolInput(propertyMetadata);
            case { } t when t.IsEnum:
                return CreateEnumInput(propertyMetadata, notNullType);
            case { } t when t == typeof(Size):
                return CreateSizeInput(propertyMetadata);
            default:
                return new TextBlock { Text = $"[Unsupported Type: {notNullType.Name}]" };
        }
    }

    #region inputControl
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

    private Control CreateSizeInput(EntityFeaturePropertyMetadata propertyMetadata)
    {
        var min = int.MinValue;
        var max = int.MaxValue;
        
        if (propertyMetadata.Attribute.MinValue is int minProp)
            min = minProp;
        if (propertyMetadata.Attribute.MaxValue is int maxProp)
            max = maxProp;

        var sizeStackPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 5
        };

        var sizeXInput = new NumericIntUpDown()
        {
            Minimum = min,
            Maximum = max,
            Value = (int)((Size)(propertyMetadata.PropertyInfo.GetValue(Feature) ?? new Size(0, 0))).Width,
            InnerLeftContent = "Width",
            InnerRightContent = "Pixel"
        };

        var sizeYInput = new NumericIntUpDown()
        {
            Minimum = min,
            Maximum = max,
            Value = (int)((Size)(propertyMetadata.PropertyInfo.GetValue(Feature) ?? new Size(0, 0))).Height,
            InnerLeftContent = "Height",
            InnerRightContent = "Pixel"
        };

        void SizeChanged(object? o, ValueChangedEventArgs<int> valueChangedEventArgs)
        {
            var currentSize = new Size((float)sizeXInput.Value, (float)sizeYInput.Value);
            propertyMetadata.PropertyInfo.SetValue(Feature, currentSize);
        }

        sizeXInput.ValueChanged += SizeChanged;
        sizeYInput.ValueChanged += SizeChanged;
        
        sizeStackPanel.Children.Add(sizeXInput);
        sizeStackPanel.Children.Add(sizeYInput);
        
        BindToConfig(sizeXInput, propertyMetadata, (Size? newValue) =>
        {
            if (newValue.HasValue)
            {
                var currentSize = new Size((float)sizeXInput.Value, (float)sizeYInput.Value);
                if (Math.Abs(currentSize.Width - newValue.Value.Width) > 0.01)
                {
                    propertyMetadata.PropertyInfo.SetValue(Feature, new Size(newValue.Value.Width, currentSize.Height));
                    sizeXInput.Value = (int)newValue.Value.Width;
                }
            }
        });

        BindToConfig(sizeYInput, propertyMetadata, (Size? newValue) =>
        {
            if (newValue.HasValue)
            {
                var currentSize = new Size((float)sizeXInput.Value, (float)sizeYInput.Value);
                if (Math.Abs(currentSize.Height - newValue.Value.Height) > 0.01)
                {
                    propertyMetadata.PropertyInfo.SetValue(Feature, new Size(currentSize.Width, newValue.Value.Height));
                    sizeYInput.Value = (int)newValue.Value.Height;
                }
            }
        });

        return sizeStackPanel;
    }
    #endregion
    
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

        void OnConfigurationDataChanged(string propName)
        {
            if (propName == propertyMetadata.PropertyInfo.Name)
            {
                var newValue = Feature.Configuration.GetAs<T>(propName);
                updateAction(newValue);
            }
        }

        Feature.Configuration.DataChanged += OnConfigurationDataChanged;

        this.DetachedFromVisualTree += (_, _) =>
        {
            Feature.Configuration.DataChanged -= OnConfigurationDataChanged;
        };
    }
    
    private void BindToSharedConfig<T>(Control control, EntityFeaturePropertyMetadata propertyMetadata,
        Action<T?> updateAction)
    {
        void OnSharedMemoryConfigurationDataChanged(string propName)
        {
            if (propName == propertyMetadata.PropertyInfo.Name)
            {
                var newValue = Feature.SharedMemoryConfiguration.GetAs<T>(propName);
                updateAction(newValue);
            }
        }

        Feature.SharedMemoryConfiguration.DataChanged += OnSharedMemoryConfigurationDataChanged;

        this.DetachedFromVisualTree += (_, _) =>
        {
            Feature.Configuration.DataChanged -= OnSharedMemoryConfigurationDataChanged;
        };
    }
}