namespace RPGCreator.Core.Types.Blueprint.Nodes;


public enum EGraphNodeContext
{
    None,
    SkillEffect,
    Event,
    Macro,
    Class,
    Interface,
    All
}

public static class EGraphNodeContextExtensions
{
    public static string ToContextString(this EGraphNodeContext context) => context switch
    {
        EGraphNodeContext.None => "None",
        EGraphNodeContext.SkillEffect => "SkillEffect",
        EGraphNodeContext.Event => "Event",
        EGraphNodeContext.Macro => "Macro",
        EGraphNodeContext.Class => "Class",
        EGraphNodeContext.Interface => "Interface",
        EGraphNodeContext.All => "All",
        _ => "Unknown"
    };
}


/// <summary>
/// Define the context of a node, used by the engine to filter nodes in the editor based on the current graph context.<br/>
/// Can be either a string or an <see cref="EGraphNodeContext"/> enum value.<br/>
/// Can be applied multiple times to a class to define multiple contexts.<br/>
/// <br/>
/// <b>Note:</b> If no context is defined, the node will be available in all contexts.<br/>
/// <b>Note:</b> If you put a <see cref="EGraphNodeContext"/>, the enum will be converted to a string using <see cref="EGraphNodeContextExtensions.ToContextString(EGraphNodeContext)"/><br/>
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class GraphNodeContextAttribute : Attribute
{
    public string Context { get; }
    public GraphNodeContextAttribute(string context) => Context = context;
    
    public GraphNodeContextAttribute(EGraphNodeContext context) => Context = context.ToContextString();
}