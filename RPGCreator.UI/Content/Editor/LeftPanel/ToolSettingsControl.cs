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
using System.ComponentModel;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Media;
using RPGCreator.SDK;
using RPGCreator.SDK.GlobalState;
using RPGCreator.SDK.Logging;
using RPGCreator.UI.Common;
using Ursa.Controls;

namespace RPGCreator.UI.Content.Editor.LeftPanel;

public class ToolSettingsControl : UserControl
{

    private TextBlock _contentHeader = null!;
    private HelpButton _contentHeaderToolHelpButton;
    private Expander _content = null!;
    private Grid? _body;
    private ScrollViewer? _scroll;
    private ItemsControl? _settingsPanel;

    public ToolSettingsControl()
    {
        CreateComponents();
        RegisterEvents();
        this.Content = _content;
    }

    private void CreateComponents()
    {
        
        _contentHeader = new TextBlock()
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Inlines = new InlineCollection()
        };

        _contentHeaderToolHelpButton = new HelpButton(ToolLogic.NoHelp)
        {
            VerticalAlignment = VerticalAlignment.Center,
            IsEnabled = false
        };
        ToolTip.SetShowOnDisabled(_contentHeaderToolHelpButton, true);
        ToolTip.SetTip( _contentHeaderToolHelpButton, "Sorry, no help available for this tool.");
        
        var contentHeaderUiContainer = new InlineUIContainer(_contentHeaderToolHelpButton);
        var contentHeaderText = new Run("Tool Settings")
        {
            BaselineAlignment = BaselineAlignment.Center,
            FontWeight = FontWeight.Bold,
            FontSize = 14
        };
        
        _contentHeader.Inlines.Add(contentHeaderUiContainer);
        _contentHeader.Inlines.Add(new Border() { Margin = new Thickness(4, 0), Width = 1});
        _contentHeader.Inlines.Add(contentHeaderText);
        
        _content = new Expander()
        {
            IsExpanded = false,
            Header = _contentHeader,
            ExpandDirection = ExpandDirection.Up
        };
        
