using System;
using Avalonia.Controls;
using Avalonia.Input;
using RPGCreator.SDK.Assets.Definitions.Skills;

namespace RPGCreator.UI.Content.AssetsManage.Components.Skills;

public class SkillManageItemControl : UserControl
{
    
    #region Constants
    #endregion
    
    #region Events
    public event EventHandler? ItemSelected;
    #endregion
    
    #region Properties
    public ISkillDef SkillDef { get; private set; }
    #endregion
    
    #region Components
    private Grid Body { get; set; }
    private TextBlock SkillNameTextBlock { get; set; }
    #endregion
    
    #region Constructors
    public SkillManageItemControl(ISkillDef skillDef)
    {
        ArgumentNullException.ThrowIfNull(skillDef, nameof(skillDef));
        
        SkillDef = skillDef;
        
        CreateComponents();
        RegisterEvents();
        
        Content = Body;
    }

    #endregion
    
    #region Methods

    private void CreateComponents()
    {
        // Initialize UI components here
        Body = new Grid
        {
            Margin = App.style.Margin,
            // Set the background color to a custom dark transparent color
            Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromArgb(0x55, 0x2A, 0x2A, 0x2A)),
            RowDefinitions = new RowDefinitions("Auto"),
        };
        
        SkillNameTextBlock = new TextBlock
        {
            Text = SkillDef.Name,
            Margin = App.style.Margin,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left
        };
        Body.Children.Add(SkillNameTextBlock);
        Grid.SetRow(SkillNameTextBlock, 0);
    }
    private void RegisterEvents()
    {
        Body.PointerPressed += OnItemSelected;
    }
    #endregion

    #region Events Handlers
    
    private void OnItemSelected(object? sender, PointerPressedEventArgs e)
    {
        ItemSelected?.Invoke(this, EventArgs.Empty);
    }
    #endregion
}