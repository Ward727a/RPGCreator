using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Media;
using RPGCreator.Core.Parser.PRATT;
using RPGCreator.Core.Type.Assets.Characters.Stats;
using RPGCreator.UI.Common;
using Serilog;

namespace RPGCreator.UI.Content.AssetsManage.AssetsEditors.StatsEditor.Tabs;

public class StatEditorTab : UserControl
{
    #region Constants
    #endregion
    
    #region Events
    #endregion
    
    #region Properties
    public IStatDef StatDef { get; private set; }
    #endregion
    
    #region Components
    private ScrollBox _body;
    private StackPanel _bodyPanel;
    private TextBox _statName;
    private TextBox _statDescription;
    private ComboBox _statTypeKind;
    private NumericUpDown _statDefaultValue;
    private NumericUpDown _statMinValue;
    private ComboBox _statTypeCap;
    private NumericUpDown _statMaxValue;
    private ToggleSwitch _statIsVisible;
    #endregion
    
    #region Constructors
    public StatEditorTab(IStatDef statDef)
    {
        StatDef = statDef;
        CreateComponents();
        RegisterEvents();
        Content = _body;
    }
    #endregion
    
    #region Methods
    private void CreateComponents()
    {
        _body = new ScrollBox()
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            Margin = App.style.Margin
        };
        _bodyPanel = new StackPanel
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            Margin = App.style.Margin
        };
        _body.Content = _bodyPanel;
        
        _statName = new TextBox
        {
            Watermark = "(Required)",
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Margin = App.style.Margin,
            Text = StatDef.Name
        };
        _bodyPanel.Children.Add(new InputLabel("Stat Name", _statName));
        
        _statDescription = new TextBox
        {
            Watermark = "(Optional)",
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Margin = App.style.Margin,
            Text = StatDef.Description,
            AcceptsReturn = true,
            AcceptsTab = true,
            TextWrapping = TextWrapping.Wrap
        };
        _bodyPanel.Children.Add(new InputLabel("Stat Description", _statDescription));
        
        _statTypeKind = new ComboBox
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Margin = App.style.Margin,
            SelectedItem = StatDef.StatTypeKind
        };
        var inputStatTypeKind = new InputLabel("Stat Type Kind", _statTypeKind);
        _bodyPanel.Children.Add(inputStatTypeKind);
        ToolTip.SetTip(inputStatTypeKind, "Defines the type of the stat.\n" +
                                          "1. Resource: A stat that represents an diminishable resource, such as health or mana.\n" +
                                          "2. Attribute: A stat that represents an attribute or characteristic, such as strength or intelligence.\n" +
                                          "3. Derived: A special stat that is calculated based on other stats, such as attack power or defense.");
        
        foreach (var kind in Enum.GetValues(typeof(EStatTypeKind)))
        {
            _statTypeKind.Items.Add(kind.ToString());
        }
        
        _statDefaultValue = new NumericUpDown
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Margin = App.style.Margin,
            Value = (decimal)StatDef.DefaultValue
        };
        _bodyPanel.Children.Add(new InputLabel("Default Value", _statDefaultValue));
        
        _statMinValue = new NumericUpDown
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Margin = App.style.Margin,
            Value = (decimal)StatDef.StatMinValue
        };
        _bodyPanel.Children.Add(new InputLabel("Minimum Value", _statMinValue));
        
        _statTypeCap = new ComboBox
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Margin = App.style.Margin,
            SelectedItem = StatDef.StatCapType.ToString()
        };
        var inputStatTypeCap = new InputLabel("Stat Max Type", _statTypeCap);
        _bodyPanel.Children.Add(inputStatTypeCap);
        ToolTip.SetTip(inputStatTypeCap, "Defines how the maximum value of the stat is determined.\n" +
                                         "1. ByValue: The maximum value is set by the value field.\n" +
                                         "2. ByStat: The maximum value is determined by another stat.");
        
        foreach (var capType in Enum.GetValues(typeof(EStatTypeCap)))
        {
            _statTypeCap.Items.Add(capType.ToString());
        }
        
        _statMaxValue = new NumericUpDown
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Margin = App.style.Margin,
            Value = (decimal)StatDef.StatCapValue
        };
        _bodyPanel.Children.Add(new InputLabel("Maximum Value", _statMaxValue));
        
        _statIsVisible = new ToggleSwitch
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Margin = App.style.Margin,
            IsChecked = StatDef.IsVisible
        };
        var inputStatIsVisible = new InputLabel("Is Visible", _statIsVisible);
        _bodyPanel.Children.Add(inputStatIsVisible);
        ToolTip.SetTip(inputStatIsVisible, "Determines if the stat is visible in the UI.\n" +
                                           "If unchecked, the stat will not be displayed in the UI, but it can still be used in calculations.");
        
        // TEST FORMULA PRATT PARSE
        #if DEBUG
        
        var testFormula = new TextBox
        {
            Watermark = "(Optional)",
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Margin = App.style.Margin,
        };
        var inputTestFormula = new InputLabel("Test Formula", testFormula);
        _bodyPanel.Children.Add(inputTestFormula);
        var buttonTestFormula = new Button
        {
            Content = "Test Formula",
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Margin = App.style.Margin
        };
        buttonTestFormula.Click += (sender, args) =>
        {
            if (string.IsNullOrEmpty(testFormula.Text))
            {
                Log.Error("Test formula is empty, please enter a valid formula to test.");
                return;
            }
            try
            {
                PrattCompiler compiler = new PrattCompiler();
                var result = compiler.Compile(testFormula.Text);
                if (result != null)
                {
                    var value = result.Eval(
                        new PrattEvaluationEnvironment()
                        {
                            Variables = new ReadOnlyDictionary<string, double>(
                                new Dictionary<string, double>()
                                {
                                    ["testVar"] = 20.0
                                })
                        });
                    Log.Debug($"PrattCompiledFormula: {value}");
                }
            }
            catch (Exception e)
            {
                Log.Error("Got error while testing formula: {Message}", e.Message);
            }
        };
        _bodyPanel.Children.Add(buttonTestFormula);
#endif
    }

    private void RegisterEvents()
    {
    }

    #endregion

    #region Events Handlers
    #endregion
}