namespace RPGCreator.Core.Parser.Graph.TableHandler;

public enum EOpCodeHandlerType
{
    /// <summary>
    /// Public operations that are intended for use by nodes.<br/>
    /// They are part of the public API and can be used by any node.<br/>
    /// Example: Basic operations like Add, Subtract, Multiply, etc., which are commonly used in graph nodes.<br/>
    /// This is the default type if not specified.
    /// </summary>
    Public,
    /// <summary>
    /// Internal operations that are not intended for public use (e.g. should not be used by node directly).<br/>
    /// They are used by the interpreter or other internal mechanisms.<br/>
    /// Example: Alloc operations, which are used to allocate memory for variables in the graph interpreter.
    /// </summary>
    Internal,
    /// <summary>
    /// Deprecated operations that are no longer recommended for use.<br/>
    /// They may still work, but their use is discouraged, and they may be removed in the future.<br/>
    /// Example: Old operations that have been replaced by new ones, but are still present for compatibility reasons.
    /// </summary>
    Deprecated,
    /// <summary>
    /// Experimental operations that are in the testing phase.<br/>
    /// They may not be fully implemented or may change in the future.<br/>
    /// Example: New operations that are being tested and may be added to the public API in the future, but are not yet stable.
    /// </summary>
    Experimental
}