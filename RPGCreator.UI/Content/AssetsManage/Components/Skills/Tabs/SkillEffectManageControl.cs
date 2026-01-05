using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using RPGCreator.Core.Types;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Skills;
using RPGCreator.SDK.Extensions;
using RPGCreator.SDK.Logging;
using RPGCreator.UI.Content.AssetsManage.AssetsEditors.SkillsEffectEditor;

namespace RPGCreator.UI.Content.AssetsManage.Components.Skills.Tabs;

public class SkillEffectManageControl : UserControl
{
    #region Constants
    #endregion
    
    #region Events
    #endregion
    
    #region Properties
    private ISkillEffect? SelectedSkillEffect { get; set; }
    #endregion
    
    #region Components
    private Grid Body { get; set; }
    private Grid Header { get; set; }
    private TextBox SearchBar { get; set; }
    private Button SearchButton { get; set; }
    private ScrollBox MainView { get; set; }
    private StackPanel MainContent { get; set; }
    private Grid Footer { get; set; }
    private Button AddButton { get; set; }
    private Button DeleteButton { get; set; }
    private Button EditButon { get; set; }
    #endregion
    
    #region Constructors
    public SkillEffectManageControl()
    {
        CreateComponents();
        RegisterEvents();
        ReloadContent();
        Content = Body;
    }


    #endregion
    
    #region Methods

    private void CreateComponents()
    {
        Body = new Grid
        {
            Margin = App.style.Margin,
            RowDefinitions = new RowDefinitions("Auto, *, Auto")
        };
        CreateHeader();
        CreateMainView();
        CreateFooter();
    }
    private void CreateHeader()
    {
        Header = new Grid
        {
            Margin = App.style.Margin,
            RowDefinitions = new RowDefinitions("Auto"),
            ColumnDefinitions = new ColumnDefinitions("*, Auto")
        };
        Body.Children.Add(Header);
        Grid.SetRow(Header, 0);
        
        SearchBar = new TextBox
        {
            Watermark = "Search Skill effect...",
            Margin = App.style.Margin,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch
        };
        Header.Children.Add(SearchBar);
        Grid.SetColumn(SearchBar, 0);
        
        SearchButton = new Button
        {
            Content = "Search",
            Margin = App.style.Margin,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right
        };
        Header.Children.Add(SearchButton);
        Grid.SetColumn(SearchButton, 1);
    }

    private void CreateMainView()
    {
        MainView = new ScrollBox
        {
            Margin = App.style.Margin,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch
        };
        
        Body.Children.Add(MainView);
        Grid.SetRow(MainView, 1);
        
        MainContent = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Vertical,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch
        };
        MainView.Content = MainContent;
    }

    private void CreateFooter()
    {
        Footer = new Grid
        {
            Margin = App.style.Margin,
            RowDefinitions = new RowDefinitions("Auto"),
            ColumnDefinitions = new ColumnDefinitions("*, Auto, Auto, Auto")
        };
        
        Body.Children.Add(Footer);
        Grid.SetRow(Footer, 2);
        
        AddButton = new Button
        {
            Content = "Add",
            Margin = App.style.Margin,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right
        };
        Footer.Children.Add(AddButton);
        Grid.SetColumn(AddButton, 1);
        
        DeleteButton = new Button
        {
            Content = "Delete",
            Margin = App.style.Margin,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right
        };
        Footer.Children.Add(DeleteButton);
        Grid.SetColumn(DeleteButton, 2);
        
        EditButon = new Button
        {
            Content = "Edit",
            Margin = App.style.Margin,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right
        };
        Footer.Children.Add(EditButon);
        Grid.SetColumn(EditButon, 3);
        
    }
    private void RegisterEvents()
    {
        SearchButton.Click += OnSearchButtonClicked;
        AddButton.Click += OnAddButtonClicked;
        DeleteButton.Click += OnDeleteButtonClicked;
        EditButon.Click += OnEditButtonClicked;
    }
    private void ReloadContent()
    {
        Logger.Debug("Reloading Skills Effect Manager content...");
        MainContent.Children.Clear();
        var allSkillEffects = EngineServices.AssetsManager.GetAssets<ISkillEffect>();

        
        foreach (var skillDef in allSkillEffects)
        {
            var itemControl = new SkillEffectManageItemControl(skillDef);
            itemControl.ItemSelected += OnItemSelected;
            MainContent.Children.Add(itemControl);
            Logger.Debug("Added skill effect item: {skillName}", skillDef.DisplayName);
        }
    }
    
    #endregion

    #region Events Handlers
    private void OnSearchButtonClicked(object? sender, RoutedEventArgs e)
    {
        Logger.Debug("Search button clicked. Search term: {searchTerm}", SearchBar.Text);
    }
    
    private void OnAddButtonClicked(object? sender, RoutedEventArgs e)
    {
        Logger.Debug("Add button clicked. Opening new skill effect editor.");
        var newSkillEffectEditor = new SkillsEffectEditorWindowControl();
        var host = ((AssetsManageWindow)this.GetVisualRoot()!);
        host?.OpenCustom(newSkillEffectEditor);
    }
    private void OnDeleteButtonClicked(object? sender, RoutedEventArgs e)
    {
        Logger.Debug("Delete button clicked.");
    }

    private void OnEditButtonClicked(object? sender, RoutedEventArgs e)
    {
        Logger.Debug("Edit button clicked.");
    }
    private void OnItemSelected(object? sender, EventArgs e)
    {
        Logger.Debug("Skill Effect {name} selected.", ((SkillEffectManageItemControl)sender).SkillEffect.DisplayName);
        SelectedSkillEffect = ((SkillEffectManageItemControl)sender).SkillEffect;
    }
    #endregion
}