        _body = new Grid()
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            RowDefinitions = new RowDefinitions("*"),
            ColumnDefinitions = new ColumnDefinitions("*"),
            MaxHeight = 250
        };
        
        _content.Content = _body;
        _scroll = new ScrollViewer()
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        };
        
        _body.Children.Add(_scroll);
        _settingsPanel = new ItemsControl()
        {
            ItemsSource = GlobalStates.ToolState.ActiveToolParameters,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
            ItemsPanel = new FuncTemplate<Panel?>(() => new StackPanel()
            {
                Orientation = Avalonia.Layout.Orientation.Vertical,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
                Spacing = 4,
                Margin = new Thickness(0, 0, 8, 0)
            }),
            DataTemplates =
            {
                new FuncDataTemplate<RangeParameter>(TemplateForRangeParameter),
                new FuncDataTemplate<StringParameter>(TemplateForStringParameter),
                new FuncDataTemplate<IntParameter>(TemplateForIntParameter),
                new FuncDataTemplate<FloatParameter>(TemplateForFloatParameter),
                new FuncDataTemplate<DoubleParameter>(TemplateForDoubleParameter),
                new FuncDataTemplate<BoolParameter>(TemplateForBoolParameter),
                new FuncDataTemplate<ICustomObjectParameter>(TemplateForCustomParameter)
            }
        };
        _scroll.Content = _settingsPanel;
    }
    
    private void RegisterEvents()
    {
        GlobalStates.ToolState.PropertyChanged += (s, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(IToolState.ActiveTool):
                    _content.IsExpanded = GlobalStates.ToolState.ActiveTool != null;
                    _settingsPanel!.ItemsSource = GlobalStates.ToolState.ActiveToolParameters;
                    _contentHeaderToolHelpButton.IsEnabled = false;
                    ToolTip.SetTip(_contentHeaderToolHelpButton, "Sorry, no help is available for this tool.");
                    if (_content.IsExpanded)
                    {
                        var helpKey = GlobalStates.ToolState.ActiveTool?.HelpKey;

                        if (helpKey != null && helpKey != ToolLogic.NoHelp && GlobalStates.ToolState.ActiveTool != null)
                        {
                            _contentHeaderToolHelpButton.SetHelpDocsKey(GlobalStates.ToolState.ActiveTool.HelpKey);
                            ToolTip.SetTip(_contentHeaderToolHelpButton, "Click to view help documentation for this tool.");
                            _contentHeaderToolHelpButton.IsEnabled = true;
                        }
                    }
                    break;
                case nameof(IToolState.ActiveToolParameters):
                    _settingsPanel!.ItemsSource = GlobalStates.ToolState.ActiveToolParameters;
                    break;
            }
        };
    }

    private StackPanel WrapInLabel(IToolParameter param, Control control)
    {

        var paramName = new TextBlock()
        {
            Text = param.DisplayName,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            FontWeight = FontWeight.SemiBold
        };
        ToolTip.SetTip(paramName, param.Description);
        
        return new StackPanel()
        {
            Spacing = 2,
            Children =
            {
                paramName,
                control
            }
        };
    }
    
    #region ParameterControlTemplates

    private StackPanel TemplateForRangeParameter(RangeParameter parameter, INameScope scope)
    {
        var control = new Slider()
        {
            Minimum = parameter.Min,
            Maximum = parameter.Max,
            Orientation =  Avalonia.Layout.Orientation.Horizontal,
            TickFrequency = parameter.Step,
            IsSnapToTickEnabled = true,
            Value = parameter.Value as double? ?? 0,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };

        control.ValueChanged += OnControlChanged;

        parameter.PropertyChanged += OnParameterChanged;

        control.DetachedFromVisualTree += OnControlDetachedFromVisualTree;
        
        return WrapInLabel(parameter, control);

        void OnControlDetachedFromVisualTree(object? s, VisualTreeAttachmentEventArgs e)
        {
            control.ValueChanged -= OnControlChanged;
            parameter.PropertyChanged -= OnParameterChanged;
            control.DetachedFromVisualTree -= OnControlDetachedFromVisualTree;
        }

        void OnControlChanged(object? s, RangeBaseValueChangedEventArgs e)
        {
            parameter.Value = control.Value;
        }

        void OnParameterChanged(object? s, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IToolParameter.Value))
            {
                var paramValue = parameter.GetValueAs<double>(out _);
                if (Math.Abs(control.Value - paramValue) > 0.001) control.Value = paramValue;
            }
        }
    }
    private StackPanel TemplateForStringParameter(StringParameter parameter, INameScope scope)
    {
        var control = new TextBox()
        {
            Text = parameter.GetValueAs<string>(out _),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };

        control.TextChanged += OnControlChanged;

        parameter.PropertyChanged += OnParameterChanged;

        control.DetachedFromVisualTree += OnControlDetachedFromVisualTree;
        
        
        return WrapInLabel(parameter, control);

        void OnControlChanged(object? s, TextChangedEventArgs e)
        {
            if (control.Text != (parameter.Value as string ?? "")) parameter.Value = control.Text;
        }

        void OnParameterChanged(object? s, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IToolParameter.Value) && control.Text != (parameter.Value as string ?? ""))
            {
                control.Text = parameter.GetValueAs<string>(out _);
            }
        }

        void OnControlDetachedFromVisualTree(object? s, VisualTreeAttachmentEventArgs e)
        {
            control.TextChanged -= OnControlChanged;
            parameter.PropertyChanged -= OnParameterChanged;
            control.DetachedFromVisualTree -= OnControlDetachedFromVisualTree;
        }
    }
    private StackPanel TemplateForIntParameter(IntParameter parameter, INameScope scope)
    {
        var control = new NumericIntUpDown()
        {
            Minimum = parameter.Min,
            Maximum = parameter.Max,
            Step = parameter.Step,
            Value = parameter.Value as int? ?? 0,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };

        control.ValueChanged += OnControlChanged;

        parameter.PropertyChanged += OnParameterChanged;

        control.DetachedFromVisualTree += OnControlDetachedFromVisualTree;
        
        
        return WrapInLabel(parameter, control);

        void OnControlChanged(object? s, ValueChangedEventArgs<int> e)
        {
            if (control.Value != (parameter.Value as int? ?? 0)) parameter.Value = (int)control.Value;
        }

        void OnParameterChanged(object? s, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IToolParameter.Value))
            {
                var paramValue = parameter.GetValueAs<int>(out _);
                if (control.Value != paramValue) control.Value = paramValue;
            }
        }

        void OnControlDetachedFromVisualTree(object? s, VisualTreeAttachmentEventArgs e)
        {
            control.ValueChanged -= OnControlChanged;
            parameter.PropertyChanged -= OnParameterChanged;
            control.DetachedFromVisualTree -= OnControlDetachedFromVisualTree;
        }
    }
    private StackPanel TemplateForFloatParameter(FloatParameter parameter, INameScope scope)
    {
        var control = new NumericFloatUpDown()
        {
            Minimum = parameter.Min,
            Maximum = parameter.Max,
            Step = parameter.Step,
            Value = parameter.GetValueAs<float>(out _),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };

        control.ValueChanged += OnControlChanged;

        parameter.PropertyChanged += OnParameterChanged;

        control.DetachedFromVisualTree += OnControlDetachedFromVisualTree;
        
        
        return WrapInLabel(parameter, control);

        void OnControlDetachedFromVisualTree(object? s, VisualTreeAttachmentEventArgs e)
        {
            control.ValueChanged -= OnControlChanged;
            parameter.PropertyChanged -= OnParameterChanged;
            control.DetachedFromVisualTree -= OnControlDetachedFromVisualTree;
        }

        void OnControlChanged(object? s, ValueChangedEventArgs<float> e)
        {
            var paramValue = parameter.GetValueAs<float>(out _);
            var newValue = (float)control.Value;
            if (Math.Abs(newValue - paramValue) > 0.0001) parameter.Value = (float)control.Value;
        }

        void OnParameterChanged(object? s, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IToolParameter.Value))
            {
                var paramValue = parameter.GetValueAs<float>(out _);
                var newValue = (float)control.Value;
                if (Math.Abs(newValue - paramValue) > 0.0001) control.Value = paramValue;
            }
        }
    }
    private StackPanel TemplateForDoubleParameter(DoubleParameter parameter, INameScope scope)
    {
        var control = new NumericDoubleUpDown()
        {
            Minimum = parameter.Min,
            Maximum = parameter.Max,
            Step = parameter.Step,
            Value = parameter.GetValueAs<double>(out _),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            FormatString = "0.###########" // To avoid displaying too much decimal places that could be caused by precision errors.
        };

        control.ValueChanged += OnControlChanged;

        parameter.PropertyChanged += OnParameterChanged;
        
        control.DetachedFromVisualTree += OnControlDetachedFromVisualTree;
        
        return WrapInLabel(parameter, control);

        void OnControlChanged(object? s, ValueChangedEventArgs<double> e)
        {
            double rawValue = control.Value ?? 0;
    
            int decimals = GetDecimalPlaces(parameter.Step);
            double cleanValue = Math.Round(rawValue, decimals);

            if (Math.Abs(rawValue - cleanValue) > double.Epsilon)
            {
                control.Value = cleanValue;
                return;
            }

            var paramValue = parameter.GetValueAs<double>(out _);
            if (Math.Abs(cleanValue - paramValue) > 0.0000000001)
            {
                parameter.Value = cleanValue;
            }
        }

        void OnParameterChanged(object? s, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IToolParameter.Value))
            {
                var paramValue = parameter.GetValueAs<double>(out _);
                var newValue = (double)control.Value;
                paramValue = Math.Round(paramValue, 10);
                if (Math.Abs(newValue - paramValue) > 0.000001) control.Value = paramValue;
            }
        }

        void OnControlDetachedFromVisualTree(object? s, VisualTreeAttachmentEventArgs e)
        {
            control.ValueChanged -= OnControlChanged;
            parameter.PropertyChanged -= OnParameterChanged;
            control.DetachedFromVisualTree -= OnControlDetachedFromVisualTree;
        }
    }
    private StackPanel TemplateForBoolParameter(BoolParameter parameter, INameScope scope)
    {
        var control = new CheckBox()
        {
            IsChecked = parameter.GetValueAs<bool>(out _),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };

        control.IsCheckedChanged += OnControlChanged;

        parameter.PropertyChanged += OnParameterChanged;
        
        control.DetachedFromVisualTree += OnControlDetachedFromVisualTree;
        
        return WrapInLabel(parameter, control);

        void OnParameterChanged(object? s, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IToolParameter.Value))
            {
                var paramValue = parameter.GetValueAs<bool>(out _);
                if (control.IsChecked != paramValue) control.IsChecked = paramValue;
            }
        }

        void OnControlChanged(object? s, RoutedEventArgs e)
        {
            if (parameter.Value as bool? != control.IsChecked) parameter.Value = control.IsChecked == true;
        }

        void OnControlDetachedFromVisualTree(object? s, VisualTreeAttachmentEventArgs e)
        {
            control.IsCheckedChanged -= OnControlChanged;
            parameter.PropertyChanged -= OnParameterChanged;
            control.DetachedFromVisualTree -= OnControlDetachedFromVisualTree;
        }
    }
    private Control TemplateForCustomParameter(ICustomObjectParameter parameter, INameScope scope)
    {
        var obj = parameter.GetValidUiControl();
        
        if(obj is Control control)
            return control;
        
        Logger.Error("Custom parameter '{ParameterName}' returned an invalid UI control. Expected a Control but got {ControlType}.", args: [parameter.DisplayName, obj?.GetType().FullName ?? "null"]);
        return new TextBlock()
        {
            Text = "Invalid control for parameter '" + parameter.DisplayName + "'.",
            Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Red)
        };
    }
    
    #endregion
    
    #region DoubleHelper
    
    private int GetDecimalPlaces(double step)
    {
        string s = step.ToString(CultureInfo.InvariantCulture);
        int index = s.IndexOf('.');
        return index == -1 ? 0 : s.Length - index - 1;
    }
    
    #endregion
    
    
}