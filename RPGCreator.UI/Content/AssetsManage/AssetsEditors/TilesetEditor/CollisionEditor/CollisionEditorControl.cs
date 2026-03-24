// RPG Creator - Open-source RPG Engine.
// (c) 2026 Ward
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

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using RPGCreator.SDK;
using RPGCreator.SDK.Assets.Definitions.Tilesets;
using RPGCreator.SDK.Assets.Definitions.Tilesets.Collision;
using RPGCreator.SDK.Helpers;
using RPGCreator.UI.Common;
using RPGCreator.SDK.Types;
using Ursa.Controls;
using Color = Avalonia.Media.Color;

#pragma warning disable CS0414 // Field is assigned but its value is never used

namespace RPGCreator.UI.Content.AssetsManage.AssetsEditors.TilesetEditor.CollisionEditor;

public class CollisionEditorControl : UserControl
{
    private Dictionary<Vector2, ECollisionFlag> CollisionsData { get; } = new();
    private Dictionary<Vector2, ECollisionGroupingType> CollisionGroupingData { get; } = new();
    
    private ECollisionFlag _currentCollisionFlag = ECollisionFlag.None;
    private ECollisionGroupingType _collisionGroupingType = ECollisionGroupingType.Union;

    private ECollisionFlag _previewCollisionFlag = ECollisionFlag.None;
    private Path? _previewCollBox = null;
    private CancellationTokenSource? _cts = null;
    
    private Grid _mainGrid = null!;
    private StackPanel _topPanel = null!;
    private StackPanel _buttonsPanel = null!;

    #region CollisionTypeButtons

    private readonly ButtonGroup _collisionTypeButtons = new();

    private readonly Button _topColButton = new()
    {
        Width = 32,
        Height = 32,
        FontSize = 32,
        VerticalContentAlignment = VerticalAlignment.Top,
        VerticalAlignment = VerticalAlignment.Top,
    };

    private readonly Button _leftColButton = new ()
    {
        Width = 32,
        Height = 32,
        FontSize = 32,
        VerticalContentAlignment = VerticalAlignment.Bottom,
        VerticalAlignment = VerticalAlignment.Bottom,
    };

    private readonly Button _rightColButton = new ()
    {
        Width = 32,
        Height = 32,
        FontSize = 32,
    };
    
    private readonly Button _bottomColButton = new ()
    {
        Width = 32,
        Height = 32,
        FontSize = 32,
    };

    #endregion
    
    private readonly ComboBox _collisionGroupingTypeComboBox = new ComboBox()
    {
        ItemsSource = Enum.GetValues<ECollisionGroupingType>(),
        SelectedIndex = 0,
        HorizontalAlignment = HorizontalAlignment.Left,
        VerticalAlignment = VerticalAlignment.Center
    };

    private readonly HelpButton _helpCollisionEditorButton =
        new HelpButton("rpgc".ToUrnNamespace().ToUrnModule("docs").ToUrn("collision_editor")){
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center
            
        };
    
    private MoveableCanvas _editorCanvas = null!;
    private Canvas _collisionPreviewCanvas = null!;
    
    private Vector2 _lastPlacedColAt = Vector2.One;
    
    private readonly BaseTilesetDef _tilesetDef;
    private readonly string _imagePath;
    
    public CollisionEditorControl(BaseTilesetDef tilesetDef, string? imagePath = null)
    {
        _tilesetDef = tilesetDef;
        _imagePath = imagePath ?? tilesetDef.ImagePath;
        
        // Loading collisions from tileset
        foreach (var tilesetDefCollision in _tilesetDef.Collisions)
        {
            var pos = tilesetDefCollision.Key.FromKey();
            var type = tilesetDefCollision.Value.CollisionGroupingType;
            var flag = tilesetDefCollision.Value.CollisionFlag;
            
            CollisionsData[pos] = flag;
            CollisionGroupingData[pos] = type;
        }
        
        CreateComponents();
        RegisterEvents();
        LinkToExtension();
        RefreshPreview();
        
        Content = _mainGrid;
    }

    private void CreateComponents()
    {
        _mainGrid = new Grid()
        {
            RowDefinitions = new RowDefinitions("Auto,*"),
        };
        
        _topPanel = new StackPanel()
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(5),
            Spacing = 4,
        };
        
