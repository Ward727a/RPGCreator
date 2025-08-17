using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using AvaloniaEdit.Utils;
using Point = RPGCreator.Core.Type.Internal.Point;

namespace RPGCreator.UI.Common.Blueprint;

public sealed class NodeControl : Control
{
    public Node Node { get; }
    private readonly Action<NodeControl, PortControl> _beginLink;
    private readonly StackPanel _left = new(){ Spacing=4 };
    private readonly StackPanel _right = new(){ Spacing=4 };

    public NodeControl(Node node, Action<NodeControl,PortControl> beginLink)
    {
        Node = node;
        _beginLink = beginLink;
        var border = new Border
        {
            CornerRadius=new(8), 
            BorderThickness=new(1), 
            Padding=new(8),
            Background = Brushes.DarkSlateGray
        };
        var grid = new Grid{
            ColumnDefinitions = { new ColumnDefinition(), new ColumnDefinition() },
            RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto) }
        };
        var title = new TextBlock
        {
            Text=node.Title,
            FontWeight=FontWeight.Bold,
            Margin=new Thickness(0,0,0,8),
            Foreground = Brushes.White
        };
        Grid.SetColumnSpan(title, 2);
        grid.Children.Add(title);
        
        Grid.SetRow(_left,1);
        Grid.SetColumn(_left,0);
        grid.Children.Add(_left);
        
        Grid.SetRow(_right,1);
        Grid.SetColumn(_right,1);
        grid.Children.Add(_right);
        border.Child = grid;
        this.VisualChildren.Add(border);

        foreach (var p in node.Inputs)
        {
            _left.Children.Add(
                new PortControl(p, this, _beginLink, isOutput:false)
            );
        }

        foreach (var p in node.Outputs)
        {
            _right.Children.Add(
                new PortControl(p, this, _beginLink, isOutput:true)
            );
        }

        PointerPressed += StartDrag;
        PointerMoved += Dragging;
        PointerReleased += EndDrag;
    }

    public PortControl GetPortControl(string portId)
    {
        foreach (var c in _left.Children) if (c is PortControl pc && pc.Def.Id==portId) return pc;
        foreach (var c in _right.Children) if (c is PortControl pc && pc.Def.Id==portId) return pc;
        throw new KeyNotFoundException(portId);
    }

    private Point _grab, _start;
    public event Action<double,double>? Moved;
    private void StartDrag(object? s, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            this.CapturePointer(e.Pointer);
            
            _grab = e.GetPosition(Parent as Visual);
            _start = new Point(Node.X, Node.Y);
            e.Handled = true;
        }
    }
    private void Dragging(object? s, PointerEventArgs e)
    {
        if (e.Pointer.Captured == this)
        {
            var p = e.GetPosition(Parent as Visual);
            
            var dx = p.X - _grab.X;
            var dy = p.Y - _grab.Y;
            _grab = p;
            Moved?.Invoke(dx, dy);
            e.Handled = true;
        }
    }
    private void EndDrag(object? s, PointerReleasedEventArgs e) => this.ReleasePointerCapture(e.Pointer);
}
public sealed class PortControl : Control
{
    public Port Def { get; }
    public NodeControl ParentNode { get; }
    private readonly Action<NodeControl, PortControl> _beginLink;
    private readonly bool _isOutput;
    
    private double _width = 0;

    public PortControl(Port def, NodeControl parent, Action<NodeControl,PortControl> beginLink, bool isOutput)
    {
        Def = def;
        ParentNode = parent;
        _beginLink = beginLink;
        _isOutput = isOutput;
        Height = 16;
        var name = new FormattedText(Def.Name, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, 12, Brushes.White);
        Width = name.Width + 32; // 16px on each side for padding
        PointerPressed += (s, e) =>
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                _beginLink(ParentNode, this);
                e.Handled=true;
            }
        };
    }

    public override void Render(DrawingContext ctx)
    {
        var r = new Rect(Bounds.Size);
        var color = Def.Kind==PortKind.Exec ? Brushes.Orange : Brushes.SkyBlue;
        ctx.DrawEllipse(color, new Pen(Brushes.Black,1), new((_isOutput ? r.Right : r.Left ), r.Center.Y) , 6, 6);
        var name = new FormattedText(Def.Name, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, 12, Brushes.White);
        var x = !_isOutput ? 16 : (Bounds.Width - name.Width - 16);
        ctx.DrawText(name, new Point(x, (Bounds.Height - name.Height)/2));
    }
}