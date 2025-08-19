using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media;
using AvaloniaEdit.Utils;
using RPGCreator.Core.Type.Blueprint;
using RPGCreator.Core.Type.Blueprint.Nodes.Debug;
using Serilog;
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
            BorderBrush = Brushes.DimGray,
            Background = new SolidColorBrush(
                Color.FromRgb(45, 50, 60)),
        };
        var grid = new Grid{
            ColumnDefinitions = { new ColumnDefinition(), new ColumnDefinition() },
            RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto) }
        };
        var titleBorder = new Border
        {
            Background = Brushes.Black,
            BorderThickness = new Thickness(0),
            CornerRadius = new CornerRadius(8, 8, 0, 0),
            Padding = new Thickness(8, 8),
            Margin = new Thickness(0, 0, 0, 8)
        };
        grid.Children.Add(titleBorder);
        Grid.SetRow(titleBorder, 0);
        Grid.SetColumnSpan(titleBorder, 2);
        var title = new TextBlock
        {
            Text=node.Title,
            FontWeight=FontWeight.Bold,
            TextAlignment = TextAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Foreground = Brushes.White,
        };
        titleBorder.Child = title;
        
        Grid.SetRow(_left,1);
        Grid.SetColumn(_left,0);
        grid.Children.Add(_left);
        
        Grid.SetRow(_right,1);
        Grid.SetColumn(_right,1);
        grid.Children.Add(_right);
        border.Child = grid;
        this.VisualChildren.Add(border);
        this.LogicalChildren.Add(border);

        foreach (var p in node.Inputs)
        {
            switch (p.Kind)
            {
                case PortKind.Boolean:
                    if(!p.AllowManualInput)
                        goto default;
                    _left.Children.Add(
                        new PortBoolInputControl(p, this, _beginLink, isOutput:false));
                    break;
                case PortKind.String:
                    if(!p.AllowManualInput)
                        goto default;
                    _left.Children.Add(
                        new PortTextInputControl(p, this, _beginLink, isOutput:false));
                    break;
                case PortKind.Number:
                    if(!p.AllowManualInput)
                        goto default;
                    _left.Children.Add(
                        new PortNumInputControl(p, this, _beginLink, isOutput:false)
                    );
                    break;
                case PortKind.Enum:
                    if (!p.AllowManualInput)
                        goto default;
                    _left.Children.Add(
                        new PortEnumInputControl(p, this, _beginLink, isOutput:false)
                    );
                    break;
                default:
                    _left.Children.Add(
                        new PortControl(p, this, _beginLink, isOutput:false)
                    );
                    break;
            }
        }

        foreach (var p in node.Outputs)
        {
            switch (p.Kind)
            {
                case PortKind.Boolean:
                    if(!p.AllowManualInput)
                        goto default;
                    _right.Children.Add(
                        new PortBoolInputControl(p, this, _beginLink, isOutput:true));
                    break;
                case PortKind.String:
                    if(!p.AllowManualInput)
                        goto default;
                    _right.Children.Add(
                        new PortTextInputControl(p, this, _beginLink, isOutput:true));
                    break;
                case PortKind.Number:
                    if(!p.AllowManualInput)
                        goto default;
                    _right.Children.Add(
                        new PortNumInputControl(p, this, _beginLink, isOutput:true)
                    );
                    break;
                case PortKind.Enum:
                    if (!p.AllowManualInput)
                        goto default;
                    _right.Children.Add(
                        new PortEnumInputControl(p, this, _beginLink, isOutput:true)
                    );
                    break;
                default:
                    _right.Children.Add(
                        new PortControl(p, this, _beginLink, isOutput:true)
                    );
                    break;
            }
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

    private Avalonia.Point _grab, _start;
    public event Action<double,double>? Moved;
    private void StartDrag(object? s, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            this.CapturePointer(e.Pointer);
            
            var _graph = this.FindLogicalAncestorOfType<GraphView>();
            var _hitboxBorder = Parent.FindLogicalAncestorOfType<Grid>().FindLogicalDescendantOfType<Border>();
            // Get the position relative to the parent visual
            if (_hitboxBorder is not Visual parentVisual)
            {
                Log.Error("NodeControl parent is not a Visual, cannot start drag.");
                return;
            }
            
            var pScreen = e.GetPosition(_hitboxBorder);
            _grab = _graph.ScreenToWorld(pScreen);
            
            _start = new Point(Node.X, Node.Y);
            e.Handled = true;
        }
    }
    private void Dragging(object? s, PointerEventArgs e)
    {
        if (e.Pointer.Captured == this)
        {
            var _graph = this.FindLogicalAncestorOfType<GraphView>();
            var _hitboxBorder = Parent.FindLogicalAncestorOfType<Grid>().FindLogicalDescendantOfType<Border>();

            if (_hitboxBorder is not Visual parentVisual)
            {
                Log.Error("NodeControl parent is not a Visual, cannot start drag.");
                return;
            }
            var pScreen = e.GetPosition(_hitboxBorder);
            var currWorld = _graph.ScreenToWorld(pScreen);
            
            var dx = currWorld.X - _grab.X;
            var dy = currWorld.Y - _grab.Y;
            
            _grab = currWorld;
            Moved?.Invoke(dx, dy);
            e.Handled = true;
        }
    }
    private void EndDrag(object? s, PointerReleasedEventArgs e) => this.ReleasePointerCapture(e.Pointer);
}
public class PortControl : Control
{
    public Port Def { get; }
    public NodeControl ParentNode { get; }
    protected readonly Action<NodeControl, PortControl> _beginLink;
    protected readonly bool _isOutput;
    
