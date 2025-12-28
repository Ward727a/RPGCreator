using System;
using System.Collections.Generic;
using System.Drawing;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using RPGCreator.SDK.Logging;
using RPGCreator.UI.Common.CustomBrush;
using Color = Avalonia.Media.Color;
using Point = Avalonia.Point;

namespace RPGCreator.UI.Common;
public class EditorGridLayer : Control
{
    // On peut binder ça ou le passer en propriété
    public Point Offset { get; set; } = new Point(0, 0);
    public Size GridCellSize { get; set; } = new (32, 32);
    public bool ShowGrid { get; set; } = true;

    // Stylos (Cached pour la perf)
    private static readonly Pen _penBlack = new Pen(Brushes.Black, 1);
    private static readonly Pen _penWhite = new Pen(Brushes.White, 1, new DashStyle(new[] { 2.0, 2.0 }, 0));

    public EditorGridLayer()
    {
        this.IsHitTestVisible = false; 
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        if (!ShowGrid) return;
        if (GridCellSize.Width <= 0 || GridCellSize.Height <= 0) return;

        double width = Bounds.Width;
        double height = Bounds.Height;
        double startX = Offset.X % GridCellSize.Width;
        double startY = Offset.Y % GridCellSize.Height;
        // X lines
        for (double x = startX; x < width; x += GridCellSize.Width)
        {
            double snapX = Math.Floor(x) + 0.5;
            context.DrawLine(_penBlack, new Point(snapX, 0), new Point(snapX, height));
            context.DrawLine(_penWhite, new Point(snapX, 0), new Point(snapX, height));
        }

        // Y lines
        for (double y = startY; y < height; y += GridCellSize.Height)
        {
            double snapY = Math.Floor(y) + 0.5;
            context.DrawLine(_penBlack, new Point(0, snapY), new Point(width, snapY));
            context.DrawLine(_penWhite, new Point(0, snapY), new Point(width, snapY));
        }
    }
}

public class MoveableCanvas : UserControl
{

    public Canvas CanvasBody { get; private set; }
    private EditorGridLayer _gridLayer;
    
    private Dictionary<Control, Point> _moveableElements = new();
    private Point _lastMousePosition = new Point(0, 0);
    public Point CurrentElementsPosition { get; private set; } = new Point(0, 0);
    
    public bool LimitToContentSize { get; set; } = false;
    public bool LimitTo00Coordinates { get; set; } = true;
    
    public bool ShowGrid { get; set; } = false;
    public System.Drawing.Size GridCellSize { get; private set; } = new (32, 32);

    private bool _showCheckboard = false;
    public bool ShowCheckboard { get => _showCheckboard;
        set
        {
            _showCheckboard = value;
            CanvasBody.Background = _showCheckboard ? CheckerBoardBrush.CreateCheckerBoardBrush(new Color(50, 100, 100, 100), Colors.Transparent, _checkerboardSize) : Avalonia.Media.Brushes.Transparent;
        } 
    }
    
    private double _checkerboardSize = 20;

    public double CheckboardSize
    {
        get => _checkerboardSize;
        set
        {
            _checkerboardSize = value;
            if (ShowCheckboard)
            {
                CanvasBody.Background = CheckerBoardBrush.CreateCheckerBoardBrush(new Color(50, 100, 100, 100), Colors.Transparent, _checkerboardSize);
            }
        }
    }

    public MoveableCanvas()
    {
        CreateComponents();
        RegisterEvents();
        Content = CanvasBody;
    }
    
    public void AddMoveableElement(Control element)
    {
        _moveableElements.Add(element, new Point(Canvas.GetLeft(element), Canvas.GetTop(element)));
        CanvasBody.Children.Add(element);
        Canvas.SetLeft(element, CurrentElementsPosition.X);
        Canvas.SetTop(element, CurrentElementsPosition.Y);
    }
    
    public void UpdateOrigin(Control element)
    {
        if (_moveableElements.ContainsKey(element))
        {
            _moveableElements[element] = new Point(Canvas.GetLeft(element) - CurrentElementsPosition.X,
                Canvas.GetTop(element) - CurrentElementsPosition.Y);
        }
    }
    
