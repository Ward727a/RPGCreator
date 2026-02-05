using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Characters;
using RPGCreator.SDK.ECS.Features;
using RPGCreator.SDK.Extensions;
using RPGCreator.UI.Content.AssetsManage.AssetsEditors.CharactersEditor;
using Ursa.Controls;
using AutoCompleteBox = Avalonia.Controls.AutoCompleteBox;

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
    
    public StackPanel InfoPanel { get; private set; }
    public TextBlock NameTextBlock { get; private set; }
    public TextBlock UrnTextBlock { get; private set; }
    
    public StackPanel ButtonsPanel { get; private set; }
    public Button EditButton { get; private set; }
    public Button DeleteButton { get; private set; }
    
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
            ColumnDefinitions = new ColumnDefinitions("64, *, Auto"),
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        
        CharacterImage = new Image
        {
            Source = EngineServices.ResourcesService.Load<Bitmap>(CharacterData.PortraitPath),
            Width = 64,
            Height = 64,
            VerticalAlignment = VerticalAlignment.Center
        };
        Body.Children.Add(CharacterImage);
        Grid.SetColumn(CharacterImage, 0);
        
        InfoPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Avalonia.Thickness(5)
        };
        Body.Children.Add(InfoPanel);
        Grid.SetColumn(InfoPanel, 1);
        
        NameTextBlock = new TextBlock
        {
            Text = CharacterData.Name,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Avalonia.Thickness(5, 0, 0, 0)
        };
        InfoPanel.Children.Add(NameTextBlock);
        
        UrnTextBlock = new TextBlock
        {
            Text = CharacterData.Urn.ToString(),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Avalonia.Thickness(5, 0, 0, 0),
            FontSize = 10,
            Opacity = 0.6
        };
        InfoPanel.Children.Add(UrnTextBlock);
        
        ButtonsPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Margin = new Avalonia.Thickness(5)
        };
        Body.Children.Add(ButtonsPanel);
        Grid.SetColumn(ButtonsPanel, 2);
        
        EditButton = new Button
        {
            Content = "Edit",
            Margin = new Avalonia.Thickness(5)
        };
        ButtonsPanel.Children.Add(EditButton);
        
        DeleteButton = new Button
        {
            Content = "Delete",
            Margin = new Avalonia.Thickness(5)
        };
        ButtonsPanel.Children.Add(DeleteButton);
        
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
    
    public List<String> AvailableCharNames { get; private set; } = new List<string>();
    
    #endregion
    
    #region Components
    public Grid Body { get; private set; }
    
        #region FiltersComponents
        public Grid FiltersGrid { get; private set; }
        public AutoCompleteBox Filter_Search { get; private set; }
        public StackPanel Filter_Options { get; private set; }
        public StackPanel Filter_ButtonsBar { get; private set; }
        public Button Filter_Reset { get; private set; }
        #endregion
        
        #region FooterComponents
        public StackPanel FooterGrid { get; private set; }
        public Button Footer_Add { get; private set; }
        public Button Footer_Edit { get; private set; }
        public Button Footer_Delete { get; private set; }
        #endregion
        
        #region ViewComponents
        public ScrollViewer ViewComponents { get; private set; }
        public ListBox ViewPanel { get; private set; }
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
        };
        
        CreateFiltersComponents();
        LoadCharacters();
        CreateViewComponents();
        CreateFooterComponents();
    }

    private void CreateFiltersComponents()
    {
        FiltersGrid = new Grid()
        {
            RowDefinitions = new RowDefinitions("*, *, *"),
            RowSpacing = 5,
            Margin = new Thickness(5)
        };
        Body.Children.Add(FiltersGrid);
        Grid.SetRow(FiltersGrid, 0);
        
        Filter_Search = new AutoCompleteBox()
        {
            Watermark = "Search characters...",
            FilterMode = AutoCompleteFilterMode.Contains,
            ItemsSource = _filteredCharacterDatas
        };
        FiltersGrid.Children.Add(Filter_Search);
        Grid.SetRow(Filter_Search, 0);
        
        Filter_Options = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 5
        };
        FiltersGrid.Children.Add(Filter_Options);
        Grid.SetRow(Filter_Options, 1);
        
        Filter_ButtonsBar = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 5
        };
        FiltersGrid.Children.Add(Filter_ButtonsBar);
        Grid.SetRow(Filter_ButtonsBar, 2);
        
        Filter_Reset = new Button
        {
            Content = "Reset Filters",
        };
        Filter_ButtonsBar.Children.Add(Filter_Reset);
    }

    private IEnumerable<CharacterData> _characterDatas;
    private ObservableCollection<CharacterData> _filteredCharacterDatas = new ObservableCollection<CharacterData>();
    private void LoadCharacters()
    {
        _characterDatas = EngineServices.AssetsManager.GetAssets<CharacterData>();
        RefreshFilters();
    }

    private void CreateViewComponents()
    {
        ViewComponents = new ScrollViewer()
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Margin = new Avalonia.Thickness(5)
        };
        Body.Children.Add(ViewComponents);
        Grid.SetRow(ViewComponents, 1);
        
        ViewPanel = new ListBox()
        {
            Margin = new Avalonia.Thickness(5),
            ItemsSource = _filteredCharacterDatas,
            ItemTemplate = new FuncDataTemplate<CharacterData>((charactersData, _) =>
            {
                if (charactersData == null) return null;
                return new CharacterManageItem(charactersData);
            }),
        };

        ViewComponents.Content = (ViewPanel);
    }

    private string _filterSearchText = "";
    
    private void RefreshFilters()
    {
        Filter_Search.ItemsSource = null;
        _filteredCharacterDatas.Clear();
        AvailableCharNames.Clear();
        
        var filtered = _characterDatas.Where(characterData =>
        {
            if (!string.IsNullOrWhiteSpace(_filterSearchText))
            {
                var search = _filterSearchText.ToLower();
                bool matches = characterData.Name.ToLower().Contains(search);
                if (!matches) return false;
            }
            // Add more filter conditions here
            
            return true;
        });
        
        foreach (var characterData in filtered)
        {
            _filteredCharacterDatas.Add(characterData);
            AvailableCharNames.Add(characterData.Name);
        }
        
        Filter_Search.ItemsSource = AvailableCharNames;
    }

    private void CreateFooterComponents()
    {
        FooterGrid = new StackPanel()
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 5,
            Margin = new Thickness(5)
        };
        Body.Children.Add(FooterGrid);
        Grid.SetRow(FooterGrid, 2);
        
        Footer_Add = new Button
        {
            Content = "Add Character",
        };
        FooterGrid.Children.Add(Footer_Add);
        
        Footer_Edit = new Button
        {
            Content = "Edit Character",
        };
        FooterGrid.Children.Add(Footer_Edit);
        
        Footer_Delete = new Button
        {
            Content = "Delete Character",
        };
        FooterGrid.Children.Add(Footer_Delete);
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
        Filter_Search.TextChanged += (_, e) =>
        {
            _filterSearchText = Filter_Search.Text ?? "";
            Dispatcher.UIThread.Post(RefreshFilters, DispatcherPriority.Normal);
        };
        
        Filter_Reset.Click += (_, e) =>
        {
            _filterSearchText = "";
            Filter_Search.Text = "";;
            Dispatcher.UIThread.Post(RefreshFilters, DispatcherPriority.Normal);
        };
    }

    private void RegisterViewEvents()
    {
        ViewPanel.SelectionChanged += (_, e) =>
        {
            if (ViewPanel.SelectedItem is CharacterData characterData)
            {
                SelectedCharacterData = characterData;
            }
        };
    }

    private void RegisterFooterEvents()
    {
        Footer_Add.Click += OnAddCharacter;
        Footer_Edit.Click += OnEditCharacter;
        Footer_Delete.Click += OnDeleteCharacter;
    }

    private void ReloadView()
    {
        
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
        var data = new CharacterData("");
        var characterEditor = new CharacterEditorWindowControl(data);
        host_.OpenCustom(characterEditor);
    }
    
    private void OnEditCharacter(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (SelectedCharacterData != null)
        {
            var host_ = ((AssetsManageWindow)this.GetVisualRoot()!);
            var characterEditor = new CharacterEditorWindowControl(SelectedCharacterData);
            host_.OpenCustom(characterEditor);
        }
    }
    
    private void OnDeleteCharacter(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (SelectedCharacterData == null)
        {
            // Show a message or handle the case where no character is selected
            return;
        }
        
    }
    
    #endregion
}