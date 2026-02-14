using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using RPGCreator.Core.Types;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Skills;
using RPGCreator.SDK.Extensions;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Types;
using Ursa.Controls;
using Thickness = Avalonia.Thickness;

namespace RPGCreator.UI.Content.AssetsManage.AssetsEditors.SkillsEditor.Tabs;

/// <summary>
/// This tab is used to edit the effect properties of a skill, such as damage, healing, buffs, debuffs, and status effects.
/// </summary>
public class SkillEffectTab : UserControl
{

    public class SkillEffectItemControl : UserControl
    {
        private ISkillEffect _skillEffect;
        public ISkillEffect SkillEffect => _skillEffect;


        private Grid _gridBody;
        
        
        public SkillEffectItemControl(ISkillEffect effect)
        {
            ArgumentNullException.ThrowIfNull(effect);
            var cloned = effect.Clone();
            
            if(cloned is not ISkillEffect skillEffect)
                throw new ArgumentException("Cloned effect must be of type ISkillEffect", nameof(effect));
            
            _skillEffect = skillEffect;
            
            CreateComponents();
            Content = _gridBody;
        }
        
        private void CreateComponents()
        {
            _gridBody = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("200,*"),
                RowDefinitions = new RowDefinitions("Auto, Auto"),
                Margin = new Thickness(0,0,0,5),
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            var nameLabel = new TextBlock
            {
                Text = _skillEffect.DisplayName,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = App.style.Margin,
                TextTrimming = TextTrimming.WordEllipsis
            };
            ToolTip.SetTip(nameLabel, _skillEffect.DisplayName);
            _gridBody.Children.Add(nameLabel);
            Grid.SetColumn(nameLabel, 0);
            
            
            // We need to display the properties input of the effect
            // For this, we have the 'PropertyDescriptors' static property in the ISkillEffect interface
            // We can use this to create the input fields for the properties following the EffectPropertyType enum
            var type = _skillEffect.GetType();
            var isFromGraph = type.IsAssignableTo(typeof(GraphSkillEffect));
            var propertyDescriptorsProperty = type.GetProperty("PropertyDescriptors", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            
            // check if the effect is a GraphSkillEffect
            if(isFromGraph)
            {
                // For GraphSkillEffect, we need to get the PropertyDescriptors from the instance
                propertyDescriptorsProperty = type.GetProperty("PropertyDescriptors", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            }
            
            if (propertyDescriptorsProperty != null)
            {
                var propertyDescriptors = propertyDescriptorsProperty.GetValue(isFromGraph?_skillEffect : null) as IReadOnlyList<SkillEffectPropertyDescriptor>;
                if (propertyDescriptors != null)
                {

                    if (propertyDescriptors.Count <= 0)
                    {
                        var noProperties = new TextBlock
                        {
                            Text = "No properties available for this effect.",
                            Margin = App.style.Margin,
                            FontStyle = FontStyle.Italic,
                            Foreground = Brushes.Gray,
                        };
                        _gridBody.Children.Add(noProperties);
                        Grid.SetColumn(noProperties, 1);
                        return;
                    }
                    var propertiesPanel = new StackPanel
                    {
                        Orientation = Orientation.Vertical,
                        Spacing = 5,
                        Margin = App.style.Margin
                    };
                    foreach (var descriptor in propertyDescriptors)
                    {
                        var propertyName = descriptor.Name;
                        var propertyType = descriptor.Type;
                        var propertyValue = _skillEffect.Properties.ContainsKey(propertyName) ? _skillEffect.Properties[propertyName] : descriptor.DefaultValue;

                        var propertyControl = CreatePropertyControl(propertyName, propertyType, propertyValue);
                        if (propertyControl != null)
                        {
                            propertiesPanel.Children.Add(propertyControl);
                        }
                    }
                    _gridBody.Children.Add(propertiesPanel);
                    Grid.SetColumn(propertiesPanel, 1);
                }
            }
            else
            {
                var noProperties = new TextBlock
                {
                    Text = "No properties available for this effect.",
                    Margin = App.style.Margin,
                    FontStyle = FontStyle.Italic,
                    Foreground = Brushes.Gray,
                };
                _gridBody.Children.Add(noProperties);
                Grid.SetColumn(noProperties, 1);
            }
            
            var sep = new Separator
            {
                Margin = new Thickness(0,5,0,0),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            _gridBody.Children.Add(sep);
            Grid.SetRow(sep, 1);
            Grid.SetColumnSpan(sep, 2);

        }

        private Control? CreatePropertyControl(string name, EffectPropertyType type, object value)
        {
            switch (type)
            {
                case EffectPropertyType.Number:
                {
                    
                    // Check that the value is a float, if not, set it to 0f
                    if (value is not float)
                    {
                        value = 0f;
                        _skillEffect.Properties[name] = value;
                        Logger.Warning("SkillEffectItemControl: Property '{Name}' expected to be of type 'float', but got '{Type}'. Setting to 0f.", name, value.GetType());
                    }
                    
                    return CreateNumberPropertyControl(name, value);
                }
                case EffectPropertyType.Text:
                {
                    // Check that the value is a string, if not, set it to empty string
                    if (value is not string)
                    {
                        value = string.Empty;
                        _skillEffect.Properties[name] = value;
                        Logger.Warning("SkillEffectItemControl: Property '{Name}' expected to be of type 'string', but got '{Type}'. Setting to empty string.", name, value.GetType());
                    }

                    return CreateTextPropertyControl(name, value);
                }
                case EffectPropertyType.Boolean:
                {
                    // Check that the value is a bool, if not, set it to false
                    if (value is not bool)
                    {
                        value = false;
                        _skillEffect.Properties[name] = value;
                        Logger.Warning("SkillEffectItemControl: Property '{Name}' expected to be of type 'bool', but got '{Type}'. Setting to false.", name, value.GetType());
                    }

                    return CreateBoolPropertyControl(name, value);
                }
                case EffectPropertyType.Vector2:
                {
                    // Check that the value is a Point, if not, set it to Point.Empty
                    if (value is not Point)
                    {
                        value = default(Point);
                        _skillEffect.Properties[name] = value;
                        Logger.Warning("SkillEffectItemControl: Property '{Name}' expected to be of type 'Point', but got '{Type}'. Setting to Point.Zero.", name, value.GetType());
                    }

                    return CreatePointPropertyControl(name, value);
                }
                case EffectPropertyType.StatReference:
                {
                    
                    // Check that the value is a Ulid, if not, set it to Ulid.Empty
                    if (value is not Ulid)
                    {
                        value = Ulid.Empty;
                        _skillEffect.Properties[name] = value;
                        Logger.Warning("SkillEffectItemControl: Property '{Name}' expected to be of type 'Ulid', but got '{Type}'. Setting to Ulid.Empty.", name, value.GetType());
                    }
                    
                    return CreateStatRefPropertyControl(name, value);
                }
                case EffectPropertyType.Custom:
                {
                    // For custom properties, we need to check if the effect implemente a static method of name 'CreateControl{PropertyName}'
                    var methodName = $"CreateControl{name}";
                    var methodInfo = _skillEffect.GetType().GetMethod(methodName, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                    if (methodInfo != null)
                    {
                        try
                        {
                            var control = methodInfo.Invoke(null, new object[] { name, value, _skillEffect });
                            if (control is Control ctrl)
                            {
                                return ctrl;
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Error("SkillEffectItemControl: Error invoking custom property control method '{MethodName}' for property '{Name}'. {ex}", methodName, name, ex.Message);
                            return null;
                        }
                    }
                    Logger.Error("SkillEffectItemControl: Custom property control method '{MethodName}' not found for property '{Name}'.", methodName, name);
                    return null;
                }
                default:
                {
                    Logger.Error("SkillEffectItemControl: Unsupported property type '{Type}' for property '{Name}'.", type, name);
                    return null;
                }
            }
        }
    
        #region Property Controls
    
        private Control CreateNumberPropertyControl(string name, object value)
        {
            var panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 5,
                VerticalAlignment = VerticalAlignment.Center
            };
        
            var label = new TextBlock
            {
                Text = name,
                VerticalAlignment = VerticalAlignment.Center,
            };
            panel.Children.Add(label);

            var numBox = new NumericFloatUpDown()
            {
                Minimum = 0,
                Maximum = float.MaxValue,
                Value = Convert.ToSingle(value),
                Width = 100
            };
            panel.Children.Add(numBox);
            
            numBox.PropertyChanged += (s, e) =>
            {
                if (e.Property == NumericFloatUpDown.ValueProperty)
                {
                    var newValue = numBox.Value ?? 0f;
                    _skillEffect.Properties[name] = newValue;
                }
            };
        
            return panel;
        }

        private Control CreateTextPropertyControl(string name, object value)
        {
            var panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 5,
                VerticalAlignment = VerticalAlignment.Center
            };
        
            var label = new TextBlock
            {
                Text = name,
                VerticalAlignment = VerticalAlignment.Center,
            };
            panel.Children.Add(label);

            var textBox = new TextBox
            {
                Text = value as string ?? string.Empty,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Width = 200
            };
            panel.Children.Add(textBox);
            
            textBox.PropertyChanged += (s, e) =>
            {
                if (e.Property == TextBox.TextProperty)
                {
                    var newValue = textBox.Text ?? string.Empty;
                    _skillEffect.Properties[name] = newValue;
                }
            };
        
            return panel;
        }

        private Control CreateBoolPropertyControl(string name, object value)
        {
            var panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 5,
                VerticalAlignment = VerticalAlignment.Center
            };
        
            var label = new TextBlock
            {
                Text = name,
                VerticalAlignment = VerticalAlignment.Center,
            };
            panel.Children.Add(label);

            var checkBox = new CheckBox()
            {
                IsChecked = (bool)value,
                VerticalAlignment = VerticalAlignment.Center
            };
            panel.Children.Add(checkBox);
            
            checkBox.PropertyChanged += (s, e) =>
            {
                if (e.Property == CheckBox.IsCheckedProperty)
                {
                    var newValue = checkBox.IsChecked ?? false;
                    _skillEffect.Properties[name] = newValue;
                }
            };
        
            return panel;
        }
        
        private Control CreatePointPropertyControl(string name, object value)
        {
            var panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 5,
                VerticalAlignment = VerticalAlignment.Center
            };
        
            var label = new TextBlock
            {
                Text = name,
                VerticalAlignment = VerticalAlignment.Center,
            };
            panel.Children.Add(label);

            var xLabel = new TextBlock
            {
                Text = "X:",
                VerticalAlignment = VerticalAlignment.Center,
            };
            panel.Children.Add(xLabel);
            var xBox = new NumericFloatUpDown()
            {
                Minimum = float.MinValue,
                Maximum = float.MaxValue,
                Value = (int)((Point)value).X,
                Width = 80
            };
            panel.Children.Add(xBox);
            
            var yLabel = new TextBlock
            {
                Text = "Y:",
                VerticalAlignment = VerticalAlignment.Center,
            };
            panel.Children.Add(yLabel);
            
            var yBox = new NumericFloatUpDown()
            {
                Minimum = float.MinValue,
                Maximum = float.MaxValue,
                Value = (int)((Point)value).Y,
                Width = 80
            };
            panel.Children.Add(yBox);
            
            void UpdatePoint()
            {
                var newPoint = new Point(xBox.Value ?? 0f, yBox.Value ?? 0f);
                _skillEffect.Properties[name] = newPoint;
            }
            
            xBox.PropertyChanged += (s, e) =>
            {
                if (e.Property == NumericFloatUpDown.ValueProperty)
                {
                    UpdatePoint();
                }
            };
            
            yBox.PropertyChanged += (s, e) =>
            {
                if (e.Property == NumericFloatUpDown.ValueProperty)
                {
                    UpdatePoint();
                }
            };
        
            return panel;
        }
        
        private Control CreateStatRefPropertyControl(string name, object value)
        {
            var panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 5,
                VerticalAlignment = VerticalAlignment.Center
            };
        
            var label = new TextBlock
            {
                Text = name,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 100
            };
            panel.Children.Add(label);

            var comboBox = new ComboBox
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Width = 200,
                PlaceholderText = "Select Stat"
            };
            panel.Children.Add(comboBox);
            //
            // // Populate the combo box with available stats from the Assets Manager
            // foreach (var statDef in EngineCore.Instance.Managers.Assets.StatsRegistry.All())
            // {
            //     comboBox.Items.Add(new ComboBoxItem
            //     {
            //         Content = statDef.Name,
            //         Tag = statDef.Unique
            //     });
            //     
            //     // If this is the current value, select it
            //     if (statDef.Unique.Equals(value))
            //     {
            //         comboBox.SelectedItem = comboBox.Items[comboBox.Items.Count - 1];
            //     }
            // }

