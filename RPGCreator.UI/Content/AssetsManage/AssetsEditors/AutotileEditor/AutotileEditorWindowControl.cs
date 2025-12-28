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
using RPGCreator.Core.Types.Assets;
using RPGCreator.Core.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.VisualTree;
using RPGCreator.Core.Managers.AssetsManager;
using RPGCreator.Core.Types.Windows;
using Point = RPGCreator.Core.Types.Internal.Point;

namespace RPGCreator.UI.Content.AssetsManage.AssetsEditors.AutotileEditor
{
    public class AutotileEditorWindowControl : UserControl
    {

        public enum EAutotileTools
        {
            SELECT,
            ADD_TAG,
            REMOVE_TAG
        }
        
        private string TestSavedData;

        public event Action? AutotileSaved;
        
        private AssetScope _scope;
        
        public AutotileDef? SelectedAutotiling;
        public TilesetDef? SelectedTileset;
        public AutoTilesetDef AutoTilesetInstance;
        public AutotileGroupDef? SelectedGroup { get; private set; }
        public AutotileDef AutotileInstance;

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
        public Accordion LeftPreviewBox;
        public StackPanel LeftPreview;
        public Image TestPreview;

        public ComboBox AutotileTagPreview;
        #endregion

        #region Properties

        public Accordion GroupTagsBox;
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

        public Button AddGroupButton;
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

        public AutotileEditorWindowControl(AutoTilesetDef tilesetInstance)
        {
            _scope = EngineCore.Instance.Managers.Assets.CreateAssetScope("autotile_editor_scope");
            AutoTilesetInstance = tilesetInstance;
            CreateComponents();

            this.Content = Body;
        }
        
        #region UI Methods

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

            LeftPreviewBox = new Accordion(LeftPreview, "Preview");

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

            GroupTagsBox = new Accordion(GroupTagsGrid, "GroupDefinition Tags");
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

                AddGroupButton = new Button
                {
                    Content = "Add GroupDefinition",
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                TopBar.Children.Add(AddGroupButton);

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
                
                // Add a test button to generate preview image
                var testButton = new Button
                {
                    Content = "Generate Preview",
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(5),
                };
                TopBar.Children.Add(testButton);
                testButton.Click += (s, e) =>
                {
                    if (SelectedTileset == null || SelectedGroup == null)
                        return;

                    SelectedTileset.GetBitmap();
                    // SelectedTileset.Save();
                };

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
                saveButton.Click += OnSaveButtonOnClick;
            }

        }
        
        #endregion

