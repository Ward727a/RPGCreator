namespace RPGCreator.SDK.Graph.Nodes;

public static class GraphNodeRegistry
{
    
    static readonly Dictionary<string, Node> Nodes = new();
    public static bool AlreadyAnalyzed { get; private set; } = false;

    public static Node GetNode(string path, string nodeName)
    {
        if (Nodes.TryGetValue(CreateNodePath(path, nodeName), out var node))
            return node;

        throw new KeyNotFoundException($"Node with path {path} not found!");
    }

    public static Node GetNode(string path)
    {
        if (Nodes.TryGetValue(path, out var node))
            return node;

        throw new KeyNotFoundException($"Node with path {path} not found!");
    }
    
    public static List<Node> GetNodes(string path)
    {
        return Nodes
            .Where(kv => kv.Key.StartsWith(path))
            .Select(kv => kv.Value)
            .ToList();
    }
    
    public static List<Node> GetAllNodes()
    {
        return Nodes.Values.ToList();
    }
    
    public static List<String> GetNodesPaths(int depth = 0)
    {

        // Return all paths, but only the parts that are at depth (ex: depth = 1 will return only the first part of the path)
        // Example: "Math|Operations|Add" at depth 1 will return "Math"
        // Example: "Math|Operations|Add" at depth 2 will return "Math|Operations"
        return Nodes.Keys
            .Select(path => path.Split('|').Take(depth + 1).Aggregate((a, b) => $"{a}|{b}"))
            .Distinct()
            .ToList();
    }

    public static Dictionary<string, object?> GetNestedPaths()
    {
        
        var root = new Dictionary<string, object>();

        foreach (var kvp in Nodes)
        {
            var parts = kvp.Key.Split('|');
            var current = root;

            for (int i = 0; i < parts.Length; i++)
            {
                var part = parts[i];

                if (i == parts.Length - 1)
                {
                    current[part] = kvp.Value!;
                }
                else
                {
                    if (!current.TryGetValue(part, out var next))
                    {
                        next = new Dictionary<string, object>();
                        current[part] = next;
                    }

                    current = (Dictionary<string, object>)next;
                }
            }
        }

        return root;
    }
    
    public static List<string> GetNodesPaths(string rootPath, int depth = 0)
    {
        if (depth <= 0)
            return Nodes.Keys.Where(path => path.StartsWith(rootPath)).ToList();

        return Nodes.Keys
            .Where(path => path.StartsWith(rootPath) && path.Split('|').Length <= depth + 1)
            .ToList();
    }
    
    public static void RegisterNode(Node node)
    {
        var path = CreateNodePath(node.Path, node.DisplayName);
        if (Nodes.ContainsKey(path))
            throw new InvalidOperationException($"Node with path {path} is already registered!");

        Nodes.Add(path, node);
    }
    
    private static string CreateNodePath(string path, string nodeName)
    {
        return string.IsNullOrEmpty(path) ? nodeName : $"{path}|{nodeName}";
    }
}