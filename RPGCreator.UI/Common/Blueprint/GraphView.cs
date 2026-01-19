using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;
using RPGCreator.Core.Types.Blueprint;
using RPGCreator.SDK.Graph;
using RPGCreator.SDK.Logging;

namespace RPGCreator.UI.Common.Blueprint;

public sealed class GraphView : Control
{
    public readonly Grid _root = new();
    private readonly Canvas _content = new(){Name = "_contentCanvas"};
    private readonly Canvas _links = new(){Name = "_linksCanvas"};
    private readonly Canvas _nodes = new(){Name = "_nodesCanvas"};
    private readonly Canvas _overlay = new(){Name = "_overlayCanvas"};
    private readonly Canvas _ui = new(){Name = "_uiCanvas"};
    
    private readonly Border _hitbox = new() { Background = Brushes.Transparent }; 

    private Matrix _view = Matrix.Identity;
    private GraphDocument? _doc;

    private readonly Dictionary<string, NodeControl> _nodeCtrls = new();
    private readonly List<LinkControl> _linkCtrls = new();
    private (NodeControl node, PortControl port)? _linking;
    private LinkControl? _previewLink;
    private Point _previewLinkPosition;

    private Point? _lastPointerPosition;

    private bool _isMovingView = false;
    private bool _rightClicking = false;
    
    public GraphView()
    {
        _content.Children.Add(_links);
        _content.Children.Add(_nodes);
        _content.Children.Add(_overlay);
        
        _root.Children.Add(_hitbox);
        _root.Children.Add(_content);
        _root.Children.Add(_ui);
        VisualChildren.Add(_root);
        LogicalChildren.Add(_root);

        PointerWheelChanged += OnWheel;
        PointerPressed += OnPointerDown;
        PointerMoved += OnPointerMove;
        PointerReleased += OnPointerUp;
    }
    public Point ScreenToWorld(Point pScreen)
    {
        if (!_view.TryInvert(out var inv))
            return pScreen; // fallback
        return inv.Transform(pScreen);
    }

    public Vector ScreenDeltaToWorld(Vector dScreen)
    {
        if (!_view.TryInvert(out var inv))
            return dScreen;

        // Convert the delta by transforming the origin and the end point, then subtracting
        var originW = inv.Transform(new Point(0, 0));
        var endW    = inv.Transform(new Point(dScreen.X, dScreen.Y));
        return endW - originW;
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
        else
        {
            if (_rightClicking)
            {
                _isMovingView = true;
                // Move the view
                var delta = e.GetCurrentPoint(_hitbox).Position;
                if (_lastPointerPosition == null)
                {
                    _lastPointerPosition = delta;
                }
                delta -= _lastPointerPosition.Value;
                delta /= _view.M11; // scale by current zoom level
                if (delta.X == 0 && delta.Y == 0) return; // no movement
                _lastPointerPosition = e.GetCurrentPoint(_hitbox).Position;
                if (delta != default)
                {
                    _view = Matrix.CreateTranslation(delta.X, delta.Y) * _view;
                    InvalidateArrange();
                    Logger.Debug("GraphView.OnPointerMove: View moved by {Delta}", delta);
                    e.Handled = true;
                }
            }
        }
    }

    private void OnPointerDown(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(_hitbox).Properties.IsRightButtonPressed)
        {
            _rightClicking = true;
            Logger.Info("GraphView.OnPointerDown: Right click detected, clearing link state.");
            e.Handled = true;
        }
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
        
        _ui.Children.Clear();
        var leftBar = new GraphViewLeftBar(_doc);
        _ui.Children.Add(leftBar);

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
        var ctrl = new NodeControl(n, BeginLinkFromPort, RemovingNode);
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
        var links = _doc?.Links.Where(l => l.FromNodeId == n.Id || l.ToNodeId == n.Id).ToList();
        
