using System;
using System.Collections.Generic;

namespace RPGCreator.UI.Common.Blueprint;

public sealed class GraphDocument
{
    public readonly Dictionary<string, Node> Nodes = new();
    public readonly List<Link> Links = new();

    public event Action<Node>? NodeAdded;
    public event Action<Node>? NodeRemoved;
    public event Action<Link>? LinkAdded;
    public event Action<Link>? LinkRemoved;
    public event Action<Node>? NodeMoved;
    
    public Node AddNode(Node n) { Nodes[n.Id] = n; NodeAdded?.Invoke(n); return n; }
    public void RemoveNode(string id) { if (Nodes.Remove(id, out var n)) NodeRemoved?.Invoke(n); }
    public void MoveNode(string id, double x, double y) { var n = Nodes[id]; n.X=x; n.Y=y; NodeMoved?.Invoke(n); }
    public void AddLink(Link l) { Links.Add(l); LinkAdded?.Invoke(l); }
    public void RemoveLink(Link l) { Links.Remove(l); LinkRemoved?.Invoke(l); }
}

public sealed class Node
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public string Type { get; set; } = "";
    public string Title { get; set; } = "Node";
    public double X, Y;
    public readonly List<Port> Inputs = new();
    public readonly List<Port> Outputs = new();
}

public enum PortKind { Exec, Value }

public sealed class Port
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "";
    public PortKind Kind { get; set; }
    public string ValueType { get; set; } = ""; // "float", "bool", etc.
    public bool IsInput { get; set; } = false;
}

public sealed record Link(string FromNodeId, string FromPortId, string ToNodeId, string ToPortId);