    public void SetGridCellSize(System.Drawing.Size cellSize)
    {
        GridCellSize = cellSize;
        _gridLayer.GridCellSize = GridCellSize;
        _gridLayer.ShowGrid = ShowGrid;
        _gridLayer.InvalidateVisual();
    }
    
    private void CreateComponents()
    {
        CanvasBody = new Canvas()
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            Background = ShowCheckboard ? CheckerBoardBrush.CreateCheckerBoardBrush(Colors.LightGray, Colors.Gray) :Avalonia.Media.Brushes.Transparent
        };
        
        _gridLayer = new EditorGridLayer();
        _gridLayer.Bind(WidthProperty, new Binding("Bounds.Width") { Source = this });
        _gridLayer.Bind(HeightProperty, new Binding("Bounds.Height") { Source = this });
        CanvasBody.Children.Add(_gridLayer);
        _gridLayer.ZIndex = int.MaxValue;
        _gridLayer.GridCellSize = GridCellSize;
        _gridLayer.ShowGrid = ShowGrid;
    }
    
    private void RegisterEvents()
    {
        CanvasBody.PointerMoved += CanvasBodyOnPointerMoved;
        CanvasBody.PointerReleased += CanvasBodyOnPointerReleased;
    }

    private void CanvasBodyOnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (e.GetCurrentPoint(CanvasBody).Properties.IsRightButtonPressed)
        {
            var position = e.GetPosition(CanvasBody);
            if(position == _lastMousePosition)
            {
                return;
            }
            if(_lastMousePosition == new Point(0, 0))
            {
                _lastMousePosition = position;
                return;
            }
            var deltaX = position.X - _lastMousePosition.X;
            var deltaY = position.Y - _lastMousePosition.Y;

            _lastMousePosition = position;

            if (_moveableElements.Count > 0)
            {
                var biggestElementSize = new Size(0, 0);
                foreach (var element in _moveableElements)
                {
                    var elementWidth = element.Key.Bounds.Width;
                    var elementHeight = element.Key.Bounds.Height;
                    if (elementWidth > biggestElementSize.Width)
                        biggestElementSize = biggestElementSize with { Width = (int)elementWidth };
                    if (elementHeight > biggestElementSize.Height)
                        biggestElementSize = biggestElementSize with { Height = (int)elementHeight };
                }
                
                if (position != null)
                {
                    var newX = deltaX + CurrentElementsPosition.X;
                    var newY = deltaY + CurrentElementsPosition.Y;

                    if (LimitTo00Coordinates)
                    {
                        newX = newX > 0 ? 0 : newX;
                        newY = newY > 0 ? 0 : newY;
                    }

                    if (LimitToContentSize)
                    {
                        var elementWidth = biggestElementSize.Width;
                        var elementHeight = biggestElementSize.Height;
                        var canvasWidth = CanvasBody.Bounds.Width;
                        var canvasHeight = CanvasBody.Bounds.Height;

                        if (elementWidth > 0)
                        {
                            var minX = canvasWidth - elementWidth;
                            var maxX = 0;
                            if (newX < minX) newX = minX;
                            if (newX > maxX) newX = maxX;
                        }

                        if (elementHeight > 0)
                        {
                            var minY = canvasHeight - elementHeight;
                            var maxY = 0;
                            if (newY < minY) newY = minY;
                            if (newY > maxY) newY = maxY;
                        }
                    }

                    CurrentElementsPosition = new Point(newX, newY);
                    Logger.Debug("Element moved to X: {X}, Y: {Y}", newX, newY);
                }
            }

            foreach (var element in _moveableElements)
            {
                Canvas.SetLeft(element.Key, CurrentElementsPosition.X + element.Value.X);
                Canvas.SetTop(element.Key, CurrentElementsPosition.Y + element.Value.Y);
            }
            
            _gridLayer.Offset = CurrentElementsPosition;
            _gridLayer.InvalidateVisual();
        }
    }
    
    private void CanvasBodyOnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _lastMousePosition = new Point(0, 0);
    }
}