using Avalonia.Controls;
using RPGCreator.SDK.Assets.Definitions.Skills;

namespace RPGCreator.UI.Content.AssetsManage.AssetsEditors.SkillsEditor.Tabs;
/// <summary>
/// This tab is used to edit the display properties of a skill, such as its icon, animation, and visual effects.
/// </summary>
public class SkillDisplayTab : UserControl
{
    #region Constants
    #endregion
    
    #region Events
    #endregion
    
    #region Properties

    private ISkillDef _skillDef;

    #endregion

    #region Components

    #endregion

    #region Constructors

    public SkillDisplayTab(ISkillDef skillDef)
    {
        _skillDef = skillDef;

        Content = new TextBlock
        {
            Text = "Skill Display Tab - To be implemented",
            Margin = App.style.Margin,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
        };
    }
    
    #endregion

    #region Methods

    #endregion

    #region Events Handlers

    #endregion
}