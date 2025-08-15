using Avalonia.Controls;
using Avalonia.Layout;
using RPGCreator.Core.Type.Assets.Characters;
using RPGCreator.UI.Common;
using Ursa.Controls;

namespace RPGCreator.UI.Content.AssetsManage.AssetsEditors.CharactersEditor.Tabs;

public class CharacterStatsTab : UserControl
{
    
    
    #region Events
    #endregion

    #region Properties

    public CharacterData Data;
    
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

        HealthPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Margin = new Avalonia.Thickness(10)
        };
        HealthAccordion = new Accordion(HealthPanel, "Health stat", true);
        Body.Children.Add(HealthAccordion);
        
        InitialHealth = new NumericIntUpDown()
        {
            Watermark = "Initial Health",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Minimum = 1,
            Maximum = 100,
            Value = 100,
            Margin = new Avalonia.Thickness(0, 0, 0, 10)
        };
        HealthPanel.Children.Add(
            new InputLabel("Initial Health", InitialHealth, "100")
        );
        
        MaxHealth = new NumericIntUpDown()
        {
            Watermark = "Max Health",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Minimum = 1,
            Maximum = 100,
            Value = 100,
            Margin = new Avalonia.Thickness(0, 0, 0, 10)
        };
        HealthPanel.Children.Add(
            new InputLabel("Max Health", MaxHealth, "100")
        );
        
        ManaPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Margin = new Avalonia.Thickness(10)
        };
        ManaAccordion = new Accordion(ManaPanel, "Mana stat", true);
        Body.Children.Add(ManaAccordion);
        InitialMana = new NumericIntUpDown()
        {
            Watermark = "Initial Mana",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Minimum = 0,
            Maximum = 100,
            Value = 50,
            Margin = new Avalonia.Thickness(0, 0, 0, 10)
        };
        ManaPanel.Children.Add(
            new InputLabel("Initial Mana", InitialMana, "100")
        );
        MaxMana = new NumericIntUpDown()
        {
            Watermark = "Max Mana",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Minimum = 0,
            Maximum = 100,
            Value = 50,
            Margin = new Avalonia.Thickness(0, 0, 0, 10)
        };
        ManaPanel.Children.Add(
            new InputLabel("Max Mana", MaxMana, "100")
        );
        StaminaPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Margin = new Avalonia.Thickness(10)
        };
        StaminaAccordion = new Accordion(StaminaPanel, "Stamina stat", true);
        Body.Children.Add(StaminaAccordion);
        InitialStamina = new NumericIntUpDown()
        {
            Watermark = "Initial Stamina",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Minimum = 0,
            Maximum = 100,
            Value = 50,
            Margin = new Avalonia.Thickness(0, 0, 0, 10)
        };
        StaminaPanel.Children.Add(
            new InputLabel("Initial Stamina", InitialStamina, "100")
        );
        MaxStamina = new NumericIntUpDown()
        {
            Watermark = "Max Stamina",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Minimum = 0,
            Maximum = 100,
            Value = 50,
            Margin = new Avalonia.Thickness(0, 0, 0, 10)
        };
        StaminaPanel.Children.Add(
            new InputLabel("Max Stamina", MaxStamina, "100")
        );

    }
    
    #endregion
    
    #region Events Handlers
    #endregion
}