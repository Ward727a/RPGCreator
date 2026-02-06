#region LICENSE
//
// RPG Creator - Open-source RPG Engine.
// (c) 2025 Ward
// 
// This file is part of RPG Creator and is distributed under the Apache 2.0 License.
// You are free to use, modify, and distribute this file under the terms of the Apache 2.0 License.
// See LICENSE for details.
// 
// ---
// 
// Ce fichier fait partie de RPG Creator et est distribué sous licence Apache 2.0.
// Vous êtes libre de l'utiliser, de le modifier et de le distribuer sous les termes de la licence Apache 2.0.
// Voir LICENSE pour plus de détails.
// 
// Contact:
// => Mail: Ward727a@gmail.com
//    Please use this object: "RPG Creator [YourObject]"
// => Discord: ward727
// 
// For urgent inquiries, sending both an email and a message on Discord is highly recommended for a quicker response.
// 
// 
#endregion
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.VisualTree;
using RPGCreator.Core.Types;
using RPGCreator.UI.Content.AssetsManage.AssetsEditors.TilesetEditor;
using System;
using System.Collections.Generic;
using Avalonia.Media.Imaging;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Tilesets;
using RPGCreator.SDK.Assets.Definitions.Tilesets.IntGrid;
using RPGCreator.SDK.Extensions;
using RPGCreator.SDK.Types.Collections;
using RPGCreator.UI.Common;

namespace RPGCreator.UI.Content.AssetsManage.Components
{

    public class NewTilesetDialogOnCreatedEventArgs : EventArgs
    {
        public string Name { get; }
        public int TileWidth { get; }
        public int TileHeight { get; }
        public string AssetPack { get; }
        public int TilesetType { get; }
        public NewTilesetDialogOnCreatedEventArgs(string name, int tileWidth, int tileHeight, string assetPack, int tilesetType)
        {
            Name = name;
            TileWidth = tileWidth;
            TileHeight = tileHeight;
            AssetPack = assetPack;
            TilesetType = tilesetType;
        }
    }

    public class NewTilesetDialog : Window
    {
        public event Action<NewTilesetDialogOnCreatedEventArgs>? OnCreated;
        public Grid Body { get; private set; }
        public NewTilesetDialog()
        {
            Title = "New Tileset";
            Width = 400;
            Height = 300;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            CreateComponents();
            Content = Body;
        }

