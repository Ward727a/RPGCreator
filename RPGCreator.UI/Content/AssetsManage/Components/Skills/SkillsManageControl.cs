using Avalonia.Controls;
using RPGCreator.UI.Content.AssetsManage.Components.Skills.Tabs;

namespace RPGCreator.UI.Content.AssetsManage.Components.Skills;

public class SkillsManageControl : UserControl
{    
    #region Events
    #endregion
    
    #region Properties
    #endregion
    
    #region Components
    private Grid Body { get; set; }
    #endregion
    
    #region Constructors
    public SkillsManageControl()
    {
        CreateComponents();
        Content = Body;
    }


    #endregion
    
    #region Methods

    private void CreateComponents()
    {
        Body = new Grid
        {
            Margin = App.style.Margin,
            RowDefinitions = new RowDefinitions("*")
        };
        
        var tabControl = new TabControl();
        
        var skillTab = new TabItem
        {
            Header = "Skills",
            Content = new SkillManageControlTab()
        };
        
        var skillEffectTab = new TabItem
        {
            Header = "Skill Effects",
            Content = new SkillEffectManageControl()
        };
        
        tabControl.Items.Add(skillTab);
        tabControl.Items.Add(skillEffectTab);
        
        Body.Children.Add(tabControl);
    }
    #endregion

    #region Events Handlers
    #endregion
}