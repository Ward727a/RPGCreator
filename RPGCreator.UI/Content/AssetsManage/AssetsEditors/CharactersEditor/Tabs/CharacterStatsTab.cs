using Avalonia.Controls;
using Avalonia.Layout;
using RPGCreator.Core.Types;
using RPGCreator.SDK.Assets.Definitions.Characters;
using RPGCreator.SDK.Assets.Definitions.Stats;
using Ursa.Controls;

namespace RPGCreator.UI.Content.AssetsManage.AssetsEditors.CharactersEditor.Tabs;

public class CharacterStatsTab : UserControl
{
    
    
    #region Events
    #endregion

    #region Properties

    private CharacterData Data;
    
    #endregion
    
    #region Components
    
    private StackPanel Body { get; set; }
    private Accordion HealthAccordion { get; set; }
    private StackPanel HealthPanel { get; set; }
    private NumericIntUpDown InitialHealth { get; set; }
    private NumericIntUpDown MaxHealth { get; set; }
    private Accordion ManaAccordion { get; set; }
    private StackPanel ManaPanel { get; set; }
    private NumericIntUpDown InitialMana { get; set; }
    private NumericIntUpDown MaxMana { get; set; }
    
    private Accordion StaminaAccordion { get; set; }
    private StackPanel StaminaPanel { get; set; }
    private NumericIntUpDown InitialStamina { get; set; }
    private NumericIntUpDown MaxStamina { get; set; }
    
    #endregion
    
    #region Constructors
    public CharacterStatsTab(CharacterData data)
    {
        Data = data;
        Name = "Stats"; // Define the name of the tab
        CreateComponents();
        Content = Body;
    }
    #endregion
    
    #region Methods

    private void CreateComponents()
    {
        Body = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Margin = new Avalonia.Thickness(10)
        };
    }
    
    #endregion
    
    #region Events Handlers
    #endregion
}