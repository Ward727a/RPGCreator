using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using RPGCreator.Core.Types;
using RPGCreator.SDK.Assets.Definitions.Skills;

namespace RPGCreator.UI.Content.AssetsManage.AssetsEditors.SkillsEffectEditor.Tabs;

public class SkillEffectGeneralEditorControl : UserControl
{

    public EventHandler<SkillEffectPropertyDescriptor>? AddedProperty;
    public EventHandler<SkillEffectPropertyDescriptor>? RemovedProperty;
    public EventHandler<SkillEffectPropertyDescriptor>? UpdatedProperty;
    
    public class SkillEffectPropertyItem : UserControl
    {
        
        public event EventHandler? RemoveRequested;
        public event EventHandler? Updated;
        
        public SkillEffectPropertyDescriptor Descriptor => new SkillEffectPropertyDescriptor()
        {
            Name = NameLabel.Text ?? string.Empty,
            Type = PropertyType,
            DefaultValue = DefaultValue ?? PropertyType.GetDefaultValue()
        };

        private Grid Body;
        
        private Button RemoveButton;
        private TextBox NameLabel;
        private ComboBox TypeComboBox;
        private Control ValueInput;
        private EffectPropertyType PropertyType;
        private object? DefaultValue = null;

        public SkillEffectPropertyItem()
        {
            CreateComponents();
            RegisterEvents();
            Content = Body;
        }

        public void SetFromOther(SkillEffectPropertyDescriptor descriptor)
        {
            NameLabel.Text = descriptor.Name;
            foreach (ComboBoxItem item in TypeComboBox.Items)
            {
                if (item.Tag is EffectPropertyType type && type == descriptor.Type)
                {
                    TypeComboBox.SelectedItem = item;
                    break;
                }
            }

            switch (ValueInput)
            {
                case TextBox textBox when descriptor.DefaultValue is string strValue:
                    textBox.Text = strValue;
                    break;
                case NumericUpDown numericUpDown when descriptor.DefaultValue is decimal decValue:
                    numericUpDown.Value = decValue;
                    break;
                case CheckBox checkBox when descriptor.DefaultValue is bool boolValue:
                    checkBox.IsChecked = boolValue;
                    break;
            }
        }

        private void CreateComponents()
        {
            Body = new Grid()
            {
                ColumnDefinitions = new ColumnDefinitions("Auto,*, *, *"),
                RowDefinitions = new RowDefinitions("Auto"),
                Margin = App.style.Margin
            };
            RemoveButton = new Button()
            {
                Content = "X",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = App.style.Margin
            };
            Body.Children.Add(RemoveButton);
            Grid.SetColumn(RemoveButton, 0);
            NameLabel = new TextBox()
            {
                Watermark = "Property Name",
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = App.style.Margin
            };
            Body.Children.Add(NameLabel);
            Grid.SetColumn(NameLabel, 1);
            
            TypeComboBox = new ComboBox()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = App.style.Margin
            };
            Body.Children.Add(TypeComboBox);
            Grid.SetColumn(TypeComboBox, 2);

            foreach (var propertyType in Enum.GetValues<EffectPropertyType>())
            {
                // Skip None and Custom types
                // Graph skill effects do not support none or custom types
                if(propertyType is EffectPropertyType.None or EffectPropertyType.Custom)
                    continue;
                
                TypeComboBox.Items.Add(
                    new ComboBoxItem()
                    {
                        Content = propertyType.ToString(),
                        Tag = propertyType
                    }
                );
            }

            ValueInput = new TextBlock()
            {
                Text = "Default value - No Type Selected",
                Foreground = Brushes.Gray,
                IsEnabled = true,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = App.style.Margin,
                FontStyle = FontStyle.Italic
            };
            Body.Children.Add(ValueInput);
            Grid.SetColumn(ValueInput, 3);
        }

