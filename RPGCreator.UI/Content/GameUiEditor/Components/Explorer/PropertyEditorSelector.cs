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
using System.Linq;
using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.VisualTree;
using RPGCreator.SDK;
using RPGCreator.SDK.GameUI;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Types;
using Ursa.Controls;

namespace RPGCreator.UI.Content.GameUiEditor.Components.Explorer;

internal static class PropertyEditorCommonData
{
    private static readonly Dictionary<Type, object[]> _enumCache = new();
    
    public static object[] GetEnumValues(Type enumType)
    {
        if (!_enumCache.TryGetValue(enumType, out var values))
        {
            values = Enum.GetValues(enumType).Cast<object>().ToArray();
            _enumCache[enumType] = values;
        }
        return values;
    }
    
}

public class PropertyEditorSelector: IDataTemplate
{
    
    public IDataTemplate? BooleanTemplate { get; set; }
    public IDataTemplate? Vector2Template { get; set; }
    public IDataTemplate? ColorTemplate { get; set; }
    public IDataTemplate? StringTemplate { get; set; }
    
    public IDataTemplate? IntegerTemplate { get; set; }
    public IDataTemplate? FloatTemplate { get; set; }
    
    public PropertyEditorSelector()
    {
        BooleanTemplate = new FuncDataTemplate<EditableControlPropertyDescriptor>((descriptor, _) =>
        {
            var checkBox = new CheckBox()
            {
                IsChecked = descriptor.Get<bool>(),
            };
            descriptor.ValueChanged += (newValue, oldValue) =>
            {
                checkBox.IsChecked = newValue as bool? ?? false;
            };
            checkBox.IsCheckedChanged += (_, _) =>
            {
                if(descriptor.Get<bool>() == checkBox.IsChecked) return;
                descriptor.Set(checkBox.IsChecked ?? false);
            };
            return checkBox;
        });

        Vector2Template = new FuncDataTemplate<EditableControlPropertyDescriptor>((descriptor, _) =>
            {
                var grid = new Grid()
                {
                    ColumnDefinitions = new ColumnDefinitions("*, *"),
                    ColumnSpacing = 4
                };

                var vectorX = new NumericIntUpDown()
                {
                    InnerLeftContent = "X",
                    AllowDrag = true,
                    IsReadOnly = false,
                    Value = (int)descriptor.Get<Vector2>().X
                };
                ToolTip.SetTip(vectorX, "Drag to change the value.\nDouble click to edit it with your keyboard.");

                var vectorY = new NumericIntUpDown()
                {
                    InnerLeftContent = "Y",
                    AllowDrag = true,
                    IsReadOnly = false,
                    Value = (int)descriptor.Get<Vector2>().Y
                };
                ToolTip.SetTip(vectorY, "Drag to change the value.\nDouble click to edit it with your keyboard.");

                descriptor.ValueChanged += (newValue, oldValue) =>
                {
                    if (newValue is not Vector2 vectorValue) return;
                    vectorX.Value = (int)vectorValue.X;
                    vectorY.Value = (int)vectorValue.Y;
                };
                vectorX.ValueChanged += (_, _) =>
                {
                    var valueX = vectorX.Value;
                    if (valueX == null) valueX = 0;
                    var valueY = vectorY.Value;
                    if (valueY == null) valueY = 0;
                    descriptor.Set(new Vector2((float)valueX, (float)valueY));
                };
                vectorY.ValueChanged += (_, _) =>
                {
                    var valueX = vectorX.Value;
                    if (valueX == null) valueX = 0;
                    var valueY = vectorY.Value;
                    if (valueY == null) valueY = 0;
                    descriptor.Set(new Vector2((float)valueX, (float)valueY));
                };

                grid.Children.Add(vectorX);
                grid.Children.Add(vectorY);
                Grid.SetColumn(vectorY, 1);

                return grid;
            }
        );
        
        ColorTemplate = new FuncDataTemplate<EditableControlPropertyDescriptor>((descriptor, _) =>
        {
            var colorPicker = new ColorPicker()
            {
                Color = ConvertToAval(descriptor.Get<Color>()),
            };
            descriptor.ValueChanged += (newValue, oldValue) => colorPicker.Color = ConvertToAval(newValue as Color? ?? Color.Pink);
            colorPicker.ColorChanged += (_, args) => descriptor.Set(ConvertToRpg(args.NewColor));
            
            return colorPicker;

            Avalonia.Media.Color ConvertToAval(Color color)
            {
                return new Avalonia.Media.Color(color.A, color.R, color.G, color.B);
            }

            Color ConvertToRpg(Avalonia.Media.Color color)
            {
                return new Color(color.R, color.G, color.B, color.A);
            }
        });
        
        StringTemplate = new FuncDataTemplate<EditableControlPropertyDescriptor>((descriptor, _) =>
        {
            var textBox = new TextBox()
            {
                Watermark = "Enter a value...",
                Text = descriptor.Get<string>()
            };
            descriptor.ValueChanged += (newValue, oldValue) => textBox.Text = newValue as string ?? "";
            textBox.GotFocus += (_, _) => textBox.SelectAll();
            textBox.KeyDown += (_, args) =>
            {
                if (args.Key == Key.Enter)
                {
                    descriptor.Set(textBox.Text ?? "");
                }
            };
            textBox.LostFocus += (_, _) => descriptor.Set(textBox.Text ?? "");
            return textBox;
        });

        IntegerTemplate = new FuncDataTemplate<EditableControlPropertyDescriptor>((descriptor, _) =>
        {
            var numericUpDown = new NumericIntUpDown()
            {
                InnerLeftContent = "Value",
                AllowDrag = true,
                IsReadOnly = false,
                Value = descriptor.Get<int>()
            };
            descriptor.ValueChanged += (newValue, oldValue) => numericUpDown.Value = newValue as int? ?? 0;
            numericUpDown.ValueChanged += (_, _) => descriptor.Set(numericUpDown.Value ?? 0);
            return numericUpDown;
        });

        FloatTemplate = new FuncDataTemplate<EditableControlPropertyDescriptor>((descriptor, _) =>
        {
            var numericUpDown = new NumericFloatUpDown()
            {
                InnerLeftContent = "Value",
                AllowDrag = true,
                IsReadOnly = false,
                Value = descriptor.Get<float>()
            };
            descriptor.ValueChanged += (newValue, oldValue) => numericUpDown.Value = newValue as float? ?? 0f;
            numericUpDown.ValueChanged += (_, _) => descriptor.Set(numericUpDown.Value ?? 0);
            return numericUpDown;
        });
    }
    
