using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using RPGCreator.Core.Types;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Stats;
using RPGCreator.SDK.Extensions;
using RPGCreator.SDK.Logging;

namespace RPGCreator.UI.Content.AssetsManage.AssetsEditors.StatsEditor;

public class StatsManageControl : UserControl
{
    
    #region Constants
    #endregion
    
    #region Events
    #endregion
    
    #region Properties
    
    private IStatDef? SelectedStat { get; set; }
    
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
    public StatsManageControl()
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
            Watermark = "Search Stats...",
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

    public void ReloadContent()
    {
        Logger.Debug("Reloading Stats Editor content.");
        
        MainContent.Children.Clear();

        var statDefs = EngineServices.AssetsManager.GetAssets<IStatDef>();
        foreach (var statDef in statDefs)
        {
            var itemControl = new StatsManageItemControl(statDef);
            itemControl.ItemSelected += OnItemSelected;
            MainContent.Children.Add(itemControl);
            Logger.Debug("Added stat item: {statName}", statDef.Name);
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
        Logger.Debug("Add button clicked. Opening new stat editor.");
        var newStatEditor = new StatsEditorWindowControl(null);
        var host = ((AssetsManageWindow)this.GetVisualRoot()!);
        host?.OpenCustom(newStatEditor);
    }
    private void OnDeleteButtonClicked(object? sender, RoutedEventArgs e)
    {
        Logger.Debug("Delete button clicked.");
    }

    private void OnEditButtonClicked(object? sender, RoutedEventArgs e)
    {
        if(SelectedStat == null) 
        {
            Logger.Warning("No stat selected for editing.");
            return;
        }
        Logger.Debug("Edit button clicked.");
        var newStatEditor = new StatsEditorWindowControl(SelectedStat);
        var host = ((AssetsManageWindow)this.GetVisualRoot()!);
        host?.OpenCustom(newStatEditor);
    }

    private void OnItemSelected(object? sender, EventArgs e)
    {
        Logger.Debug("Stat {name} selected.", ((StatsManageItemControl)sender).StatDef.Name);
        SelectedStat = ((StatsManageItemControl)sender).StatDef;
    }
    
    #endregion
}