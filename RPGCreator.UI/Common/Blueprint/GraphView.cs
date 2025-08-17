using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;
using AvaloniaEdit.Utils;
using RPGCreator.UI.Common.Blueprint;

public sealed class GraphView : Control
{
    private readonly Canvas _root = new();
    private readonly Canvas _links = new();
    private readonly Canvas _nodes = new();
    private readonly Canvas _overlay = new();

    private Matrix _view = Matrix.Identity;
    private GraphDocument? _doc;

    private readonly Dictionary<string, NodeControl> _nodeCtrls = new();
    private readonly List<LinkControl> _linkCtrls = new();
    private (NodeControl node, PortControl port)? _linking;
    private LinkControl? _previewLink;
    private Point _previewLinkPosition;

    public GraphView()
    {
        _root.Children.Add(_links);
        _root.Children.Add(_nodes);
        _root.Children.Add(_overlay);
        VisualChildren.Add(_root);
        LogicalChildren.Add(_root);

        PointerWheelChanged += OnWheel;
        PointerPressed += OnPointerDown;
        PointerMoved += OnPointerMove;
        PointerReleased += OnPointerUp;
    }

    private void OnPointerMove(object? sender, PointerEventArgs e)
    {
        if (_linking is { } start && _previewLink == null)
        {
            _previewLinkPosition = e.GetPosition(_nodes);
            AddPreviewLinkControl(start.node, start.port, e.GetPosition(this));
        } else if (_previewLink != null)
        {
            // update preview 
            _previewLinkPosition = e.GetPosition(_nodes);
            _previewLink.InvalidateVisual();
        }
    }

    private void OnPointerDown(object? sender, PointerPressedEventArgs e)
    {
        return;
    }

    public void SetDocument(GraphDocument doc)
    {
        if (_doc != null) Unsubscribe(_doc);
        _doc = doc;
        Subscribe(doc);

        // rebuild
        _nodes.Children.Clear();
        _links.Children.Clear();
        _nodeCtrls.Clear();
        _linkCtrls.Clear();

        foreach (var n in doc.Nodes.Values) AddNodeControl(n);
        foreach (var l in doc.Links) AddLinkControl(l);
        InvalidateArrange();
    }

    private void Subscribe(GraphDocument d)
    {
        d.NodeAdded += AddNodeControl;
        d.NodeRemoved += RemoveNodeControl;
        d.LinkAdded += AddLinkControl;
        d.LinkRemoved += RemoveLinkControl;
        d.NodeMoved += OnNodeMoved;
    }
    private void Unsubscribe(GraphDocument d)
    {
        d.NodeAdded -= AddNodeControl;
        d.NodeRemoved -= RemoveNodeControl;
        d.LinkAdded -= AddLinkControl;
        d.LinkRemoved -= RemoveLinkControl;
        d.NodeMoved -= OnNodeMoved;
    }

    private void AddNodeControl(Node n)
    {
        var ctrl = new NodeControl(n, BeginLinkFromPort);
        _nodeCtrls[n.Id] = ctrl;
        Canvas.SetLeft(ctrl, n.X);
        Canvas.SetTop(ctrl, n.Y);
        _nodes.Children.Add(ctrl);
        ctrl.Moved += (dx,dy) => _doc!.MoveNode(n.Id, n.X+dx, n.Y+dy);
    }

    private void RemoveNodeControl(Node n)
    {
        var ctrl = _nodeCtrls[n.Id];
        _nodes.Children.Remove(ctrl);
        _nodeCtrls.Remove(n.Id);
        // remove all link controls attached to this node
        foreach (var lp in _linkCtrls.Where(l => l.IsAttachedTo(n.Id)).ToList())
            _links.Children.Remove(lp);
        _linkCtrls.RemoveAll(l => l.IsAttachedTo(n.Id));
        // delete all links attached to this node
        _doc?.Links.RemoveAll(l => l.FromNodeId == n.Id || l.ToNodeId == n.Id);
    }

