using RPGCreator.SDK.Graph;
using RPGCreator.SDK.Graph.Nodes;

namespace RPGCreator.Core.Types.Blueprint.Nodes.Math;

[GraphNode]
public class NodeMathMultiply : Node
{
    public override EGraphOpCode OpCode { get; protected set; } = EGraphOpCode.math_multiply;
    public override string DisplayName { get; protected set; } = "Math Multiply";

    public override string Description  { get; protected set; } = "Multiplies two numbers together.";
    public override string Path  { get; protected set; } = "Math|Operations";
    NumberPort InputA => (NumberPort)Inputs[0];
    NumberPort InputB => (NumberPort)Inputs[1];
    
    public NodeMathMultiply()
    {
        Inputs.Add(new NumberPort() { AllowManualInput = true, Name = "a", IsInput = true });
        Inputs.Add(new NumberPort() { AllowManualInput = true, Name = "b", IsInput = true });

        Outputs.Add(new NumberPort() { AllowManualInput = false, Name = "Result" });

        Properties["a"] = 0;
        Properties["b"] = 0;
    }
    
    public override IEnumerable<GraphInstr> Emit(GraphDocument graph, GraphCompileContext context)
    {
        Properties["a"] = InputA.Value;
        Properties["b"] = InputB.Value;
        
        // Reserve a new register for the result
        var dst = context.NewRegister();
        
        var instrs = new List<GraphInstr>();
        // Resolve the input values
        var a = context.ResolveInput(graph, this, "a", "a", instrs, 0);
        var b = context.ResolveInput(graph, this, "b", "b", instrs, 1);
        // Bind the output value to the register
        context.BindOuput(this, Outputs[0].Id, dst);
        
        instrs.Add(
            GraphIR.Op(EGraphOpCode.math_multiply,
                GraphIR.Operands(EGraphOperandKind.LiteralNumber | EGraphOperandKind.Register, a),
                GraphIR.Operands(EGraphOperandKind.LiteralNumber | EGraphOperandKind.Register, b),
                GraphIR.Operands(EGraphOperandKind.Register, dst))
        );
        context.AllocateRegister(this, Outputs[0].Id, dst, instrs);

        return [];
    }
}