    public Control? Build(object? param)
    {
        if (param is not EditableControlPropertyDescriptor descriptor)
        {
            return new TextBlock() { Text = "ERROR: Invalid descriptor type." };
        }

        var grid = new Grid()
        {
            ColumnDefinitions = new ColumnDefinitions("*, Auto"),
            ColumnSpacing = 4
        };
        
        var dataType = descriptor.Type;

        return dataType switch
        {
            _ when dataType == typeof(bool) => BooleanTemplate?.Build(descriptor),
            _ when dataType == typeof(Vector2) => Vector2Template?.Build(descriptor),
            _ when dataType == typeof(Color) => ColorTemplate?.Build(descriptor),
            _ when dataType == typeof(string) => StringTemplate?.Build(descriptor),
            _ when dataType == typeof(float) => FloatTemplate?.Build(descriptor),
            _ when dataType == typeof(int) => IntegerTemplate?.Build(descriptor),
            _ when dataType.IsEnum => BuildEnumEditor(descriptor),
            _ => FindOrDefault(descriptor)
        };
    }

    private Control FindOrDefault(EditableControlPropertyDescriptor descriptor)
    {
        var generationResult = RegistryServices.PropertyEditorRegistry.GeneratePropertyEditor(descriptor);

        if (generationResult.Value is not Control && generationResult.IsFailure)
        {
            generationResult = Result.Fail($"Property editor generation failed for {descriptor.Name}({descriptor.Type}) due to the fact that the result is not a valid Avalonia Control.");
        }
        
        if (generationResult.IsFailure)
        {
            Logger.Error(generationResult.Error);
            var text = new TextBlock
            {
                Text = descriptor.Get()?.ToString() ?? "N/A", Opacity = 0.5, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Cursor = new Cursor(StandardCursorType.No),
            };
            ToolTip.SetTip(text, "This property is not editable due to an error.");
            return text;
        }

        return generationResult.Value as Control ?? new TextBlock { Text = "ERROR: Result returned a success, but the result is not a valid Avalonia Control. This should not happen!"};
    }

