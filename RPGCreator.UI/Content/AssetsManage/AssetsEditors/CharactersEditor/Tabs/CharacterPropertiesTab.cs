using Avalonia.Controls;
using Avalonia.Layout;
using RPGCreator.Core.Types;
using RPGCreator.SDK.Assets.Definitions.Characters;
using Ursa.Controls;

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
    private TextBox CharacterName { get; set; }
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
        RegisterEvents();
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
            Margin = new Avalonia.Thickness(10),
            Spacing = 10
        };
        
        CharacterName = new TextBox()
        {
            Watermark = "Character Name...",
            Text = Data.Name ?? string.Empty,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        Body.Children.Add(new InputLabel("Character Name", CharacterName, "120"));
        
        InitialLevel = new NumericIntUpDown()
        {
            Watermark = "Initial Level",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Minimum = 1,
            Maximum = 100,
            Value = 1,
        };
        Body.Children.Add(
            new InputLabel("Initial Level", InitialLevel, "120")
            );
        MaxLevel = new NumericIntUpDown()
        {
            Watermark = "Max Level",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Minimum = 1,
            Maximum = 100,
            Value = 100,
        };
        Body.Children.Add(
            new InputLabel("Max Level", MaxLevel, "120"));
        
        Classes = new TextBox
        {
            Watermark = "Classes (comma separated)",
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        Body.Children.Add(
            new InputLabel("Classes", Classes, "120"));
        
        EXPCurves = new TextBox
        {
            Watermark = "EXP Curves (comma separated)",
            HorizontalAlignment = HorizontalAlignment.Stretch,
        };
        Body.Children.Add(
            new InputLabel("EXP Curves", EXPCurves, "120"));
        
    }
    
    private void RegisterEvents()
    {
        CharacterName.TextChanged += OnCharacterNameChanged;
    }

    #endregion
    
    #region Events Handlers
    private void OnCharacterNameChanged(object? sender, TextChangedEventArgs e)
    {
        
        Data.Name = CharacterName.Text ?? string.Empty;
        
    }
    #endregion
    
}