        private void OnSaveButtonOnClick(object? s, RoutedEventArgs e)
        {
            var pack = AutoTilesetInstance.Pack;
            pack.AddOrUpdateAsset(AutoTilesetInstance);
            // Check if the tileset already exists in the pack
            // if (pack != null)
            // {
            //     if (pack.AssetsCache.Any(a => a.Value.Unique == AutoTilesetInstance.Unique))
            //     {
            //         // If it exists, update the existing tileset
            //         pack.UpdateAsset(AutoTilesetInstance);
            //         AutoTilesetInstance.GetBitmap(true);
            //         Console.WriteLine($"Autotile Updated: {AutoTilesetInstance.Name}, Width: {AutoTilesetInstance.ImageWidth}, Height: {AutoTilesetInstance.ImageHeight}, Asset Pack: {AutoTilesetInstance.PackName}");
            //     }
            //     else
            //     {
            //         // If it doesn't exist, add the new tileset to the pack
            //         pack.AddAsset(AutoTilesetInstance);
            //         Console.WriteLine($"Autotile Added: {AutoTilesetInstance.Name}, Width: {AutoTilesetInstance.ImageWidth}, Height: {AutoTilesetInstance.ImageHeight}, Asset Pack: {AutoTilesetInstance.PackName}");
            //     }
            // }

            Console.WriteLine($"New Autotile Created: {AutoTilesetInstance.Name}, Width: {AutoTilesetInstance.ImageWidth}, Height: {AutoTilesetInstance.ImageHeight}, Asset Pack: {AutoTilesetInstance.Pack.Name}");

            AutotileSaved?.Invoke();
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
            
            var tileWidth = SelectedTileset.TileWidth;
            var tileHeight = SelectedTileset.TileHeight;
            var tileX = (int)(position.X / tileWidth);
            var tileY = (int)(position.Y / tileHeight);
            
            var tilePosition = new Point(tileX, tileY);

            switch (SelectedTool)
            {
                case EAutotileTools.SELECT:
                {
                    if (e.GetCurrentPoint(CenterSubCanvas).Properties.IsLeftButtonPressed)
                    {
                        // Add or select tile in groupInstance
                        Console.WriteLine($"Tile left clicked at grid position: {tileX}, {tileY}");

                        TestPreview.Source = new CroppedBitmap(SelectedTileset.GetBitmap(), new PixelRect(tileX * tileWidth, tileY * tileHeight, tileWidth, tileHeight));
                        if (!SelectedGroup.HasTile(tilePosition))
                        {

                            var tileUv = new Point(tileWidth, tileHeight);
                            
                            SelectedAutotiling = new AutotileDef(
                                SelectedTileset.Unique,
                                tileUv,
                                tilePosition, 
                                SelectedGroup);
                            SelectedGroup.AddTile(SelectedAutotiling);
                            Console.WriteLine($"New autotile created at: {tileX}, {tileY}");
                        }
                        else
                        {
                            SelectedAutotiling = SelectedGroup.GetDirectTileAt(tilePosition);
                            Console.WriteLine($"Autotile already exists at: {tileX}, {tileY}");
                        }

                        RefreshProperties();
                    }
                    else if (e.GetCurrentPoint(CenterSubCanvas).Properties.IsRightButtonPressed)
                    {
                        // Remove tile from groupInstance
                        Console.WriteLine($"Tile right clicked at grid position: {tileX}, {tileY}");
                        if (SelectedGroup.HasTile(tilePosition))
                        {
                            SelectedGroup.RemoveTile(position);
                        }
                        RefreshProperties();
                    } 
                    else if (e.GetCurrentPoint(CenterSubCanvas).Properties.IsMiddleButtonPressed)
                    {
                        // Set base tile
                        Console.WriteLine($"Tile middle clicked at grid position: {tileX}, {tileY}");
                        if (!SelectedGroup.HasTile(tilePosition))
                            return;
                        
                        var tiling = SelectedGroup.GetDirectTileAt(tilePosition);

                        if (tiling == null)
                            return;
                        
                        SelectedGroup.SetBaseTile(tiling);
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
                            var tiling = SelectedGroup.GetDirectTileAt(tilePosition);

                            if (tiling == null)
                                return;

                            if (SelectedGroup.Tags.Contains(SelectedTag))
                                return;

                            if (tiling.Tags.Contains(SelectedTag))
                                return;
                            
                            tiling.Tags.Add(SelectedTag);
                            
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
                    
                    var tiling = SelectedGroup.GetDirectTileAt(tilePosition);

                    if (tiling == null)
                        return;

                    if (!tiling.Tags.Contains(SelectedTag))
                        return;

                    tiling.Tags.Remove(SelectedTag);
                    
                    if(tiling == SelectedAutotiling)
                        RefreshProperties();
                }
                break;
            }
        }

        private void RefreshAutotileCombo()
        {
            AutotileComboBox.Items.Clear();
            if (AutoTilesetInstance == null)
                return;
            AutoTilesetInstance.AutotileGroups.ForEach(group =>
            {
                AutotileComboBox.Items.Add(group.Name);
            });
        }

        private void RefreshTilesetSelectorList()
        {
            var searchResults = EngineCore.Instance.Managers.Assets.SearchAllPacks<TilesetDef>();

            foreach (var result in searchResults)
            {
                var asset = _scope.Load<TilesetDef>(result.AssetId);
                
                var item = new AutotileTilesetItem(asset);
                item.TilesetSelected += () =>
                {
                    Console.WriteLine($"Tileset selected: {asset.Name}");
                    CenterTilesetImage.Source = asset.GetBitmap();
                    SelectedTileset = asset;
                };
                TilesetSelectorPanel.Children.Add(item);
            }
        }

        private void ClearProperties()
        {
            GroupTagsTagsList.Items.Clear();
            TagsList.Items.Clear();
            RemoveBaseCase();
            ClearBasedOnTilesCases();
        }
        
        private void RefreshProperties()
        {
            if (SelectedAutotiling == null || SelectedGroup == null)
                return;
            
            GroupTagsTagsList.Items.Clear();
            SelectedGroup.Tags.ToList().ForEach(tag =>
            {
                GroupTagsTagsList.Items.Add(tag);
            });
            
            TagsList.Items.Clear();
            SelectedAutotiling.Tags.ForEach(tag =>
            {
                TagsList.Items.Add(tag);
            });

            if (SelectedAutotiling != SelectedGroup.BaseTile && SelectedGroup.BaseTile != null)
            {
                var basePosition = SelectedGroup.BaseTile.PositionInTileset;

                DrawBaseCase(basePosition);
            }
            else
            {
                RemoveBaseCase();
            }
            
            ClearBasedOnTilesCases();
            // Get all autotiles based on the base autotile
            foreach (var tiling in SelectedGroup.Tiles)
            {
                if(tiling != SelectedGroup.BaseTile && tiling != SelectedAutotiling)
                    AddBasedOnTileCase(tiling.PositionInTileset);
            }
            
            // Get the position of the selected autotile
            DrawSelectedTileCase(SelectedAutotiling.PositionInTileset);
        }

        public void RegisterEvents()
        {
            AddGroupButton.Click += OnAddGroupButtonOnClick;
            AutotileComboBox.SelectionChanged += OnAutotileComboBoxOnSelectionChanged;
            GroupTagsAddButn.Click += OnGroupTagsAddButnOnClick;
            AutotileTagsRemove.Click += OnAutotileTagsRemoveOnClick;
            AutotileTagsAdd.Click += OnAutotileTagsAddOnClick;
            OpenRuleEditorButton.Click += OnOpenRuleEditorButtonOnClick;
        }

        private void OnOpenRuleEditorButtonOnClick(object? s, RoutedEventArgs e)
        {
            Console.WriteLine("Opening Rule Editor...");
            if (SelectedGroup == null)
            {
                Console.WriteLine("No groupInstance selected.");
                return;
            }

            var ruleEditor = new AutotileEditorRuleEditorWindow(AutoTilesetInstance);

            ruleEditor.ShowDialog((Window)this.GetVisualRoot()!);
        }

        private void OnAddGroupButtonOnClick(object? o, RoutedEventArgs routedEventArgs)
        {
            AutoTilesetInstance.AutotileGroups.Add(new AutotileGroupDef($"New GroupDefinition - {AutotileComboBox.Items.Count}"));
            // SelectedTileset.Groups.Add(new AutotilesGroup($"New GroupDefinition - {AutotileComboBox.Items.Count}", SelectedTileset));
            RefreshAutotileCombo();
            AutotileComboBox.SelectedIndex = AutotileComboBox.Items.Count - 1;
        }

        private void OnAutotileComboBoxOnSelectionChanged(object? s, SelectionChangedEventArgs e)
        {
            if (AutotileComboBox.SelectedIndex >= 0 && AutotileComboBox.SelectedIndex < AutoTilesetInstance.AutotileGroups.Count)
            {
                // Load the selected groupInstance
                // SelectedGroup = SelectedTileset.Groups[AutotileComboBox.SelectedIndex];
                SelectedGroup = AutoTilesetInstance.AutotileGroups[AutotileComboBox.SelectedIndex];
                RefreshProperties();
            }
        }

        private void OnGroupTagsAddButnOnClick(object? o, RoutedEventArgs routedEventArgs)
        {
            if (SelectedGroup == null) return;
            // Open a dialog to add a new tag
            var dialog = new TextInputDialog("New Tag", "Enter the name of the new tag:", allowEmpty: false);
            dialog.Confirmed += (tag) =>
            {
                if (SelectedGroup.Tags.Contains(tag)) return;

                SelectedGroup.AddTag(tag);
                RefreshProperties();
            };
            dialog.ShowDialog((Window)this.GetVisualRoot()!);
        }

        private void OnAutotileTagsAddOnClick(object? s, RoutedEventArgs e)
        {
            if (SelectedGroup == null) return;

            // Open a dialog to add a new tag
            var dialog = new TextInputDialog("New Tag", "Enter the name of the new tag:", allowEmpty: false);
            dialog.Confirmed += (tag) =>
            {
                if (SelectedGroup.Tags.Contains(tag)) return;

                AutotileTagsSelector.Items.Add(tag);
                AutotileTagsSelector.SelectedItem = tag;
                RefreshProperties();
            };

            dialog.ShowDialog((Window)this.GetVisualRoot()!);
        }

        private void OnAutotileTagsRemoveOnClick(object? s, RoutedEventArgs e)
        {
            if (SelectedGroup == null) return;

            if (string.IsNullOrWhiteSpace(SelectedTag)) return;

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
        }

        private void RemoveBaseCase()
        {
            if (BaseTileCase == null)
                return;
            
            CenterSubCanvas.Children.Remove(BaseTileCase);
            BaseTileCase = null;
        }
        private void DrawBaseCase(Core.Types.Internal.Point? at)
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
            BaseTileCase.Width = SelectedTileset.TileWidth;
            BaseTileCase.Height = SelectedTileset.TileHeight;
            
            Canvas.SetLeft(BaseTileCase, at.Value.X * SelectedTileset.TileWidth);
            Canvas.SetTop(BaseTileCase, at.Value.Y * SelectedTileset.TileHeight);
        }
        
