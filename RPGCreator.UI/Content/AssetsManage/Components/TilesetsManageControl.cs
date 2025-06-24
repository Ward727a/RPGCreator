#region LICENSE
//
// RPG Creator - Open-source RPG Engine.
// (c) 2025 Ward
// 
// This file is part of RPG Creator and is distributed under the MIT License.
// You are free to use, modify, and distribute this file under the terms of the MIT License.
// See LICENSE for details.
// 
// ---
// 
// Ce fichier fait partie de RPG Creator et est distribué sous licence MIT.
// Vous êtes libre de l'utiliser, de le modifier et de le distribuer sous les termes de la licence MIT.
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
using RPGCreator.Core;
using RPGCreator.Core.Type.Assets;
using RPGCreator.UI.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPGCreator.UI.Content.AssetsManage.Components
{

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
        public void Select();
        public void Deselect();
    }

    public class TilesetViewListItem : UserControl, ITilesetViewItem
    {
        public event Action? OnSelected;
        public event Action? OnDeselected;
        public Tileset Tileset { get; private set; }
        public bool IsSelected { get; private set; } = false;

        public Grid Body { get; private set; }

        public TilesetViewListItem(Tileset tileset)
        {
            Tileset = tileset;
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
                Source = Tileset.GetBitmap(),
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
                Text = Tileset.Name,
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
            ToolTip.SetTip(nameTextBlock, Tileset.Name);

            sep = new VSeparator();
            Body.Children.Add(sep);
            Grid.SetColumn(sep, 3);

            var imageSizeTextBlock = new TextBlock
            {
                Text = $"{Tileset.GetBitmap().Size.Width}x{Tileset.GetBitmap().Size.Width}",
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
                Text = $"{Tileset.tile_width}x{Tileset.tile_height}",
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
                Text = Tileset.ImagePath,
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
                    Console.WriteLine($"Editing Tileset: {Tileset.Name}");
                };
                contextMenu.Items.Add(editMenuItem);

                if (GlobalStaticUIData.CurrentContext != null)
                {
                    GlobalStaticUIData.CurrentContext.Close();
                }
                GlobalStaticUIData.CurrentContext = contextMenu;
                GlobalStaticUIData.CurrentContext.Open(this);

                Body.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromArgb(100, 0, 0, 0));
            }
            else if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                if (IsSelected)
                {
                    Console.WriteLine($"Deselected Tileset: {Tileset.Name}");
                    Body.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Transparent);
                    IsSelected = false;
                    OnDeselected?.Invoke();
                }
                else
                {
                    Console.WriteLine($"Selected Tileset: {Tileset.Name}");
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
                Console.WriteLine($"Selected Tileset: {Tileset.Name}");
                Body.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromArgb(50, 0, 0, 0));
                IsSelected = true;
            }
        }

        public void Deselect()
        {
            if (IsSelected)
            {
                Console.WriteLine($"Deselected Tileset: {Tileset.Name}");
                Body.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Colors.Transparent);
                IsSelected = false;
            }
        }
    }

    public class TilesetsManageControl : UserControl
    {

        #region Events

        public event Action? OnSelectedTilesetChanged;

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

        #endregion

        public TilesetsManageControl()
        {
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
                Content = "Group by Assets Pack",
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

            var tilesets = EngineCore.Instance.Data.EditedProject?.GetAssetsType<Tileset>(BaseAsset.TYPE.TILESETS) ?? new List<Tileset>();

            switch (Filters.ShowType)
            {
                case 0: // ListView
                    CreateListViewComponents(tilesets);
                    break;

                case 1: // GridView
                    CreateGridViewComponents(tilesets);
                    break;
            }
        }

        private void CreateListViewComponents(List<Tileset> tilesets)
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

            var viewPanel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Vertical,
                Margin = new Avalonia.Thickness(10),
            };

            viewGrid.Children.Add(viewPanel);

            foreach (var tileset in tilesets)
            {
                var item = new TilesetViewListItem(tileset);
                item.OnSelected += () =>
                {
                    SelectedTilesetViewItem = item;
                };
                item.OnDeselected += () =>
                {
                    if (SelectedTilesetViewItem == item)
                    {
                        SelectedTilesetViewItem = null;
                    }
                };
                viewPanel.Children.Add(item);
            }
        }

        private void CreateGridViewComponents(List<Tileset> tilesets)
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

            // TODO: Voir pour implementer la création de nouveaux tilesets.
            // Si j'ai le temps, faut que j'vois aussi pour le système d'autotiling (Création du nouveaux types, etc...)
            // Pour l'autotiling, je pourrait voir pour créé un type contenant une liste de tilesets utilisés pour l'autotiling,
            // Puis à chaque placement d'une tile, il vérifie si l'autotile le contient, et si oui, il place la tile correspondante.
            // OU
            // Voir pour implémenter un nouveau système dans le TilesetSelector qui permet de choisir un autotile,
            // l'utilisateur pourrait assigner des tiles "basique" sur une image généré depuis les datas de l'autotile, puis les placer,
            // Comme ça y'aura pas besoin de check chaque autotiles à chaque placement de tile,
            // Mais qu'un seul, celui que l'utilisateur a choisi.
            // A réfléchir, mais je pense que c'est la meilleure solution.
            //
            //
            // Pour l'instant je vais bossé sur la création de nouveaux tilesets,
            // Puis je créé le type d'autotile qui contiendra une liste de tilesets,
            // Et je créé un système de base de création d'autotile,
            // Je modifierai le TilesetSelector pour qu'il puisse afficher les autotiles et en séléctionner les tiles,
            // Et enfin je verrai pour implémenter le système de placement d'autotile.

            Footer_New.Click += (sender, e) =>
            {
                // This should open a dialog and ask what type of tileset they want to create.
                // The choice could be either a basic tileset (from an image) or an autotile (from already existing tilesets).
                Console.WriteLine("New button clicked.");
            };

            Footer_Edit.Click += (sender, e) =>
            {
                Console.WriteLine("Edit Tileset button clicked.");
            };

            Footer_Delete.Click += (sender, e) =>
            {
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
