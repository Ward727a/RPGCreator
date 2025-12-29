using RPGCreator.SDK.Graph;
using Serilog;

namespace RPGCreator.Core.Parser.Graph;

/// <summary>
/// This class validates a graph program.
/// Right now it only checks for valid opcodes and operand kinds.
/// But later on, it will also check for valid control flow (e.g. no jumps to non-existing labels)
/// </summary>
public static class GraphValidator
{
    /// <summary>
    /// Validate the given program against the graph table.
    /// </summary>
    /// <param name="program"></param>
    public static bool Validate(IReadOnlyList<GraphLabeledInstr> program)
    {
        for (int i=0; i < program.Count; i++)
        {
            var programInstruction = program[i].Instrs;

            var instrIndex = 0;
            foreach (var instr in programInstruction)
            {
                var spec = GraphTable.Get(instr.OpCode);
                if (spec == null)
                {
                    Log.Error("Invalid opcode {OpCode} at program block {BlockIndex}, instruction {InstructionIndex}.", 
                        instr.OpCode, i, instrIndex);
                    Log.Error("Available opcodes: {AvailableOpcodes}", string.Join(", ", GraphTable.ValidOpcodes));
                    // Invalid opcode
                    return false;
                }

                // Validate operands
                if (instr.Operands.Length != spec.Signature.Length)
                {
                    Log.Error("Invalid number of operands for opcode {OpCode} at program block {BlockIndex}, instruction {InstructionIndex}. Expected {Expected}, got {Got}.",
                        instr.OpCode, i, instrIndex, spec.Signature.Length, instr.Operands.Length);
                    return false;
                }
                
                for (int j = 0; j < instr.Operands.Length; j++)
                {
                    var operand = instr.Operands[j];
                    var signature = spec.Signature[j];

                    if ((operand.Kind & signature) == 0)
                    {
                        Log.Error("Invalid operand kind {OperandKind} for opcode {OpCode} at program block {BlockIndex}, instruction {InstructionIndex}. Expected {Expected}.",
                            operand.Kind, instr.OpCode, i, instrIndex, signature);
                        return false;
                    }
                }
                // If we reach here, the instruction is valid
                Log.Information("Instruction {InstructionIndex} in block {BlockIndex} is valid: {OpCode} with operands {Operands}.",
                    instrIndex, i, instr.OpCode, string.Join(", ", instr.Operands.Select(o => $"{o.Kind}:{o.Text}")));

                instrIndex++;

            }
        }

        return true;
    }
}