    protected double _width = 0;

    public PortControl(Port def, NodeControl parent, Action<NodeControl,PortControl> beginLink, bool isOutput)
    {
        Def = def;
        ParentNode = parent;
        _beginLink = beginLink;
        _isOutput = isOutput;
        Height = 22;
        HorizontalAlignment = !_isOutput? HorizontalAlignment.Left : HorizontalAlignment.Right;
        var name = new FormattedText(Def.Name, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, 12, Brushes.White);
        Width = name.Width + 32; // 16px on each side for padding
        PointerPressed += (s, e) =>
        {
            if(e.Handled)
                return; // Already handled, do not start a link
            if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;
            
            _beginLink(ParentNode, this);
            e.Handled=true;
        };
    }

    public override void Render(DrawingContext ctx)
    {
        var r = new Rect(Bounds.Size);
        var color = new SolidColorBrush(Def.Kind.GetColor());
        ctx.DrawEllipse(color, new Pen(Brushes.Black,1), new((_isOutput ? r.Right : r.Left ), r.Center.Y) , 6, 6);
        var name = new FormattedText(Def.Name, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, 12, Brushes.White);
        var x = !_isOutput ? 16 : (Bounds.Width - name.Width - 16);
        ctx.DrawText(name, new Point(x, (Bounds.Height - name.Height)/2));
    }
}

public interface IPortInput
{
    public event Action<IPortInput, string>? ValueChanged; // Port input value changed
    public event Action? PortAttached; // Port input attached to a node
    public event Action? PortDetached; // Port input detached from a node

    public void OnAttached();
    public void OnDetached();
}

public sealed class PortTextInputControl : PortControl, IPortInput
{
    public event Action<IPortInput, string>? ValueChanged;
    public event Action? PortAttached;
    public event Action? PortDetached;
    public void OnAttached()
    {
        PortAttached?.Invoke();
        _inputBox.IsVisible = false;
        Height = 22;
        Width -= 100;
    }

    public void OnDetached()
    {
        PortDetached?.Invoke();
        _inputBox.IsVisible = true;
        Height = 40;
        Width += 100;
    }

    private Grid? _inputPanel;
    private TextBlock? _label;
    private TextBox? _inputBox;
    private StackPanel? _hostPanel;

    private const double CircleR = 6;
    private const double EdgePad = 16;
    private const double MinBoxHeight = 22;

