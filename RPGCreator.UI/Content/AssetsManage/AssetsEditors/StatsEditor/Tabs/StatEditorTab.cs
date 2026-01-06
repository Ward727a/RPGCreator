using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using RPGCreator.Core.Types;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Stats;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Parser.PrattFormula;
using TextMateSharp.Grammars;

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
    private ComboBox _statPack;
    private TextBox _statName;
    private TextBox _statDescription;
    private ComboBox _statTypeKind;
    private NumericUpDown _statDefaultValue;
    private NumericUpDown _statMinValue;
    private ComboBox _statTypeCap;
    private NumericUpDown _statMaxValue;
    private ToggleSwitch _statIsVisible;
    private TextEditor _statFormulaEditor;
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
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Margin = App.style.Margin
        };
        _bodyPanel = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Margin = App.style.Margin
        };
        _body.Content = _bodyPanel;
        
        _statPack = new ComboBox()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = App.style.Margin,
        };
        var inputStatPack = new InputLabel("Assets Pack", _statPack);
        _bodyPanel.Children.Add(inputStatPack);
        ToolTip.SetTip(inputStatPack, "The assets pack this stat belongs to.");
        
        // foreach (var pack in EngineCore.Instance.Managers.Assets.GetAssetsPacks())
        // {
        //     _statPack.Items.Add(pack.Name);
        // }
        //
        // if (StatDef.PackId.HasValue && StatDef.PackId.Value != Ulid.Empty)
        // {
        //     var hasPack = EngineCore.Instance.Managers.Assets.TryGetAssetsPack(StatDef.PackId.Value, out var assetsPack);
        //     if (hasPack)
        //     {
        //         _statPack.SelectedItem = assetsPack;
        //     }
        // }
        // else
        // {
        //     _statPack.SelectedIndex = 0;
        //     StatDef.PackId = EngineCore.Instance.Managers.Assets.GetAssetsPacks()[0].Id;
        // }
        
        _statName = new TextBox
        {
            Watermark = "(Required)",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = App.style.Margin,
            Text = StatDef.Name
        };
        _bodyPanel.Children.Add(new InputLabel("Stat Name", _statName));
        
        _statDescription = new TextBox
        {
            Watermark = "(Optional)",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = App.style.Margin,
            Text = StatDef.Description,
            AcceptsReturn = true,
            AcceptsTab = true,
            TextWrapping = TextWrapping.Wrap
        };
        _bodyPanel.Children.Add(new InputLabel("Stat Description", _statDescription));
        
        _statTypeKind = new ComboBox
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
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

        _statTypeKind.SelectedIndex = StatDef.StatTypeKind switch
        {
            EStatTypeKind.Resource => 0,
            EStatTypeKind.Attribute => 1,
            EStatTypeKind.Derived => 2,
            _ => 0
        };
        
        _statDefaultValue = new NumericUpDown
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = App.style.Margin,
            Value = (decimal)StatDef.DefaultValue
        };
        _bodyPanel.Children.Add(new InputLabel("Default Value", _statDefaultValue));
        
        _statMinValue = new NumericUpDown
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = App.style.Margin,
            Value = (decimal)StatDef.StatMinValue
        };
        _bodyPanel.Children.Add(new InputLabel("Minimum Value", _statMinValue));
        
        _statTypeCap = new ComboBox
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
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

        _statTypeCap.SelectedIndex = StatDef.StatCapType switch
        {
            EStatTypeCap.ByValue => 0,
            EStatTypeCap.ByStat => 1,
            _ => 0
        };
        
        _statMaxValue = new NumericUpDown
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = App.style.Margin,
            Value = (decimal)StatDef.StatCapValue
        };
        _bodyPanel.Children.Add(new InputLabel("Maximum Value", _statMaxValue));
        
        _statIsVisible = new ToggleSwitch
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = App.style.Margin,
            IsChecked = StatDef.IsVisible
        };
        var inputStatIsVisible = new InputLabel("Is Visible", _statIsVisible);
        _bodyPanel.Children.Add(inputStatIsVisible);
        ToolTip.SetTip(inputStatIsVisible, "Determines if the stat is visible in the UI.\n" +
                                           "If unchecked, the stat will not be displayed in the UI, but it can still be used in calculations.");

        _statFormulaEditor = new TextEditor
        {
            Watermark = "(Required only if Stat Type Kind is Derived)",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Text = StatDef.StatNonCompiledFormula,
            Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x2A, 0x2A, 0x2A)),
            Padding = new Thickness(8, 8, 8, 8),
            MinHeight = 100,
            CornerRadius = new CornerRadius(3)
        };
        var fakeRadiusBorder = new Border()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            CornerRadius = new CornerRadius(3),
            Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x2A, 0x2A, 0x2A)),
            ClipToBounds = true,
            Child = _statFormulaEditor,
        };
        _bodyPanel.Children.Add(new InputLabel("Stat Formula", fakeRadiusBorder));
        
        var registryOptions = new RegistryOptions(ThemeName.DarkPlus);
        var textMateInstallation = _statFormulaEditor.InstallTextMate(registryOptions);
        var filepath = $"{AppDomain.CurrentDomain.BaseDirectory}Assets/TMGrammar/RPGFormula.tmLanguage.json";
        textMateInstallation.SetGrammarFile(filepath);
        
    }

    private void RegisterEvents()
    {
        _statName.TextChanged += (sender, args) =>
        {
            if (string.IsNullOrEmpty(_statName.Text))
            {
                _statName.BorderBrush = Brushes.Red;
                _statName.BorderThickness = new Thickness(1);
            } else
            {
                _statName.ClearValue(Border.BorderBrushProperty);
                _statName.ClearValue(Border.BorderThicknessProperty);
            }
            StatDef.Name = _statName.Text ?? "";
        };
        
        _statDescription.TextChanged += (sender, args) =>
        {
            StatDef.Description = _statDescription.Text ?? "";
        };
        
        _statTypeKind.SelectionChanged += (sender, args) =>
        {
            if (_statTypeKind.SelectedItem == null)
                return;
            if (Enum.TryParse<EStatTypeKind>(_statTypeKind.SelectedItem.ToString(), out var kind))
            {
                StatDef.StatTypeKind = kind;
            }
        };
        
        _statDefaultValue.ValueChanged += (sender, args) =>
        {
            StatDef.DefaultValue = (float)(_statDefaultValue.Value ?? 0);
        };
        
        _statMinValue.ValueChanged += (sender, args) =>
        {
            StatDef.StatMinValue = (float)(_statMinValue.Value ?? 0);
        };
        
        _statTypeCap.SelectionChanged += (sender, args) =>
        {
            if (_statTypeCap.SelectedItem == null)
                return;
            if (!Enum.TryParse<EStatTypeCap>(_statTypeCap.SelectedItem.ToString(), out var capType)) return;
            
            StatDef.StatCapType = capType;

            _statMaxValue.IsEnabled = capType != EStatTypeCap.ByStat;
        };
        
        _statMaxValue.ValueChanged += (sender, args) =>
        {
            StatDef.StatCapValue = (float)(_statMaxValue.Value ?? 0);
        };
        
        _statIsVisible.IsCheckedChanged += (sender, args) =>
        {
            StatDef.IsVisible = _statIsVisible.IsChecked ?? false;
        };

        _statFormulaEditor.TextChanged += (sender, args) =>
        {
            StatDef.StatNonCompiledFormula = _statFormulaEditor.Text ?? "";
            try
            {
                if (EngineServices.PrattFormulaService.TryCompile(StatDef.StatNonCompiledFormula, out var formula))
                {
                    StatDef.StatCompiledFormula = formula;
                    _statFormulaEditor.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x2A, 0x2A, 0x2A));
                }
                else
                {
                    throw new Exception("Failed to compile formula.");
                }
            }
            catch (Exception e)
            {
                Logger.Error("Error parsing formula: {Formula} due to {ex}", StatDef.StatNonCompiledFormula, e.Message);
                // Set background to red
                _statFormulaEditor.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x5A, 0x1A, 0x1A));
            }
        };
    }

    #endregion

    #region Events Handlers
    #endregion
}