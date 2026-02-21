using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using AvaloniaInside.MonoGame;
using RPGCreator.SDK;
using RPGCreator.SDK.Inputs;
using RPGCreator.UI.Test;
using MouseButton = RPGCreator.SDK.Inputs.MouseButton;

namespace RPGCreator.UI.Common.Bridge;

public class AvaloniaMouseBridge
{
    private int _scrollAccumulatorY;
    private int _scrollAccumulatorX;
    
    /// <summary>
    /// Registers pointer events on the given Avalonia control to update the global engine mouse state.
    /// </summary>
    /// <param name="control"></param>
    public void RegisterEvents(Control control)
    {
        control.PointerMoved += (s, e) => Update(control, e);
        control.PointerPressed += (s, e) => Update(control, e);
        control.PointerReleased += (s, e) => Update(control, e);
        control.PointerWheelChanged += (s, e) =>
        {
            _scrollAccumulatorX += (int)e.Delta.X;
            _scrollAccumulatorY += (int)e.Delta.Y;
            Update(control, e);
        };

        control.PointerExited += (s, e) =>
        {
            var rawData = new RawMouseData
            {
                X = -1,
                Y = -1,
                Scroll = _scrollAccumulatorY,
                HScroll = _scrollAccumulatorX,
                IsInsideWindow = true,
                InObject = null,
                Buttons = MouseButton.None
            };
            EngineProviders.MouseProvider.Update(rawData);
        };
    }
    private void Update(Control control, PointerEventArgs e)
    {
        var localPoint = e.GetPosition(control);
        var topLevel = control.GetVisualRoot() as Window;
        var windowPoint = e.GetPosition(topLevel);
        var p = e.GetCurrentPoint(control).Properties;

        MouseButton buttonsPressed =
            (p.IsLeftButtonPressed ? MouseButton.Left : MouseButton.None)
            | (p.IsRightButtonPressed ? MouseButton.Right : MouseButton.None)
            | (p.IsMiddleButtonPressed ? MouseButton.Middle : MouseButton.None)
            | (p.IsXButton1Pressed ? MouseButton.XButton1 : MouseButton.None)
            | (p.IsXButton2Pressed ? MouseButton.XButton2 : MouseButton.None);
        
        // Logger.Debug("Mouse Update - Window Pos: ({0}, {1}), Local Pos: ({2}, {3}), Buttons: {4}, Scroll: ({5}, {6})",
        //     windowPoint.X, windowPoint.Y,
        //     localPoint.X, localPoint.Y,
        //     buttonsPressed,
        //     _scrollAccumulatorX, _scrollAccumulatorY);

        var rawData = new RawMouseData
        {
            X = (int)windowPoint.X,
            Y = (int)windowPoint.Y,
            Scroll = _scrollAccumulatorY,
            HScroll = _scrollAccumulatorX,
            IsInsideWindow = true,
            InObject = control,
            Buttons = buttonsPressed
        };
        
        var localRawData = new RawMouseData
        {
            X = (int)localPoint.X,
            Y = (int)localPoint.Y,
            Scroll = _scrollAccumulatorY,
            HScroll = _scrollAccumulatorX,
            IsInsideWindow = true,
            InObject = control is Image ? control.Name : null,
            Buttons = buttonsPressed
        };

        EngineProviders.MouseProvider.UpdateViewport(localRawData);
        EngineProviders.MouseProvider.Update(rawData);
    }
}