        private void ClearBasedOnTilesCases()
        {
            foreach (var border in BasedOnTilesCases)
            {
                CenterSubCanvas.Children.Remove(border);
            }
            BasedOnTilesCases.Clear();
        }

        private void AddBasedOnTileCase(Core.Types.Internal.Point? at)
        {
            var border = new Border()
            {
                // Draw a green background, with 30% opacity
                Background = new SolidColorBrush(Color.FromArgb(76, 0, 255, 0)),
            };
            CenterSubCanvas.Children.Add(border);
            BasedOnTilesCases.Add(border);

            border.Width = SelectedTileset.TileWidth;
            border.Height = SelectedTileset.TileHeight;

            Canvas.SetLeft(border, at.Value.X * SelectedTileset.TileWidth);
            Canvas.SetTop(border, at.Value.Y * SelectedTileset.TileHeight);
        }

        private void DrawSelectedTileCase(Core.Types.Internal.Point? at)
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

            SelectedTileCase.Width = SelectedTileset.TileWidth;
            SelectedTileCase.Height = SelectedTileset.TileHeight;

            Canvas.SetLeft(SelectedTileCase, at.Value.X * SelectedTileset.TileWidth);
            Canvas.SetTop(SelectedTileCase, at.Value.Y * SelectedTileset.TileHeight);
        }
    }
}
