using RPGCreator.SDK.Graph;
using Serilog;

namespace RPGCreator.Core.Parser.Graph;

/// <summary>
/// This class is responsible for allocating and managing graph nodes IR registers.<br/>
/// Optimization is one of the main goals of this class.
/// </summary>
public class GraphAllocator
{
    public bool FoundInvalidRegister { get; private set; } = false;
    
    private List<GraphLabeledInstr> _block;
    // String = RegisterId, int = Instruction Index
    Dictionary<string, (List<int>, int)> _registerLiveness = new Dictionary<string, (List<int>, int)>(StringComparer.Ordinal);
    
    // String = RegisterId, string = New RegisterId
    // This is used to remap registers to free ones when possible
    // This is useful for optimizing register allocation and replacing registers that could be replaced with non-used registers.
    // This is a dictionary to avoid duplicates and to keep track of the original register id
    Dictionary<string, string> _registerRemap = new Dictionary<string, string>(StringComparer.Ordinal);
    
    public GraphAllocator(List<GraphLabeledInstr> instructions)
    {
        _block = instructions ?? throw new ArgumentNullException(nameof(instructions), "Instructions cannot be null.");
    }

    public void OptimizeRegisterAllocation()
    {
        ScanLiveness();
        RemapRegisters();
    }

    private void AllocateRegister(string RegisterId, int instructionIndex)
    {
        // Check if the register is already allocated
        if(_registerLiveness.TryGetValue(RegisterId, out var instructionIndices))
        {
            // If it is, add the instruction index to the list
            instructionIndices.Item1.Add(instructionIndex);
            // Update the last instruction index where this register was used
            instructionIndices.Item2 = instructionIndex;
            _registerLiveness[RegisterId] = instructionIndices;
        }
        else
        {
            // If not, create a new entry for the register
            _registerLiveness[RegisterId] = (new List<int> { instructionIndex }, instructionIndex);
        }
    }
    

    /// <summary>
    /// Scans the instructions register liveness to determine which registers are used at which points in the code.<br/>
    /// This is useful for optimizing register allocation and replacing registers that could be replaced with non-used registers.<br/>
    /// </summary>
    private void ScanLiveness()
    {
        var index = 0;
        foreach (var block in _block)
        {
            var instructions = block.Instrs;
            foreach (var instruction in instructions)
            {
                if (instruction.Operands.Where(a => a.Kind.HasFlag(EGraphOperandKind.Register)).ToList() is List<GraphOperands> operands)
                {
                    foreach (var operand in operands)
                    {
                        if (!operand.Text.StartsWith("rx"))
                            continue; // Not a register, skip it
                        if (operand.Kind == EGraphOperandKind.None)
                            continue;
                        AllocateRegister(operand.Text, index);
                    }
                }

                index++;
            }
            
        }
    }

    private string FindFreeRegister(int current_index)
    {
        var x = _registerLiveness.OrderBy(kvp => kvp.Value.Item2);
        var y = x.FirstOrDefault(kvp => kvp.Value.Item2 < current_index).Key ?? GraphCompileContext.NoneRegisterId;
        return y;
    }
    
    private int FindRegisterStartingIndex(string registerId)
    {
        if (_registerLiveness.TryGetValue(registerId, out var instructionIndices))
        {
            // Return the first instruction index where this register was used
            return instructionIndices.Item1.FirstOrDefault();
        }
        return -1; // Register not found
    }
    

    private void RemapRegisters()
    {
        var index = 0;
        List<string> seenRegisters = new List<string>();

        foreach (var block in _block)
        {
            var instructions = block.Instrs;
            foreach (var instruction in instructions)
            {
                int str_index = 0;
                // Check if the instruction has a register to remap
                if (instruction.Operands.Where(a =>
                    {
                        str_index++;
                        return a.Kind.HasFlag(EGraphOperandKind.Register);
                    }).ToList() is List<GraphOperands> operands)
                {
                    foreach (var operand in operands)
                    {
                        string registerId = operand.Text;
                        // We do also a quick check to see if the register is valid (meaning it is not "rx-1")
                        if (registerId == GraphCompileContext.NoneRegisterId)
                        {
                            Log.Error("Invalid register found in instruction {instructionIndex}: {instruction}. Register ID: {registerId}", index, instruction, registerId);
                            FoundInvalidRegister = true;
                            return; // Skip this instruction if the register is invalid
                        }
                        
                        if (_registerRemap.ContainsKey(registerId))
                        {
                            // If the register is already being remapped, we need to update the instruction
                            instruction.Operands[str_index - 1] = GraphIR.Operands(operand.Kind, _registerRemap[registerId]);
                            continue;
                        }
                        // Check if the current register can be replaced (if it's the first time we see it)
                        if (seenRegisters.Contains(registerId))
                            continue; // If we have already seen this register, it means it's used before we could remap it, so we skip it
                        seenRegisters.Add(registerId);
                        
                        // Find a free register to replace the current one
                        var newRegisterId = FindFreeRegister(index);
                        
                        // If the new register is the same as the current one, skip
                        if (newRegisterId == registerId)
                            continue;

                        if (newRegisterId == GraphCompileContext.NoneRegisterId)
                        {
                            // If no free register is found, we cannot remap this register
                            // This could happen if all registers are in use or if the graph is too complex
                            continue;
                        }
                        
                        seenRegisters.Remove(registerId);
                        
                        _registerRemap[registerId] = newRegisterId;
                        _registerLiveness[registerId] = ([], 0); // Clear the liveness of the old register so we can reuse it
                        
                        instruction.Operands[str_index - 1] = GraphIR.Operands(operand.Kind, newRegisterId);
                    }
                }
                
                index++;
            }
        }
    }
    
}