        _buttonsPanel = new StackPanel()
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(5),
        };
        
        MakeButtons();
        _topPanel.Children.Add(_collisionTypeButtons);
        _collisionTypeButtons.ItemsPanel = new FuncTemplate<Panel>(() => _buttonsPanel)!;
        _topPanel.Children.Add(_collisionGroupingTypeComboBox);
        _topPanel.Children.Add(_helpCollisionEditorButton);
        
        _mainGrid.Children.Add(_topPanel);
        Grid.SetRow(_topPanel, 0);
        
        _editorCanvas = new MoveableCanvas()
        {
            LimitTo00Coordinates = true,
            LimitToContentSize = true,
            ShowGrid = true,
            ShowCheckboard = true,
            CheckboardSize = 32,
            VerticalAlignment = VerticalAlignment.Stretch,
            Margin = new Thickness(5)
        };
        _mainGrid.Children.Add(_editorCanvas);
        Grid.SetRow(_editorCanvas, 1);
        var previewImage = new Image()
        {
            Source = EngineServices.Resources.Load<Bitmap>(_imagePath),
        };
        Canvas.SetLeft(previewImage, 0);
        Canvas.SetTop(previewImage, 0);
        _editorCanvas.AddMoveableElement(previewImage);
        _collisionPreviewCanvas = new Canvas()
        {
            IsHitTestVisible = false,
        };
        Canvas.SetLeft(_collisionPreviewCanvas, 0);
        Canvas.SetTop(_collisionPreviewCanvas, 0);
        _editorCanvas.AddMoveableElement(_collisionPreviewCanvas);
    }

    private void MakeButtons()
    {
        _topColButton.Content = new PathIcon()
        {
            Data = Geometry.Parse("M 0 0 H 32 V 16 H 0 Z"),
            Width = 24,
            Height = 16,
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Center,
            Tag = ECollisionFlag.Top
        };
        _collisionTypeButtons.Items.Add(_topColButton);
        _rightColButton.Content = new PathIcon()
        {
            Data = Geometry.Parse("M 16 0 H 32 V 32 H 16 Z"),
            Width = 24,
            Height = 24,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Tag = ECollisionFlag.Right
        };
        _collisionTypeButtons.Items.Add(_rightColButton);
        _leftColButton.Content = new PathIcon()
        {
            Data = Geometry.Parse("M 0 0 H 32 V 16 H 0 Z"),
            Width = 24,
            Height = 16,
            VerticalAlignment = VerticalAlignment.Bottom,
            HorizontalAlignment = HorizontalAlignment.Center,
            Tag = ECollisionFlag.Bottom
        };
        _collisionTypeButtons.Items.Add(_leftColButton);
        _bottomColButton.Content = new PathIcon()
        {
            Data = Geometry.Parse("M 16 0 H 32 V 32 H 16 Z"),
            Width = 24,
            Height = 24,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Right,
            Tag = ECollisionFlag.Left
        };
        _collisionTypeButtons.Items.Add(_bottomColButton);
        // Ajouter bouton suppression
    }
    
    protected override void OnInitialized()
    {
        base.OnInitialized();
        
        _editorCanvas.SetGridCellSize(new SDK.Types.Size(32, 32));
    }

    private void RegisterEvents()
    {
        _editorCanvas.CanvasBody.PointerPressed += OnCanvasPressed;
        _editorCanvas.CanvasBody.PointerMoved += OnCanvasMoved_Preview;
        _topColButton.Click += OnButtonClicked;
        _leftColButton.Click += OnButtonClicked;
        _rightColButton.Click += OnButtonClicked;
        _bottomColButton.Click += OnButtonClicked;
        _collisionGroupingTypeComboBox.SelectionChanged += (_, _) =>
        {
            if (_collisionGroupingTypeComboBox.SelectedItem is not ECollisionGroupingType selectedType) return;
            _collisionGroupingType = selectedType;
            RefreshPreview();
        };
    }

    private void OnCanvasPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.Properties.IsLeftButtonPressed && _currentCollisionFlag != ECollisionFlag.None)
            return;
        e.Handled = true;
        
        var position = e.GetPosition(_editorCanvas.CanvasBody);
        var cellSize = _editorCanvas.GridCellSize;
        var canvasPos = _editorCanvas.CurrentElementsPosition;
        
        var alignedX = (float)(Math.Floor((position.X - (canvasPos.X % cellSize.Width)) / cellSize.Width) * cellSize.Width + (canvasPos.X % cellSize.Width));
        var alignedY = (float)(Math.Floor((position.Y - canvasPos.Y % cellSize.Height) / cellSize.Height) * cellSize.Height + (canvasPos.Y % cellSize.Height));
        
        var targetPos = new Vector2(alignedX, alignedY);
        
        CollisionsData.TryGetValue(targetPos, out var existingFlag);
        var newFlag = existingFlag ^ _currentCollisionFlag;

        if (newFlag == ECollisionFlag.None)
            CollisionsData.Remove(targetPos);
        else
            CollisionsData[targetPos] = newFlag;
        
        CollisionGroupingData[targetPos] = _collisionGroupingType;
        _lastPlacedColAt = targetPos;

        RefreshPreview();
    }

    private void OnCanvasMoved_Preview(object? sender, PointerEventArgs e)
    {
        if (!e.Properties.IsLeftButtonPressed && _currentCollisionFlag != ECollisionFlag.None)
        {
            var position = e.GetPosition(_editorCanvas.CanvasBody);
            var cellSize = _editorCanvas.GridCellSize;
            var canvasPos = _editorCanvas.CurrentElementsPosition;
            
            var alignedX = (float)(Math.Floor((position.X - (canvasPos.X % cellSize.Width)) / cellSize.Width) * cellSize.Width + (canvasPos.X % cellSize.Width));
            var alignedY = (float)(Math.Floor((position.Y - canvasPos.Y % cellSize.Height) / cellSize.Height) * cellSize.Height + (canvasPos.Y % cellSize.Height));
            
            var targetPos = new Vector2(alignedX, alignedY);

            if (_previewCollBox == null || _previewCollisionFlag != _currentCollisionFlag)
            {
                if(_previewCollBox != null) _editorCanvas.CanvasBody.Children.Remove(_previewCollBox);
                _previewCollBox = CreateCollisionShape(_currentCollisionFlag);
                if (_previewCollBox != null)
                {
                    Canvas.SetLeft(_previewCollBox, targetPos.X);
                    Canvas.SetTop(_previewCollBox, targetPos.Y);
                    _previewCollBox.Fill = new SolidColorBrush(Color.FromArgb(120, 255, 255, 255));
                    _previewCollBox.Stroke = new SolidColorBrush(Colors.White);
                    _editorCanvas.AddMoveableElement(_previewCollBox);
                    
                    // MAke animation that blink the _collisionPreviewCanvas
                    var blinkAnimation = new Animation
                    {
                        Duration = TimeSpan.FromMilliseconds(800),
                        IterationCount = IterationCount.Infinite,
                        Easing = new CubicEaseOut(),
                        Children =
                        {
                            new KeyFrame
                            {
                                KeyTime = TimeSpan.FromMilliseconds(0),
                                Setters =
                                {
                                    new Setter(Shape.FillProperty, new SolidColorBrush(Color.FromArgb(120, 255, 255, 255))),
                                    new Setter(Shape.StrokeProperty, new SolidColorBrush(Colors.White))
                                },
                            },
                            new KeyFrame
                            {
                                KeyTime = TimeSpan.FromMilliseconds(200),
                                Setters =
                                {
                                    new Setter(Shape.FillProperty, new SolidColorBrush(Color.FromArgb(120, 255, 150, 150))),
                                    new Setter(Shape.StrokeProperty, new SolidColorBrush(Colors.Red))
                                },
                            },
                            new KeyFrame
                            {
                                KeyTime = TimeSpan.FromMilliseconds(600),
                                Setters =
                                {
                                    new Setter(Shape.FillProperty, new SolidColorBrush(Color.FromArgb(120, 255, 150, 150))),
                                    new Setter(Shape.StrokeProperty, new SolidColorBrush(Colors.Red))
                                },
                            },
                            new KeyFrame
                            {
                                KeyTime = TimeSpan.FromMilliseconds(800),
                                Setters =
                                {
                                    new Setter(Shape.FillProperty, new SolidColorBrush(Color.FromArgb(120, 255, 255, 255))),
                                    new Setter(Shape.StrokeProperty, new SolidColorBrush(Colors.White))
                                },
                            }
                        }
                    };

                    _cts?.Cancel();
                    _cts = new CancellationTokenSource();
                    blinkAnimation.RunAsync(_previewCollBox, _cts.Token);

                }
                _previewCollisionFlag = _currentCollisionFlag;
            }
            else
            {
                Canvas.SetLeft(_previewCollBox, targetPos.X);
                Canvas.SetTop(_previewCollBox, targetPos.Y);
            }
        }
        else if (e.Properties.IsLeftButtonPressed &&
                 _currentCollisionFlag != ECollisionFlag.None)
        {
            e.Handled = true;
        
            var position = e.GetPosition(_editorCanvas.CanvasBody);
            var cellSize = _editorCanvas.GridCellSize;
            var canvasPos = _editorCanvas.CurrentElementsPosition;
        
            var alignedX = (float)(Math.Floor((position.X - (canvasPos.X % cellSize.Width)) / cellSize.Width) * cellSize.Width + (canvasPos.X % cellSize.Width));
            var alignedY = (float)(Math.Floor((position.Y - canvasPos.Y % cellSize.Height) / cellSize.Height) * cellSize.Height + (canvasPos.Y % cellSize.Height));
        
            var targetPos = new Vector2(alignedX, alignedY);

            if (targetPos == _lastPlacedColAt)
                return;
            _lastPlacedColAt = targetPos;
            
            CollisionsData.TryGetValue(targetPos, out var existingFlag);
            var newFlag = existingFlag ^ _currentCollisionFlag;

            if (newFlag == ECollisionFlag.None)
                CollisionsData.Remove(targetPos);
            else
                CollisionsData[targetPos] = newFlag;
        
            CollisionGroupingData[targetPos] = _collisionGroupingType;

            RefreshPreview();
        }
    }

    private void OnButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { Content: PathIcon { Tag: ECollisionFlag flag } })
        {
            _currentCollisionFlag = flag;
        }
    }

    private void LinkToExtension()
    {
    }
    
    private Path? CreateCollisionShape(ECollisionFlag flag)
    {
        var brush = new SolidColorBrush(Color.FromArgb(120, 255, 0, 0));
        var stroke = new SolidColorBrush(Colors.Red);

        var path = new Path
        {
            Fill = brush,
            Stroke = stroke,
            StrokeThickness = 1,
            IsHitTestVisible = false,
            Data = flag switch
            {
                ECollisionFlag.Top    => Geometry.Parse("M 0,0 H 32 V 16 H 0 Z"),
                ECollisionFlag.Bottom => Geometry.Parse("M 0,16 H 32 V 32 H 0 Z"),
                ECollisionFlag.Left   => Geometry.Parse("M 0,0 H 16 V 32 H 0 Z"),
                ECollisionFlag.Right  => Geometry.Parse("M 16,0 H 32 V 32 H 16 Z"),
                _ => null
            }
        };

        return path.Data == null ? null : path;
    }
    
    private void RefreshPreview()
    {
        _collisionPreviewCanvas.Children.Clear();

        foreach (var kvp in CollisionsData)
        {
            var pos = kvp.Key;
            var flags = kvp.Value;

            var groupingType = CollisionGroupingData.GetValueOrDefault(pos, ECollisionGroupingType.Union);

            switch (groupingType)
            {
                case ECollisionGroupingType.Union:
                    foreach (var flag in Enum.GetValues<ECollisionFlag>())
                    {
                        if (flag != ECollisionFlag.None && flags.HasFlag(flag))
                        {
                            var shape = CreateCollisionShape(flag);
                            if (shape != null)
                            {
                                Canvas.SetLeft(shape, pos.X);
                                Canvas.SetTop(shape, pos.Y);
                                _collisionPreviewCanvas.Children.Add(shape);
                            }
                        }
                    }
                    break;
                case ECollisionGroupingType.Intersection:
                    var geometries = new List<Geometry>();
                    foreach (var flag in Enum.GetValues<ECollisionFlag>())
                    {
                        if (flag != ECollisionFlag.None && flags.HasFlag(flag))
                        {
                            var shape = CreateCollisionShape(flag) as Path;
                            if (shape?.Data != null) geometries.Add(shape.Data);
                        }
                    }

                    if (geometries.Count >= 2)
                    {
                        var combined = geometries[0];
                        for (var i = 1; i < geometries.Count; i++)
                        {
                            combined = new CombinedGeometry(GeometryCombineMode.Intersect, combined, geometries[i]);
                        }

                        var intersectPath = new Path
                        {
                            Fill = new SolidColorBrush(Color.FromArgb(180, 255, 0, 0)),
                            Data = combined
                        };
                        Canvas.SetLeft(intersectPath, pos.X);
                        Canvas.SetTop(intersectPath, pos.Y);
                        _collisionPreviewCanvas.Children.Add(intersectPath);
                    }
                    else
                    {
                        var singleShape = CreateCollisionShape(flags);
                        if (singleShape != null)
                        {
                            Canvas.SetLeft(singleShape, pos.X);
                            Canvas.SetTop(singleShape, pos.Y);
                            _collisionPreviewCanvas.Children.Add(singleShape);
                        }
                    }

                    break;
            }
        }
    }

    public void SaveCollision()
    {
        foreach (var kvp in CollisionsData)
        {
            var pos = kvp.Key;
            var flags  = kvp.Value;
            var groupType  = CollisionGroupingData.GetValueOrDefault(pos, ECollisionGroupingType.Union);

            var data = new CollisionData(groupType, flags);

            var key = pos.ToKey();
            
            if (!_tilesetDef.Collisions.TryAdd(key, data))
            {
                _tilesetDef.Collisions[key] = data;
            }
        }
        EditorUiServices.NotificationService.Success("Collisions saved.", "Collision saved successfully.");
    }
}