        private void RegisterEvents()
        {
            
            NameLabel.TextChanged += (sender, args) =>
            {
                Updated?.Invoke(this, EventArgs.Empty);
            };
            
            TypeComboBox.SelectionChanged += (sender, args) =>
            {
                if (TypeComboBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is EffectPropertyType selectedType)
                {
                    PropertyType = selectedType;
                    Body.Children.Remove(ValueInput);
                    switch (PropertyType)
                    {
                        case EffectPropertyType.Text:
                            ValueInput = new TextBox()
                            {
                                Watermark = "Default Value",
                                Text = PropertyType.GetDefaultValue() as string,
                                VerticalAlignment = VerticalAlignment.Center,
                                HorizontalAlignment = HorizontalAlignment.Stretch,
                                Margin = App.style.Margin
                            };
                            break;
                        case EffectPropertyType.Number:
                            ValueInput = new NumericUpDown()
                            {
                                Minimum = int.MinValue,
                                Maximum = int.MaxValue,
                                Value = PropertyType.GetDefaultValue() as decimal? ?? 0,
                                VerticalAlignment = VerticalAlignment.Center,
                                HorizontalAlignment = HorizontalAlignment.Stretch,
                                Margin = App.style.Margin
                            };
                            break;
                        case EffectPropertyType.Boolean:
                            ValueInput = new CheckBox()
                            {
                                Content = "Default Value",
                                IsChecked = PropertyType.GetDefaultValue() as bool?,
                                VerticalAlignment = VerticalAlignment.Center,
                                HorizontalAlignment = HorizontalAlignment.Stretch,
                                Margin = App.style.Margin
                            };
                            break;
                        default:
                            ValueInput = new TextBlock()
                            {
                                Text = "Default value - No Type Selected",
                                Foreground = Brushes.Gray,
                                VerticalAlignment = VerticalAlignment.Center,
                                HorizontalAlignment = HorizontalAlignment.Left,
                                Margin = App.style.Margin,
                                FontStyle = FontStyle.Italic
                            };
                            break;
                    }
                    Body.Children.Add(ValueInput);
                    Grid.SetColumn(ValueInput, 3);
                    Updated?.Invoke(this, EventArgs.Empty);
                }
            };
            
            RemoveButton.Click += (sender, args) =>
            {
                RemoveRequested?.Invoke(this, EventArgs.Empty);
                if (this.Parent is Panel parentPanel)
                {
                    parentPanel.Children.Remove(this);
                }
            };
        }

    }
    
    #region Constants
    #endregion
    
    #region Events
    #endregion
    
    #region Properties
        
    public Ulid? SelectedEffectPackId => (_effectPack.SelectedItem as ComboBoxItem)?.Tag as Ulid?;
    public string EffectName => string.IsNullOrEmpty(DisplayNameInput.Text)? "Unnamed Skill Effect" : DisplayNameInput.Text;
    #endregion
    
    #region Components

    private Grid Body;
    private StackPanel BodyPanel;

    private ComboBox _effectPack;
    private TextBox DisplayNameInput;
    private Button AddPropButton;
    #endregion
    
    #region Constructors

    public SkillEffectGeneralEditorControl()
    {
        CreateComponents();
        RegisterEvents();
    }
    
    #endregion
    
    #region Methods
    
    private void CreateComponents()
    {
        Body = new Grid
        {
            Margin = App.style.Margin,
            RowDefinitions = new RowDefinitions("*")
        };

        BodyPanel = new StackPanel
        {
            Margin = App.style.Margin,
            Orientation = Orientation.Vertical,
            Spacing = 5
        };
        _effectPack = new ComboBox()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = App.style.Margin,
        };
        var inputEffectPack = new InputLabel("Assets Pack", _effectPack);
        BodyPanel.Children.Add(inputEffectPack);
        ToolTip.SetTip(inputEffectPack, "The assets pack this effect belongs to.");
        
        // foreach (var pack in EngineCore.Instance.Managers.Assets.GetAssetsPacks())
        // {
        //     _effectPack.Items.Add(new ComboBoxItem()
        //     {
        //         Content = pack.Name,
        //         Tag = pack.Id
        //     });
        // }
        
        if(_effectPack.Items.Count > 0)
            _effectPack.SelectedIndex = 0;
        

        Body.Children.Add(BodyPanel);
        Content = Body;
        
        DisplayNameInput = new TextBox
        {
            Watermark = "Display Name",
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        
        BodyPanel.Children.Add(new InputLabel("Display Name", DisplayNameInput));
        ToolTip.SetTip(DisplayNameInput, "Display Name of the Skill Effect.");
        
        AddPropButton = new Button
        {
            Content = "Add Property",
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        BodyPanel.Children.Add(AddPropButton);
    }
    
    private void RegisterEvents()
    {
        AddPropButton.Click += (sender, args) =>
        {
            var propertyItem = new SkillEffectPropertyItem();
            BodyPanel.Children.Add(propertyItem);
            propertyItem.RemoveRequested += (s, e) =>
            {
                RemovedProperty?.Invoke(this, propertyItem.Descriptor);
            };
            propertyItem.Updated += (s, e) =>
            {
                UpdatedProperty?.Invoke(this, propertyItem.Descriptor);
            };
            AddedProperty?.Invoke(this, propertyItem.Descriptor);
        };
    }
    
    public List<SkillEffectPropertyDescriptor> GetProperties()
    {
        var properties = new List<SkillEffectPropertyDescriptor>();
        foreach (var child in BodyPanel.Children)
        {
            if (child is SkillEffectPropertyItem propertyItem)
            {
                properties.Add(propertyItem.Descriptor);
            }
        }
        return properties;
    }
    
    #endregion

    #region Events Handlers
    #endregion
}