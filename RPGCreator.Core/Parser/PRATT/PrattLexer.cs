using System.Globalization;
using RPGCreator.SDK.Parser.PrattFormula;

namespace RPGCreator.Core.Parser.PRATT;

public sealed class PrattLexer
{
    private bool Eof => _index >= _source.Length;
    private char Peek(int look = 0) => (_index + look < _source.Length) ? _source[_index + look] : '\0';
    private readonly string _source;
    private int _index;

    public PrattLexer(string source)
    {
        _source = source ?? string.Empty;
    }

    public List<SPrattToken> Tokenize()
    {
        var tokens = new List<SPrattToken>();
        while (!Eof)
        {
            SkipTrivial(); // Skip whitespace and comments
            if (Eof) break;
            
            int start = _index;
            char character = Peek();

            if (char.IsDigit(character) || (character == '.' && char.IsDigit(Peek(1))))
            {
                tokens.Add(ReadNumber());
                continue;
            }

            if (IsIdentifierStart(character))
            {
                tokens.Add(ReadIdentifier());
                continue;
            }

            switch (character)
            {
                case '(':
                    tokens.Add(Make(EPrattTokenKind.LParen, start, 1));
                    _index++;
                    break;
                case ')': 
                    tokens.Add(Make(EPrattTokenKind.RParen, start, 1));
                    _index++;
                    break;
                case '[':
                    tokens.Add(Make(EPrattTokenKind.LBracket, start, 1));
                    _index++;
                    break;
                case ']':
                    tokens.Add(Make(EPrattTokenKind.RBracket, start, 1));
                    _index++;
                    break;
                case ',':
                    tokens.Add(Make(EPrattTokenKind.Comma, start, 1));
                    _index++;
                    break;
                case '.':
                    tokens.Add(Make(EPrattTokenKind.Dot, start, 1));
                    _index++;
                    break;
                case '?':
                    tokens.Add(Make(EPrattTokenKind.Question, start, 1));
                    _index++;
                    break;
                case ':':
                    tokens.Add(Make(EPrattTokenKind.Colon, start, 1));
                    _index++;
                    break;
                
                case '+':
                    tokens.Add(Make(EPrattTokenKind.Plus, start, 1));
                    _index++;
                    break;
                case '-':
                    tokens.Add(Make(EPrattTokenKind.Minus, start, 1));
                    _index++;
                    break;
                case '*':
                    tokens.Add(Make(EPrattTokenKind.Multiply, start, 1));
                    _index++;
                    break;
                case '/':
                    tokens.Add(Make(EPrattTokenKind.Divide, start, 1));
                    _index++;
                    break;
                case '%':
                    tokens.Add(Make(EPrattTokenKind.Percent, start, 1));
                    _index++;
                    break;
                case '^':
                    tokens.Add(Make(EPrattTokenKind.Caret, start, 1));
                    _index++;
                    break;
                
                case '!':
                    if (Match('=')) 
                    {
                        tokens.Add(Make(EPrattTokenKind.NotEq, start, 2));
                    }
                    else
                    {
                        tokens.Add(Make(EPrattTokenKind.Bang, start, 1));
                    }
                    break;
                case '=':
                    if (Match('='))
                    {
                        tokens.Add(Make(EPrattTokenKind.EqEq, start, 2));
                    }
                    else
                    {
                        throw LexError("Unexpected '=' (use '==' for equality)", start, 1);
                    }
                    break;
                case '<':
                    if (Match('='))
                    {
                        tokens.Add(Make(EPrattTokenKind.LessThanEq, start, 2));
                    }
                    else
                    {
                        tokens.Add(Make(EPrattTokenKind.LessThan, start, 1));
                    }
                    break;
                case '>':
                    if (Match('='))
                    {
                        tokens.Add(Make(EPrattTokenKind.GreaterThanEq, start, 2));
                    }
                    else
                    {
                        tokens.Add(Make(EPrattTokenKind.GreaterThan, start, 1));
                    }

                    break;
                case '&':
                    if (Match('&'))
                    {
                        tokens.Add(Make(EPrattTokenKind.AndAnd, start, 2));
                    }
                    else
                    {
                        throw LexError("Unexpected '&' (use '&&' for logical AND)", start, 1);
                    }
                    break;
                case '|':
                    if (Match('|'))
                    {
                        tokens.Add(Make(EPrattTokenKind.OrOr, start, 2));
                    }
                    else 
                    {
                        throw LexError("Unexpected '|' (use '||' for logical OR)", start, 1);
                    }
                    break;
                default:
                    throw LexError($"Unexpected character '{character}'", start, 1);
            }
        }
        
        tokens.Add(new SPrattToken(EPrattTokenKind.EOF, string.Empty, _index, 0));
        return tokens;
    }

    private void SkipTrivial()
    {
        while (!Eof)
        {
            if (char.IsWhiteSpace(Peek()))
            {
                _index++;
                continue;
            }

            if (Peek() == '/' && Peek(1) == '/')
            {
                _index += 2;
                while (!Eof && Peek() != '\n') _index++;
                continue;
            }
            
            if (Peek() == '/' && Peek(1) == '*')
            {
                _index += 2;
                while (!Eof && !(Peek() == '*' && Peek(1) == '/')) _index++;
                if (!Eof) _index += 2; // Skip the closing */
                continue;
            }

            break;
        }
    }

    private bool Match(char expected)
    {
        if (!Eof && Peek(1) == expected && Peek() != '\0')
        {
            _index += 2;
            return true;
        }

        return false;
    }
    
    private static bool IsIdentifierStart(char character) => char.IsLetter(character) || character == '_';
    private static bool IsIdentifierPart(char character) => char.IsLetterOrDigit(character) || character == '_';

    private SPrattToken ReadIdentifier()
    {
        int start = _index;
        _index++;
        while (!Eof && IsIdentifierPart(Peek())) _index++;
        var lexeme = _source.Substring(start, _index - start);
        return new SPrattToken(EPrattTokenKind.Identifier, lexeme, start, _index - start);
    }

    private SPrattToken ReadNumber()
    {
        int start = _index;
        bool seenDot = false;

        if (Peek() == '.')
        {
            seenDot = true;
            _index++;
            if(!char.IsDigit(Peek()))
                throw new Exception($"Unexpected end of infix operator: {_source[_index]} (expected digit after '.' dot)");
        }

        while (char.IsDigit(Peek())) _index++;

        if (Peek() == '.' && !seenDot)
        {
            seenDot = true;
            _index++;
            while (char.IsDigit(Peek())) _index++;
        }

        if (Peek() == 'e' || Peek() == 'E')
        {
            int exponentStart = _index;
            _index++;
            if (Peek() == '+' || Peek() == '-') _index++;
            if (!char.IsDigit(Peek()))
            {
                _index = exponentStart; // Reset index if no digit follows 'e' or 'E'
            }
            else
            {
                while (char.IsDigit(Peek())) _index++;
            }
        }
        
        var lexeme = _source.Substring(start, _index - start);

        if (!double.TryParse(lexeme, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
            throw LexError($"Invalid number format literal '{lexeme}'", start, _index - start);
        
        return new SPrattToken(EPrattTokenKind.Number, lexeme, start, _index - start);
    }
    
    private static SPrattToken Make(EPrattTokenKind kind, int start, int length)
    {
        return new SPrattToken(kind, kind.ToString(), start, length);
    }
    
    private Exception LexError(string message, int start, int length)
    {
        return new Exception($"{message} at {start}..{start+length}[{length}]:{_source.Substring(start, length)}");
    }
    
}