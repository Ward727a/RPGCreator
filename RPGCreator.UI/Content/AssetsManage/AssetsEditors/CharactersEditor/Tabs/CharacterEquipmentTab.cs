using Avalonia.Controls;
using RPGCreator.SDK.Assets.Definitions.Characters;

namespace RPGCreator.UI.Content.AssetsManage.AssetsEditors.CharactersEditor;

public class CharacterEquipmentTab : UserControl
{
    
    #region Events
    #endregion

    #region Properties

    public CharacterData Data;
    
    #endregion
    
    #region Components
    
    private StackPanel Body { get; set; }
    
    #endregion
    
    #region Constructors
    public CharacterEquipmentTab(CharacterData data)
    {
        Data = data;
        Name = "Equipment"; // Define the name of the tab
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
        
    }
    
    #endregion
    
    #region Events Handlers
    #endregion
}