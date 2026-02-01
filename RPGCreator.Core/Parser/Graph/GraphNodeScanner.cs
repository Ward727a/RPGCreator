using System.Reflection;
using RPGCreator.SDK.Graph;
using RPGCreator.SDK.Graph.Nodes;

namespace RPGCreator.Core.Parser.Graph;

public class GraphNodeScanner : IGraphNodeScanner
{
    public void AnalyzeNodes()
    {
        ScanAssembly(typeof(Node).Assembly);
        
        ScanAssembly(Assembly.GetExecutingAssembly());
    }

    public void ScanAssembly(Assembly asm)
    {
        var nodeTypes = asm.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(Node).IsAssignableFrom(t))
            .ToArray();

        foreach (var type in nodeTypes)
        {
            if (Activator.CreateInstance(type) is Node node)
            {
                GraphNodeRegistry.RegisterNode(node);
            }
        }
    }

    public void ScanCurrentAssembly()
    {
        ScanAssembly(Assembly.GetCallingAssembly());
    }
}