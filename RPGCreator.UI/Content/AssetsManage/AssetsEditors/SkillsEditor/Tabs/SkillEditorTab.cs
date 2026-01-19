using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using RPGCreator.Core.Types;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Skills;
using RPGCreator.SDK.Assets.Definitions.Stats;
using RPGCreator.SDK.Logging;
using TextMateSharp.Grammars;
using Ursa.Controls;

namespace RPGCreator.UI.Content.AssetsManage.AssetsEditors.SkillsEditor.Tabs;



public class SkillEditorTab : UserControl
{

    private class SkillCostItem : UserControl
    {

        public event Action? OnSelected;
        public event Action? OnDeselected;
        public event Action? OnDeleted;
        
        public IStatDef CostStat { get; set; }
        public float CostAmount { get; set; }

        private Grid _body;
        
        private CheckBox _selectBox;
        private TextBlock _statName;
        private TextBlock _statAmount;
        private Button _deleteButton;
        
        public SkillCostItem(IStatDef stat, float amount)
        {
            CostStat = stat;
            CostAmount = amount;
            CreateComponents();
        }

        private void CreateComponents()
        {
            _body = new Grid()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = App.style.Margin,
                ColumnDefinitions = new ColumnDefinitions("Auto, *, *, Auto")
            };
            Content = _body;
            _selectBox = new CheckBox
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = App.style.Margin
            };
            _selectBox.Checked += (s, e) => OnSelected?.Invoke();
            _selectBox.Unchecked += (s, e) => OnDeselected?.Invoke();
            _body.Children.Add(_selectBox);
            _statName = new TextBlock
            {
                Text = CostStat.Name,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = App.style.Margin
            };
            _body.Children.Add(_statName);
            Grid.SetColumn(_statName, 1);
            _statAmount = new TextBlock
            {
                Text = CostAmount.ToString(),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = App.style.Margin
            };
            _body.Children.Add(_statAmount);
            Grid.SetColumn(_statAmount, 2);
            