        if (links != null)
        {
            foreach (var l in links)
            {
                _doc?.RemoveLink(l);
            }
        }
    }

    private void AddLinkControl(Link l)
    {
        if(l.FromNodeId == l.ToNodeId)
        {
            // self-link, skip for now
            return;
        }
        var lp = new LinkControl(
            () => GetPortScreenPoint(l.FromNodeId, l.FromPortId),
            () => GetPortScreenPoint(l.ToNodeId, l.ToPortId),
            GetPortKind(l.FromNodeId, l.FromPortId),
            GetPortKind(l.ToNodeId, l.ToPortId),
            l.FromNodeId, 
            l.ToNodeId,
            l.FromPortId,
            l.ToPortId
        );
        
        lp.BreakLink += (s, e) => _doc?.RemoveLink(l);
        
        // Get Port Controls
        if (_nodeCtrls.TryGetValue(l.FromNodeId, out var fromNode) &&
            fromNode.GetPortControl(l.FromPortId) is PortControl fromPort &&
            _nodeCtrls.TryGetValue(l.ToNodeId, out var toNode) &&
            toNode.GetPortControl(l.ToPortId) is PortControl toPort)
        {
            if(fromPort is IPortInput portInput)
            {
                portInput.OnAttached();
                foreach (var lp0 in _linkCtrls.Where(lc => lc.IsAttachedTo(l.FromNodeId) && lc != lp).ToList())
                {
                    lp0.InvalidateVisual();
                }
            }
            if(toPort is IPortInput portInput2)
            {
                portInput2.OnAttached();
                foreach (var lp0 in _linkCtrls.Where(lc => lc.IsAttachedTo(l.ToNodeId) && lc != lp).ToList())
                {
                    lp0.InvalidateVisual();
                }
            }
        }

        _linkCtrls.Add(lp);
        _links.Children.Add(lp);
    }

    private void AddPreviewLinkControl(NodeControl node, PortControl port, Point p)
    {
        _previewLink = new PreviewLinkControl(
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
        if (idx >= 0)
        {
            _links.Children.Remove(_linkCtrls[idx]);
            _linkCtrls.RemoveAt(idx);
            // Get Port Controls
            if (_nodeCtrls.TryGetValue(l.FromNodeId, out var fromNode) &&
                fromNode.GetPortControl(l.FromPortId) is PortControl fromPort)
            {
                if(fromPort is IPortInput portInput)
                {
                    portInput.OnDetached();
                    foreach (var lp0 in _linkCtrls.Where(lc => lc.IsAttachedTo(l.FromNodeId)).ToList())
                    {
                        lp0.InvalidateVisual();
                    }
                }
            }
            if(_nodeCtrls.TryGetValue(l.ToNodeId, out var toNode) &&
                toNode.GetPortControl(l.ToPortId) is PortControl toPort)
            {

                if (toPort is IPortInput portInput2)
                {
                    portInput2.OnDetached();
                    foreach (var lp0 in _linkCtrls.Where(lc => lc.IsAttachedTo(l.ToNodeId)).ToList())
                    {
                        lp0.InvalidateVisual();
                    }
                }
            }
        }
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
    private PortKind GetPortKind(string nodeId, string portId)
    {
        if (_nodeCtrls.TryGetValue(nodeId, out var ctrl))
        {
            return ctrl.GetPortControl(portId).Def.Kind;
        }
        throw new KeyNotFoundException($"Node {nodeId} or port {portId} not found.");
    }

    // Pan/zoom
    protected override Size ArrangeOverride(Size finalSize)
    {
        _content.RenderTransform = new MatrixTransform(_view);
        _content.Arrange(new Rect(finalSize));
        _root.Arrange(new Rect(finalSize));
        _hitbox.Arrange(new Rect(finalSize));
        return finalSize;
    }
    protected override Size MeasureOverride(Size availableSize)
    {
        _root.Measure(availableSize);
        return _root.DesiredSize;
    }
    
    private void OnWheel(object? s, PointerWheelEventArgs e)
    {
        Logger.Debug("GraphView.OnWheel: {Delta}", e.Delta);
        var p = e.GetPosition(_hitbox);
        var f = e.Delta.Y > 0 ? 1.1 : 1/1.1;
        // _view = Matrix.CreateTranslation(-p.X, -p.Y) * _view;
        
        // Max zoom out to 0.1, max zoom in to 5
        if (_view.M11 * f < 0.1 || _view.M11 * f > 5)
        {
            Logger.Debug("GraphView.OnWheel: Zoom Limit reached, ignoring.");
            return;
        }
        _view = Matrix.CreateScale(f, f) * _view;
        // _view = Matrix.CreateTranslation(p.X, p.Y) * _view;
        InvalidateArrange();
    }

    // Removing node
    private void RemovingNode(Node node)
    {
        _doc.RemoveNode(node.Id);
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
        } else if (!e.GetCurrentPoint(_hitbox).Properties.IsRightButtonPressed && _rightClicking && !_isMovingView)
        {
            _rightClicking = false;
            // Right click released, and wasn't moving the view, We need to show the context menu
            var pos = e.GetPosition(_overlay);
            Logger.Info("GraphView.OnPointerUp: Right click released at {Position}, showing context menu.", pos);
            var menu = new GraphViewCtxMenu(_doc);
            
            var newPos = pos;
            menu.Open(_root, newPos);
            e.Handled = true;
        }

        _lastPointerPosition = null;
        _isMovingView = false;
        _rightClicking = false;
    }

    private PortControl? HitTestPort(Point p) => _nodes.GetVisualAt(p) as PortControl;

    private bool PortsAreCompatible(Port a, Port b)
    {
        if(a.IsInput == b.IsInput) 
            return false; // can't link input to input or output to output
        
        return a.Kind == b.Kind ? true : (a.Kind == PortKind.Value ? a.ValueType == b.ValueType : true);
    }
}