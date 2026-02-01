using RPGCreator.SDK.Graph;
using RPGCreator.SDK.Graph.Nodes;

namespace RPGCreator.Core.Types.Blueprint.Nodes.Entity;

[GraphNode]
public class NodeIsEntity : Node
{
    public override EGraphOpCode OpCode { get; protected set; } = EGraphOpCode.check_is_type;
    public override string DisplayName { get; protected set; } = "Is Entity?";
    public override string Path { get; protected set; } = "Entity";
    
    private Port ObjectPort => Inputs[1];
    
    public NodeIsEntity()
    {
        Inputs.Add(new Port()
        {
            Name = "In",
            AllowManualInput = false,
            Kind = PortKind.Exec,
            IsInput = true
        });
        
        Inputs.Add(new Port()
        {
            Name = "Object",
            AllowManualInput = true,
            Kind = PortKind.Object,
            IsInput = true
        });
        
        Outputs.Add(new Port()
        {
            Name = "Out",
            AllowManualInput = false,
            Kind = PortKind.Exec
        });
        
        Outputs.Add(new Port()
        {
            Name = "IsEntity",
            AllowManualInput = false,
            Kind = PortKind.Boolean
        });
    }
    
    public override IEnumerable<GraphInstr> Emit(GraphDocument graph, GraphCompileContext context)
    {
        Properties["Object"] = ObjectPort.Value;
        // Properties["Object"] = new Runtimes.ECS.Entity();
        
        var instrs = new List<GraphInstr>();
        
        var boolReg = context.NewRegister();
        var objReg = context.ResolveInput(graph, this, ObjectPort.Id, "Object", instrs, null);
        
        // Bind the output value to the register
        context.BindOuput(this, Outputs[1].Id, boolReg);
        
        instrs.Add(
            GraphIR.Op(EGraphOpCode.check_is_type,
                GraphIR.Operands(EGraphOperandKind.Register, objReg),
                GraphIR.Operands(EGraphOperandKind.Register, boolReg))
        );
        
        return instrs;
    }
}