    public PortTextInputControl(Port def, NodeControl parent, Action<NodeControl, PortControl> beginLink, bool isOutput)
        : base(def, parent, beginLink, isOutput)
    {
        Margin = new Thickness(0, 4, 0, 4);
        Width = 100;
        Height = 40;
        var name = new FormattedText(Def.Name, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, 12, Brushes.White);
        Width = name.Width + 32 + 100; // 16px on each side for padding
        
        _inputPanel = new Grid()
        {
            ColumnDefinitions = new ColumnDefinitions("Auto, 4, *"),
            HorizontalAlignment = isOutput ? HorizontalAlignment.Right : HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(!_isOutput?16:8, 0, !_isOutput?8:16, 0),
        };
        this.VisualChildren.Add(_inputPanel);
        this.LogicalChildren.Add(_inputPanel);
        
        _label = new TextBlock
        {
            Text = def.Name,
            Foreground = Brushes.White,
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Left,
            FontSize = 12,
        };
        _inputPanel.Children.Add(_label);
        
        _inputBox = new TextBox
        {
            Text = def.Value as string,
            Width = 100,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Padding = new Thickness(4, 0),
            BorderThickness = new Thickness(1),
        };
        _inputBox.TextChanged += (s, e) =>
        {
            def.Value = _inputBox.Text;
            ValueChanged?.Invoke(this, _inputBox.Text);
        };
        _inputPanel.Children.Add(_inputBox);
        Grid.SetColumn(_inputBox, 2);
        
        _inputPanel.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        this.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));  
        if(def.Kind == PortKind.Exec)
            Log.Error("PortNumInputControl should not be Exec type, but got: {Def}", def.Kind);
    }

    public override void Render(DrawingContext ctx)
    {
        var r = new Rect(Bounds.Size);
        var color = new SolidColorBrush(Def.Kind.GetColor());

        var cx = _isOutput ? r.Right : r.Left;
        var cy = r.Center.Y;

        ctx.DrawEllipse(color, new Pen(Brushes.Black, 1), new Point(cx, cy), CircleR, CircleR);
    }
}
public sealed class PortNumInputControl : PortControl, IPortInput
{
    public event Action<IPortInput, string>? ValueChanged;
    public event Action? PortAttached;
    public event Action? PortDetached;
    public void OnAttached()
    {
        PortAttached?.Invoke();
        _inputBox.IsVisible = false;
        
    }

    public void OnDetached()
    {
        PortDetached?.Invoke();
        _inputBox.IsVisible = true;
    }
    
    private NumericUpDown? _inputBox;
    private StackPanel? _hostPanel;

    private const double CircleR = 6;
    private const double EdgePad = 16;
    private const double MinBoxHeight = 22;

    public PortNumInputControl(Port def, NodeControl parent, Action<NodeControl, PortControl> beginLink, bool isOutput)
        : base(def, parent, beginLink, isOutput)
    {
        // Rien dans le visuel ici: on hostera la TextBox dans le parent Panel
        // AttachedToVisualTree += OnAttached;
        // DetachedFromVisualTree += OnDetached;
        Height = 32;
        Width = 100;
        Margin = new Thickness(0, 4, 0, 4);
        _inputBox = new NumericUpDown
        {
            Value = decimal.TryParse(Def.Name, out var result) ? result : (decimal)0.0,
            Foreground = Brushes.White,
            FontSize = 12,
            Padding = new Thickness(4, 0),
            Margin = new Thickness(!_isOutput?16:8, 4, !_isOutput?8:16, 4),
            BorderThickness = new Thickness(1),
        };

        this.VisualChildren.Add(_inputBox);
        this.LogicalChildren.Add(_inputBox);
        
        if(def.Kind == PortKind.Exec)
            Log.Error("PortNumInputControl should not be Exec type, but got: {Def}", def.Kind);
    }

    public override void Render(DrawingContext ctx)
    {
        var r = new Rect(Bounds.Size);
        var color = new SolidColorBrush(Def.Kind.GetColor());

        var cx = _isOutput ? r.Right : r.Left;
        var cy = r.Center.Y;

        ctx.DrawEllipse(color, new Pen(Brushes.Black, 1), new Point(cx, cy), CircleR, CircleR);
    }
}
public sealed class PortBoolInputControl : PortControl, IPortInput
{
    public event Action<IPortInput, string>? ValueChanged;
    public event Action? PortAttached;
    public event Action? PortDetached;
    public void OnAttached()
    {
        PortAttached?.Invoke();
        _inputBox.IsVisible = false;
    }

    public void OnDetached()
    {
        PortDetached?.Invoke();
        _inputBox.IsVisible = true;
    }

    private Grid? _inputPanel;
    private TextBlock? _label;
    private CheckBox? _inputBox;
    private StackPanel? _hostPanel;

    private const double CircleR = 6;
    private const double EdgePad = 16;
    private const double MinBoxHeight = 22;

