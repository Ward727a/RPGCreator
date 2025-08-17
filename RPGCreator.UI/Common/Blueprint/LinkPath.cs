using System;
using Avalonia.Controls;
using Avalonia.Media;
using RPGCreator.Core.Type.Internal;

namespace RPGCreator.UI.Common.Blueprint;

public sealed class LinkControl : Control
{
    private readonly Func<Point> _from;
    private readonly Func<Point> _to;
    private string? _sourceId;
    private string? _targetId;

    public LinkControl(Func<Point> from, Func<Point> to, string? sourceId = null, string? targetId = null)
    {
        _sourceId = sourceId;
        _targetId = targetId;
        _from = from;
        _to = to;
    }

    public override void Render(DrawingContext ctx)
    {
        var p0 = _from();
        var p3 = _to();

        var dx = Math.Max(40, Math.Abs(p3.X - p0.X) * 0.5);
        var p1 = new Point(p0.X + dx, p0.Y);
        var p2 = new Point(p3.X - dx, p3.Y);

        var g = new StreamGeometry();
        using (var c = g.Open())
        {
            c.BeginFigure(p0, false);
            c.CubicBezierTo(p1, p2, p3);
        }

        ctx.DrawGeometry(null, new Pen(Brushes.LightGray, 2), g);
    }
    public bool Matches(Link l) => false;
    public bool IsAttachedTo(string nodeId) => 
        _sourceId == nodeId || _targetId == nodeId;
}