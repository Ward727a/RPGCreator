using System;
using Avalonia.Controls;
using RPGCreator.Core.Types;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Characters;
using RPGCreator.SDK.Assets.Definitions.Skills;
using RPGCreator.SDK.Extensions;
using RPGCreator.SDK.Logging;
using Brushes = Avalonia.Media.Brushes;
using NumericUpDown = Avalonia.Controls.NumericUpDown;

namespace RPGCreator.UI.Content.AssetsManage.AssetsEditors.CharactersEditor.Tabs;

public class CharacterSkillsTab : UserControl
{

    public class CharaSkillItem : UserControl
    {
        private ISkillDef _skillDef;
        private CharacterSkill _skillData;
            
        #region Components

        private Grid Body;
        private TextBlock SkillName;
        private StackPanel Panel;
        private StackPanel MinPanel;
        private TextBlock MinLabel;
        private NumericUpDown MinBox;
        private StackPanel MaxPanel;
        private TextBlock MaxLabel;
        private NumericUpDown MaxBox;
        private CheckBox MaxBool;
        private Separator Separator;
            
        #endregion
        
        public CharaSkillItem(ISkillDef skill, CharacterSkill skillData)
        {
            _skillDef = skill;
            _skillData = skillData;

            CreateComponents();
            RegisterEvents();
            
            Content = Body;
        }

        private void CreateComponents()
        {
            Body = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("*, Auto"),
                RowDefinitions = new RowDefinitions("*, Auto"),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Margin = new Avalonia.Thickness(5)
            };
            
            SkillName = new TextBlock
            {
                Text = _skillDef.Name,
                Margin = new Avalonia.Thickness(5),
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left
            };
            Body.Children.Add(SkillName);
            Grid.SetColumn(SkillName, 0);
            
            Panel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Vertical,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            };
            
