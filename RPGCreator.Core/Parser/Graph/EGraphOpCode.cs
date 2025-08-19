// ReSharper disable InconsistentNaming
namespace RPGCreator.Core.Type.Blueprint.Nodes;

public enum EGraphOpCode
{
    /* NON-NODE OPCODES */
    // Those are special opcodes that are not associated with a specific node type.
    // They are used in the back-end to control the flow of the graph, allocate constants or variables, etc.
    none, // This is a no-op, it does nothing. It can be used to fill empty slots in the graph.
    comment, // This is a comment, it will be ignored by the compiler. It can be used to add notes to the graph.
    label, // This is a label, it can be used to mark a position in the graph for jumps or branches.
    
    /* SYSTEM OPCODES */
    
    // This is the start of the graph, it will be the first node executed.
    // Each graph must have exactly one start node.
    start,
    // This is the end of the graph, it will be the last node executed.
    // Each graph must have exactly one end node.
    end,
    alloc_literal_string, // This will allocate a literal string in the VM. (operands: string, register)
    alloc_literal_int, // This will allocate a literal int in the VM. (operands: int, register)
    alloc_literal_float, // This will allocate a literal float in the VM. (operands: float, register)
    alloc_literal_bool, // This will allocate a literal bool in the VM. (operands: bool, register)
    alloc_literal_object, // This will allocate a literal vector2 in the VM. (operands: object, register)
    
    /* DEBUG OPCODES */
    
    debug_print, // This will print a debug message to the console. (operands: string | register)
    
    /* GET/SET OPCODES */
    
    get_variable, // This will get a variable from the context. (operands: path, register)
    set_variable, // This will set a variable in the context. (operands: path, register)
    get_vm, // This will get a variable from the VM. (operands: path, register)
    set_vm, // This will set a variable in the VM. (operands: path, register)
}