using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using RPGCreator.Core.Types.Blueprint;
using RPGCreator.SDK.Graph.Ports;
using RPGCreator.UI.Extensions;

namespace RPGCreator.UI.Common.Blueprint;

public class LinkControl : Control
{
    protected const double MinLength = 40;
    protected const double LinkThickness = 2;

    public event EventHandler? BreakLink;
    
    protected readonly Func<Point> _from;
    protected readonly Func<Point> _to;
    protected PortKind _sourceKind;
    protected PortKind _targetKind;
    protected string? _sourceId;
    protected string? _targetId;
    protected string? _sourcePortId, _targetPortId;

    public LinkControl(
        Func<Point> from, Func<Point> to,
        PortKind sourceKind = PortKind.Exec,
        PortKind targetKind = PortKind.Value,
        string? sourceId = null, string? targetId = null,
        string? sourcePortId = null, string? targetPortId = null)
    {
        _sourceId = sourceId;
        _targetId = targetId;
        _sourcePortId = sourcePortId;
        _targetPortId = targetPortId;
        _sourceKind = sourceKind;
        _targetKind = targetKind;
        _from = from;
        _to = to;
    }

    public override void Render(DrawingContext ctx)
    {
        var p0 = _from();
        var p3 = _to();

        var dx = Math.Max(MinLength, Math.Abs(p3.X - p0.X) * 0.5);
        var p1 = new Point(p0.X + dx, p0.Y);
        var p2 = new Point(p3.X - dx, p3.Y);

        var g = new StreamGeometry();
        using (var c = g.Open())
        {
            c.BeginFigure(p0, false);
            c.CubicBezierTo(p1, p2, p3);
        }
        var brush = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(p0, RelativeUnit.Absolute),
            EndPoint   = new RelativePoint(p3, RelativeUnit.Absolute),
            GradientStops = new GradientStops
            {
                new GradientStop(_sourceKind.GetColor().ToAvalonia(), 0),
                new GradientStop(_targetKind.GetColor().ToAvalonia(),  .7),
            }
        };
        
        ctx.DrawGeometry(null, new Pen(brush, LinkThickness), g);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            BreakLink?.Invoke(this, e);
            e.Handled = true;
        }
    }

    public bool Matches(Link l)
    {
        return _sourceId == l.FromNodeId && _targetId == l.ToNodeId
        && _sourcePortId == l.FromPortId && _targetPortId == l.ToPortId;
    }
    public bool IsAttachedTo(string nodeId) => 
        _sourceId == nodeId || _targetId == nodeId;
}
public class PreviewLinkControl : LinkControl
{
    private const double DashLength = 6;
    private const double DashSpace = 3;
    private const double DashOffset = 0;

    public PreviewLinkControl(Func<Point> from, Func<Point> to,
        string? sourceId = null, string? targetId = null,
        string? sourcePortId = null, string? targetPortId = null)
        : base(from, to, sourceId:sourceId, targetId:targetId, sourcePortId:sourcePortId, targetPortId:targetPortId)
    {
    }

    public override void Render(DrawingContext ctx)
    {
        var p0 = _from();
        var p3 = _to();

        var dx = Math.Max(MinLength, Math.Abs(p3.X - p0.X) * 0.5);
        var p1 = new Point(p0.X + dx, p0.Y);
        var p2 = new Point(p3.X - dx, p3.Y);

        var g = new StreamGeometry();
        using (var c = g.Open())
        {
            c.BeginFigure(p0, false);
            c.CubicBezierTo(p1, p2, p3);
        }
        var pen = new Pen(Brushes.LightGray, LinkThickness)
        {
            
            // motif: 6px de trait, 3px d’espace, etc.
            DashStyle = new DashStyle(new double[] { DashLength, DashSpace }, DashOffset),
            LineCap   = PenLineCap.Round
        };

        ctx.DrawGeometry(null, pen, g);
    }
    public bool Matches(Link l) => false;
    public bool IsAttachedTo(string nodeId) => 
        _sourceId == nodeId || _targetId == nodeId;
}