// ReSharper disable InconsistentNaming
namespace RPGCreator.SDK.Graph;

public enum EGraphOpCode
{
    /* NON-NODE OPCODES */
    // Those are special opcodes that are not associated with a specific node type.
    // They are used in the back-end to control the flow of the graph, allocate constants or variables, etc.
    none, // This is a no-op, it does nothing. It can be used to fill empty slots in the graph. (should not be used in a valid graph)
    comment, // This is a comment, it will be ignored by the compiler. It can be used to add notes to the graph. (not implemented yet)
    
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
    
    debug_print, // This will print a debug message to the console. (operands: string | register, enum | register)
    
    /* GET/SET OPCODES */
    
    /// <summary>
    /// Get variable from the current context (like the current skill effect, event, etc).
    /// </summary>
    get_variable, // This will get a variable from the context. (operands: path, register)
    /// <summary>
    /// Set variable in the current context (like the current skill effect, event, etc).
    /// </summary>
    set_variable, // This will set a variable in the context. (operands: register, path)
    /// <summary>
    /// Get variable from the Global Game Memory (the VM).
    /// </summary>
    get_vm, // This will get a variable from the VM. (operands: path, register)
    /// <summary>
    /// Set variable in the Global Game Memory (the VM).
    /// </summary>
    set_vm, // This will set a variable in the VM. (operands: register, path)
    
    /* CHECK OPCODES */
    
    check_is_null, // This will check if a value is null. (operands: register | object, register)
    check_is_type, // This will check if a value is of a specific type. (operands: register | object, string | register, register)
    
    /* MATH OPCODES */
    
    math_add, // This will add two values together. (operands: register | int, register | int, register)
    math_subtract, // This will subtract two values. (operands: register | int, register | int, register)
    math_multiply, // This will multiply two values. (operands: register | int, register | int, register)
    math_divide, // This will divide two values. (operands: register | int, register | int, register)
    math_modulo, // This will calculate the modulo of two values. (operands: register | int, register | int, register)
    math_negate, // This will negate a value. (operands: register | int, register)
    math_abs, // This will calculate the absolute value of a number. (operands: register | int, register)
    math_sqrt, // This will calculate the square root of a number. (operands: register | int, register)
    math_pow, // This will calculate the power of a number. (operands: register | int, register | int, register)
    math_min, // This will calculate the minimum of two values. (operands: register | int, register | int, register)
    math_max, // This will calculate the maximum of two values. (operands: register | int, register | int, register)
    math_clamp, // This will clamp a value between two values. (operands: register | int, register | int, register | int, register)
    math_round, // This will round a value to the nearest integer. (operands: register | int, register)
    math_floor, // This will round a value down to the nearest integer. (operands: register | int, register)
    math_ceil, // This will round a value up to the nearest integer. (operands: register | int, register)
    
}