using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using RPGCreator.Core.Types.Internal;
using Serilog;
using Point = Avalonia.Point;

namespace RPGCreator.UI.Common;

public class MoveableCanvas : UserControl
{

    public Canvas CanvasBody { get; private set; }
    
    private Dictionary<Control, Point> _moveableElements = new();
    private Point _lastMousePosition = new Point(0, 0);
    private Point _currentElementsPosition = new Point(0, 0);
    
    public bool LimitTo00Coordinates { get; set; } = true;
    
    public MoveableCanvas()
    {
        CreateComponents();
        RegisterEvents();
        Content = CanvasBody;
    }
    
    public void AddMoveableElement(Control element)
    {
        _moveableElements.Add(element, new Point(0, 0));
        CanvasBody.Children.Add(element);
        Canvas.SetLeft(element, _currentElementsPosition.X);
        Canvas.SetTop(element, _currentElementsPosition.Y);
    }
    
    private void CreateComponents()
    {
        CanvasBody = new Canvas()
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch
        };
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
            foreach (var element in _moveableElements)
            {
                if (position != null)
                {
                    var newX = deltaX + _currentElementsPosition.X;
                    var newY = deltaY + _currentElementsPosition.Y;

                    if (LimitTo00Coordinates)
                    {
                        newX = newX > 0 ? 0 : newX;
                        newY = newY > 0 ? 0 : newY;
                    }

                    Canvas.SetLeft(element.Key, newX);
                    Canvas.SetTop(element.Key, newY);
                    _currentElementsPosition = new Point(newX, newY);
                    Log.Debug("Element moved to X: {X}, Y: {Y}", newX, newY);
                }
            }
        }
    }
    
    private void CanvasBodyOnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _lastMousePosition = new Point(0, 0);
    }
}