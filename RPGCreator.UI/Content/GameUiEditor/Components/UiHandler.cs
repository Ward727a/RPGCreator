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
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using RPGCreator.SDK.GameUI.Controls;
using RPGCreator.SDK.GameUI.Visual;

namespace RPGCreator.UI.Content.GameUiEditor.Components;

public class UiSnapLineController : UserControl
{

    public bool IsTopLineVisible { get; private set; } = false;
    public bool IsBottomLineVisible { get; private set; } = false;
    public bool IsLeftLineVisible { get; private set; } = false;
    public bool IsRightLineVisible { get; private set; } = false;
    public bool IsCenterHeightLineVisible { get; private set; } = false;
    public bool IsCenterWidthLineVisible { get; private set; } = false;
    
    public record struct SnapLine(Point Start, Point End);

    private readonly UiHandler _handler;
    private SnapLine?[] SnapLines = new SnapLine?[4];
    
    private SnapLine?[] CenterSnapLines = new SnapLine?[2];

    private const double StartLength = -10_000;
    private const double EndLength = +10_000;
    
    private double Top => _handler.SelectionRect.Top;
    private double Bottom => _handler.SelectionRect.Bottom;
    private double Left => _handler.SelectionRect.Left;
    private double Right => _handler.SelectionRect.Right;
    private double CenterHeight => Top + (_handler.SelectionRect.Height / 2);
    private double CenterWidth => Left + _handler.SelectionRect.Width / 2;
    
    private const int CenterIndexDenominator = 10;
    
    private const int TopIndex = 0;
    private const int BottomIndex = 1;
    private const int LeftIndex = 2;
    private const int RightIndex = 3;
    private const int CenterHeightIndex = 0 + CenterIndexDenominator;
    private const int CenterWidthIndex = 1 + CenterIndexDenominator;
    
    public UiSnapLineController(UiHandler handler)
    {
        _handler = handler;
    }

    private Pen _mainSnapLine = new Pen(Brushes.YellowGreen, 2, DashStyle.Dash);
    private Pen _centerSnapLine = new Pen(Brushes.DarkOrchid, 1, DashStyle.Dash);
    public override void Render(DrawingContext context)
    {
        base.Render(context);

        foreach (var snapLine in SnapLines)
        {
            if(snapLine == null)
                continue;
            
            context.DrawLine(_mainSnapLine, snapLine.Value.Start, snapLine.Value.End);
        }
        
        foreach (var snapLine in CenterSnapLines)
        {
            if(snapLine == null)
                continue;
            
            context.DrawLine(_centerSnapLine, snapLine.Value.Start, snapLine.Value.End);
        }
        
    }

    public void Enable(int index)
    {
        switch (index)
        {
            case TopIndex:
                IsTopLineVisible = true;
                SnapLines[TopIndex] = new SnapLine(new Point(StartLength, Top), new Point(EndLength, Top));
                break;
            case BottomIndex:
                IsBottomLineVisible = true;
                SnapLines[BottomIndex] = new SnapLine(new Point(StartLength, Bottom), new Point(EndLength, Bottom));
                break;
            case LeftIndex:
                IsLeftLineVisible = true;
                SnapLines[LeftIndex] = new SnapLine(new Point(Left, StartLength), new Point(Left, EndLength));
                break;
            case RightIndex:
                IsRightLineVisible = true;
                SnapLines[RightIndex] = new SnapLine(new Point(Right, StartLength), new Point(Right, EndLength));
                break;
            case CenterHeightIndex:
            {
                IsCenterHeightLineVisible = true;
                CenterSnapLines[CenterHeightIndex - CenterIndexDenominator] = new SnapLine(new Point(StartLength, CenterHeight), new Point(EndLength, CenterHeight));
                break;
            }
            case CenterWidthIndex:
            {
                IsCenterWidthLineVisible = true;
                CenterSnapLines[CenterWidthIndex - CenterIndexDenominator] = new SnapLine(new Point(CenterWidth, StartLength), new Point(CenterWidth, EndLength));
                break;
            }
        }
    }

    private void Disable(int index)
    {
        switch (index)
        {
            case TopIndex:
                IsTopLineVisible = false;
                SnapLines[TopIndex] = null;
                break;
            case BottomIndex:
                IsBottomLineVisible = false;
                SnapLines[BottomIndex] = null;
                break;
            case LeftIndex:
                IsLeftLineVisible = false;
                SnapLines[LeftIndex] = null;
                break;
            case RightIndex:
                IsRightLineVisible = false;
                SnapLines[RightIndex] = null;
                break;
            case CenterHeightIndex:
            {
                IsCenterHeightLineVisible = false;
                CenterSnapLines[CenterHeightIndex - CenterIndexDenominator] = null;
                break;
            }
            case CenterWidthIndex:
            {
                IsCenterWidthLineVisible = false;
                CenterSnapLines[CenterWidthIndex - CenterIndexDenominator] = null;
                break;
            }
        }
    }
    
    private void ToggleSnapLine(int index, bool enable)
    {
        if (enable)
            Enable(index);
        else
            Disable(index);
    }