        private void CreateComponents()
        {
            Body = new Grid
            {
                Margin = new Avalonia.Thickness(10),
                RowDefinitions = new RowDefinitions("Auto, Auto, Auto, Auto, Auto, *"),
                ColumnDefinitions = new ColumnDefinitions("Auto, *")
            };

            var nameLabel = new TextBlock
            {
                Text = "Name:",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Avalonia.Thickness(5)
            };
            Body.Children.Add(nameLabel);
            Grid.SetRow(nameLabel, 0);
            Grid.SetColumn(nameLabel, 0);
            var nameTextBox = new TextBox
            {
                Margin = new Avalonia.Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                UseFloatingWatermark = true,
                Watermark = "Enter tileset name..."
            };
            Body.Children.Add(nameTextBox);
            Grid.SetRow(nameTextBox, 0);
            Grid.SetColumn(nameTextBox, 1);

            var tileWidthLabel = new TextBlock
            {
                Text = "Tile Width:",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Avalonia.Thickness(5)
            };
            Body.Children.Add(tileWidthLabel);
            Grid.SetRow(tileWidthLabel, 1);
            Grid.SetColumn(tileWidthLabel, 0);

            var tileWidthTextBox = new TextBox
            {
                Margin = new Avalonia.Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                UseFloatingWatermark = true,
                Watermark = "Enter tile width..."
            };
            Body.Children.Add(tileWidthTextBox);
            Grid.SetRow(tileWidthTextBox, 1);
            Grid.SetColumn(tileWidthTextBox, 1);

            var tileHeightLabel = new TextBlock
            {
                Text = "Tile Height:",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Avalonia.Thickness(5)
            };
            Body.Children.Add(tileHeightLabel);
            Grid.SetRow(tileHeightLabel, 2);
            Grid.SetColumn(tileHeightLabel, 0);

            var tileHeightTextBox = new TextBox
            {
                Margin = new Avalonia.Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                UseFloatingWatermark = true,
                Watermark = "Enter tile height..."
            };
            Body.Children.Add(tileHeightTextBox);
            Grid.SetRow(tileHeightTextBox, 2);
            Grid.SetColumn(tileHeightTextBox, 1);

            var tilesetTypeSelectorLabel = new TextBlock
            {
                Text = "Tileset Type:",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Avalonia.Thickness(5)
            };
            Body.Children.Add(tilesetTypeSelectorLabel);
            Grid.SetRow(tilesetTypeSelectorLabel, 3);
            Grid.SetColumn(tilesetTypeSelectorLabel, 0);

            var tilesetTypeSelector = new ComboBox
            {
                Margin = new Avalonia.Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
            };
            tilesetTypeSelector.Items.Add("Tileset");
            tilesetTypeSelector.Items.Add("Autotile");

            tilesetTypeSelector.SelectedIndex = 0; // Default to Tileset

            Body.Children.Add(tilesetTypeSelector);
            Grid.SetRow(tilesetTypeSelector, 3);
            Grid.SetColumn(tilesetTypeSelector, 1);

            var assetPackLabel = new TextBlock
            {
                Text = "Asset Pack:",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Avalonia.Thickness(5)
            };
            Body.Children.Add(assetPackLabel);
            Grid.SetRow(assetPackLabel, 4);
            Grid.SetColumn(assetPackLabel, 0);

            var assetPackSelector = new ComboBox
            {
                Margin = new Avalonia.Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
            };

            EngineServices.AssetsManager.GetLoadedPacks().ForEach(pack =>
            {
                assetPackSelector.Items.Add(pack.Name);
            });

            assetPackSelector.SelectedIndex = 0; // Default to the first asset pack

            Body.Children.Add(assetPackSelector);
            Grid.SetRow(assetPackSelector, 4);
            Grid.SetColumn(assetPackSelector, 1);

            var createButton = new Button
            {
                Content = "Create",
                Margin = new Avalonia.Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
            };
            createButton.Click += (sender, e) =>
            {
                // Here you would handle the creation of the new tileset.
                // For now, we just close the dialog.
                OnCreated?.Invoke(new NewTilesetDialogOnCreatedEventArgs(
                    nameTextBox.Text ?? "",
                    int.TryParse(tileWidthTextBox.Text, out var tileWidth) ? tileWidth : 32, // Default to 32 if parsing fails
                    int.TryParse(tileHeightTextBox.Text, out var tileHeight) ? tileHeight : 32, // Default to 32 if parsing fails
                    assetPackSelector.SelectedItem?.ToString() ?? "",
                    tilesetTypeSelector.SelectedIndex));
                Console.WriteLine($"Creating new tileset: {nameTextBox.Text}, Width: {tileWidthTextBox.Text}, Height: {tileHeightTextBox.Text}, Type: {tilesetTypeSelector.SelectedItem}");
                Close();
            };
            Body.Children.Add(createButton);
            Grid.SetRow(createButton, 5);
            Grid.SetColumn(createButton, 1);
        }

    }
    public class TilesetsManageControlFilters
    {

        public TilesetsManageControlFilters()
        {
            RegisterEvents();
        }

        private void RegisterEvents()
        {
            SearchQueryChanged += () => OnFilterChanged?.Invoke();
            GroupByAssetsPackChanged += () => OnFilterChanged?.Invoke();
            ShowTypeChanged += () => OnFilterChanged?.Invoke();
        }

        public event Action? OnFilterChanged;