            _deleteButton = new Button
            {
                Content = "Delete",
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = App.style.Margin
            };
            _deleteButton.Click += (s, e) => OnDeleted?.Invoke();
            _body.Children.Add(_deleteButton);
            Grid.SetColumn(_deleteButton, 3);
        }
    }
    
    private class SkillCostList : UserControl
    {
        
        private ScrollBox _body;
        private StackPanel _bodyPanel;
        
        public SkillCostList()
        {
            CreateComponents();
        }

        private void CreateComponents()
        {
            _body = new ScrollBox()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                MaxHeight = 180,
                Margin = App.style.Margin
            };
            _bodyPanel = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Margin = App.style.Margin
            };
            _body.Content = _bodyPanel;
            Content = _body;
        }
        
        public void AddCost(IStatDef stat, float amount)
        {
            var costItem = new SkillCostItem(stat, amount);
            costItem.OnDeleted += () => _bodyPanel.Children.Remove(costItem);
            _bodyPanel.Children.Add(costItem);
        }

        public void AddCost(SkillCostItem item)
        {
            item.OnDeleted += () => _bodyPanel.Children.Remove(item);
            _bodyPanel.Children.Add(item);
        }
        
        public void ClearCosts()
        {
            _bodyPanel.Children.Clear();
        }

        public List<SkillCostItem> GetCosts()
        {
            return _bodyPanel.Children.OfType<SkillCostItem>().ToList();
        }
        
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
    private ComboBox _skillPack;
    private TextBox _skillName;
    private TextBox _skillDescription;
    private PathPicker _skillIconPath;
    private SkillCostList _skillCosts;
    private TextEditor _skillFormulaEditor;

    #endregion
    
    #region Constructors
    public SkillEditorTab(ISkillDef skillDef)
    {
        ArgumentNullException.ThrowIfNull(skillDef, nameof(skillDef));
        SkillDef = skillDef;
        CreateComponents();
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

        _skillPack = new ComboBox()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = App.style.Margin,
        };
        var inputStatPack = new InputLabel("Assets Pack", _skillPack);
        _bodyPanel.Children.Add(inputStatPack);
        ToolTip.SetTip(inputStatPack, "The assets pack this skill belongs to.");
        //
        // foreach (var pack in EngineCore.Instance.Managers.Assets.GetAssetsPacks())
        // {
        //     _skillPack.Items.Add(pack.Name);
        // }
        //
        // if (SkillDef.PackId.HasValue && SkillDef.PackId.Value != Ulid.Empty)
        // {
        //     var hasPack =
        //         EngineCore.Instance.Managers.Assets.TryGetAssetsPack(SkillDef.PackId.Value, out var assetsPack);
        //     if (hasPack)
        //     {
        //         _skillPack.SelectedItem = assetsPack;
        //     }
        // }
        // else
        // {
        //     _skillPack.SelectedIndex = 0;
        //     SkillDef.PackId = EngineCore.Instance.Managers.Assets.GetAssetsPacks()[0].Id;
        // }

        _skillName = new TextBox()
        {
            Text = SkillDef.Name,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = App.style.Margin
        };
        var inputStatName = new InputLabel("Name", _skillName);
        _bodyPanel.Children.Add(inputStatName);
        ToolTip.SetTip(inputStatName, "The name of the skill.");

        _skillDescription = new TextBox()
        {
            Text = SkillDef.Description,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = App.style.Margin
        };
        var inputStatDescription = new InputLabel("Description", _skillDescription);
        _bodyPanel.Children.Add(inputStatDescription);
        ToolTip.SetTip(inputStatDescription, "The description of the skill.");

        _skillIconPath = new PathPicker()
        {
            Title = "Select Skill Icon",
            FileFilter = "[Image Files,*.png,*.jpg,*.jpeg,*.bmp,*.gif][All Files,*.*]",
            SelectedPathsText = SkillDef.IconPath,
            AllowMultiple = false,
            UsePickerType = UsePickerTypes.OpenFile,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = App.style.Margin
        };
        var inputStatIconPath = new InputLabel("Icon Path", _skillIconPath);
        _bodyPanel.Children.Add(inputStatIconPath);
        ToolTip.SetTip(inputStatIconPath, "The icon path of the skill.");

        var gridAddingCost = new Grid()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            ColumnDefinitions = new ColumnDefinitions("*, *, Auto")
        };
        _bodyPanel.Children.Add(gridAddingCost);

        var selectCostBox = new ComboBox()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
        };
        gridAddingCost.Children.Add(selectCostBox);
        Grid.SetColumn(selectCostBox, 0);
        // foreach (var statDef in EngineCore.Instance.Managers.Assets.StatsRegistry.All())
        // {
        //     if (statDef.StatTypeKind != EStatTypeKind.Resource) continue;
        //     selectCostBox.Items.Add(statDef.Name);
        // }

        var costAmountBox = new NumericFloatUpDown()
        {
            Value = 0,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = App.style.Margin
        };
        gridAddingCost.Children.Add(costAmountBox);
        Grid.SetColumn(costAmountBox, 1);

        var addCostButton = new Button()
        {
            Content = "Add Cost",
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = App.style.Margin
        };
        // addCostButton.Click += (s, e) =>
        // {
        //     var statDef = EngineCore.Instance.Managers.Assets.StatsRegistry.All()
        //         .FirstOrDefault(s => s.Name == (string?)selectCostBox.SelectedItem);
        //     if (statDef == null)
        //     {
        //         // No stat definitions available
        //         Log.Warning("No stat definitions available to add as skill cost.");
        //         return;
        //     }
        //
        //     _skillCosts.AddCost(statDef, costAmountBox.Value ?? 0);
        // };
        gridAddingCost.Children.Add(addCostButton);
        Grid.SetColumn(addCostButton, 2);

        if (selectCostBox.Items.Count > 0)
        {
            selectCostBox.SelectedIndex = 0;
        }
        else
        {
            selectCostBox.IsEnabled = false;
            costAmountBox.IsEnabled = false;
            addCostButton.IsEnabled = false;
            selectCostBox.PlaceholderText = "No resource stats defined";
        }

        _skillCosts = new SkillCostList();
        var inputSkillCosts = new InputLabel("Skill Costs", _skillCosts);
        _bodyPanel.Children.Add(inputSkillCosts);

        foreach (var cost in SkillDef.Cost)
        {
            _skillCosts.AddCost(cost.Key, cost.Value);
        }

        var cooldownInput = new NumericFloatUpDown()
        {
            Value = SkillDef.Cooldown,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = App.style.Margin,
            Minimum = 0
        };
        var inputCooldown = new InputLabel("Cooldown (seconds)", cooldownInput);
        _bodyPanel.Children.Add(inputCooldown);
        ToolTip.SetTip(inputCooldown, "The cooldown time of the skill in seconds.");

        var selectTargetType = new ComboBox()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = App.style.Margin
        };
        ESkillTargetType[] targetTypes = (ESkillTargetType[])Enum.GetValues(typeof(ESkillTargetType));
        foreach (var targetType in targetTypes)
        {
            selectTargetType.Items.Add(targetType.ToString());
        }

        selectTargetType.SelectedItem = SkillDef.TargetType.ToString();
        var inputTargetType = new InputLabel("Target Type", selectTargetType);
        _bodyPanel.Children.Add(inputTargetType);
        ToolTip.SetTip(inputTargetType, "The target type of the skill.");

        var inputRange = new NumericFloatUpDown()
        {
            Value = SkillDef.Range,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            Minimum = 0,
            Margin = App.style.Margin
        };
        var rangeLabel = new InputLabel("Range", inputRange);
        _bodyPanel.Children.Add(rangeLabel);
        ToolTip.SetTip(rangeLabel, "The range of the skill.");
        _skillFormulaEditor = new TextEditor
        {
            Watermark = "Enter skill formula here...",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Text = SkillDef.SkillNonCompiledFormula,
            Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x2A, 0x2A, 0x2A)),
            Padding = new Thickness(8, 8, 8, 8),
            MinHeight = 100,
            CornerRadius = new CornerRadius(3),
        };
        var fakeRadiusBorder = new Border()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            CornerRadius = new CornerRadius(3),
            Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x2A, 0x2A, 0x2A)),
            ClipToBounds = true,
            Child = _skillFormulaEditor,
            Margin = App.style.Margin
        };
        _bodyPanel.Children.Add(new InputLabel("Skill Formula", fakeRadiusBorder));

        var registryOptions = new RegistryOptions(ThemeName.DarkPlus);
        var textMateInstallation = _skillFormulaEditor.InstallTextMate(registryOptions);
        var filepath = $"{AppDomain.CurrentDomain.BaseDirectory}Assets/TMGrammar/RPGFormula.tmLanguage.json";
        textMateInstallation.SetGrammarFile(filepath);

        // Save Button
        var saveButton = new Button()
        {
            Content = "Save Skill",
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = App.style.Margin
        };
        saveButton.Click += (s, e) =>
        {
            SkillDef.SetName(_skillName.Text ?? string.Empty);
            SkillDef.Description = _skillDescription.Text ?? string.Empty;
            SkillDef.IconPath = _skillIconPath.SelectedPathsText ?? string.Empty;
            SkillDef.Cost.Clear();
            foreach (var child in _skillCosts.GetCosts())
            {
                if (child is SkillCostItem costItem)
                {
                    SkillDef.Cost[costItem.CostStat] = costItem.CostAmount;
                }
            }

            SkillDef.Cooldown = cooldownInput.Value ?? 0;
            if (selectTargetType.SelectedItem != null &&
                Enum.TryParse<ESkillTargetType>((string)selectTargetType.SelectedItem, out var targetType))
            {
                SkillDef.TargetType = targetType;
            }

            SkillDef.Range = inputRange.Value ?? 0;
            SkillDef.SkillNonCompiledFormula = _skillFormulaEditor.Text ?? string.Empty;
            if (_skillPack.SelectedItem != null)
            {
                var selectedPackName = (string)_skillPack.SelectedItem;
                // var selectedPack = EngineCore.Instance.Managers.Assets.GetAssetsPacks()
                //     .FirstOrDefault(p => p.Name == selectedPackName);
                // if (selectedPack != null)
                // {
                //     SkillDef.PackId = selectedPack.Id;
                // }
            }

            //AssetsManager.AssetMapping[typeof(ISkillDef)](SkillDef);

            Logger.Info("Skill '{SkillName}' saved.", SkillDef.Name);
            
            
            EngineServices.SerializerService.Serialize(SkillDef, out string data);
            // Add the stat definition to the selected asset pack in the statdef
            if(SkillDef.PackId.HasValue && SkillDef.PackId != Ulid.Empty)
            {
                //var hasPack = EngineCore.Instance.Managers.Assets.TryGetAssetsPack(SkillDef.PackId.Value, out var assetsPack);
                //if (hasPack != null)
                //{
                //    assetsPack.AddAsset(SkillDef);
                //    assetsPack.Save();
                //    File.WriteAllText(SkillDef.SavePath, data);
                //    Log.Information("Stat Definition added to the selected Assets Pack.");
                //}
                //else
                //{
                //    Log.Warning("Assets Pack with ID {PackId} not found. Stat Definition not added to any pack.", SkillDef.PackId);
                //}
            }
            else
            {
                Logger.Warning("No Assets Pack selected for this Stat Definition. It won't be part of any pack.");
            }
        };
        _bodyPanel.Children.Add(saveButton);
    }

    #endregion

    #region Events Handlers
    #endregion
}