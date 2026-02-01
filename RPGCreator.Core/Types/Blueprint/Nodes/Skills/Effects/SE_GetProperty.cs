using RPGCreator.SDK.Graph;
using RPGCreator.SDK.Graph.Nodes;

namespace RPGCreator.Core.Types.Blueprint.Nodes.Skills.Effects;

[GraphNode]
[GraphNodeContext(EGraphNodeContext.SkillEffect)]
public class SE_GetProperty : Node
{
    public override EGraphOpCode OpCode { get; protected set; } = EGraphOpCode.get_variable;
    public override string DisplayName { get; protected set; } = "Get Effect Property";
    public override string Path { get; protected set; } = "Skill Effect|Getters";

    private StringPort PortPropName { get; init; }
    private Port PortValue => Outputs[0];
    
    public SE_GetProperty()
    {
        PortPropName = new StringPort()
        {
            Name = "Property Name",
            AllowManualInput = true,
        };
        Inputs.Add(PortPropName);
        
        Outputs.Add(new Port()
        {
            Name = "Value",
            AllowManualInput = false,
            Kind = PortKind.Object
        });
        
        Properties["PropName"] = "";
    }
    public override IEnumerable<GraphInstr> Emit(GraphDocument graph, GraphCompileContext context)
    {
        Properties["PropName"] = PortPropName.Value;
        
        if(string.IsNullOrEmpty(Properties["PropName"] as string))
            throw new InvalidOperationException($"Property name cannot be empty in node {Id} ({DisplayName})");
        
        var dst = context.NewRegister();
        var instrs = new List<GraphInstr>();
        
        var propName = context.ResolveInput(graph, this, PortPropName.Id, "PropName", instrs, "");
        
        if(string.IsNullOrEmpty(propName))
            throw new InvalidOperationException($"Property name cannot be empty in node {Id} ({DisplayName})");
        
        var propVarPath = $"skill_effect.props.{propName}";
        
        // Bind the output value to the register
        context.BindOuput(this, PortValue.Id, dst);
        // Allocate the register as near as possible to the node that will consume it
        // This will add the instruction to the list of instructions that will be executed before the node that consumes it
        context.AllocateRegister(
                this,
                PortValue.Id,
                dst,
                new List<GraphInstr>{
                GraphIR.Op(
                    EGraphOpCode.get_variable, 
                    GraphIR.Operands(EGraphOperandKind.Path, propVarPath), 
                    GraphIR.Operands(EGraphOperandKind.Register, dst)
                    )}
        );
        
        return instrs;
    }
}