    public PortBoolInputControl(Port def, NodeControl parent, Action<NodeControl, PortControl> beginLink, bool isOutput)
        : base(def, parent, beginLink, isOutput)
    {
        // Rien dans le visuel ici: on hostera la TextBox dans le parent Panel
        // AttachedToVisualTree += OnAttached;
        // DetachedFromVisualTree += OnDetached;
        
        Margin = new Thickness(0, 4, 0, 4);
        Height = 22;
        var name = new FormattedText(Def.Name, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, 12, Brushes.White);
        
        _inputPanel = new Grid()
        {
            ColumnDefinitions = new ColumnDefinitions("Auto, 4, *"),
            HorizontalAlignment = isOutput ? HorizontalAlignment.Right : HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(!_isOutput?16:8, 0, !_isOutput?8:16, 0),
        };
        this.VisualChildren.Add(_inputPanel);
        this.LogicalChildren.Add(_inputPanel);
        
        _label = new TextBlock
        {
            Text = def.Name,
            Foreground = Brushes.White,
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Left,
            FontSize = 12,
        };
        _inputPanel.Children.Add(_label);
        
        _inputBox = new CheckBox()
        {
            Foreground = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Padding = new Thickness(4, 0),
            BorderThickness = new Thickness(1),
        };
        _inputPanel.Children.Add(_inputBox);
        Grid.SetColumn(_inputBox, 2);
        
        Width = name.Width + 32 + 16; // 16px on each side for padding
        
        _inputPanel.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        this.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));  
        if(def.Kind == PortKind.Exec)
            Log.Error("PortNumInputControl should not be Exec type, but got: {Def}", def.Kind);
    }

    public override void Render(DrawingContext ctx)
    {
        var r = new Rect(Bounds.Size);
        var color = new SolidColorBrush(Def.Kind.GetColor());

        var cx = _isOutput ? r.Right : r.Left;
        var cy = r.Center.Y;

        ctx.DrawEllipse(color, new Pen(Brushes.Black, 1), new Point(cx, cy), CircleR, CircleR);
    }
}

public sealed class PortEnumInputControl : PortControl, IPortInput
{
    public event Action<IPortInput, string>? ValueChanged;
    public event Action? PortAttached;
    public event Action? PortDetached;
    public void OnAttached()
    {
        PortAttached?.Invoke();
        _inputBox.IsVisible = false;
        Height = 22;
        Width -= 100;
    }

    public void OnDetached()
    {
        PortDetached?.Invoke();
        _inputBox.IsVisible = true;
        Height = 40;
        Width += 100;
    }

    private Grid? _inputPanel;
    private TextBlock? _label;
    private ComboBox? _inputBox;
    private StackPanel? _hostPanel;

    private const double CircleR = 6;
    private const double EdgePad = 16;
    private const double MinBoxHeight = 22;

    public PortEnumInputControl(Port def, NodeControl parent, Action<NodeControl, PortControl> beginLink, bool isOutput)
        : base(def, parent, beginLink, isOutput)
    {
        Margin = new Thickness(0, 4, 0, 4);
        Width = 100;
        Height = 40;
        var name = new FormattedText(Def.Name, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, 12, Brushes.White);
        Width = name.Width + 32 + 100; // 16px on each side for padding
        
        _inputPanel = new Grid()
        {
            ColumnDefinitions = new ColumnDefinitions("Auto, 4, *"),
            HorizontalAlignment = isOutput ? HorizontalAlignment.Right : HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(!_isOutput?16:8, 0, !_isOutput?8:16, 0),
        };
        this.VisualChildren.Add(_inputPanel);
        this.LogicalChildren.Add(_inputPanel);
        
        _label = new TextBlock
        {
            Text = def.Name,
            Foreground = Brushes.White,
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Left,
            FontSize = 12,
        };
        _inputPanel.Children.Add(_label);
        
        _inputBox = new ComboBox()
        {
            Width = 100,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Padding = new Thickness(4, 0),
            BorderThickness = new Thickness(1),
        };
        
        if (def is EnumPort enumPort)
        {
            foreach (var value in enumPort.EnumType.GetEnumValues())
            {
                _inputBox.Items.Add(value);
            }
            _inputBox.SelectedItem = def.Value;
        
            _inputBox.PointerPressed += (s, e) =>
            {
                e.Handled = true;
            };
            _inputBox.SelectionChanged += (s, e) =>
            {
                def.Value = (_inputBox.SelectedItem as Enum)?.ToString() ?? string.Empty;
                ValueChanged?.Invoke(this, _inputBox.SelectedItem as string ?? string.Empty);
            };
        }
        else
        {
            Log.Error("PortEnumInputControl should be an EnumPort, but got: {Def}", def.Kind);
        }
        _inputPanel.Children.Add(_inputBox);
        Grid.SetColumn(_inputBox, 2);
        
        _inputPanel.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        this.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));  
        if(def.Kind == PortKind.Exec)
            Log.Error("PortNumInputControl should not be Exec type, but got: {Def}", def.Kind);
    }

    public override void Render(DrawingContext ctx)
    {
        var r = new Rect(Bounds.Size);
        var color = new SolidColorBrush(Def.Kind.GetColor());

        var cx = _isOutput ? r.Right : r.Left;
        var cy = r.Center.Y;

        ctx.DrawEllipse(color, new Pen(Brushes.Black, 1), new Point(cx, cy), CircleR, CircleR);
    }
}