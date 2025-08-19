namespace RPGCreator.Core.Common;

/// <summary>
/// This class is used to build a dependency graph from a set of links.
/// Each link is a tuple of two strings representing the "from" and "to" nodes.
/// The graph is represented as a dictionary where the key is the node ID and the value is a set of node IDs that depend on it.
/// </summary>
public static class DependencyGraph
{
    /// <summary>
    /// Build a dependency graph from a set of links.
    /// </summary>
    /// <param name="Links"></param>
    /// <returns></returns>
    public static Dictionary<string, HashSet<string>> BuildGraph(IEnumerable<(string from, string to)> Links)
    {
        var depGraph = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);

        void AddNode(string node)
        {
            if (!depGraph.ContainsKey(node))
            {
                depGraph[node] = new HashSet<string>(StringComparer.Ordinal);
            }
        }

        foreach (var (from, to) in Links)
        {
            AddNode(from);
            AddNode(to);
            depGraph[from].Add(to);
        }
        
        return depGraph;
    }

    public static List<string> TopologicalSort(Dictionary<string, HashSet<string>> depGraph)
    {
        var inDegree = depGraph.Keys.ToDictionary(key => key, _ => 0, StringComparer.Ordinal);
        foreach (var keyValue in depGraph)
        {
            foreach(var value in keyValue.Value)
            {
                if (!inDegree.TryAdd(value, 1))
                {
                    inDegree[value]++;
                }
            }
        }
        
        var queue = new Queue<string>(inDegree.Where(kv => kv.Value == 0).Select(kv => kv.Key));
        var sortedList = new List<string>(depGraph.Count);

        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            sortedList.Add(node);
            foreach (var child in depGraph[node])
            {
                inDegree[child]--;
                if (inDegree[child] == 0)
                {
                    queue.Enqueue(child);
                }
            }
        }

        if (sortedList.Count != depGraph.Count)
        {
            var cycleNodes = inDegree.Where(kv => kv.Value == 0).Select(kv => kv.Key).ToList();
            throw new InvalidOperationException($"Graph has cycles or is not fully connected. Nodes with zero in-degree: {string.Join(", ", cycleNodes)}");
        }

        return sortedList;
    }
    
    public static HashSet<string> AncestorsOf(string node, Dictionary<string, HashSet<string>> depGraph)
    {
        var ancestors = new HashSet<string>(StringComparer.Ordinal);
        var stack = new Stack<string>();
        stack.Push(node);

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            if (ancestors.Add(current))
            {
                if (depGraph.TryGetValue(current, out var children))
                {
                    foreach (var child in children)
                    {
                        stack.Push(child);
                    }
                }
            }
        }

        return ancestors;
    }
    
    public static HashSet<string> DescendantsOf(string node, Dictionary<string, HashSet<string>> depGraph)
    {
        var descendants = new HashSet<string>(StringComparer.Ordinal);
        var stack = new Stack<string>();
        stack.Push(node);

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            if (descendants.Add(current))
            {
                if (depGraph.TryGetValue(current, out var parents))
                {
                    foreach (var parent in parents)
                    {
                        stack.Push(parent);
                    }
                }
            }
        }

        return descendants;
    }
}