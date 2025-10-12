using System.Collections.Generic;
using Avalonia.Controls;
using RPGCreator.Core.Type.Assets.Skills;
using RPGCreator.UI.Content.AssetsManage.AssetsEditors.SkillsEffectEditor.Tabs;

namespace RPGCreator.UI.Content.AssetsManage.AssetsEditors.SkillsEffectEditor;

public class SkillsEffectEditorWindowControl : UserControl
{
    #region Constants
    #endregion
    
    #region Events
    #endregion
    
    #region Properties
    
    private List<SkillEffectPropertyDescriptor> Properties { get; set; } = new List<SkillEffectPropertyDescriptor>();
    
    #endregion
    
    #region Components
    
    private TabControl Body { get; set; }
    private TabItem GeneralTab { get; set; }
    private TabItem EffectTab { get; set; }
    
    #endregion
    
    #region Constructors
    public SkillsEffectEditorWindowControl()
    {

        CreateComponents();
        RegisterEvents();
        Content = Body;

    }
    #endregion
    
    #region Methods

    private void CreateComponents()
    {

        Body = new TabControl()
            { };
        
        GeneralTab = new TabItem
        {
            Header = "General",
            Content = new SkillEffectGeneralEditorControl()
        };
        
        EffectTab = new TabItem
        {
            Header = "Effect (Graph)",
            Content = new SkillEffectEffectEditorControl(Properties)
        };
        
        Body.Items.Add(GeneralTab);
        Body.Items.Add(EffectTab);

    }
    private void RegisterEvents()
    {
        
        Body.SelectionChanged += (s, e) =>
        {
            if (Body.SelectedItem == EffectTab)
            {
                var effectTabContent = EffectTab.Content as SkillEffectEffectEditorControl;
                effectTabContent.SetSkillEffectProperties((GeneralTab.Content as SkillEffectGeneralEditorControl).GetProperties());
            }
        };
    }
    
    #endregion

    #region Events Handlers
    #endregion
}