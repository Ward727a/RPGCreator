using RPGCreator.SDK.Graph;
using RPGCreator.SDK.Graph.Nodes;
using RPGCreator.SDK.Graph.Ports;

namespace RPGCreator.Core.Types.Blueprint.Nodes.Debug;

[GraphNode]
public class NodePrint : Node
{

    public enum EPrintLevel
    {
        Debug,
        Info,
        Warning,
        Error
    }
    
    public override EGraphOpCode OpCode { get; protected set; } = EGraphOpCode.debug_print;
    public override string DisplayName { get; protected set; } = "Print message";
    public override string Description { get; protected set; } = "Print a message to the debug console with a specific level.";
    public override string Path { get; protected set; } = "Debug";
    
    private Port MessagePort => Inputs[1];
    private EnumPort LevelPort => (EnumPort)Inputs[2];
    
    public NodePrint()
    {
        Inputs.Add(
        new Port(){
            Kind = PortKind.Exec,
            AllowManualInput = false,
            Name = "In",
            IsInput = true
        });
        
        Inputs.Add(
        new Port(){
            Kind = PortKind.String,
            AllowManualInput = true,
            Name = "message",
            IsInput = true
        });
        Inputs.Add(
            new EnumPort(typeof(EPrintLevel))
            {
                Name = "Level",
                AllowManualInput = true,
                IsInput = true,
            });
        
        Outputs.Add(
        new Port(){
            Kind = PortKind.Exec,
            AllowManualInput = false,
            Name = "Out"
        });
    }
    
    public override IEnumerable<GraphInstr> Emit(GraphDocument graph, GraphCompileContext context)
    {
        // Set value to properties
        Properties["message"] = MessagePort.Value;
        Properties["level"] = LevelPort.Value;
        
        var instrs = new List<GraphInstr>();
        // Resolve the input values
        var message = context.ResolveInput(graph, this, MessagePort.Id, "message", instrs, "");
        var level = context.ResolveInput(graph, this, LevelPort.Id, "level", instrs, EPrintLevel.Debug);
        
        // Add the instruction to print the message
        instrs.Add(GraphIR.Op(EGraphOpCode.debug_print, GraphIR.Operands(EGraphOperandKind.LiteralString | EGraphOperandKind.Register, message), GraphIR.Operands(EGraphOperandKind.Enum | EGraphOperandKind.Register, level)));
        return instrs;
    }
}