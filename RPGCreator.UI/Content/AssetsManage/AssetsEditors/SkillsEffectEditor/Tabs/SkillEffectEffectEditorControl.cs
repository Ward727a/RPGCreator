using System.Collections.Generic;
using Avalonia.Controls;
using RPGCreator.SDK.Assets.Definitions.Skills;
using RPGCreator.SDK.Logging;

namespace RPGCreator.UI.Content.AssetsManage.AssetsEditors.SkillsEffectEditor.Tabs;

public class SkillEffectEffectEditorControl : UserControl
{
    
    
    #region Constants
    #endregion
    
    #region Events
    #endregion
    
    #region Properties
    
    private List<SkillEffectPropertyDescriptor> _effectProperties;
    
    #endregion
    
    #region Components
    
    private Grid Body { get; set; }
    
    #endregion
    
    #region Constructors

    public SkillEffectEffectEditorControl(List<SkillEffectPropertyDescriptor> properties)
    {
        _effectProperties = properties;
        // Initialize UI components here
        Content = new TextBlock
        {
            Text = "Effect Editor (Graph) - To be implemented",
            Margin = App.style.Margin,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
        };

        CreateComponents();
        RegisterEvents();
        RefreshGraphVariables();

        Content = Body;
    }

    #endregion
    
    #region Methods
    
    public void SetSkillEffectProperties(List<SkillEffectPropertyDescriptor> properties)
    {
        _effectProperties = properties;
        RefreshGraphVariables();
    }
    
    private void CreateComponents()
    {
        Body = new Grid
        {
            Margin = App.style.Margin,
            RowDefinitions = new RowDefinitions("*, Auto")
        };

        
        // We move the start and end nodes so it's not hidden by the variables panel
    }
    
    private void RegisterEvents()
    {
        // Register event handlers here
    }

    private void RefreshGraphVariables()
    {
        Logger.Debug("Refreshing graph variables...");
        foreach (var propertyDescriptor in _effectProperties)
        {

            var propertyPath = $"skill_effect.props.{propertyDescriptor.Name}";
            
            Logger.Debug("Added graph variable: {PropertyPath}", propertyPath);
        }
        
    }
    
    #endregion

    #region Events Handlers
    #endregion
    
}