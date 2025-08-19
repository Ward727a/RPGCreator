using RPGCreator.Core.Type.Blueprint.Nodes;

namespace RPGCreator.Core.Parser.Graph;

public sealed class GraphTable
{
    public sealed record GraphInstrSpec(EGraphOpCode Op, Graph.EGraphOperandKind[] Signatures);

    private static readonly Dictionary<EGraphOpCode, GraphInstrSpec> Table = new()
    {
        [EGraphOpCode.get_vm] = new GraphInstrSpec(EGraphOpCode.get_vm, new[] { EGraphOperandKind.Path, EGraphOperandKind.Register }),
        [EGraphOpCode.debug_print] = new GraphInstrSpec(EGraphOpCode.debug_print, new[] { EGraphOperandKind.Register | EGraphOperandKind.Literal }),
        [EGraphOpCode.alloc_literal_string] = new GraphInstrSpec(EGraphOpCode.alloc_literal_string,new[] { EGraphOperandKind.LiteralString, EGraphOperandKind.Register }),
        [EGraphOpCode.alloc_literal_int] = new GraphInstrSpec(EGraphOpCode.alloc_literal_int, new[] { EGraphOperandKind.Literal, EGraphOperandKind.Register }),
        [EGraphOpCode.alloc_literal_float] = new GraphInstrSpec(EGraphOpCode.alloc_literal_float, new[] { EGraphOperandKind.Literal, EGraphOperandKind.Register }),
        [EGraphOpCode.alloc_literal_bool] = new GraphInstrSpec(EGraphOpCode.alloc_literal_bool, new[] { EGraphOperandKind.Literal, EGraphOperandKind.Register }),
        [EGraphOpCode.alloc_literal_object] = new GraphInstrSpec(EGraphOpCode.alloc_literal_object, new[] { EGraphOperandKind.LiteralString, EGraphOperandKind.Register }),
    };
    
    public void Register(GraphInstrSpec spec) => Table[spec.Op] = spec;

    public GraphInstrSpec Get(EGraphOpCode opCode) => Table[opCode];
    public IReadOnlyList<EGraphOpCode> ValidOpcodes => Table.Keys.ToList();
}