        public event Action? SearchQueryChanged;
        private string _searchQuery = string.Empty;
        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (_searchQuery != value)
                {
                    _searchQuery = value;
                    SearchQueryChanged?.Invoke();
                }
            }
        }

        public event Action? GroupByAssetsPackChanged;
        private bool _groupByAssetsPack = false;
        public bool GroupByAssetsPack
        {
            get => _groupByAssetsPack;
            set
            {
                if (_groupByAssetsPack != value)
                {
                    _groupByAssetsPack = value;
                    GroupByAssetsPackChanged?.Invoke();
                }
            }
        }

        public event Action? ShowTypeChanged;
        // I should probably use an enum here, but for now, as it's only two values, an int is sufficient.
        private int _showType = 0; // 0 for List View, 1 for Grid View
        public int ShowType
        {
            get => _showType;
            set
            {
                if (_showType != value)
                {
                    _showType = value;
                    ShowTypeChanged?.Invoke();
                }
            }
        }
    }

    public interface ITilesetViewItem
    {
        public BaseTilesetDef? TilesetDef { get; }
        public void Select();
        public void Deselect();
    }

    public class TilesetViewListItem : UserControl, ITilesetViewItem
    {
        public event Action? OnSelected;
        public event Action? OnDeselected;
        public BaseTilesetDef? TilesetDef { get; private set; }
        public bool IsSelected { get; private set; } = false;

        public Grid Body { get; private set; }

        public TilesetViewListItem(BaseTilesetDef tilesetDef)
        {
            TilesetDef = tilesetDef;
            CreateComponents();
            RegisterEvents();
            Content = Body;
        }

        private void CreateComponents()
        {
            Body = new Grid
            {
                Margin = new Avalonia.Thickness(5),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                ColumnDefinitions = new ColumnDefinitions("" +
                "Auto, " + // Image
                "Auto, " + // Separator
                "*, " + // Name
                "Auto, " + // Separator
                "Auto, " + // Image Size
                "Auto, " + // Separator
                "Auto, " + // Tile Size
                "Auto, " + // Separator
                "Auto"), // Image path
                Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Transparent),
            };

            var iconImage = new Image
            {
                Source = TilesetDef != null ? EngineServices.ResourcesService.Load<Bitmap>(TilesetDef.ImagePath) : EditorAssets.FallbackImage,
                Width = 50,
                Height = 50,
                Margin = new Avalonia.Thickness(5),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            };
            Body.Children.Add(iconImage);

            var sep = new VSeparator();
            Body.Children.Add(sep);
            Grid.SetColumn(sep, 1);

            var nameTextBlock = new TextBlock
            {
                Text = TilesetDef != null ? TilesetDef.Name : "None",
                MinWidth = 250,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                Margin = new Avalonia.Thickness(5),
                TextAlignment = Avalonia.Media.TextAlignment.Center,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Transparent),
            };
            Body.Children.Add(nameTextBlock);
            Grid.SetColumn(nameTextBlock, 2);
            ToolTip.SetTip(nameTextBlock, TilesetDef != null? TilesetDef.Name : "No tileset found!");

            sep = new VSeparator();
            Body.Children.Add(sep);
            Grid.SetColumn(sep, 3);

            var imageSizeTextBlock = new TextBlock
            {
                Text = TilesetDef != null ? $"{TilesetDef.ImageWidth}x{TilesetDef.ImageHeight}" : "No Tileset Found",
                Width = 100,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                Margin = new Avalonia.Thickness(5),
                TextAlignment = Avalonia.Media.TextAlignment.Center,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Transparent),
            };
            Body.Children.Add(imageSizeTextBlock);
            Grid.SetColumn(imageSizeTextBlock, 4);
            ToolTip.SetTip(imageSizeTextBlock, $"(Width)x(Height)");

            sep = new VSeparator();
            Body.Children.Add(sep);
            Grid.SetColumn(sep, 5);

            var tileSizeTextBlock = new TextBlock
            {
                Text = TilesetDef != null ? $"{TilesetDef.TileWidth}x{TilesetDef.TileHeight}" : "No Tileset Found",
                Width = 80,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                Margin = new Avalonia.Thickness(5),
                TextAlignment = Avalonia.Media.TextAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Transparent),
                VerticalAlignment = VerticalAlignment.Center,
            };
            Body.Children.Add(tileSizeTextBlock);
            Grid.SetColumn(tileSizeTextBlock, 6);
            ToolTip.SetTip(tileSizeTextBlock, $"(Width)x(Height)");

            sep = new VSeparator();
            Body.Children.Add(sep);
            Grid.SetColumn(sep, 7);
            var imagePathTextBlock = new TextBlock
            {
                Text = TilesetDef != null ? TilesetDef.ImagePath : "No Tileset Found",
                MinWidth = 200,
                MaxWidth = 400,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                Margin = new Avalonia.Thickness(5),
                TextAlignment = Avalonia.Media.TextAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Transparent),
            };
            Body.Children.Add(imagePathTextBlock);
            Grid.SetColumn(imagePathTextBlock, 8);

            //Body.Children.Add(new VSeparator());
        }

        private void RegisterEvents()
        {
            Body.PointerPressed += Body_PointerPressed;
            Body.PointerReleased += Body_PointerReleased;
        }

        private void Body_PointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (!e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
            {
                if(IsSelected)
                    Body.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromArgb(50, 0, 0, 0));
                else
                    Body.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Transparent);
            }
        }

        private void Body_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
            {
                var contextMenu = new ContextMenu();
                var editMenuItem = new MenuItem
                {
                    Header = "Edit Tileset",
                };
                editMenuItem.Click += (s, args) =>
                {
                    Console.WriteLine($"Editing Tileset: {(TilesetDef != null ? TilesetDef.Name : "No Tileset Found")}");
                };
                contextMenu.Items.Add(editMenuItem);

                if (GlobalStaticUIData.CurrentContext != null)
                {
                    GlobalStaticUIData.CloseContext();
                }
                GlobalStaticUIData.CurrentContext = contextMenu;
                GlobalStaticUIData.OpenContext(this);

                Body.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromArgb(100, 0, 0, 0));
            }
            else if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                if (IsSelected)
                {
                    Console.WriteLine($"Deselected Tileset: {(TilesetDef != null ? TilesetDef.Name : "No Tileset Found")}");
                    Body.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Transparent);
                    IsSelected = false;
                    OnDeselected?.Invoke();
                }
                else
                {
                    Console.WriteLine($"Selected Tileset: {(TilesetDef != null ? TilesetDef.Name : "No Tileset Found")}");
                    Body.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromArgb(50, 0, 0, 0));
                    IsSelected = true;
                    OnSelected?.Invoke();
                }
            }
        }

        public void Select()
        {
            if (!IsSelected)
            {
                Console.WriteLine($"Selected Tileset: {(TilesetDef != null ? TilesetDef.Name : "No Tileset Found")}");
                Body.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromArgb(50, 0, 0, 0));
                IsSelected = true;
            }
        }

        public void Deselect()
        {
            if (IsSelected)
            {
                Console.WriteLine($"Deselected Tileset: {(TilesetDef != null ? TilesetDef.Name : "No Tileset Found")}");
                Body.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Transparent);
                IsSelected = false;
            }
        }
    }

    public class TilesetsManageControl : UserControl
    {

        private IAssetScope _scope;

        #region Events

        public event Action? OnSelectedTilesetChanged;
        public event Action?  OnNeedRefresh;

        #endregion

        #region Data

        public readonly TilesetsManageControlFilters Filters = new TilesetsManageControlFilters();

        private ITilesetViewItem? _selectedTilesetViewItem;
        public ITilesetViewItem? SelectedTilesetViewItem
        {
            get => _selectedTilesetViewItem;
            set
            {
                if (_selectedTilesetViewItem != value)
                {
                    _selectedTilesetViewItem?.Deselect();
                    _selectedTilesetViewItem = value;
                    _selectedTilesetViewItem?.Select();
                    OnSelectedTilesetChanged?.Invoke();
                }
            }
        }

        #endregion

        #region Components

        public Grid Body { get; private set; }

        #region Filter Components
        public Grid FilterGrid { get; private set; }

        public Grid FilterTop { get; private set; }
        public TextBox Filter_SearchBar { get; private set; }
        public StackPanel FilterBottom { get; private set; }
        public ToggleButton Filter_GroupByAssetsPack { get; private set; }
        public ComboBox Filter_ShowType { get; private set; }
        public ToggleButton Filter_ShowOnlyTileset { get; private set; }
        public ToggleButton Filter_ShowOnlyAutotiles { get; private set; }

        public Separator Filter_Separator { get; private set; }
        #endregion

        #region Footer Components

        public Grid FooterGrid { get; private set; }
        public Separator Footer_Separator { get; private set; }
        public StackPanel Footer { get; private set; }
        public Button Footer_New { get; private set; }
        public Button Footer_Edit { get; private set; }
        public Button Footer_Delete { get; private set; }

        #endregion

        #region View Components

        public StackPanel ViewPanel;

        #endregion

        #endregion

        public TilesetsManageControl()
        {
            _scope = EngineServices.AssetsManager.CreateAssetScope("tilesets_manage_control");
            CreateComponents();
            Content = Body;
        }

        private void CreateComponents()
        {
            Body = new Grid
            {
                RowDefinitions = new RowDefinitions("Auto, *, Auto"),
            };

            CreateFilterComponents();
            RegisterFiltersEvents();

            CreateViewComponents();

            CreateFooter();
            RegisterFooterEvents();
        }

        private void CreateFilterComponents()
        {
            FilterGrid = new Grid
            {
                Margin = new Avalonia.Thickness(10),
                RowDefinitions = new RowDefinitions("Auto, Auto, Auto"),
            };
            Body.Children.Add(FilterGrid);

            FilterTop = new Grid
            {
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                Margin = new Avalonia.Thickness(5),
                RowDefinitions = new RowDefinitions("*"),
            };
            FilterGrid.Children.Add(FilterTop);

            Filter_SearchBar = new TextBox
            {
                Margin = new Avalonia.Thickness(5),
                UseFloatingWatermark = true,
                Watermark = "Search Tileset or Autotiles...",
            };
            FilterTop.Children.Add(Filter_SearchBar);

            FilterBottom = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                Margin = new Avalonia.Thickness(5),
            };
            FilterGrid.Children.Add(FilterBottom);
            Grid.SetRow(FilterBottom, 1);

            Filter_GroupByAssetsPack = new ToggleButton
            {
                Content = "GroupDefinition by Assets Pack",
                Margin = new Avalonia.Thickness(5),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            };
            FilterBottom.Children.Add(Filter_GroupByAssetsPack);

            Filter_ShowOnlyTileset = new ToggleButton
            {
                Content = "Only Tilesets",
                Margin = new Avalonia.Thickness(5),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            };

            FilterBottom.Children.Add(Filter_ShowOnlyTileset);

            Filter_ShowOnlyAutotiles = new ToggleButton
            {
                Content = "Only Autotiles",
                Margin = new Avalonia.Thickness(5),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            };
            FilterBottom.Children.Add(Filter_ShowOnlyAutotiles);

            Filter_ShowType = new ComboBox
            {
                Margin = new Avalonia.Thickness(5),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            };
            Filter_ShowType.Items.Add("List View");
            Filter_ShowType.Items.Add("Grid View");
            Filter_ShowType.SelectedIndex = 0; // Default to List View

            FilterBottom.Children.Add(Filter_ShowType);

            Filter_Separator = new Separator
            {
                Margin = new Avalonia.Thickness(5),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            };
            FilterGrid.Children.Add(Filter_Separator);
            Grid.SetRow(Filter_Separator, 2);
        }

        private void RegisterFiltersEvents()
        {
            Filter_SearchBar.TextChanged += (sender, e) =>
            {
                Filters.SearchQuery = Filter_SearchBar.Text ?? string.Empty;
            };
            Filter_GroupByAssetsPack.IsCheckedChanged += (sender, e) =>
            {
                Filters.GroupByAssetsPack = Filter_GroupByAssetsPack.IsChecked.GetValueOrDefault();
            };
            Filter_ShowType.SelectionChanged += (sender, e) =>
            {
                Filters.ShowType = Filter_ShowType.SelectedIndex;
            };
        }

        private void CreateViewComponents()
        {


            switch (Filters.ShowType)
            {
                case 0: // ListView
                    CreateListViewComponents();
                    break;

                case 1: // GridView
                    // CreateGridViewComponents(tilesets);
                    break;
            }
        }

        private void CreateListViewComponents()
        {
            var viewGrid = new Grid();

            var viewScroller = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Content = viewGrid
            };

            Body.Children.Add(viewScroller);
            Grid.SetRow(viewScroller, 1);

            ViewPanel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Vertical,
                Margin = new Avalonia.Thickness(10),
            };

            viewGrid.Children.Add(ViewPanel);

            OnNeedRefresh += TilesetsManageControl_OnNeedRefresh;
            TilesetsManageControl_OnNeedRefresh();
        }

        private void TilesetsManageControl_OnNeedRefresh()
        {
            ViewPanel.Children.Clear();

            var searchResults = EngineServices.AssetsManager.GetAssets<BaseTilesetDef>();

            foreach (var result in searchResults)
            {
                if (result is IntGridTilesetDef)
                    continue;
                var item = new TilesetViewListItem(result);
                item.OnSelected += () => { SelectedTilesetViewItem = item; };
                item.OnDeselected += () =>
                {
                    if (SelectedTilesetViewItem == item)
                    {
                        SelectedTilesetViewItem = null;
                    }
                };
                ViewPanel.Children.Add(item);
            }
        }

        private void CreateGridViewComponents(List<BaseTilesetDef> tilesets)
        {

        }

        private void CreateFooter()
        {
            FooterGrid = new Grid
            {
                Margin = new Avalonia.Thickness(10),
                RowDefinitions = new RowDefinitions("Auto, Auto"),
            };
            Body.Children.Add(FooterGrid);
            Grid.SetRow(FooterGrid, 2);

            Footer_Separator = new Separator
            {
                Margin = new Avalonia.Thickness(5),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            };
            FooterGrid.Children.Add(Footer_Separator);

            Footer = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                Margin = new Avalonia.Thickness(5),
            };
            FooterGrid.Children.Add(Footer);
            Grid.SetRow(Footer, 2);
            Footer_New = new Button
            {
                Content = "New...",
                Margin = new Avalonia.Thickness(5),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            };
            Footer.Children.Add(Footer_New);
            ToolTip.SetTip(Footer_New, "Add a new tileset to the project.");

            Footer_Edit = new Button
            {
                Content = "Edit Tileset",
                Margin = new Avalonia.Thickness(5),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                IsEnabled = false
            };
            Footer.Children.Add(Footer_Edit);
            ToolTip.SetTip(Footer_Edit, "Select a tileset to edit it.");

            Footer_Delete = new Button
            {
                Content = "Delete Tileset",
                Margin = new Avalonia.Thickness(5),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                IsEnabled = false
            };
            Footer.Children.Add(Footer_Delete);
            ToolTip.SetTip(Footer_Delete, "Select a tileset to delete it.");

        }

        private void RegisterFooterEvents()
        {


            Footer_New.Click += (sender, e) =>
            {
                // This should open a dialog and ask what type of tileset they want to create.
                // The choice could be either a basic tileset (from an image) or an autotile (from already existing tilesets).

                var newTilesetDialog = new NewTilesetDialog();
                newTilesetDialog.OnCreated += (args) =>
                {
                    if(args.TilesetType == 0) // Tileset
                    {
                        var tileset = new TilesetDef(string.Empty, args.Name, args.TileWidth, args.TileHeight)
                        {
                        };
                        var editor_control = new TilesetEditorWindowControl(tileset);
                        var host_ = ((AssetsManageWindow)this.GetVisualRoot()!);
                        editor_control.TilesetSaved += () =>
                        {
                            host_.ShowAssetsPanel("Tilesets");

                            TilesetsManageControl_OnNeedRefresh();
                        };

                        host_.OpenCustom(editor_control);
                    }
                };
                

                newTilesetDialog.ShowDialog((Window)this.GetVisualRoot()!);


                Console.WriteLine("New button clicked.");
            };

            Footer_Edit.Click += (sender, e) =>
            {

                if (SelectedTilesetViewItem == null)
                {
                    Console.WriteLine("No tileset selected to edit.");
                    return;
                }

                if (SelectedTilesetViewItem.TilesetDef == null)
                {
                    Console.WriteLine("Selected item is not a tileset.");
                    return;
                }
                
                if(SelectedTilesetViewItem.TilesetDef is TilesetDef tileset)
                {
                    TilesetEditorWindowControl editor_control =
                        new TilesetEditorWindowControl(tileset);

                    var host_ = ((AssetsManageWindow)this.GetVisualRoot()!);
                    editor_control.TilesetSaved += () =>
                    {
                        host_.ShowAssetsPanel("Tilesets");
                        TilesetsManageControl_OnNeedRefresh();
                    };
                    host_.OpenCustom(editor_control);

                    Console.WriteLine("Edit Tileset button clicked.");
                }
                else
                {
                    Console.WriteLine("Selected item is not a tileset.");
                }
            };

            Footer_Delete.Click += (sender, e) =>
            {
                if (SelectedTilesetViewItem == null)
                {
                    Console.WriteLine("No tileset selected to delete.");
                    return;
                }
                // Here you would implement the logic to delete the selected tileset.
                var tilesetBase = SelectedTilesetViewItem.TilesetDef;
                // if(tilesetBase is TilesetDef tileset)
                //     EngineCore.Instance.Managers.Assets.TilesetRegistry.Unregister(tileset);
                // else if(tilesetBase is AutoTilesetDef autotiles)
                //     EngineCore.Instance.Managers.Assets.TilesetRegistry.Unregister(autotiles);
                TilesetsManageControl_OnNeedRefresh();
                Console.WriteLine("Delete Tileset button clicked.");
            };

            OnSelectedTilesetChanged += () =>
            {
                if (SelectedTilesetViewItem != null)
                {
                    Footer_Edit.IsEnabled = true;
                    Footer_Delete.IsEnabled = true;
                }
                else
                {
                    Footer_Edit.IsEnabled = false;
                    Footer_Delete.IsEnabled = false;
                }
            };
        }
    }
}
