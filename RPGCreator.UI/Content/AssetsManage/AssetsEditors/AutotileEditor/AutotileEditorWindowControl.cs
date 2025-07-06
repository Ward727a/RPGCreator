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
using Avalonia.Media;
using Avalonia.Media.Imaging;
using RPGCreator.Core;
using RPGCreator.Core.Type.Assets;
using RPGCreator.UI.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Layout;
using Avalonia.VisualTree;
using RPGCreator.UI.Common.Windows;
using Point = RPGCreator.Core.Type.Internal.Point;

namespace RPGCreator.UI.Content.AssetsManage.AssetsEditors.AutotileEditor
{
    public class AutotileEditorWindowControl : UserControl
    {
        private Autotiling BasedOn;
        public Autotiling? SelectedAutotiling;
        public Tileset? SelectedTileset;
        public AutotilesGroup? SelectedGroup { get; private set; }
        public Autotiles Autotile;

        // For now, the tools are not used, but they can be implemented later if needed.
        //public enum EAutotileTools
        //{
        //    SELECT,
        //    ADD_RULE,
        //    REMOVE_RULE
        //}

        //public EAutotileTools SelectedTool => AutotileTools.SelectedIndex switch
        //{
        //    0 => EAutotileTools.SELECT,
        //    1 => EAutotileTools.ADD_RULE,
        //    2 => EAutotileTools.REMOVE_RULE,
        //    _ => EAutotileTools.SELECT,
        //};

        #region Components
        public Grid Body;

        #region Left Bar

        public StackPanel LeftBar;

        #region Preview
        public ClosableBox LeftPreviewBox;
        public StackPanel LeftPreview;
        public Image TestPreview;

        public ComboBox AutotileTagPreview;
        #endregion

        #region Properties

        public ClosableBox GroupTagsBox;
        public Grid GroupTagsGrid;
        public ScrollViewer GroupTagsScroller;
        public StackPanel GroupTagsPanel;

        public Button GroupTagsAddButn;
        public ListBox GroupTagsTagsList;

        #endregion

        #endregion

        #region Main Body

        public Grid MainBody;

        #region Top Bar

        public StackPanel TopBar;

        public Button AddAutotileButton;
        public ComboBox AutotileComboBox;

        public ComboBox AutotileTools;

        #endregion

        #region Body Content

        public Grid BodyContentGrid;

        #region Center

        public Grid CenterGrid;

        public Canvas CenterMainCanvas;
        public Canvas CenterSubCanvas;
        public Image CenterTilesetImage;

        public Border? BaseTileCase;
        public List<Border> BasedOnTilesCases = [];
        public Border? SelectedTileCase;
        
        #region Bottom Part
        
        public StackPanel CenterBottomPanel;
        
        public StackPanel BasePropertiesPanel;
        public TextBlock BasePropertiesText;
        public CheckBox BasePropertiesChecker;
        public TextBlock BasedOnPropertiesText;
        public ComboBox BasedOnPropertiesCombo;
        public StackPanel TagsPanel;
        public StackPanel TagsPanelTop;
        public TextBlock TagsText;
        public Button AddTagButton;
        
        #endregion
        
        #endregion

        #region TilesetSelectorRight

        public ScrollViewer TilesetSelectorScroller;
        public Grid TilesetSelectorGrid;
        public StackPanel TilesetSelectorPanel;

        #endregion

        #endregion

        #endregion

        #endregion

        public AutotileEditorWindowControl(Autotiles autotiles)
        {
            Autotile = autotiles;
            CreateComponents();

            this.Content = Body;
        }

        public void CreateComponents()
        {
            Body = new Grid
            {
                ColumnDefinitions = new ColumnDefinitions("250, *"),
            };

            CreateLeftBar();
            CreateMainBody();

            RegisterEvents();
        }

        private void CreateLeftBar()
        {
            LeftBar = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Margin = new Thickness(10),
            };
            Body.Children.Add(LeftBar);
            Grid.SetColumn(LeftBar, 0);

            CreatePreview();
            CreateProperties();
        }

        private void CreatePreview()
        {
            LeftPreview = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(10),
            };

            LeftPreviewBox = new ClosableBox(LeftPreview, "Preview");

            LeftBar.Children.Add(LeftPreviewBox);

