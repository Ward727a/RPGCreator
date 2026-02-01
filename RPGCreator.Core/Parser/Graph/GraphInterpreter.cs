using RPGCreator.Core.Parser.Graph.TableHandler;
using RPGCreator.SDK.Graph;
using RPGCreator.SDK.Types.Internals;
using Serilog;

namespace RPGCreator.Core.Parser.Graph;

public sealed class GraphInterpreter : IResettable<GraphInterpreter>
{
    private IList<GraphLabeledInstr> _program;
    private GraphEvalEnvironment _env;
    private Dictionary<string, Object?> GlobalsVariables => GraphEvalEnvironment.GlobalsVariables;

    public GraphInterpreter(IList<GraphLabeledInstr> program, GraphEvalEnvironment env)
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

    /// <summary>
    /// Run the program from the beginning.
    /// </summary>
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

    /// <summary>
    /// Execute a single instruction.<br/>
    /// This method interprets the instruction and performs the corresponding operation.<br/>
    /// It checks if the opcode is registered in the graph table, retrieves the handler, and executes it.<br/>
    /// If the opcode is not registered or the number of operands does not match the handler's signature, it throws an exception.<br/>
    /// The handler's <see cref="IGraphInstrHandler.Exec(GraphInstr, GraphEvalEnvironment, GraphInterpreter)"/> method is called to perform the operation.<br/>
    /// </summary>
    /// <param name="instr">Instruction to execute</param>
    /// <exception cref="InvalidOperationException">Thrown when the opcode is not registered, no handler is found, or the number of operands does not match the handler's signature.</exception>
    public void Execute(GraphInstr instr)
    {
        if (!GraphTable.IsOpcodeRegistered(instr.OpCode))
        {
            throw new InvalidOperationException($"Opcode {instr.OpCode} is not registered in the graph table.");
        }
        var handler = GraphTable.Get(instr.OpCode);
        if (handler == null)
        {
            throw new InvalidOperationException($"No handler found for opcode {instr.OpCode}.");
        }
        if (instr.Operands.Length != handler.Signature.Length)
        {
            throw new InvalidOperationException($"Invalid number of operands for opcode {instr.OpCode}. Expected {handler.Signature.Length}, got {instr.Operands.Length}.");
        }
        handler.Exec(instr, _env, this);
    }

    /// <summary>
    /// Return the register ID from a register operand.<br/>
    /// This method parses the operand text to extract the register ID.<br/>
    /// It expects the operand to be in the format "rxN", where N is the register ID.
    /// </summary>
    /// <param name="operand"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal int ParseRegisterOperand(GraphOperands operand)
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

    /// <summary>
    /// Return the path from a path operand.<br/>
    /// This method parses the operand text to extract the path.<br/>
    /// It expects the operand to be a valid path string.
    /// </summary>
    /// <param name="operand"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal string ParsePathOperand(GraphOperands operand)
    {
        if(!operand.Kind.HasFlag(EGraphOperandKind.Path))
            throw new InvalidOperationException($"Expected a path operand, but got {operand.Kind}.");
        if (string.IsNullOrEmpty(operand.Text))
            throw new InvalidOperationException("Path operand cannot be null or empty.");
        return operand.Text;
    }

    /// <summary>
    /// Return the string from a string operand.<br/>
    /// This method parses the operand text to extract the string value.<br/>
    /// It expects the operand to be a valid string literal.
    /// </summary>
    /// <param name="operand"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal string ParseStringOperand(GraphOperands operand)
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
    
    /// <summary>
    /// Return the float from a float operand.<br/>
    /// This method parses the operand text to extract the float value.<br/>
    /// It expects the operand to be a valid float literal.
    /// </summary>
    internal double ParseFloatOperand(GraphOperands operand)
    {
        if (!operand.Kind.HasFlag(EGraphOperandKind.LiteralNumber))
        {
            throw new InvalidOperationException($"Expected a float operand, but got {operand.Kind}.");
        }

        var value = EvalOperand(operand);
        if (value == null)
        {
            throw new InvalidOperationException($"Float operand cannot be null. Expected a valid float value.");
        }
        
        if (value is not string sValue)
        {
            throw new InvalidOperationException($"Invalid float operand type: {value.GetType()}. Expected a string.");
        }

        if (string.IsNullOrEmpty(sValue))
        {
            throw new InvalidOperationException($"Invalid float operand: {operand.Text}. Expected a non-empty string.");
        }

        if (double.TryParse(sValue, out var result))
        {
            return result;
        }
        
        throw new InvalidOperationException($"Invalid float value: {sValue}. Expected a valid float value.");
    }
    