    public void ToggleBottom(bool enable)
    {
        ToggleSnapLine(BottomIndex, enable);
    }
    public void ToggleTop(bool enable)
    {
        ToggleSnapLine(TopIndex, enable);
    }
    public void ToggleLeft(bool enable)
    {
        ToggleSnapLine(LeftIndex, enable);
    }
    public void ToggleRight(bool enable)
    {
        ToggleSnapLine(RightIndex, enable);
    }

    public void ToggleCenterHeight(bool enable)
    {
        ToggleSnapLine(CenterHeightIndex, enable);
    }
    
    public void ToggleCenterWidth(bool enable)
    {
        ToggleSnapLine(CenterWidthIndex, enable);
    }

    public void DisableAll()
    {
        ToggleBottom(false);
        ToggleTop(false);
        ToggleLeft(false);
        ToggleRight(false);
        ToggleCenterHeight(false);
        ToggleCenterWidth(false);
        _handler.InvalidateVisual();
    }
}

public class UiHandler : UserControl
{

    public event Action<PinType, Vector2>? PinPressed;
    public event Action? PinReleased;
    
    public enum PinType
    {
        None,
        TopLeft,
        BottomRight,
        Rotation,
        Movement
    }
    
    public PinType PressedRect { get; private set; } = PinType.None;
    public Vector2 PressedPosition { get; private set; } = Vector2.Zero;
    public Vector2 PressedPositionRelativeToControl { get; private set; } = Vector2.Zero;
    public bool HasSelection => SelectedControl != null;

    private BaseControl? SelectedControl { get; set; }
    public Rect SelectionRect { get; private set; }
    private Rect TopLeftHandleRect { get; set; }
    private Rect BottomRightHandleRect { get; set; }
    private Rect RotationHandleRect { get; set; }
    private Point RotationHandlePoint { get; set; }
    
    public void SelectControl(BaseControl? control)
    {
        if (control == null)
        {
            InvalidateVisual();
            return;
        }
        
        if (SelectedControl != null)
        {
            SelectedControl.Visual.DirtyChanged -= RefreshSelection;
        }

        SelectedControl = control;
        RefreshSelection();

        SelectedControl.Visual.VisualUpdated += RefreshSelection;
    }

    public void RefreshSelection()
    {
        if (SelectedControl == null) return;
        var selectionRect = SelectedControl.Visual.GetGlobalBounds();
        SelectionRect = new Rect(selectionRect.X, selectionRect.Y, selectionRect.Width, selectionRect.Height);
        TopLeftHandleRect = new Rect(selectionRect.X, selectionRect.Y, 10, 10);
        BottomRightHandleRect = new Rect(selectionRect.Right - 10, selectionRect.Bottom - 10, 10, 10);
        RotationHandlePoint = new Point(selectionRect.X + selectionRect.Width / 2, selectionRect.Y + selectionRect.Height / 2);
        RotationHandleRect = new Rect((selectionRect.X + selectionRect.Width / 2) - 6, (selectionRect.Y + selectionRect.Height / 2)-6, 12, 12);
        InvalidateVisual();
    }

    public UiSnapLineController UiSnapLineController { get; private set; }
    public UiHandler()
    {
        UiSnapLineController = new UiSnapLineController(this);
    }
    
    private Point StartSnap = default;
    private Point StopSnap = default;
    
    public override void Render(DrawingContext context)
    {
        if(!HasSelection)
            return;
        
        base.Render(context);
        var pen = new Pen(Brushes.DodgerBlue, 2);
        var handleBrush = Brushes.White;
        
        context.DrawRectangle(Brushes.Transparent, pen, SelectionRect);
        //
        // context.DrawRectangle(handleBrush, pen, TopLeftHandleRect);
        // context.DrawRectangle(handleBrush, pen, BottomRightHandleRect);
        //
        // context.DrawEllipse(handleBrush, pen, RotationHandlePoint, 5, 5);

        UiSnapLineController.Render(context);
    }
    
    public bool SnappedX { get; private set; }
    public bool SnappedY { get; private set; }
    
