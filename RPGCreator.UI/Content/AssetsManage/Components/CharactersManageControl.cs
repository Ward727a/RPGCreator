using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media.Imaging;
using Avalonia.VisualTree;
using RPGCreator.Core;
using RPGCreator.Core.Types.Assets;
using RPGCreator.Core.Types.Assets.Characters;
using RPGCreator.SDK.Assets.Definitions.Characters;
using RPGCreator.UI.Content.AssetsManage.AssetsEditors.CharactersEditor;

namespace RPGCreator.UI.Content.AssetsManage.Components;

public class CharacterManageItem : UserControl
{
    
    #region Events

    public event Action<CharacterData>? OnSelected;
    
    #endregion
    
    #region Properties
    public CharacterData CharacterData;
    #endregion
    
    #region Components
    public Grid Body { get; private set; }
    
    public Image CharacterImage { get; private set; }
    public TextBlock NameTextBlock { get; private set; }
    #endregion
    
    #region constructor
    public CharacterManageItem(CharacterData characterData)
    {
        CharacterData = characterData;
        CreateComponents();
        Content = Body;
    }
    #endregion

    #region methods
    private void CreateComponents()
    {
        Body = new Grid()
        {
            ColumnDefinitions = new ColumnDefinitions("64, *"),
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        
        CharacterImage = new Image
        {
            Source = new Bitmap(CharacterData.PortraitPath),
            Width = 64,
            Height = 64,
            VerticalAlignment = VerticalAlignment.Center
        };
        Body.Children.Add(CharacterImage);
        Grid.SetColumn(CharacterImage, 0);
        
        NameTextBlock = new TextBlock
        {
            Text = CharacterData.Name,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Avalonia.Thickness(5, 0, 0, 0)
        };
        Body.Children.Add(NameTextBlock);
        Grid.SetColumn(NameTextBlock, 1);
    }

    private void RegisterEvents()
    {
        Body.PointerPressed += (_, e) =>
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                SelectCharacter();
            }
        };
    }
    private void SelectCharacter()
    {
        OnSelected?.Invoke(CharacterData);
    }
    
    #endregion
}

public class CharactersManageControl : UserControl
{
    
    #region Events
    public event Action? OnSelectedCharacter;
    public event Action? OnNeedRefresh;
    #endregion
    
    #region Properties

    public CharacterData? SelectedCharacterData;
    
    #endregion
    
    #region Components
    public Grid Body { get; private set; }
    
        #region FiltersComponents
        public Grid FiltersGrid { get; private set; }
        public TextBox Filter_Search { get; private set; }
        public StackPanel Filter_Options { get; private set; }
        public StackPanel Filter_ButtonsBar { get; private set; }
        public Button Filter_Apply { get; private set; }
        public Button Filter_Reset { get; private set; }
        #endregion
        
        #region FooterComponents
        public Grid FooterGrid { get; private set; }
        public Button Footer_Add { get; private set; }
        public Button Footer_Edit { get; private set; }
        public Button Footer_Delete { get; private set; }
        public Button Footer_Refresh { get; private set; }
        #endregion
        
        #region ViewComponents
        public StackPanel ViewPanel { get; private set; }
        #endregion
    
    #endregion
    
    #region Constructors
    public CharactersManageControl()
    {
        CreateComponents();
        RegisterEvents();
        Content = Body;
    }
    #endregion

    #region Methods
    private void CreateComponents()
    {
        Body = new Grid()
        {
            RowDefinitions = new RowDefinitions("Auto, *, Auto"),
            Margin = new Avalonia.Thickness(5)
        };
        
        CreateFiltersComponents();
        CreateViewComponents();
        CreateFooterComponents();
    }

    private void CreateFiltersComponents()
    {
        FiltersGrid = new Grid()
        {
            RowDefinitions = new RowDefinitions("*, *, *"),
            Margin = new Avalonia.Thickness(5)
        };
        Body.Children.Add(FiltersGrid);
        Grid.SetRow(FiltersGrid, 0);
        
        Filter_Search = new TextBox
        {
            Watermark = "Search characters...",
            Margin = new Avalonia.Thickness(5)
        };
        FiltersGrid.Children.Add(Filter_Search);
        Grid.SetRow(Filter_Search, 0);
        
        Filter_Options = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Avalonia.Thickness(5)
        };
        FiltersGrid.Children.Add(Filter_Options);
        Grid.SetRow(Filter_Options, 1);
        