            TestPreview = new Image
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Margin = new Thickness(5),
                Width = 200,
                Height = 200,
            };
            // Remove AA from test preview
            RenderOptions.SetBitmapInterpolationMode(TestPreview, BitmapInterpolationMode.None);
            LeftPreview.Children.Add(TestPreview);

            AutotileTagPreview = new ComboBox
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(5),
            };
            LeftPreview.Children.Add(AutotileTagPreview);
            AutotileTagPreview.Items.Add("Water");
            AutotileTagPreview.Items.Add("Grass");
            AutotileTagPreview.Items.Add("Road");
            AutotileTagPreview.SelectedIndex = 0;

            LeftPreview.Children.Add(new Separator());
        }

        private void CreateProperties()
        {
            GroupTagsGrid = new Grid
            {
                Margin = new Thickness(10),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };

            GroupTagsBox = new ClosableBox(GroupTagsGrid, "Group Tags");
            LeftBar.Children.Add(GroupTagsBox);
            GroupTagsScroller = new ScrollViewer
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
            GroupTagsGrid.Children.Add(GroupTagsScroller);
            GroupTagsPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
            GroupTagsScroller.Content = GroupTagsPanel;

            GroupTagsAddButn = new Button
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(5),
                Content = "New tag...",
            };
            GroupTagsPanel.Children.Add(GroupTagsAddButn);

            GroupTagsPanel.Children.Add(new TextSeparator { Text = "Tags"});
            GroupTagsTagsList = new ListBox
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Margin = new Thickness(5),
                Height = 100,
            };
            GroupTagsPanel.Children.Add(GroupTagsTagsList);

        }

        private void CreateMainBody()
        {
            MainBody = new Grid
            {
                RowDefinitions = new RowDefinitions("Auto, *"),
                Margin = new Thickness(10),
            };
            Body.Children.Add(MainBody);
            Grid.SetColumn(MainBody, 1);

            // TOP BAR
            {
                TopBar = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Top,
                };
                MainBody.Children.Add(TopBar);

                AddAutotileButton = new Button
                {
                    Content = "Add Group",
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                TopBar.Children.Add(AddAutotileButton);

                AutotileComboBox = new ComboBox
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                    Width = 200,
                };
                TopBar.Children.Add(AutotileComboBox);
                RefreshAutotileCombo();
                AutotileComboBox.SelectedIndex = 0;

                //AutotileTools = new ComboBox
                //{
                //    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                //    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                //    Width = 200,
                //};
                //TopBar.Children.Add(AutotileTools);
                //AutotileTools.Items.Add("Select");
                //AutotileTools.Items.Add("Add Rule");
                //AutotileTools.Items.Add("Remove Rule");
                //AutotileTools.SelectedIndex = 0;
                //AutotileTools.SelectionChanged += AutotileTools_SelectionChanged;

            }

            // BodyContent
            {
                BodyContentGrid = new Grid
                {
                    Margin = new Thickness(10),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    ColumnDefinitions = new ColumnDefinitions("*, Auto"),
                };
                MainBody.Children.Add(BodyContentGrid);
                Grid.SetRow(BodyContentGrid, 1);

                // Center
                {
                    CenterGrid = new Grid
                    {
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        Margin = new Thickness(10),
                        RowDefinitions = new RowDefinitions("*, Auto")
                    };
                    BodyContentGrid.Children.Add(CenterGrid);
                    Grid.SetColumn(CenterGrid, 0);

                    CenterMainCanvas = new Canvas
                    {
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                    };
                    CenterGrid.Children.Add(CenterMainCanvas);

                    CenterSubCanvas = new Canvas
                    {
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                    };
                    CenterMainCanvas.Children.Add(CenterSubCanvas);

                    CenterSubCanvas.PointerPressed += CenterSubCanvas_PointerPressed;

                    CenterTilesetImage = new Image
                    {
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                    };
                    CenterSubCanvas.Children.Add(CenterTilesetImage);
                    
                    // Center - bottom part (For autotile properties modification)
                    #region CenterBottomPart
                    CenterBottomPanel = new StackPanel()
                    {
                        Orientation = Orientation.Horizontal,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Bottom,
                        Margin = new Thickness(10),
                        Spacing = 5
                    };
                    CenterGrid.Children.Add(CenterBottomPanel);
                    Grid.SetRow(CenterBottomPanel, 1);
                    
                    BasePropertiesPanel = new StackPanel()
                    {
                        Orientation = Orientation.Vertical,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        Margin = new Thickness(10),
                    };
                    CenterBottomPanel.Children.Add(BasePropertiesPanel);
                    
                    BasePropertiesText = new TextBlock()
                    {
                        Text = "Base Properties",
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top,
                        Margin = new Thickness(5),
                    };
                    BasePropertiesPanel.Children.Add(BasePropertiesText);
                    BasePropertiesChecker = new CheckBox()
                    {
                        Content = "Is Base",
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top,
                        Margin = new Thickness(5),
                    };

                    BasePropertiesPanel.Children.Add(BasePropertiesChecker);
                    
                    BasedOnPropertiesText = new TextBlock()
                    {
                        Text = "Based On",
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top,
                        Margin = new Thickness(5),
                    };
                    
                    BasePropertiesPanel.Children.Add(BasedOnPropertiesText);
                    
                    BasedOnPropertiesCombo = new ComboBox()
                    {
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Top,
                        Margin = new Thickness(5),
                    };
                    BasePropertiesPanel.Children.Add(BasedOnPropertiesCombo);
                    BasedOnPropertiesCombo.Items.Add("None");
                    BasedOnPropertiesCombo.Items.Add("Autotile 1");
                    BasedOnPropertiesCombo.Items.Add("Autotile 2");
                    BasedOnPropertiesCombo.SelectedIndex = 0;

                    var separatorBaseToTags = new VSeparator();
                    
                    CenterBottomPanel.Children.Add(separatorBaseToTags);
                    
                    TagsPanel = new StackPanel()
                    {
                        Orientation = Orientation.Vertical,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        Margin = new Thickness(10),
                    };
                    CenterBottomPanel.Children.Add(TagsPanel);
                    
                    TagsPanelTop = new StackPanel()
                    {
                        Orientation = Orientation.Horizontal,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Top,
                        Margin = new Thickness(5),
                    };
                    TagsPanel.Children.Add(TagsPanelTop);
                    
                    TagsText = new TextBlock()
                    {
                        Text = "Tags",
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top,
                        Margin = new Thickness(5),
                    };
                    TagsPanelTop.Children.Add(TagsText);
                    
                    AddTagButton = new Button()
                    {
                        Content = "Add Tag",
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Top,
                        Margin = new Thickness(5),
                    };
                    TagsPanelTop.Children.Add(AddTagButton);
                    #endregion
                }

                // TilesetSelectorRight
                {
                    TilesetSelectorScroller = new ScrollViewer
                    {
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                    };

                    BodyContentGrid.Children.Add(TilesetSelectorScroller);
                    Grid.SetColumn(TilesetSelectorScroller, 1);

                    TilesetSelectorGrid = new Grid
                    {
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        Margin = new Thickness(10),
                    };
                    TilesetSelectorScroller.Content = TilesetSelectorGrid;

                    TilesetSelectorPanel = new StackPanel
                    {
                        Width = 200,
                        Orientation = Orientation.Vertical,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        Margin = new Thickness(10),
                    };
                    TilesetSelectorGrid.Children.Add(TilesetSelectorPanel);

                    RefreshTilesetSelectorList();
                }

            }

        }

        //private void AutotileTools_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        //{

        //}


        /*
         * 
         * Je dois voir pour commencer à bosser sur comment faire en sorte que l'utilisteur puisse modifier les propriétés d'un autotile.
         * Cela inclut :
         * - Sélectionner un autotile dans la liste (Fait)
         * - Afficher les propriétés de l'autotile sélectionné (Fait)
         * - Définir si l'autotile est une base, ou basé sur une base. Il faut que ça soit l'un ou l'autre, pas les deux en même temps. (Fait)
         * - Ajouter des tags à l'autotile sélectionné | Via un outil peut-être ? Genre outil "Edit tag" qui permet de sélectionner un tag, puis de cliquer sur un tile pour ajouter / retirer le tag.
         * - Ajouter des règles à l'autotile sélectionné | Si c'est une base alors aucune règle ne peut être ajoutée, si c'est basé sur une base ainsi, on peut ajouter des règles, si c'est aucun des deux on ne peut rien faire.
         *
         * Afin de faire ça, il faudrait voir pour l'interface, là rendre plus "user friendly" et intuitive. Ou en tout cas, essayé de faire le minimum déjà.
         */

        private void CenterSubCanvas_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            if (SelectedTileset == null || SelectedGroup == null)
                return;
            
            Console.WriteLine($"Tileset clicked: {SelectedTileset.Name}");
            
            // Print location of the click
            var position = e.GetPosition(CenterSubCanvas);
            Console.WriteLine($"Click at position: {position.X}, {position.Y}");
            
            var tileWidth = SelectedTileset.tile_width;
            var tileHeight = SelectedTileset.tile_height;
            var tileX = (int)(position.X / tileWidth);
            var tileY = (int)(position.Y / tileHeight);
            
            if (e.GetCurrentPoint(CenterSubCanvas).Properties.IsLeftButtonPressed)
            {
                Console.WriteLine($"Tile left clicked at grid position: {tileX}, {tileY}");

                TestPreview.Source = new CroppedBitmap(SelectedTileset.GetBitmap(), new PixelRect(tileX * tileWidth, tileY * tileHeight, tileWidth, tileHeight));
                if (!SelectedGroup.HasTileAt(new Core.Type.Internal.Point(tileX, tileY)))
                { 
                    SelectedAutotiling = new Autotiling
                    {
                        TilePosition = new Core.Type.Internal.Point(tileX, tileY),
                        TilesetID = SelectedTileset.Unique
                    };
                    SelectedGroup.AddTile(SelectedAutotiling);
                    Console.WriteLine($"New autotile created at: {tileX}, {tileY}");
                }
                else
                {
                    SelectedAutotiling = SelectedGroup.GetTileByPosition(new Core.Type.Internal.Point(tileX, tileY))!;
                    Console.WriteLine($"Autotile already exists at: {tileX}, {tileY}");
                }

                RefreshProperties();
            }
            else if (e.GetCurrentPoint(CenterSubCanvas).Properties.IsRightButtonPressed)
            {
                Console.WriteLine($"Tile right clicked at grid position: {tileX}, {tileY}");
                if (SelectedGroup.HasTileAt(new Point(tileX, tileY)))
                {
                    SelectedGroup.RemoveTile(SelectedGroup.GetTileByPosition(new Point(tileX, tileY)));
                }
                RefreshProperties();
            }
        }

        private void RefreshAutotileCombo()
        {
            AutotileComboBox.Items.Clear();
            Autotile.Autotilings.ForEach(autotile =>
            {
                AutotileComboBox.Items.Add(autotile.Name);
            });
        }

        private void RefreshTilesetSelectorList()
        {
            EngineCore.Instance.Data.EditedProject?.GetAssetsType<Tileset>(BaseAsset.TYPE.TILESETS)
                .ForEach(tileset =>
                {
                    var item = new AutotileTilesetItem(tileset);
                    item.TilesetSelected += () =>
                    {
                        Console.WriteLine($"Tileset selected: {tileset.Name}");
                        CenterTilesetImage.Source = tileset.GetBitmap();
                        SelectedTileset = tileset;
                    };
                    TilesetSelectorPanel.Children.Add(item);
                });
        }

        private void RefreshProperties(bool refreshIsBase = true, bool refreshBasedOn = true)
        {
            if (SelectedAutotiling == null || SelectedGroup == null)
                return;
            
            GroupTagsTagsList.Items.Clear();
            SelectedGroup.Tags.ForEach(tag =>
            {
                GroupTagsTagsList.Items.Add(tag);
            });

            if(refreshIsBase)
            {
                BasePropertiesChecker.IsCheckedChanged -= OnBaseChecked;
                
                BasePropertiesChecker.IsChecked = SelectedAutotiling.IsBase;

                BasePropertiesChecker.IsCheckedChanged += OnBaseChecked;
            }

            if (SelectedGroup.HasBaseTile() && !SelectedAutotiling.IsBase)
            {
                var basePosition = SelectedGroup.BaseTile.TilePosition;

                DrawBaseCase(basePosition);
            }
            else
            {
                RemoveBaseCase();
            }
            
            ClearBasedOnTilesCases();
            // Get all autotiles based on the base autotile
            foreach (var tiling in SelectedGroup.Tilings)
            {
                if(tiling != SelectedGroup.BaseTile && tiling != SelectedAutotiling)
                    AddBasedOnTileCase(tiling.TilePosition);
            }
            
            // Get the position of the selected autotile
            DrawSelectedTileCase(SelectedAutotiling.TilePosition);
        }

        public void RegisterEvents()
        {
            AddAutotileButton.Click += (s, e) =>
            {
                Autotile.Autotilings.Add(new AutotilesGroup($"New Group - {AutotileComboBox.Items.Count}", SelectedTileset));
                RefreshAutotileCombo();
                AutotileComboBox.SelectedIndex = AutotileComboBox.Items.Count - 1;
            };
            AutotileComboBox.SelectionChanged += (s, e) =>
            {
                if (AutotileComboBox.SelectedIndex >= 0 && AutotileComboBox.SelectedIndex < Autotile.Autotilings.Count)
                {
                    // Load the selected group
                    SelectedGroup = Autotile.Autotilings[AutotileComboBox.SelectedIndex];
                    RefreshProperties();
                }
            };
            
            GroupTagsAddButn.Click += (s, e) =>
            {
                if (SelectedGroup == null)
                    return;
                // Open a dialog to add a new tag
                var dialog = new TextInputDialog("New Tag", "Enter the name of the new tag:", allowEmpty: false);
                dialog.Confirmed += (tag) =>
                {
                    if (SelectedGroup.Tags.Contains(tag))
                        return;
                    
                    SelectedGroup.Tags.Add(tag);
                    RefreshProperties();
                };
                dialog.ShowDialog((Window)this.GetVisualRoot()!);
            };
            
        }

        private void OnBaseChecked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (SelectedAutotiling == null)
                return;
            if (SelectedAutotiling.IsBase || SelectedGroup.HasBaseTile())
                return;
            
            SelectedAutotiling.IsBase = BasePropertiesChecker.IsChecked ?? false;
            SelectedGroup.SetBase(SelectedAutotiling);
            RefreshProperties();
        }

        private void RemoveBaseCase()
        {
            if (BaseTileCase == null)
                return;
            
            CenterSubCanvas.Children.Remove(BaseTileCase);
            BaseTileCase = null;
        }
        private void DrawBaseCase(Core.Type.Internal.Point? at)
        {
            if (BaseTileCase == null)
            {
                BaseTileCase = new Border()
                {
                    // Draw a red background, with 30% opacity
                    Background = new SolidColorBrush(Color.FromArgb(76, 255, 0, 0)),
                };
                CenterSubCanvas.Children.Add(BaseTileCase);
            }
            BaseTileCase.Width = SelectedTileset.tile_width;
            BaseTileCase.Height = SelectedTileset.tile_height;
            
            Canvas.SetLeft(BaseTileCase, at.Value.X * SelectedTileset.tile_width);
            Canvas.SetTop(BaseTileCase, at.Value.Y * SelectedTileset.tile_height);
        }
        
        private void ClearBasedOnTilesCases()
        {
            foreach (var border in BasedOnTilesCases)
            {
                CenterSubCanvas.Children.Remove(border);
            }
            BasedOnTilesCases.Clear();
        }

        private void AddBasedOnTileCase(Core.Type.Internal.Point? at)
        {
            var border = new Border()
            {
                // Draw a green background, with 30% opacity
                Background = new SolidColorBrush(Color.FromArgb(76, 0, 255, 0)),
            };
            CenterSubCanvas.Children.Add(border);
            BasedOnTilesCases.Add(border);

            border.Width = SelectedTileset.tile_width;
            border.Height = SelectedTileset.tile_height;

            Canvas.SetLeft(border, at.Value.X * SelectedTileset.tile_width);
            Canvas.SetTop(border, at.Value.Y * SelectedTileset.tile_height);
        }

        private void DrawSelectedTileCase(Core.Type.Internal.Point? at)
        {
            if (SelectedTileCase == null)
            {
                SelectedTileCase = new Border()
                {
                    // Draw a cyan background, with 30% opacity
                    Background = new SolidColorBrush(Color.FromArgb(76, 0, 255, 255)),
                };
                CenterSubCanvas.Children.Add(SelectedTileCase);
            }

            SelectedTileCase.Width = SelectedTileset.tile_width;
            SelectedTileCase.Height = SelectedTileset.tile_height;

            Canvas.SetLeft(SelectedTileCase, at.Value.X * SelectedTileset.tile_width);
            Canvas.SetTop(SelectedTileCase, at.Value.Y * SelectedTileset.tile_height);
        }
    }
}
