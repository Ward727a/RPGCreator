using Avalonia.Controls;
using RPGCreator.Core.Types.Assets.Characters;
using RPGCreator.SDK.Assets.Definitions.Characters;

namespace RPGCreator.UI.Content.AssetsManage.AssetsEditors.CharactersEditor.Tabs;

public class CharacterFeaturesTab : UserControl
{
    
    #region Events
    #endregion

    #region Properties

    public CharacterData Data;
    
    #endregion
    
    #region Components
    
    private StackPanel Body { get; set; }
    private StackPanel FeaturesList { get; set; }

    private Grid Grid1;
    private CheckBox CanFightCheckBox;
    private CheckBox CanBeRecruitedCheckBox;
    
    private Grid Grid2;
    private CheckBox CanBePlayedCheckBox;
    private CheckBox CanMoveCheckBox;
    
    private Grid Grid3;
    private CheckBox CanDieCheckBox;
    private CheckBox CanBeTalkedToCheckBox;
    
    private Grid Grid4;
    private CheckBox IsUniqueCheckBox;
    private CheckBox IsBossCheckBox;
    
    private Grid Grid5;
    private CheckBox CanTrade;
    private CheckBox CanTriggerEventsCheckBox;
    
    #endregion
    
    #region Constructors
    public CharacterFeaturesTab(CharacterData data)
    {
        Data = data;
        Name = "Features"; // Define the name of the tab
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
        
        FeaturesList = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            Margin = new Avalonia.Thickness(10)
        };
        
        Body.Children.Add(FeaturesList);
        
        Grid1 = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*, *"),
            Margin = new Avalonia.Thickness(10),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch
        };
        FeaturesList.Children.Add(Grid1);
        
        CanFightCheckBox = new CheckBox
        {
            Content = "Can Fight",
            IsChecked = false,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch
        };
        Grid1.Children.Add(CanFightCheckBox);
        Grid.SetColumn(CanFightCheckBox, 0);
        CanBeRecruitedCheckBox = new CheckBox
        {
            Content = "Can Be Recruited",
            IsChecked = false,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch
        };
        Grid1.Children.Add(CanBeRecruitedCheckBox);
        Grid.SetColumn(CanBeRecruitedCheckBox, 1);
        
        Grid2 = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*, *"),
            Margin = new Avalonia.Thickness(10),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch
        };
        FeaturesList.Children.Add(Grid2);
        CanBePlayedCheckBox = new CheckBox
        {
            Content = "Can Be Played",
            IsChecked = false,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch
        };
        Grid2.Children.Add(CanBePlayedCheckBox);
        Grid.SetColumn(CanBePlayedCheckBox, 0);
        CanMoveCheckBox = new CheckBox
        {
            Content = "Can Move",
            IsChecked = false,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch
        };
        Grid2.Children.Add(CanMoveCheckBox);
        Grid.SetColumn(CanMoveCheckBox, 1);
        
        Grid3 = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*, *"),
            Margin = new Avalonia.Thickness(10),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch
        };
        FeaturesList.Children.Add(Grid3);
        CanDieCheckBox = new CheckBox
        {
            Content = "Can Die",
            IsChecked = false,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch
        };
        Grid3.Children.Add(CanDieCheckBox);
        Grid.SetColumn(CanDieCheckBox, 0);
        CanBeTalkedToCheckBox = new CheckBox
        {
            Content = "Can Be Talked To",
            IsChecked = false,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch
        };
        Grid3.Children.Add(CanBeTalkedToCheckBox);
        Grid.SetColumn(CanBeTalkedToCheckBox, 1);
        
        Grid4 = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*, *"),
            Margin = new Avalonia.Thickness(10),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch
        };
        FeaturesList.Children.Add(Grid4);
        IsUniqueCheckBox = new CheckBox
        {
            Content = "Is Unique",
            IsChecked = false,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch
        };
        Grid4.Children.Add(IsUniqueCheckBox);
        Grid.SetColumn(IsUniqueCheckBox, 0);
        IsBossCheckBox = new CheckBox
        {
            Content = "Is Boss",
            IsChecked = false,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch
        };
        Grid4.Children.Add(IsBossCheckBox);
        Grid.SetColumn(IsBossCheckBox, 1);
        
        Grid5 = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*, *"),
            Margin = new Avalonia.Thickness(10),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch
        };
        FeaturesList.Children.Add(Grid5);
        CanTrade = new CheckBox
        {
            Content = "Can Trade",
            IsChecked = false,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch
        };
        Grid5.Children.Add(CanTrade);
        Grid.SetColumn(CanTrade, 0);
        
        CanTriggerEventsCheckBox = new CheckBox
        {
            Content = "Can Trigger Events",
            IsChecked = false,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch
        };
        Grid5.Children.Add(CanTriggerEventsCheckBox);
        Grid.SetColumn(CanTriggerEventsCheckBox, 1);
        
    }
    
    #endregion
    
    #region Events Handlers
    #endregion
}