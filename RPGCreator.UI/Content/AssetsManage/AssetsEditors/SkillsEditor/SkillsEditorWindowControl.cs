using Avalonia.Controls;
using RPGCreator.SDK.Assets.Definitions.Skills;
using RPGCreator.SDK.Logging;
using RPGCreator.UI.Content.AssetsManage.AssetsEditors.SkillsEditor.Tabs;

namespace RPGCreator.UI.Content.AssetsManage.AssetsEditors.SkillsEditor;

public class SkillsEditorWindowControl : UserControl
{
    #region Constants
    #endregion
    
    #region Events
    #endregion
    
    #region Properties
    public ISkillDef SkillDef { get; private set; }
    #endregion
    
    #region Components

    private Grid _bodyGrid;
    private TabControl _body;
    #endregion
    
    #region Constructors
    public SkillsEditorWindowControl(ISkillDef? skillDef)
    {
        skillDef ??= new SkillDef();
        SkillDef = skillDef;
        CreateComponents();
        Content = _bodyGrid;
    }
    #endregion
    
    #region Methods

    private void CreateComponents()
    {
        _bodyGrid = new Grid
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            RowDefinitions = new RowDefinitions("*, Auto")
        };
        
        _body = new TabControl
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            Margin = App.style.Margin
        };
        _bodyGrid.Children.Add(_body);

        _body.Items.Add(
            new TabItem()
            {
                Header = "Skill",
                Content = new SkillEditorTab(SkillDef)
            });
        _body.Items.Add(
            new TabItem()
            {
                Header = "Events",
                Content = new SkillEventTab(SkillDef)
            });
        _body.Items.Add(
            new TabItem()
            {
                Header = "Effects",
                Content = new SkillEffectTab(SkillDef)
            });
        _body.Items.Add(
            new TabItem()
            {
                Header = "Display",
                Content = new SkillDisplayTab(SkillDef)
            });
        
        var buttonsPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Margin = App.style.Margin
        };
        _bodyGrid.Children.Add(buttonsPanel);
        Grid.SetRow(buttonsPanel, 1);
        
        var saveButton = new Button
        {
            Content = "Save",
            Margin = new Avalonia.Thickness(5, 0, 0, 0),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
        };
        saveButton.Click += (s, e) =>
        {
            Logger.Info("Saving Skill Definition...");
        };
        buttonsPanel.Children.Add(saveButton);
    }

    #endregion

    #region Events Handlers
    #endregion
}