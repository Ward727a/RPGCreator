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
using System.Numerics;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.VisualTree;
using RPGCreator.SDK.GameUI;
using RPGCreator.SDK.Types;
using Ursa.Controls;

namespace RPGCreator.UI.Content.GameUiEditor.Components.Explorer;

public class PropertyEditorSelector: IDataTemplate
{
    public IDataTemplate? BooleanTemplate { get; set; }
    public IDataTemplate? Vector2Template { get; set; }
    public IDataTemplate? ColorTemplate { get; set; }
    
    public PropertyEditorSelector()
    {
        BooleanTemplate = new FuncDataTemplate<ControlPropertyDescriptor<bool>>((descriptor, _) =>
        {
            var checkBox = new CheckBox()
            {
                IsChecked = descriptor.Get()
            };
            descriptor.ValueChanged += (_) =>
            {
                checkBox.IsChecked = descriptor.Get();
            };
            checkBox.IsCheckedChanged += (_, _) =>
            {
                if(descriptor.Get() == checkBox.IsChecked) return;
                descriptor.Set(checkBox.IsChecked ?? false);
            };
            return checkBox;
        });

        Vector2Template = new FuncDataTemplate<ControlPropertyDescriptor<Vector2>>((descriptor, _) =>
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
                    Value = (int)descriptor.Get().X
                };
                ToolTip.SetTip(vectorX, "Drag to change the value.\nDouble click to edit it with your keyboard.");

                var vectorY = new NumericIntUpDown()
                {
                    InnerLeftContent = "Y",
                    AllowDrag = true,
                    IsReadOnly = false,
                    Value = (int)descriptor.Get().Y
                };
                ToolTip.SetTip(vectorY, "Drag to change the value.\nDouble click to edit it with your keyboard.");

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
        
        ColorTemplate = new FuncDataTemplate<ControlPropertyDescriptor<Color>>((descriptor, _) =>
        {
            var colorPicker = new ColorPicker()
            {
                Color = ConvertToAval(descriptor.Get()),
            };
            descriptor.ValueChanged += (val) => colorPicker.Color = ConvertToAval(descriptor.Get());
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
    }
    
    public Control? Build(object? param)
    {
        if (param is not ControlPropertyDescriptor descriptor)
        {
            return new TextBlock() { Text = "ERROR: Invalid descriptor type." };
        }

        var grid = new Grid()
        {
            ColumnDefinitions = new ColumnDefinitions("*, Auto"),
            ColumnSpacing = 4
        };
        
        var dataType = descriptor.Type;

        TextBlock Default()
        {
            var text = new TextBlock
            {
                Text = descriptor.Get()?.ToString() ?? "N/A", Opacity = 0.5, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Cursor = new Cursor(StandardCursorType.No),
            };
            ToolTip.SetTip(text, "This property is not editable due to an error.");
            return text;
        }

        return dataType switch
        {
            _ when dataType == typeof(bool) => BooleanTemplate?.Build(descriptor),
            _ when dataType == typeof(Vector2) => Vector2Template?.Build(descriptor),
            _ when dataType == typeof(Color) => ColorTemplate?.Build(descriptor),
            _ when dataType.IsEnum => BuildEnumEditor(descriptor),
        
            _ => Default()
        };
    }
    
    private Control BuildEnumEditor(ControlPropertyDescriptor descriptor)
    {
        var comboBox = new ComboBox
        {
            ItemsSource = Enum.GetValues(descriptor.Type),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch
        };

        comboBox.SelectedItem = descriptor.Get();

        comboBox.SelectionChanged += (_, _) =>
        {
            if (comboBox.SelectedItem != null)
                descriptor.Set(comboBox.SelectedItem);
        };

        descriptor.ValueChanged += (val) => comboBox.SelectedItem = val;

        return comboBox;
    }

    public bool Match(object? data)
    {
        return true;
    }
}