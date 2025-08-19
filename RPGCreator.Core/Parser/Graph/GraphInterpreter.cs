using RPGCreator.Core.Type.Blueprint.Nodes;
using RPGCreator.Core.Type.Blueprint.Nodes.Debug;
using Serilog;

namespace RPGCreator.Core.Parser.Graph;

public sealed class GraphInterpreter
{
    private readonly List<GraphLabeledInstr> _program;
    private readonly GraphEvalEnvironment _env;
    private Dictionary<string, Object?> GlobalsVariables => GraphEvalEnvironment.GlobalsVariables;

    public GraphInterpreter(List<GraphLabeledInstr> program, GraphEvalEnvironment env)
    {
        _program = program;
        _env = env;
        
        // First we need to add all labels to the environment
        // This is necessary to resolve jumps and branches correctly.
        for (int i = 0; i < _program.Count; i++)
        {
            var label = _program[i].Label;
            if (!string.IsNullOrEmpty(label))
            {
                if (_env.Labels.ContainsKey(label))
                    throw new InvalidOperationException($"Duplicate label '{label}' found in the program.");
                _env.Labels[label] = i;
            }
        }
    }

    public void Run()
    {
        _env.CurrentBlock = 0;
        _env.CurrentInstruction = 0;
        while (_env.CurrentBlock < _program.Count)
        {
            var block = _program[_env.CurrentBlock];
            if(_env.CurrentInstruction >= block.Instrs.Count)
            {
                // If we reach the end of the current block, we move to the next block
                _env.CurrentBlock++;
                _env.CurrentInstruction = 0;
                continue;
            }
            
            var instr = block.Instrs[_env.CurrentInstruction];

            try
            {
                Execute(instr);
            }
            catch (Exception e)
            {
                Log.Fatal("Error executing instruction {InstructionIndex} in block {BlockIndex}: {Message}",
                    _env.CurrentInstruction, _env.CurrentBlock, e.Message);
                Log.Fatal("Instruction: {Instruction}", instr);
                Log.Fatal("Registers: {Registers}", string.Join(", ", _env.Registers.Select((r, i) => $"R{i}: {r}")));
                Log.Fatal("Globals: {Globals}", string.Join(", ", GlobalsVariables.Select(kv => $"{kv.Key}: {kv.Value}")));
                throw; // Re-throw the exception to stop execution
            }
            
            _env.CurrentInstruction++;
        }
    }

    public void Execute(GraphInstr instr)
    {
        switch (instr.OpCode)
        {
            case EGraphOpCode.get_vm:
            {
                var path = ParsePathOperand(instr.Operands[0]);
                var dest = ParseRegisterOperand(instr.Operands[1]);

                _env.Registers[dest] = _env.GetVM(path);
                break;
            }
            case EGraphOpCode.debug_print:
            {
                var value = EvalOperand(instr.Operands[0]);
                var level = ParseEnumOperand<NodePrint.EPrintLevel>(instr.Operands[1]);

                switch (level)
                {
                    case NodePrint.EPrintLevel.Debug:
                        Log.Debug("[BP] {Value}", value);
                        break;
                    case NodePrint.EPrintLevel.Info:
                        Log.Information("[BP] {Value}", value);
                        break;
                    case NodePrint.EPrintLevel.Warning:
                        Log.Warning("[BP] {Value}", value);
                        break;
                    case NodePrint.EPrintLevel.Error:
                        Log.Error("[BP] {Value}", value);
                        break;
                    default:
                        throw new InvalidOperationException($"Unknown print level: {level}");
                }
                
                break;
            }
            case EGraphOpCode.alloc_literal_string:
            {
                var value = ParseStringOperand(instr.Operands[0]);
                var dest = ParseRegisterOperand(instr.Operands[1]);
                _env.Registers[dest] = value;
                break;
            }
            case EGraphOpCode.alloc_literal_object:
            {
                var value = EvalOperand(instr.Operands[0]);
                var dest = ParseRegisterOperand(instr.Operands[1]);
                if (value == null)
                {
                    throw new InvalidOperationException("Cannot allocate a null object.");
                }
                _env.Registers[dest] = value;
                break;
            }
            default:
                Log.Error("Unknown opcode: {OpCode}", instr.OpCode);
                break;
        }
    }

    private int ParseRegisterOperand(GraphOperands operand)
    {
        if(!operand.Kind.HasFlag(EGraphOperandKind.Register))
            throw new InvalidOperationException($"Expected a register operand, but got {operand.Kind}.");
        
        var sValue = operand.Text.Replace("rx", string.Empty);
        
        if (!int.TryParse(sValue, out var registerId))
            throw new InvalidOperationException($"Invalid register ID: {sValue}. Expected a number.");
        if (registerId is < 0 or >= GraphEvalEnvironment.MaxRegisters)
            throw new InvalidOperationException($"Register ID {registerId} is out of bounds. Must be between 0 and {GraphEvalEnvironment.MaxRegisters - 1}.");
        return registerId;
    }

    private string ParsePathOperand(GraphOperands operand)
    {
        if(!operand.Kind.HasFlag(EGraphOperandKind.Path))
            throw new InvalidOperationException($"Expected a path operand, but got {operand.Kind}.");
        if (string.IsNullOrEmpty(operand.Text))
            throw new InvalidOperationException("Path operand cannot be null or empty.");
        return operand.Text;
    }

    private string ParseStringOperand(GraphOperands operand)
    {
        if (!operand.Kind.HasFlag(EGraphOperandKind.LiteralString))
        {
            throw new InvalidOperationException($"Expected a string operand, but got {operand.Kind}.");
        }
        
        if (string.IsNullOrEmpty(operand.Text))
        {
            throw new InvalidOperationException($"Invalid string operand: {operand.Text}. Expected a non-empty string.");
        }
        return operand.Text.Trim('"');
    }
    
    private T ParseEnumOperand<T>(GraphOperands operand) where T : struct, Enum
    {
        if (!operand.Kind.HasFlag(EGraphOperandKind.Enum))
        {
            throw new InvalidOperationException($"Expected an enum operand, but got {operand.Kind}.");
        }

        var enumValue = EvalOperand(operand);
        if(enumValue == null)
        {
            throw new InvalidOperationException($"Enum operand cannot be null. Expected a valid enum value for {typeof(T).Name}.");
        }
        if (enumValue is not string enumValueStr)
        {
            throw new InvalidOperationException($"Invalid enum operand type: {enumValue.GetType()}. Expected a string.");
        }

        if (string.IsNullOrEmpty(enumValueStr))
        {
            throw new InvalidOperationException($"Invalid enum operand: {operand.Text}. Expected a non-empty string.");
        }

        if (Enum.TryParse<T>(enumValueStr, out var result))
        {
            return result;
        }
        
        throw new InvalidOperationException($"Invalid enum value: {enumValueStr}. Expected a valid value for {typeof(T).Name}.");
    }
    
    private object? EvalOperand(GraphOperands operand)
    {
        if (operand.Kind is EGraphOperandKind.Label or EGraphOperandKind.Path)
        {
            throw new InvalidOperationException($"Cannot evaluate operand of kind {operand.Kind}. Expected a value operand.");
        }
        
        var sValue = operand.Text;
        if (sValue.StartsWith("rx")) return _env.Registers[ParseRegisterOperand(operand)];
        if (sValue.StartsWith("\"")) return sValue.Trim('"');
        return sValue;
    }
}