using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using RPGCreator.Core.Type;
using RPGCreator.Core.Type.Assets.Skills;

namespace RPGCreator.UI.Content.AssetsManage.AssetsEditors.SkillsEditor.Tabs;

/// <summary>
/// This tab is used to edit the effect properties of a skill, such as damage, healing, buffs, debuffs, and status effects.
/// </summary>
public class SkillEffectTab : UserControl
{
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
    
    #endregion

    #region Events Handlers
    #endregion
}