            comboBox.SelectionChanged += (s, e) =>
            {
                if (comboBox.SelectedItem is ComboBoxItem selectedItem)
                {
                    if (selectedItem.Tag is Ulid selectedUnique)
                    {
                        _skillEffect.Properties[name] = selectedUnique;
                    }
                }
            };
        
            return panel;
        }
        #endregion
    }

    #region Constants
    #endregion
    
    #region Events
    #endregion
    
    #region Properties
    public ISkillDef SkillDef { get; private set; }
    #endregion
    
    #region Components
    private ScrollBox _body;
    private StackPanel _bodyPanel;
    private Grid _contentGrid;
    private Grid _topPanel;
    private ScrollBox _effectsListBox;
    private StackPanel _effectsListPanel;
    
    private Button _createEffectButton;
    private ComboBox _effectComboBox;
    private Button _addEffectButton;
    #endregion
    
    #region Constructors
    public SkillEffectTab(ISkillDef skillDef)
    {
        ArgumentNullException.ThrowIfNull(skillDef, nameof(skillDef));
        SkillDef = skillDef;
        CreateComponents();
        Refresh();
        Content = _body;
    }
    #endregion
    
    #region Methods
    
    private void CreateComponents()
    {
        _body = new ScrollBox
        {
            Margin = new Thickness(10)
        };
        
        _bodyPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            Spacing = 10
        };
        
        _body.Content = _bodyPanel;
        
        _contentGrid = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*"),
            Margin = new Thickness(0, 0, 0, 10)
        };
        _bodyPanel.Children.Add(_contentGrid);
        
        _topPanel = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto"),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
        };
        _contentGrid.Children.Add(_topPanel);
        Grid.SetRow(_topPanel, 0);
        
        _createEffectButton = new Button
        {
            Content = "Create Effect",
        };
        _topPanel.Children.Add(_createEffectButton);
        Grid.SetColumn(_createEffectButton, 0);
        
        _effectComboBox = new ComboBox
        {
            HorizontalAlignment =HorizontalAlignment.Stretch,
            Margin = App.style.Margin,
            PlaceholderText = "Select Effect"
        };
        _topPanel.Children.Add(_effectComboBox);
        Grid.SetColumn(_effectComboBox, 1);
        
        _addEffectButton = new Button
        {
            Content = "Add Effect",
        };
        _topPanel.Children.Add(_addEffectButton);
        Grid.SetColumn(_addEffectButton, 2);
        
        _addEffectButton.Click += (s, e) =>
        {
            if (_effectComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                if (selectedItem.Tag is URN selectedUrn)
                {
                    SkillDef.EffectsUrn.Add(selectedUrn);
                    AddEffect(selectedUrn);
                }
            }
        };
        
        _effectsListBox = new ScrollBox
        {
        };
        _contentGrid.Children.Add(_effectsListBox);
        Grid.SetRow(_effectsListBox, 1);
        
        _effectsListPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            Spacing = 5
        };
        _effectsListBox.Content = _effectsListPanel;
        
    }

    private void AddEffect(URN effectUrn)
    {
        var effectDef = EngineServices.AssetsManager.TryResolveAsset(effectUrn, out ISkillEffect? effect) ? effect : null;
        if (effectDef != null)
        {
            var effectControl = new SkillEffectItemControl(effectDef);
            effectControl.Tag = effectUrn;
            _effectsListPanel.Children.Add(effectControl);
        }
        else
        {
            Logger.Warning("SkillEffectTab: Effect with URN '{Urn}' not found in registry.", effectUrn);
        }
    }
    
    private void ClearEffects()
    {
        _effectsListPanel.Children.Clear();
    }
    
    private void RemoveEffect(URN effectUrn)
    {
        foreach (var child in _effectsListPanel.Children)
        {
            if (child is SkillEffectItemControl effectControl)
            {
                if (effectControl.Tag is URN childUrn && childUrn == effectUrn)
                {
                    _effectsListPanel.Children.Remove(effectControl);
                    break;
                }
            }
        }
    }

    public void Refresh()
    {
        _effectComboBox.Items.Clear();

        var skillEffects = EngineServices.AssetsManager.GetAssets<ISkillEffect>();
        
        foreach (var effect in skillEffects)
        {
            _effectComboBox.Items.Add(new ComboBoxItem()
            {
                Content = effect.DisplayName,
                Tag = effect.Urn
            });
        }
    }
    
    #endregion

    #region Events Handlers
    #endregion
}