    private Control BuildEnumEditor(EditableControlPropertyDescriptor descriptor)
    {
        var comboBox = new ComboBox
        {
            ItemsSource = PropertyEditorCommonData.GetEnumValues(descriptor.Type),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            AutoScrollToSelectedItem = true
        };

        comboBox.SelectedItem = descriptor.Get();

        comboBox.SelectionChanged += (_, _) =>
        {
            if (comboBox.SelectedItem != null)
                descriptor.Set(comboBox.SelectedItem);
        };

        descriptor.ValueChanged += (newValue, oldValue) => comboBox.SelectedItem = newValue;

        return comboBox;
    }

    public bool Match(object? data)
    {
        return data is EditableControlPropertyDescriptor;
    }
}


public class PropertyReadOnlySelector: IDataTemplate
{
    
    public IDataTemplate? BooleanTemplate { get; set; }
    public IDataTemplate? Vector2Template { get; set; }
    public IDataTemplate? ColorTemplate { get; set; }
    public IDataTemplate? StringTemplate { get; set; }
    public IDataTemplate? IntegerTemplate { get; set; }
    public IDataTemplate? FloatTemplate { get; set; }
    
    public PropertyReadOnlySelector()
    {
        var noCursor = new Cursor(StandardCursorType.No);
        
        BooleanTemplate = new FuncDataTemplate<ReadOnlyControlPropertyDescriptor>((descriptor, _) =>
        {
            var checkBox = new CheckBox()
            {
                IsChecked = descriptor.Get<bool>(),
                IsEnabled = false,
                Cursor = noCursor
            };
            return checkBox;
        });

        Vector2Template = new FuncDataTemplate<ReadOnlyControlPropertyDescriptor>((descriptor, _) =>
            {
                var grid = new Grid()
                {
                    ColumnDefinitions = new ColumnDefinitions("*, *"),
                    ColumnSpacing = 4
                };

                var vectorX = new NumericIntUpDown()
                {
                    InnerLeftContent = "X",
                    IsReadOnly = true,
                    Value = (int)descriptor.Get<Vector2>().X,
                    Cursor = noCursor
                };

                var vectorY = new NumericIntUpDown()
                {
                    InnerLeftContent = "Y",
                    IsReadOnly = true,
                    Value = (int)descriptor.Get<Vector2>().Y,
                    Cursor = noCursor
                };

                grid.Children.Add(vectorX);
                grid.Children.Add(vectorY);
                Grid.SetColumn(vectorY, 1);

                return grid;
            }
        );
        
        ColorTemplate = new FuncDataTemplate<ReadOnlyControlPropertyDescriptor>((descriptor, _) =>
        {
            var colorPicker = new ColorPicker()
            {
                Color = ConvertToAval(descriptor.Get<Color>()),
                IsEnabled = false,
                Cursor = noCursor
            };
            
            return colorPicker;

            Avalonia.Media.Color ConvertToAval(Color color)
            {
                return new Avalonia.Media.Color(color.A, color.R, color.G, color.B);
            }
        });
        
        StringTemplate = new FuncDataTemplate<ReadOnlyControlPropertyDescriptor>((descriptor, _) =>
        {
            var textBox = new TextBox()
            {
                Watermark = "Enter a value...",
                Text = descriptor.Get<string>(),
                IsReadOnly = true,
                Cursor = noCursor
            };
            textBox.GotFocus += (_, _) => textBox.SelectAll();
            return textBox;
        });
        IntegerTemplate = new FuncDataTemplate<EditableControlPropertyDescriptor>((descriptor, _) =>
        {
            var numericUpDown = new NumericIntUpDown()
            {
                InnerLeftContent = "Value",
                AllowDrag = false,
                IsReadOnly = true,
                Value = descriptor.Get<int>(),
                Cursor = noCursor
            };
            descriptor.ValueChanged += (newValue, oldValue) => numericUpDown.Value = newValue as int? ?? 0;
            numericUpDown.ValueChanged += (_, _) => descriptor.Set(numericUpDown.Value ?? 0);
            return numericUpDown;
        });

        FloatTemplate = new FuncDataTemplate<EditableControlPropertyDescriptor>((descriptor, _) =>
        {
            var numericUpDown = new NumericFloatUpDown()
            {
                InnerLeftContent = "Value",
                AllowDrag = false,
                IsReadOnly = true,
                Value = descriptor.Get<float>(),
                Cursor = noCursor
            };
            descriptor.ValueChanged += (newValue, oldValue) => numericUpDown.Value = newValue as float? ?? 0f;
            numericUpDown.ValueChanged += (_, _) => descriptor.Set(numericUpDown.Value ?? 0);
            return numericUpDown;
        });
    }
    
