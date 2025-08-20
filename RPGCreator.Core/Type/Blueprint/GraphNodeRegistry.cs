using System.Reflection;
using RPGCreator.Core.Parser.Graph.NodesMaker;
using Serilog;

namespace RPGCreator.Core.Type.Blueprint;

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
        if (depth <= 0)
            return Nodes.Keys.ToList();

        return Nodes.Keys
            .Where(path => path.Split('|').Length <= depth + 1)
            .ToList();
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
    
    public static void AnalyzeNodes()
    {
        if (AlreadyAnalyzed)
        {
            Log.Warning("GraphNodeRegistry: already been analyzed. Skipping re-analysis.");
            return;
        }

        Log.Information("GraphNodeRegistry: analyzing nodes...");
        
        var asm = Assembly.GetExecutingAssembly();

        AlreadyAnalyzed = true;
        
        var nodeTypes = asm.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(Node).IsAssignableFrom(t))
            .Where(t => t.GetCustomAttribute<GraphNodeAttribute>() != null)
            .ToArray();
        
        Log.Information("GraphNodeRegistry: Found {Count} node candidates in assembly {AssemblyName}.",
            nodeTypes.Length, asm.GetName().Name);

        foreach (var type in nodeTypes)
        {
            var nodeObject = Activator.CreateInstance(type);
            if (nodeObject is not Node node)
            {
                Log.Error("GraphNodeRegistry: Type {TypeName} is not a valid Node (Should inherit of {Node}).", type.FullName, typeof(Node));
                continue;
            }
            RegisterNode(node);
            Log.Information("GraphNodeRegistry: Registered node {NodeName} with path {NodePath}.",
                node.DisplayName, CreateNodePath(node.Path, node.DisplayName));
        }
        Log.Information("GraphNodeRegistry: Added {Count} nodes.", Nodes.Count);
        
        Log.Information("GraphNodeRegistry: Analysis completed.");
    }
    
    private static string CreateNodePath(string path, string nodeName)
    {
        return string.IsNullOrEmpty(path) ? nodeName : $"{path}|{nodeName}";
    }
}