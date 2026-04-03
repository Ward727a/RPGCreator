using System.Globalization;

namespace RPGCreator.SDK.Graph;

public sealed class GraphCompileContext
{
    public const string NoneRegisterId = "rx-1";
    private int _nextRegisterId = 0;

    private readonly Dictionary<(string nodeId, string portId), string> _producedValues = new();
    private readonly Dictionary<object, string> _constants = new();
    
    private Dictionary<string, List<GraphInstr>> _allocInstructions = new();
    public string NewRegister()
    {
        return $"rx{_nextRegisterId++}";
    }

    public void BindOuput(Node node, string outPort, string registerId) =>
        _producedValues[(node.Id, outPort)] = registerId;

    /// This method is used to allocate a register for a node's output port.<br/>
    /// We do this so that we can "alloc" the register as near as possible to the node that will consume it.<br/>
    /// Otherwise, we would have to allocate the register as "global" (e.g. at the beginning of the graph) <br/>
    /// And this would lead to a lot of unnecessary new registers being allocated.<br/>
    /// This is useful for optimizing register allocation and replacing registers that could be replaced with register that are not used anymore.<br/>
    /// <param name="node">The node that produces the value.</param>
    /// <param name="outPort">The output port of the node.</param>
    /// <param name="registerId">The register ID to allocate.</param>
    /// <param name="instructions">The list of instructions to add the allocation to.</param>
    public void AllocateRegister(Node node, string outPort, string registerId, List<GraphInstr> instructions)
    {
        _allocInstructions[registerId] = instructions;
    }

    public List<GraphInstr> GetRegister(string registerId)
    {
        return _allocInstructions[registerId];
    }
    
    public bool TryResolveFromPort(GraphDocument graph, Node consumer, string inPort, out string registerId)
    {
        var link = graph.Links.FirstOrDefault(l => l.ToNodeId == consumer.Id && l.ToPortId == inPort);
        if (link is null)
        {
            registerId = NoneRegisterId;
            return false;
        }

        if (_producedValues.TryGetValue((link.FromNodeId, link.FromPortId), out registerId)) return true;
        throw new InvalidOperationException($"Source node {link.FromNodeId} for port {inPort} on node {consumer.Id} is not bound to a register.");
    }

    public string AllocateConstant(object? value, List<GraphInstr> instructions)
    {
        var key = (value?.GetType() ?? typeof(object), value);
        // Check if the constant is already registered somewhere (to avoid unnecessary duplication)
        if (_constants.TryGetValue(key, out var constantRegisterId))
            return constantRegisterId;
        
        // If not, create a new register for it
        var dest = NewRegister();
        var type = value?.GetType() ?? typeof(object);
        switch (value)
        {
            case string str:
                instructions.Add(
                    GraphIR.Op(
                        EGraphOpCode.alloc_literal_string, 
                        GraphIR.Operands(EGraphOperandKind.LiteralString, str),
                        GraphIR.Operands(EGraphOperandKind.Register, dest))
                    );
                break;
            case int i:
                instructions.Add(
                    GraphIR.Op(
                        EGraphOpCode.alloc_literal_int,
                        GraphIR.Operands(EGraphOperandKind.Literal, i.ToString()),
                        GraphIR.Operands(EGraphOperandKind.Register, dest))
                    );
                break;
            case float f:
                instructions.Add(
                    GraphIR.Op(
                        EGraphOpCode.alloc_literal_float, 
                        GraphIR.Operands(EGraphOperandKind.Literal, f.ToString(CultureInfo.InvariantCulture)),
                        GraphIR.Operands(EGraphOperandKind.Register, dest))
                    );
                break;
            case double d:
                instructions.Add(
                    GraphIR.Op(
                        EGraphOpCode.alloc_literal_float,
                        GraphIR.Operands(EGraphOperandKind.LiteralNumber, d.ToString(CultureInfo.InvariantCulture)),
                        GraphIR.Operands(EGraphOperandKind.Register, dest)));
                break;
            case bool b:
                instructions.Add(
                    GraphIR.Op
                        (EGraphOpCode.alloc_literal_bool, 
                        GraphIR.Operands(EGraphOperandKind.Literal, b.ToString()), 
                        GraphIR.Operands(EGraphOperandKind.Register, dest))
                    );
                break;
            default:
                instructions.Add(
                    GraphIR.Op(
                        EGraphOpCode.alloc_literal_object, 
                        GraphIR.Operands(EGraphOperandKind.Literal, value?.ToString() ?? "", value ?? null!), 
                        GraphIR.Operands(EGraphOperandKind.Register, dest))
                    );
                break;
        }
        
        _constants[key] = dest;
        return dest;
    }

    public string ResolveInput(GraphDocument graph, Node node, string inPort, string propKey, List<GraphInstr> instructions,
        object? defaultValue = null)
    {
        if (TryResolveFromPort(graph, node, inPort, out var registerId))
        {
            bool foundInstructions = false;
            // We need the instruction from the register allocation list
            if (_allocInstructions.TryGetValue(registerId, out var allocInstructions))
            {
                // If the register is already allocated, return it
                if (allocInstructions.Count > 0)
                {
                    foundInstructions = true;
                    instructions.AddRange(allocInstructions);
                }
            }
            // if we found the instructions, we now need to remove the register from the allocation Instruction list (to avoid duplicates)
            if (foundInstructions)
            {
                _allocInstructions.Remove(registerId);
            }
            
            // Return the register id
            return registerId;
        }
        if(node.Properties.TryGetValue(propKey, out var propValue) && propValue is not null)
        {
            // If the property is set, allocate a constant for it
            return AllocateConstant(propValue, instructions);
        }

        if (defaultValue is not null)
        {
            // If the property is not set, but a default value is provided, allocate a constant
            return AllocateConstant(defaultValue, instructions);
        }
        
        throw new InvalidOperationException($"Argument '{inPort}' for node '{node.Id}'({node.DisplayName}) is not bound to a register and no default value is provided.");
    }
}