        Filter_ButtonsBar = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Avalonia.Thickness(5)
        };
        FiltersGrid.Children.Add(Filter_ButtonsBar);
        Grid.SetRow(Filter_ButtonsBar, 2);
        
        Filter_Apply = new Button
        {
            Content = "Apply Filters",
            Margin = new Avalonia.Thickness(5)
        };
        Filter_ButtonsBar.Children.Add(Filter_Apply);
        
        Filter_Reset = new Button
        {
            Content = "Reset Filters",
            Margin = new Avalonia.Thickness(5)
        };
        Filter_ButtonsBar.Children.Add(Filter_Reset);
    }

    private void CreateViewComponents()
    {
        ViewPanel = new StackPanel()
        {
            Margin = new Avalonia.Thickness(5)
        };
        Body.Children.Add(ViewPanel);
        Grid.SetRow(ViewPanel, 1);
    }

    private void CreateFooterComponents()
    {
        FooterGrid = new Grid()
        {
            ColumnDefinitions = new ColumnDefinitions("Auto, Auto, Auto, Auto"),
        };
        Body.Children.Add(FooterGrid);
        Grid.SetRow(FooterGrid, 2);
        
        Footer_Add = new Button
        {
            Content = "Add Character",
            Margin = new Avalonia.Thickness(5)
        };
        FooterGrid.Children.Add(Footer_Add);
        Grid.SetColumn(Footer_Add, 0);
        
        Footer_Edit = new Button
        {
            Content = "Edit Character",
            Margin = new Avalonia.Thickness(5)
        };
        FooterGrid.Children.Add(Footer_Edit);
        Grid.SetColumn(Footer_Edit, 1);
        
        Footer_Delete = new Button
        {
            Content = "Delete Character",
            Margin = new Avalonia.Thickness(5)
        };
        FooterGrid.Children.Add(Footer_Delete);
        Grid.SetColumn(Footer_Delete, 2);
        
        Footer_Refresh = new Button
        {
            Content = "Refresh",
            Margin = new Avalonia.Thickness(5)
        };
        FooterGrid.Children.Add(Footer_Refresh);
        Grid.SetColumn(Footer_Refresh, 3);
    }

    private void RegisterEvents()
    {

        Loaded += OnLoaded;
        OnNeedRefresh += _OnNeedRefresh;
        
        RegisterFiltersEvents();
        RegisterViewEvents();
        RegisterFooterEvents();
    }
    
    private void RegisterFiltersEvents()
    {
    }

    private void RegisterViewEvents()
    {
    }

    private void RegisterFooterEvents()
    {
        Footer_Add.Click += OnAddCharacter;
        Footer_Edit.Click += OnEditCharacter;
        Footer_Delete.Click += OnDeleteCharacter;
        Footer_Refresh.Click += OnRefreshCharacters;
    }

    private void ReloadView()
    {
        ViewPanel.Children.Clear();
        
        foreach (var assetData in EngineCore.Instance.Managers.Assets.SearchAllPacks<CharacterData>())
        {
            if (!EngineCore.Instance.Managers.Assets.TryResolveAsset(assetData.AssetId, out CharacterData? characterData)) continue;
            
            var item = new CharacterManageItem(characterData);
            item.OnSelected += (data) =>
            {
                OnSelectedCharacter?.Invoke();
                SelectedCharacterData = data;
            };
            ViewPanel.Children.Add(item);

        }
    }
    
    #endregion
    
    #region Event Handlers
    
    private void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        ReloadView();
    }

    private void _OnNeedRefresh()
    {
        ReloadView();
    }
    
    private void OnAddCharacter(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var host_ = ((AssetsManageWindow)this.GetVisualRoot()!);
        var characterEditor = new CharacterEditorWindowControl(new CharacterData(""));
        host_.OpenCustom(characterEditor);
        OnNeedRefresh?.Invoke();
    }
    
    private void OnEditCharacter(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (SelectedCharacterData == null)
        {
            // Show a message or handle the case where no character is selected
            return;
        }
        
        // Logic to edit the selected character
        OnNeedRefresh?.Invoke();
    }
    
    private void OnDeleteCharacter(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (SelectedCharacterData == null)
        {
            // Show a message or handle the case where no character is selected
            return;
        }
        
        OnNeedRefresh?.Invoke();
    }
    
    private void OnRefreshCharacters(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        OnNeedRefresh?.Invoke();
    }
    
    #endregion
}