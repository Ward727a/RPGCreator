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
using System.Runtime.Serialization.Formatters.Binary;
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
        public enum EAutotileTools
        {
            SELECT,
            ADD_TAG,
            REMOVE_TAG
        }

        public EAutotileTools SelectedTool => AutotileTools.SelectedIndex switch
        {
            0 => EAutotileTools.SELECT,
            1 => EAutotileTools.ADD_TAG,
            2 => EAutotileTools.REMOVE_TAG,
            _ => EAutotileTools.SELECT,
        };

        public string SelectedTag => AutotileTagsSelector.SelectedItem as string ?? string.Empty;

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
        public ComboBox AutotileTagsSelector;
        public Button AutotileTagsAdd;
        public Button AutotileTagsRemove;

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
        
        public StackPanel TagsPanel;
        public StackPanel TagsPanelTop;
        public TextSeparator TagsText;
        public ListBox TagsList;

        public Button OpenRuleEditorButton;
        
        
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
                RowDefinitions = new RowDefinitions("Auto, *, Auto"),
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

                AutotileTools = new ComboBox
                {
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    Width = 200,
                };
                TopBar.Children.Add(AutotileTools);
                AutotileTools.Items.Add("Select");
                AutotileTools.Items.Add("Add Tag");
                AutotileTools.Items.Add("Remove Tag");
                AutotileTools.SelectedIndex = 0;
                AutotileTools.SelectionChanged += AutotileTools_SelectionChanged;
                
                // Add a separator
                TopBar.Children.Add(new VSeparator
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(5, 0, 5, 0),
                });
                
                // Add a selector for tags
                AutotileTagsSelector = new ComboBox()
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                    Width = 200,
                    IsVisible = false,
                };
                TopBar.Children.Add(AutotileTagsSelector);
                AutotileTagsSelector.Items.Add("Tag 1");
                AutotileTagsSelector.Items.Add("Tag 2");
                AutotileTagsSelector.Items.Add("Tag 3");
                AutotileTagsSelector.SelectedIndex = 0;
                #if DEBUG // For debugging purposes, we log the selected tag
                AutotileTagsSelector.SelectionChanged += AutotileTagsSelector_SelectionChanged;
                #endif
                AutotileTagsAdd = new Button()
                {
                    Content = "New tag",
                    IsVisible = false
                };
                TopBar.Children.Add(AutotileTagsAdd);
                
                AutotileTagsRemove = new Button()
                {
                    Content = "Remove tag",
                    IsVisible = false
                };
                TopBar.Children.Add(AutotileTagsRemove);

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
                    
                    TagsText = new TextSeparator()
                    {
                        Text = "Tile tags",
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Top,
                        Margin = new Thickness(5),
                    };
                    TagsPanelTop.Children.Add(TagsText);
                    
                    TagsList = new ListBox()
                    {
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        Margin = new Thickness(5),
                        Height = 100,
                    };
                    TagsPanel.Children.Add(TagsList);
                    
                    OpenRuleEditorButton = new Button()
                    {
                        Content = "Open Rule Editor...",
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(5),
                    };
                    CenterBottomPanel.Children.Add(OpenRuleEditorButton);
                    
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
            
            // Bottom bar
            {
                var BottomBar = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Margin = new Thickness(10),
                };
                MainBody.Children.Add(BottomBar);
                Grid.SetRow(BottomBar, 2);
                
                var saveButton = new Button
                {
                    Content = "Save",
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(5),
                };
                BottomBar.Children.Add(saveButton);
                saveButton.Click += (s, e) =>
                {
                    if (SelectedTileset == null || SelectedGroup == null)
                        return;
                    
                    // Save the autotiles
                    EngineSerializer.Instance.Serialize(SelectedGroup, out var data);
                };
            }

        }

        #if DEBUG
        private void AutotileTagsSelector_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            
            Console.WriteLine($"Selected tag: {SelectedTag}");
            
        }
        #endif

        private void AutotileTools_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            switch (AutotileTools.SelectedIndex)
            {
                case 0:
                {
                    // Hide the tag selector
                    AutotileTagsSelector.IsVisible = false;
                    AutotileTagsAdd.IsVisible = false;
                    AutotileTagsRemove.IsVisible = false;
                }
                break;
                case 1:
                case 2:
                {
                    // Show the tag selector
                    AutotileTagsSelector.IsVisible = true;
                    AutotileTagsAdd.IsVisible = true;
                    AutotileTagsRemove.IsVisible = true;
                }
                break;
            }
        }

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

            switch (SelectedTool)
            {
                case EAutotileTools.SELECT:
                {
                    if (e.GetCurrentPoint(CenterSubCanvas).Properties.IsLeftButtonPressed)
                    {
                        // Add or select tile in group
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
                        // Remove tile from group
                        Console.WriteLine($"Tile right clicked at grid position: {tileX}, {tileY}");
                        if (SelectedGroup.HasTileAt(new Point(tileX, tileY)))
                        {
                            SelectedGroup.RemoveTile(SelectedGroup.GetTileByPosition(new Point(tileX, tileY)));
                        }
                        RefreshProperties();
                    } 
                    else if (e.GetCurrentPoint(CenterSubCanvas).Properties.IsMiddleButtonPressed)
                    {
                        // Set base tile
                        Console.WriteLine($"Tile middle clicked at grid position: {tileX}, {tileY}");
                        if (!SelectedGroup.HasTileAt(new Point(tileX, tileY)))
                            return;
                        
                        var tiling = SelectedGroup.GetTileByPosition(new Point(tileX, tileY));

                        if (tiling == null)
                            return;
                        
                        SelectedGroup.SetBase(tiling);
                        RefreshProperties();
                    }
                }
                break;
                case EAutotileTools.ADD_TAG:
                {
                    // Add tag to autotile
                    if (e.GetCurrentPoint(CenterSubCanvas).Properties.IsLeftButtonPressed)
                    {
                        if (SelectedGroup != null)
                        {
                            var tiling = SelectedGroup.GetTileByPosition(new Core.Type.Internal.Point(tileX, tileY));

                            if (tiling == null)
                                return;

                            if (SelectedGroup.Tags.Contains(SelectedTag))
                                return;

                            if (tiling.Tags.Contains(SelectedTag))
                                return;
                            
                            tiling.Tags.Add(SelectedTag);

                            if (!SelectedGroup.PresentTags.TryGetValue(SelectedTag, out List<Ulid>? value))
                                SelectedGroup.PresentTags.Add(SelectedTag, [tiling.ID]);
                            else
                                value.Add(tiling.ID);
                            
                            if(tiling == SelectedAutotiling)
                                RefreshProperties();
                        }
                    }
                }
                break;
                case EAutotileTools.REMOVE_TAG:
                {
                    // Remove tag from autotile
                    if (!e.GetCurrentPoint(CenterSubCanvas).Properties.IsLeftButtonPressed) return;

                    if (SelectedGroup == null)
                        return;
                    
                    var tiling = SelectedGroup.GetTileByPosition(new Core.Type.Internal.Point(tileX, tileY));

                    if (tiling == null)
                        return;

                    if (!tiling.Tags.Contains(SelectedTag))
                        return;

                    tiling.Tags.Remove(SelectedTag);

                    if (!SelectedGroup.PresentTags.ContainsKey(SelectedTag)) return;
                    
                    SelectedGroup.PresentTags[SelectedTag].Remove(tiling.ID);
                    
                    if (SelectedGroup.PresentTags[SelectedTag].Count == 0)
                        SelectedGroup.PresentTags.Remove(SelectedTag);
                    
                    if(tiling == SelectedAutotiling)
                        RefreshProperties();
                }
                break;
            }
        }

        private void RefreshAutotileCombo()
        {
            AutotileComboBox.Items.Clear();
            if (SelectedTileset == null)
                return;
            SelectedTileset.Groups.ForEach(group =>
            {
                AutotileComboBox.Items.Add(group.Name);
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

        private void RefreshProperties()
        {
            if (SelectedAutotiling == null || SelectedGroup == null)
                return;
            
            GroupTagsTagsList.Items.Clear();
            SelectedGroup.Tags.ForEach(tag =>
            {
                GroupTagsTagsList.Items.Add(tag);
            });
            
            TagsList.Items.Clear();
            SelectedAutotiling.Tags.ForEach(tag =>
            {
                TagsList.Items.Add(tag);
            });

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
                SelectedTileset.Groups.Add(new AutotilesGroup($"New Group - {AutotileComboBox.Items.Count}", SelectedTileset));
                RefreshAutotileCombo();
                AutotileComboBox.SelectedIndex = AutotileComboBox.Items.Count - 1;
            };
            AutotileComboBox.SelectionChanged += (s, e) =>
            {
                if (AutotileComboBox.SelectedIndex >= 0 && AutotileComboBox.SelectedIndex < SelectedTileset.Groups.Count)
                {
                    // Load the selected group
                    SelectedGroup = SelectedTileset.Groups[AutotileComboBox.SelectedIndex];
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

            AutotileTagsRemove.Click += (s, e) =>
            {
                if (SelectedGroup == null)
                    return;

                if (string.IsNullOrWhiteSpace(SelectedTag))
                    return;

                if (SelectedGroup.PresentTags.TryGetValue(SelectedTag, out var list))
                {
                    foreach (var ulid in list)
                    {
                        SelectedGroup.GetTileById(ulid)?.Tags.Remove(SelectedTag);
                    }
                    SelectedGroup.PresentTags.Remove(SelectedTag);
                }

                if (AutotileTagsSelector.Items.Contains(SelectedTag))
                {
                    var shouldMoveSelection = (string)AutotileTagsSelector.SelectedItem! == SelectedTag;
                        
                    AutotileTagsSelector.Items.Remove(SelectedTag);

                    if (shouldMoveSelection)
                    {
                        AutotileTagsSelector.SelectedIndex = 0;
                    }
                }
                
                RefreshProperties();
            };
            
            AutotileTagsAdd.Click += (s, e) =>
            {
                if (SelectedGroup == null)
                    return;

                // Open a dialog to add a new tag
                var dialog = new TextInputDialog("New Tag", "Enter the name of the new tag:", allowEmpty: false);
                dialog.Confirmed += (tag) =>
                {
                    if (SelectedGroup.Tags.Contains(tag))
                        return;
                    
                    AutotileTagsSelector.Items.Add(tag);
                    AutotileTagsSelector.SelectedItem = tag;
                    RefreshProperties();
                };
                
                dialog.ShowDialog((Window)this.GetVisualRoot()!);
            };

            OpenRuleEditorButton.Click += (s, e) =>
            {
                Console.WriteLine("Opening Rule Editor...");
                if (SelectedGroup == null)
                {
                    Console.WriteLine("No group selected.");
                    return;
                }
                var ruleEditor = new AutotileEditorRuleEditorWindow(SelectedTileset);
                
                ruleEditor.ShowDialog((Window)this.GetVisualRoot()!);
            };

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
