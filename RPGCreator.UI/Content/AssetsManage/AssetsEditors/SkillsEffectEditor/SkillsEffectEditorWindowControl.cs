using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Skills;
using RPGCreator.SDK.Logging;
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
    
    private Grid Body { get; set; }
    private TabControl TabBody { get; set; }
    private TabItem GeneralTab { get; set; }
    private TabItem EffectTab { get; set; }
    
    private StackPanel ButtonBar { get; set; }
    private Button SaveButton { get; set; }
    
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

        Body = new Grid()
        {
            RowDefinitions = new RowDefinitions("*, Auto"),
        };
        
        
        TabBody = new TabControl()
            { };
        Body.Children.Add(TabBody);
        Grid.SetRow(TabBody, 0);
        
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
        
        TabBody.Items.Add(GeneralTab);
        TabBody.Items.Add(EffectTab);
        
        ButtonBar = new StackPanel()
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            Margin = new Thickness(5)
        };
        Body.Children.Add(ButtonBar);
        Grid.SetRow(ButtonBar, 1);
        
        SaveButton = new Button()
        {
            Content = "Save",
            Width = 80,
            Margin = new Thickness(5)
        };
        ButtonBar.Children.Add(SaveButton);

    }
    private void RegisterEvents()
    {
        
        TabBody.SelectionChanged += (s, e) =>
        {
            if (TabBody.SelectedItem == EffectTab)
            {
                var effectTabContent = EffectTab.Content as SkillEffectEffectEditorControl;
                effectTabContent.SetSkillEffectProperties((GeneralTab.Content as SkillEffectGeneralEditorControl).GetProperties());
            }
        };
        
        SaveButton.Click += (s, e) =>
        {
            var generalTabContent = GeneralTab.Content as SkillEffectGeneralEditorControl;
            var effectTabContent = EffectTab.Content as SkillEffectEffectEditorControl;

            if(generalTabContent == null || effectTabContent == null)
            {
                Logger.Error("SkillEffectEditor: Unable to save, general or effect tab content is null.");
                return;
            }
            
            var newEffect = new GraphSkillEffect(generalTabContent.EffectName);
            newEffect.PackId = generalTabContent.SelectedEffectPackId;
            newEffect.SetPropertiesDescriptors(generalTabContent.GetProperties());
            newEffect.SetEvent(effectTabContent.CompiledDocument);

            if (!newEffect.PackId.HasValue)
                return;
            Logger.Debug("Saving Skill Effect: {0} with {numberProperties} props and {numberInstructions} instrs in pack {packId}.", newEffect.DisplayName, newEffect.PropertyDescriptors.Count, newEffect.GetEvent().GetInstructions().Count, newEffect.PackId.ToString());
            

            EngineServices.AssetsManager.RegisterAsset(newEffect);
            
            
        };
    }
    
    #endregion

    #region Events Handlers
    #endregion
}