    public void CheckSnapping(BaseVisual dragged, IEnumerable<BaseVisual> others)
    {
        SnappedX = false;
        SnappedY = false;
        UiSnapLineController.DisableAll();
        var a = dragged.GlobalBounds;
        var parentMatrix = dragged.Parent?.GlobalTransform ?? Matrix3x2.Identity;
        float parentGlobalX = parentMatrix.Translation.X;
        float parentGlobalY = parentMatrix.Translation.Y;
    
        float parentScaleX = MathF.Sqrt(parentMatrix.M11 * parentMatrix.M11 + parentMatrix.M12 * parentMatrix.M12);
        float parentScaleY = MathF.Sqrt(parentMatrix.M21 * parentMatrix.M21 + parentMatrix.M22 * parentMatrix.M22);
    
        foreach (var other in others)
        {
            if (other == dragged) continue;
            var b = other.GlobalBounds;
            
            CheckAxisSnap(a.Left, b.Left, (val) => {
                dragged.X = (int)((b.Left - parentGlobalX) / parentScaleX);
                UiSnapLineController.ToggleLeft(true);
                UiSnapLineController.ToggleRight(true);
                SnappedX = true;
            });
            CheckAxisSnap(a.Right, b.Left, (val) => {
                dragged.X = (int)((b.Left - parentGlobalX - a.Width) / parentScaleX);
                
                UiSnapLineController.ToggleRight(true);
                
                SnappedX = true;
            });
            CheckAxisSnap(a.Left, b.Right, (val) => {
                dragged.X = (int)((b.Right - parentGlobalX) / parentScaleX);
                
                UiSnapLineController.ToggleLeft(true);
                
                SnappedX = true;
            });
            
            CheckAxisSnap(a.Left + a.Width/2, b.Left+(b.Width/2), (val) =>
            {
                dragged.X = (int)((b.Left + b.Width/2 - parentGlobalX) / parentScaleX);
                UiSnapLineController.ToggleCenterWidth(true);
                SnappedX = true;
            });
        
            CheckAxisSnap(a.Top, (b.Top), (val) =>
            {
                var o = other;
                dragged.Y = (int)((parentGlobalY - b.Top + dragged.Height) / parentScaleY);
                
                UiSnapLineController.ToggleTop(true);
                UiSnapLineController.ToggleBottom(true);

                SnappedY = true;
            });

            CheckAxisSnap(a.Bottom, (b.Top), (val) =>
            {
                dragged.Y = (int)((parentGlobalY - b.Top + dragged.Height*2) / parentScaleY);
                UiSnapLineController.ToggleBottom(true);
                UiSnapLineController.ToggleTop(false);
                SnappedY = true;
            });
            
            CheckAxisSnap(a.Top, (b.Bottom), (val) =>
            {
                dragged.Y = (int)((parentGlobalY - b.Bottom + dragged.Height) / parentScaleY);
                UiSnapLineController.ToggleTop(true);
                UiSnapLineController.ToggleBottom(false);
                SnappedY = true;
            });
            
            CheckAxisSnap(a.Top + a.Height/2, b.Top+(b.Height/2), (val) =>
            {
                UiSnapLineController.ToggleCenterHeight(true);
                SnappedY = true;
            });
        }
    }
    
    
    const float SnapThreshold = 10f;
    private void CheckAxisSnap(float valueA, float valueB, Action<float> applySnap)
    {
        if (Math.Abs(valueA - valueB) < SnapThreshold)
        {
            applySnap(valueB);
        }
    }
    
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (e.Source is UiHandler handler)
        {
            var position = e.GetPosition(this);
            
            if (handler == this)
            {
                if (TopLeftHandleRect.Contains(position))
                {
                    this.Cursor = new Cursor(StandardCursorType.TopLeftCorner);
                }
                else if (BottomRightHandleRect.Contains(position))
                {
                    this.Cursor = new Cursor(StandardCursorType.BottomRightCorner);
                }
                else if (RotationHandleRect.Contains(position))
                {
                    this.Cursor = new Cursor(StandardCursorType.Hand);
                }
                else if(SelectionRect.Contains(position))
                {
                    // this.Cursor = new Cursor(StandardCursorType.DragMove);
                }
                else
                {
                    this.Cursor = new Cursor(StandardCursorType.Arrow);
                }
            }
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        
        if (e.Source is UiHandler handler)
        {
            var position = e.GetPosition(this);
            
            if (handler == this)
            {
                if (TopLeftHandleRect.Contains(position))
                {
                    PressedRect = PinType.TopLeft;
                    PressedPosition = new Vector2((float)position.X, (float)position.Y);
                    PressedPositionRelativeToControl = PressedPosition - SelectedControl.Visual.GlobalBounds.Position;

                    PinPressed?.Invoke(PressedRect, PressedPosition);
                }
                else if (BottomRightHandleRect.Contains(position))
                {
                    PressedRect = PinType.BottomRight;
                    PressedPosition = new Vector2((float)position.X, (float)position.Y);
                    PressedPositionRelativeToControl = PressedPosition - SelectedControl.Visual.GlobalBounds.Position;
                    
                    PinPressed?.Invoke(PressedRect, PressedPosition);
                }
                else if (RotationHandleRect.Contains(position))
                {
                    PressedRect = PinType.Rotation;
                    PressedPosition = new Vector2((float)position.X, (float)position.Y);
                    PressedPositionRelativeToControl = PressedPosition - SelectedControl.Visual.GlobalBounds.Position;
                    
                    PinPressed?.Invoke(PressedRect, PressedPosition);
                }
                else
                {
                    PressedRect = PinType.Movement;
                    PressedPosition = new Vector2((float)position.X, (float)position.Y);
                    PressedPositionRelativeToControl = PressedPosition - SelectedControl.Visual.GlobalBounds.Position;
                    
                    PinPressed?.Invoke(PressedRect, PressedPosition);
                }
            }
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        PressedRect = PinType.None;
        PinReleased?.Invoke();
    }
}