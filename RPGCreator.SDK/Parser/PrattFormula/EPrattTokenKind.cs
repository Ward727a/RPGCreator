namespace RPGCreator.SDK.Parser.PrattFormula;

public enum EPrattTokenKind
{
    EOF, // End of file
    Number, // Numeric literal
    Identifier, // Identifier (variable, function, etc.)
    LParen, // Left parenthesis '('
    RParen, // Right parenthesis ')'
    LBracket, // Left bracket '['
    RBracket, // Right bracket ']'
    Comma, // Comma ','
    Dot, // Dot '.'
    Question, // Question mark '?'
    Colon, // Colon ':'
    Plus, // Plus '+'
    Minus, // Minus '-'
    Multiply, // Multiply '*'
    Divide, // Divide '/'
    Percent, // Modulo '%'
    Caret, // Caret '^'
    Bang, // Bang '!'
    AndAnd, // Logical AND '&&'
    OrOr, // Logical OR '||'
    EqEq, // Equality '=='
    NotEq, // Inequality '!='
    LessThan, // Less than '<'
    GreaterThan, // Greater than '>'
    GreaterThanEq, // Greater than or equal to '>='
    LessThanEq, // Less than or equal to '<='
    Assign, // Assignment '='
}