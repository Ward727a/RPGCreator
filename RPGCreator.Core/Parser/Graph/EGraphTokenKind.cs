namespace RPGCreator.Core.Parser.Graph;

public enum EGraphTokenKind
{
    BAD, // Bad token / Doesn't match any known token kind
    EOF, // End of file / End of Graph
    Identifier,
    RegisterKeyword, // 'rx[int]' for register definitions
    NodeKeyword, // Start of a node in the graph
    Dot, // Dot '.' for member access
    Comma, // Comma ',' for separating parameters or connections
    LParen, // Left parenthesis '(' for grouping or function calls
    RParen, // Right parenthesis ')' for grouping or function calls
    LBracket, // Left bracket '[' for lists or arrays
    RBracket, // Right bracket ']' for lists or arrays
    NumberLiteral,
    StringLiteral,
    BoolLiteral,
    NullKeyword,
    Arrow, // '->' for connections
    IfKeyword, // 'if' keyword for conditional nodes
    ElseKeyword, // 'else' keyword for conditional nodes
    ForKeyword, // 'for' keyword for loop nodes
    WhileKeyword, // 'while' keyword for loop nodes
    SwitchKeyword, // 'switch' keyword for switch nodes
    CaseKeyword, // 'case' keyword for switch cases
    DefaultKeyword, // 'default' keyword for switch default case
    BreakKeyword, // 'break' keyword for breaking out of loops
    ContinueKeyword, // 'continue' keyword for continuing loops
}