    private void AddLinkControl(Link l)
    {
        var lp = new LinkControl(
            () => GetPortScreenPoint(l.FromNodeId, l.FromPortId),
            () => GetPortScreenPoint(l.ToNodeId, l.ToPortId),
            l.FromNodeId, 
            l.ToNodeId
        );
        _linkCtrls.Add(lp);
        _links.Children.Add(lp);
    }

    private void AddPreviewLinkControl(NodeControl node, PortControl port, Point p)
    {
        _previewLink = new LinkControl(
            () => port.Def.IsInput? _previewLinkPosition : GetPortScreenPoint(node.Node.Id, port.Def.Id),
            () => port.Def.IsInput? GetPortScreenPoint(node.Node.Id, port.Def.Id) : _previewLinkPosition,
            node.Node.Id,
            port.Def.Id
        );
        _overlay.Children.Add(_previewLink);
        _previewLink.InvalidateVisual();
    }

    private void RemoveLinkControl(Link l)
    {
        var idx = _linkCtrls.FindIndex(x => x.Matches(l));
        if (idx >= 0) { _links.Children.Remove(_linkCtrls[idx]); _linkCtrls.RemoveAt(idx); }
    }

    private void OnNodeMoved(Node n)
    {
        if (_nodeCtrls.TryGetValue(n.Id, out var c))
        {
            Canvas.SetLeft(c, n.X);
            Canvas.SetTop(c, n.Y);
        }
        foreach (var lp in _linkCtrls.Where(l => l.IsAttachedTo(n.Id)).ToList())
        {
            lp.InvalidateVisual();
        }
    }

    private Point GetPortScreenPoint(string nodeId, string portId)
    {
        var node = _nodeCtrls[nodeId];
        var port = node.GetPortControl(portId);
        var pLocal = new Point(port.Bounds.Width/2, port.Bounds.Height/2);
        var pNode = port.TranslatePoint(pLocal, _nodes) ?? default;
        pNode = pNode.WithX(pNode.X + (port.Def.IsInput ? -pLocal.X : pLocal.X));
        return pNode;
    }

    // Pan/zoom
    protected override Size ArrangeOverride(Size finalSize)
    {
        _root.RenderTransform = new MatrixTransform(_view);
        _root.Arrange(new Rect(finalSize));
        return finalSize;
    }
    private void OnWheel(object? s, PointerWheelEventArgs e)
    {
        var p = e.GetPosition(this);
        var f = e.Delta.Y > 0 ? 1.1 : 1/1.1;
        _view = Matrix.CreateTranslation(-p.X, -p.Y) * _view;
        _view = Matrix.CreateScale(f, f) * _view;
        _view = Matrix.CreateTranslation(p.X, p.Y) * _view;
        InvalidateArrange();
    }

    // Linking
    private void BeginLinkFromPort(NodeControl node, PortControl port)
    {
        _linking = (node, port);
    }
    private void OnPointerUp(object? s, PointerReleasedEventArgs e)
    {
        if (_linking is { } start)
        {
            var pos = e.GetPosition(_nodes);
            var hit = HitTestPort(pos);
            if (hit is { } end && PortsAreCompatible(start.port.Def, end.Def))
            {
                _doc!.AddLink(start.port.Def.IsInput
                    ? new Link(end.ParentNode.Node.Id, end.Def.Id, start.node.Node.Id, start.port.Def.Id)
                    : new Link(start.node.Node.Id, start.port.Def.Id, end.ParentNode.Node.Id, end.Def.Id));
            }
            _linking = null;
            _previewLink = null;
            _overlay.Children.Clear();
        }
    }

    private PortControl? HitTestPort(Point p) => _nodes.GetVisualAt(p) as PortControl;

    private bool PortsAreCompatible(Port a, Port b)
    {
        if(a.IsInput == b.IsInput) 
            return false; // can't link input to input or output to output
        
        return a.Kind == b.Kind ? true : (a.Kind == PortKind.Value ? a.ValueType == b.ValueType : true);
    }
}