    public Control? Build(object? param)
    {
        if (param is not ReadOnlyControlPropertyDescriptor descriptor)
        {
            return new TextBlock() { Text = "ERROR: Invalid descriptor type." };
        }

        var grid = new Grid()
        {
            ColumnDefinitions = new ColumnDefinitions("*, Auto"),
            ColumnSpacing = 4
        };
        
        var dataType = descriptor.Type;

        return dataType switch
        {
            _ when dataType == typeof(bool) => BooleanTemplate?.Build(descriptor),
            _ when dataType == typeof(Vector2) => Vector2Template?.Build(descriptor),
            _ when dataType == typeof(Color) => ColorTemplate?.Build(descriptor),
            _ when dataType == typeof(string) => StringTemplate?.Build(descriptor),
            _ when dataType.IsEnum => BuildEnumEditor(descriptor),
            _ => FindOrDefault(descriptor)
        };
    }

    private Control FindOrDefault(ReadOnlyControlPropertyDescriptor descriptor)
    {
        var generationResult = RegistryServices.PropertyEditorRegistry.GeneratePropertyEditor(descriptor);

        if (generationResult.Value is not Control && generationResult.IsFailure)
        {
            generationResult = Result.Fail($"Property editor generation failed for {descriptor.Name}({descriptor.Type}) due to the fact that the result is not a valid Avalonia Control.");
        }
        
        if (generationResult.IsFailure)
        {
            Logger.Error(generationResult.Error);
            var text = new TextBlock
            {
                Text = descriptor.Get()?.ToString() ?? "N/A", Opacity = 0.5, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Cursor = new Cursor(StandardCursorType.No),
            };
            ToolTip.SetTip(text, "This property is not editable due to an error.");
            return text;
        }

        return generationResult.Value as Control ?? new TextBlock { Text = "ERROR: Result returned a success, but the result is not a valid Avalonia Control. This should not happen!"};
    }

    private Control BuildEnumEditor(ReadOnlyControlPropertyDescriptor descriptor)
    {
        var comboBox = new ComboBox
        {
            ItemsSource = PropertyEditorCommonData.GetEnumValues(descriptor.Type),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            AutoScrollToSelectedItem = true,
            IsEditable = false,
            IsEnabled = false,
            Cursor = new Cursor(StandardCursorType.No)
        };

        comboBox.SelectedItem = descriptor.Get();

        return comboBox;
    }

    public bool Match(object? data)
    {
        return data is ReadOnlyControlPropertyDescriptor;
    }
}