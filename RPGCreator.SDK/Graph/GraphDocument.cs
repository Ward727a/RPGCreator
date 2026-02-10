using System.Text;
using RPGCreator.Core.Types.Blueprint;
using RPGCreator.SDK.Graph.Nodes;
using RPGCreator.SDK.Logging;
using RPGCreator.SDK.Serializer;

namespace RPGCreator.SDK.Graph;

public sealed class GraphDocument : ISerializable, IDeserializable
{
    public string SavePath;
    public readonly Dictionary<string, Node> Nodes = new();
    public readonly List<Link> Links = new();

    private Dictionary<string, (System.Type, object)> _GraphVariables = new();
    public IReadOnlyDictionary<string, (System.Type, object)> GraphVariables => _GraphVariables;

    public event Action<string, (System.Type, object)>? GraphVariableAdded;
    public event Action<string>? GraphVariableRemoved;
    public event Action<Node>? NodeAdded;
    public event Action<Node>? NodeRemoved;
    public event Action<Link>? LinkAdded;
    public event Action<Link>? LinkRemoved;
    public event Action<Node>? NodeMoved;
    
    public void AddGraphVariable(string name, System.Type type, object defaultValue)
    {
        _GraphVariables[name] = (type, defaultValue);
        GraphVariableAdded?.Invoke(name, (type, defaultValue));
    }
    public void RemoveGraphVariable(string name)
    {
        if (_GraphVariables.Remove(name))
            GraphVariableRemoved?.Invoke(name);
    }
    public void ClearGraphVariables()
    {
        var keys = _GraphVariables.Keys.ToList();
        _GraphVariables.Clear();
        foreach (var key in keys)
            GraphVariableRemoved?.Invoke(key);
    }
    public Node AddNode(Node n) { Nodes[n.Id] = n; NodeAdded?.Invoke(n); return n; }
    public void RemoveNode(string id) { if (Nodes.Remove(id, out var n)) NodeRemoved?.Invoke(n); }
    public void MoveNode(string id, double x, double y) { var n = Nodes[id]; n.X=x; n.Y=y; NodeMoved?.Invoke(n); }
    public void AddLink(Link l) { Links.Add(l); LinkAdded?.Invoke(l); }
    public void RemoveLink(Link l) { Links.Remove(l); LinkRemoved?.Invoke(l); }

    public void Compile()
    {
        EngineServices.GraphService.Compile(this);
    }

    public void Save(string toFile)
    {
        EngineServices.SerializerService.Serialize(this, out var serializedData);
        File.WriteAllText(toFile, serializedData, Encoding.UTF8);
        SavePath = toFile;
        Logger.Info("GraphDocument saved to {toFile}", toFile);
    }
    
    public static GraphDocument Load(string fromFile)
    {
        var serializedData = File.ReadAllText(fromFile, Encoding.UTF8);
        EngineServices.SerializerService.Deserialize<GraphDocument>(serializedData, out var obj, out var type);
        if (obj is { } doc)
        {
            doc.SavePath = fromFile;
            return doc;
        }
        throw new Exception($"Error while loading GraphDocument from file {fromFile}: Deserialized object is not a GraphDocument.");
    }

    public struct NodeData() : IDeserializable, ISerializable
    {
        public NodeData(Node node) : this()
        {
            Fullpath = $"{node.Path}|{node.DisplayName}";
            Id = node.Id;
            X = node.X;
            Y = node.Y;
            Inputs = node.Inputs.Select(p => new PortData(p)).ToList();
            Outputs = node.Outputs.Select(p => new PortData(p)).ToList();
        }
        public string Fullpath;
        public string Id;
        public double X;
        public double Y;
        public List<PortData> Inputs;
        public List<PortData> Outputs;
        public void SetObjectData(DeserializationInfo info)
        { 
            info.TryGetValue(nameof(Fullpath), out string fullpath, string.Empty);
            info.TryGetValue(nameof(Id), out string id, string.Empty);
            info.TryGetValue(nameof(X), out double x, 0D);
            info.TryGetValue(nameof(Y), out double y, 0D);
            
            info.TryGetList(nameof(Inputs), out List<PortData> inputs);
            info.TryGetList(nameof(Outputs), out List<PortData> outputs);
            
            Fullpath = fullpath;
            Id = id;
            X = x;
            Y = y;
            Inputs = inputs;
            Outputs = outputs;
        }

        public SerializationInfo GetObjectData()
        {
            var info = new SerializationInfo(GetType())
                .AddValue(nameof(Fullpath), Fullpath)
                .AddValue(nameof(Id), Id)
                .AddValue(nameof(X), X)
                .AddValue(nameof(Y), Y)
                .AddValue(nameof(Inputs), Inputs)
                .AddValue(nameof(Outputs), Outputs);
            return info;
        }

        public List<Ulid> GetReferencedAssetIds()
        {
            var assetIds = new List<Ulid>();
            foreach (var port in Inputs)
            {
                assetIds.AddRange(port.GetReferencedAssetIds());
            }
            foreach (var port in Outputs)
            {
                assetIds.AddRange(port.GetReferencedAssetIds());
            }
            return assetIds;
        }
    }

    public struct PortData() : IDeserializable, ISerializable
    {
        
        public PortData(Port port) : this()
        {
            Id = port.Id;
            Name = port.Name;
            Value = port.Value;
        }
        
        public string Id;
        public string Name;
        public object? Value;
        public void SetObjectData(DeserializationInfo info)
        {
            info.TryGetValue(nameof(Id), out string id, string.Empty);
            info.TryGetValue(nameof(Name), out string name, string.Empty);
            info.TryGetValue(nameof(Value), out object? value, null);
            
            Id = id;
            Name = name;
            Value = value;
        }

        public SerializationInfo GetObjectData()
        {
            var info = new SerializationInfo(GetType())
                .AddValue(nameof(Id), Id)
                .AddValue(nameof(Name), Name)
                .AddValue(nameof(Value), Value);
            return info;
        }

        public List<Ulid> GetReferencedAssetIds()
        {
            if (Value is ISerializable serializableValue)
            {
                return serializableValue.GetReferencedAssetIds();
            }
            return [];
        }
    }
    
    public SerializationInfo GetObjectData()
    {
        
        var nodesData = Nodes.Values.Select(n => new NodeData(n)).ToList();
        
        var info = new SerializationInfo(GetType())
            .AddValue(nameof(Nodes), nodesData)
            .AddValue(nameof(Links), Links);
        return info;
    }

    public List<Ulid> GetReferencedAssetIds()
    {
        return [];
    }

    public void SetObjectData(DeserializationInfo info)
    {
        // Clear existing nodes and links before deserializing
        foreach (var link in Links)
        {
            RemoveLink(link);
        }
        foreach (var nodesKeysValue in Nodes)
        {
            RemoveNode(nodesKeysValue.Key);
        }
        
        info.TryGetList(nameof(Nodes), out List<NodeData> nodes);
        foreach (var nodeData in nodes)
        {
            // Create a new instance of the node using the Activator
            var node = GraphNodeRegistry.GetNode(nodeData.Fullpath);
            node.SetData(nodeData);
            AddNode(node);
        }
        
        info.TryGetList(nameof(Links), out List<Link> links);
        Links.Clear();
        foreach (var link in links)
        {
            AddLink(link);
        }
    }
}