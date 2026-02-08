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

        // Adding stats input accordions dynamically based on the stats defined in CharacterData (so the stats found in the StatsRegistry)
        foreach (var keyValuePair in Data.Stats)
        {
            var accordion = new Expander()
            {
                Header = keyValuePair.Value.StatDef.Name
            };
            var panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Avalonia.Thickness(5)
            };
            accordion.Content = panel;
            Body.Children.Add(accordion);
            
            var initialValue = new NumericDoubleUpDown
            {
                Minimum = 0,
                Value = keyValuePair.Value.CurrentValue,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 200,
                Margin = new Avalonia.Thickness(5),
                Watermark = "Initial Value"
            };
            initialValue.ValueChanged += (s, e) =>
            {
                if (initialValue.Value.HasValue)
                {
                    keyValuePair.Value.CurrentValue = initialValue.Value.Value;
                }
            };
            panel.Children.Add(new InputLabel("Initial Value", initialValue));
            var maxValue = new NumericDoubleUpDown
            {
                Minimum = 1,
                Value = keyValuePair.Value.MaxValue,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 200,
                Margin = new Avalonia.Thickness(5),
                Watermark = "Max Value"
            };
            maxValue.ValueChanged += (s, e) =>
            {
                if (maxValue.Value.HasValue)
                {
                    keyValuePair.Value.MaxValue = maxValue.Value.Value;
                }
            };
            panel.Children.Add(new InputLabel("Max Value", maxValue));
            
            maxValue.IsEnabled = keyValuePair.Value.StatDef.CapSettings.CapType.Equals(EStatTypeCap.ByValue);
            
            var minValue = new NumericDoubleUpDown
            {
                Minimum = 0,
                Value = keyValuePair.Value.MinValue,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 200,
                Margin = new Avalonia.Thickness(5),
                Watermark = "Minimum Value"
            };
            minValue.ValueChanged += (s, e) =>
            {
                if (minValue.Value.HasValue)
                {
                    keyValuePair.Value.MinValue = minValue.Value.Value;
                }
            };
            panel.Children.Add(new InputLabel("Minimum Value", minValue));

        }
    }
    
    #endregion
    
    #region Events Handlers
    #endregion
}