            MinPanel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            };
            
            MinLabel = new TextBlock
            {
                Text = "Min Character Level:",
                Margin = new Avalonia.Thickness(5),
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left
            };
            MinPanel.Children.Add(MinLabel);
            MinBox = new NumericUpDown()
            {
                Minimum = 0,
                Maximum = 100,
                Value = _skillData.SkillLevel,
                Width = 60,
                Margin = new Avalonia.Thickness(5),
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left
            };
            
            MinPanel.Children.Add(MinBox);
            Panel.Children.Add(MinPanel);
            
            MaxPanel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            };
            MaxBool = new CheckBox()
            {
                IsChecked = _skillData.HasMaxLevel,
                Content = "Has Max",
                Margin = new Avalonia.Thickness(5),
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left
            };
            MaxPanel.Children.Add(MaxBool);
            MaxLabel = new TextBlock
            {
                IsEnabled = false,
                Text = "Max Character Level:",
                Margin = new Avalonia.Thickness(5),
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left
            };
            MaxPanel.Children.Add(MaxLabel);
            MaxBox = new NumericUpDown()
            {
                IsEnabled = false,
                Minimum = 0,
                Maximum = 100,
                Value = _skillData.MaxSkillLevel,
                Width = 60,
                Margin = new Avalonia.Thickness(5),
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left
            };
            MaxPanel.Children.Add(MaxBox);
            Panel.Children.Add(MaxPanel);
            Body.Children.Add(Panel);
            Grid.SetColumn(Panel, 1);
            
            Separator = new Separator()
            {
                Margin = new Avalonia.Thickness(0, 10, 0, 0),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch
            };
            Body.Children.Add(Separator);
            Grid.SetRow(Separator, 1);
            Grid.SetColumnSpan(Separator, 2);
        }

        private void RegisterEvents()
        {
            
            MinBox.ValueChanged += (_, e) =>
            {
                if (e.NewValue.HasValue)
                {
                    _skillData.SkillLevel = (int)e.NewValue.Value;
                }
            };
            MaxBox.ValueChanged += (_, e) =>
            {
                if (e.NewValue.HasValue)
                {
                    _skillData.MaxSkillLevel = (int)e.NewValue.Value;
                }
            };
            MaxBool.Click += (_, _) =>
            {
                _skillData.HasMaxLevel = MaxBool.IsChecked ?? false;
                MaxBox.IsEnabled = _skillData.HasMaxLevel;
                MaxLabel.IsEnabled = _skillData.HasMaxLevel;
            };
        }
    }
    
    #region Events
    #endregion

    #region Properties

    public CharacterData Data;
    
    #endregion
    
    #region Components
    
    private Grid Body { get; set; }
    
    private TextBlock InfoTextBlock { get; set; }
    
    private Grid Header { get; set; }
    private ComboBox SkillComboBox { get; set; }
    private Button AddSkillButton { get; set; }
    private ScrollBox SkillsListScroll { get; set; }
    private StackPanel SkillsListPanel { get; set; }
    
    #endregion
    
    #region Constructors
    public CharacterSkillsTab(CharacterData data)
    {
        Data = data;
        Name = "Skills";
        CreateComponents();
        RegisterEvents();
        RefreshSkills();
        RefreshSkillData();
        Content = Body;
    }
    #endregion
    
    #region Methods

    private void CreateComponents()
    {
        Body = new Grid
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            RowDefinitions = new RowDefinitions("Auto, Auto, *")
        };
        
        InfoTextBlock = new TextBlock
        {
            Text = "Skills here are linked to the character, and not the class.",
            Margin = new Avalonia.Thickness(10),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Foreground = Brushes.DarkGray,
            FontSize = 12,
        };
        Body.Children.Add(InfoTextBlock);
        Grid.SetRow(InfoTextBlock, 0);
        
        Header = new Grid()
        {
            ColumnDefinitions = new ColumnDefinitions("*, Auto"),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            Margin = new Avalonia.Thickness(10)
        };
        Body.Children.Add(Header);
        Grid.SetRow(Header, 1);
        
        SkillComboBox = new ComboBox
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Margin = new Avalonia.Thickness(0, 0, 10, 0)
        };
        Header.Children.Add(SkillComboBox);
        Grid.SetColumn(SkillComboBox, 0);

        
        if (SkillComboBox.Items.Count > 0)
            SkillComboBox.SelectedIndex = 0;
        
        AddSkillButton = new Button
        {
            Content = "Add Skill",
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };
        Header.Children.Add(AddSkillButton);
        Grid.SetColumn(AddSkillButton, 1);
        
        
        SkillsListScroll = new ScrollBox
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            Margin = new Avalonia.Thickness(10)
        };
        Body.Children.Add(SkillsListScroll);
        
        SkillsListPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
        };
        SkillsListScroll.Content = SkillsListPanel;
        
        Grid.SetRow(SkillsListScroll, 2);
        
    }

    private void RegisterEvents()
    {
        Logger.Debug("Registering Character Skills Tab Events...");
        AddSkillButton.Click += (_, _) =>
        {
            if (SkillComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                var itemTag = (Ulid?)selectedItem.Tag;

                if (!itemTag.HasValue)
                {
                    Logger.Error("No skill selected to add.");
                    return;
                }

                var skillUnique = itemTag.Value;
                
                var skillDef = EngineServices.AssetsManager.TryResolveAsset(skillUnique, out ISkillDef? result) ? result : null;
                if (skillDef != null && !Data.Skills.ContainsKey(skillUnique))
                {
                    var skillData = new CharacterSkill(skillDef);
                    Data.Skills.Add(skillUnique, skillData);
                    SkillsListPanel.Children.Add(new CharaSkillItem(skillDef, skillData));
                }
            }
        };
    }
    
    private void RefreshSkills()
    {
        Logger.Debug("Refreshing Skills List...");
        SkillComboBox.Items.Clear();

        var skills = EngineServices.AssetsManager.GetAssets<ISkillDef>();
        
        foreach (var skillDef in skills)
        {
            SkillComboBox.Items.Add(
                new ComboBoxItem()
                {
                    Content = skillDef.Name,
                    Tag = skillDef.Unique
                });
        }
        if(SkillComboBox.Items.Count > 0)
            SkillComboBox.SelectedIndex = 0;
    }

    private void RefreshSkillData()
    {
        Logger.Debug("Refreshing Character Skills Data...");
        SkillsListPanel.Children.Clear();
        foreach (var skillEntry in Data.Skills)
        {
            var skillDef = EngineServices.AssetsManager.TryResolveAsset(skillEntry.Key, out ISkillDef? result) ? result : null;
            if (skillDef != null)
            {
                SkillsListPanel.Children.Add(new CharaSkillItem(skillDef, skillEntry.Value));
            }
        }
    }
    
    #endregion
    
    #region Events Handlers
    #endregion
    
    
}