    /// <summary>
    /// Return the enum value from an enum operand.<br/>
    /// This method parses the operand text to extract the enum value.<br/>
    /// It expects the operand to be a valid enum value string.
    /// </summary>
    internal T ParseEnumOperand<T>(GraphOperands operand) where T : struct, Enum
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
        
        if(enumValue.GetType() == typeof(T))
        {
            return (T)enumValue;
        }
        
        // if (enumValue is not string enumValueStr)
        // {        
        //     throw new InvalidOperationException($"Invalid enum value: {enumValue}({enumValue.GetType().Name}. Expected a valid value for {typeof(T).Name} (string or enum).");
        // }
        //
        // if (string.IsNullOrEmpty(enumValueStr))
        // {
        //     throw new InvalidOperationException($"Invalid enum operand: {operand.Text}. Expected a non-empty string.");
        // }
        //
        // if (Enum.TryParse<T>(enumValueStr, out var result))
        // {
        //     return result;
        // }
        
        
        
        throw new InvalidOperationException($"Invalid enum value: {enumValue}({enumValue.GetType().Name}. Expected a valid value for {typeof(T).Name}.");
    }
    
    internal bool IsOperandOfKind(GraphOperands operand, EGraphOperandKind kind)
    {
        return operand.Kind.HasFlag(kind);
    }
    
    internal bool IsOperandOnlyOfKind(GraphOperands operand, EGraphOperandKind kind)
    {
        return operand.Kind == kind;
    }
    
    /// <summary>
    /// Evaluate an operand and return its value.<br/>
    /// This method checks the kind of the operand and evaluates it accordingly.<br/>
    /// It supports label, path, register, string, and literal operands.<br/>
    /// If the operand is not of a supported kind, it throws an exception.
    /// </summary>
    internal object? EvalOperand(GraphOperands operand)
    {
        if (operand.Kind is EGraphOperandKind.Label or EGraphOperandKind.Path)
        {
            throw new InvalidOperationException($"Cannot evaluate operand of kind {operand.Kind}. Expected a value operand.");
        }
        
        var sValue = operand.Text;
        if (sValue.StartsWith("rx")) return EvalRegisterOperand(operand);
        return operand.Value;
    }
    
    internal T? EvalOperand<T>(GraphOperands operand)
    {
        var value = EvalOperand(operand);
        if (value is T tValue)
        {
            return tValue;
        }
        throw new InvalidCastException($"Cannot cast operand value '{value}' to type {typeof(T).Name}.");
    }
    
    /// <summary>
    /// Evaluate a register operand and return its value.<br/>
    /// This method checks if the operand is a register operand and retrieves the value from the environment.<br/>
    /// It parses the operand to get the register ID and then retrieves the value from the environment's registers.
    /// </summary>
    /// <param name="operand"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    internal object? EvalRegisterOperand(GraphOperands operand)
    {
        if (!operand.Kind.HasFlag(EGraphOperandKind.Register))
        {
            throw new InvalidOperationException($"Expected a register operand, but got {operand.Kind}.");
        }
        
        var registerId = ParseRegisterOperand(operand);
        return _env.GetRegister(registerId).Value.GetValue();
    }
    /// <summary>
    /// Evaluate a register operand and return its value as a specific type.<br/>
    /// This method checks if the operand is a register operand and retrieves the value from the environment.<br/>
    /// It parses the operand to get the register ID and then retrieves the value from the environment's registers.<br/>
    /// If the value is not of the expected type, it returns null.
    /// </summary>
    /// <param name="operand"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    internal T? EvalRegisterOperand<T>(GraphOperands operand)
    {
        var value = EvalRegisterOperand(operand);
        if (value is T tValue)
        {
            return tValue;
        }

        return default;
    }

    public void Reset(params object[] parameters)
    {
        if (parameters.Length != 2)
            throw new ArgumentException("Invalid number of parameters for resetting GraphInterpreter. Expected 2 parameters: IList<GraphLabeledInstr> program, GraphEvalEnvironment env.");
        _program = (IList<GraphLabeledInstr>)parameters[0]!;
        _env = (GraphEvalEnvironment)parameters[1]!;
    }
}