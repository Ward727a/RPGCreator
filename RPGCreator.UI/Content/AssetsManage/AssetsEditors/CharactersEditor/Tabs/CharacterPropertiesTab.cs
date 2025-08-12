using System.Globalization;
using Avalonia.Controls;
using RPGCreator.Core.Type.Assets.Characters;
using Ursa.Controls;
using NumericUpDown = Avalonia.Controls.NumericUpDown;

namespace RPGCreator.UI.Content.AssetsManage.AssetsEditors.CharactersEditor.Tabs;

public class CharacterPropertiesTab : UserControl
{
    
    #region Events
    #endregion

    #region Properties

    public CharacterData Data;
    
    #endregion
    
    #region Components
    
    private StackPanel Body { get; set; }
    private NumericIntUpDown InitialLevel { get; set; }
    private NumericIntUpDown MaxLevel { get; set; }
    private TextBox Classes { get; set; }
    private TextBox EXPCurves { get; set; } // For now it just a text box, but it should be a real curve editor in the future.
    
    #endregion
    
    #region Constructors
    public CharacterPropertiesTab(CharacterData data)
    {
        Data = data;
        Name = "Properties"; // Define the name of the tab
        CreateComponents();
        Content = Body;
    }
    #endregion
    
    #region Methods

    private void CreateComponents()
    {
        Body = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            Margin = new Avalonia.Thickness(10)
        };
        
        InitialLevel = new NumericIntUpDown()
        {
            Watermark = "Initial Level",
            Minimum = 1,
            Maximum = 100,
            Value = 1,
            Margin = new Avalonia.Thickness(0, 0, 0, 10)
        };
        Body.Children.Add(InitialLevel);
        MaxLevel = new NumericIntUpDown()
        {
            Watermark = "Max Level",
            Minimum = 1,
            Maximum = 100,
            Value = 100,
            Margin = new Avalonia.Thickness(0, 0, 0, 10)
        };
        Body.Children.Add(MaxLevel);
        
        Classes = new TextBox
        {
            Watermark = "Classes (comma separated)",
            Margin = new Avalonia.Thickness(0, 0, 0, 10)
        };
        Body.Children.Add(Classes);
        
        EXPCurves = new TextBox
        {
            Watermark = "EXP Curves (comma separated)",
            Margin = new Avalonia.Thickness(0, 0, 0, 10)
        };
        Body.Children.Add(EXPCurves);
        
    }
    
    #endregion
    
    #region Events Handlers
    #endregion
    
}