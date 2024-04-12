namespace BetterInterpreter;

public class Lexer
{
    private static readonly Dictionary<string, TokenType> keyWords = new ()
    {
        {"int", TokenType.Type},
        {"bool", TokenType.Type},
        {"if", TokenType.If},
        {"else", TokenType.Else},
        {"return", TokenType.Return},
        {"true", TokenType.True},
        {"false", TokenType.False}
    };

    public readonly List<Token> LexedTokens = [];
    private readonly int _tokCounter;
    private readonly string _input;
    private int _position;
    private int _readPosition;
    private string _cur = "";

    public Lexer(string input)
    {
        _input = input;
        ReadChar();
        
        Token token = NextToken();
        LexedTokens.Add(token);
        _tokCounter++;
        
        while (token.TokenType != TokenType.Eof)
        {
            token = NextToken();
            LexedTokens.Add(token);
            _tokCounter++;
        }
    }

    private Token NextToken()
    {
        Token token;
        ConsumeWhiteSpace();
        
        switch (_cur)
        {
            case "=":
                if (PeekChar() == '=')
                {
                    string tmp = _cur;
                    ReadChar();
                    token = new Token(TokenType.Eq, tmp + _cur);
                    break;
                }
                token = new Token(TokenType.Assign, _cur);
                break;
            case "+":
                token = new Token(TokenType.Plus, _cur);
                break;
            case "-":
                token = new Token(TokenType.Minus, _cur);
                break;
            case "!":
                if (PeekChar() == '=')
                {
                    string tmp = _cur;
                    ReadChar();
                    token = new Token(TokenType.NotEq, tmp + _cur);
                    break;
                }
                token = new Token(TokenType.Bang, _cur);
                break;
            case "*":
                token = new Token(TokenType.Asterisk, _cur);
                break;
            case "/":
                token = new Token(TokenType.Slash, _cur);
                break;
            case ",":
                token = new Token(TokenType.Comma, _cur);
                break;
            case "<":
                token = new Token(TokenType.Lt, _cur);
                break;
            case ">":
                token = new Token(TokenType.Gt, _cur);
                break;
            case ";":
                token = new Token(TokenType.Semicolon, _cur);
                break;
            case "(":
                token = new Token(TokenType.Lparen, _cur);
                break;
            case ")":
                token = new Token(TokenType.Rparen, _cur);
                break;
            case "{":
                token = new Token(TokenType.Lbrace, _cur);
                break;
            case "}":
                token = new Token(TokenType.Rbrace, _cur);
                break;
            case "0":
                token = new Token(TokenType.Eof, "");
                break;
            default:
                if (char.IsLetter(_cur[0]))
                {
                    string identifier = ReadIdentifier();
                    if (_tokCounter <= 0 || LexedTokens[_tokCounter - 1].TokenType != TokenType.Type ||
                        _cur != "(")
                    {
                        return new Token(LookUpIdentifier(identifier), identifier);
                    }

                    if (!keyWords.TryAdd(identifier, TokenType.Call))
                    {
                        throw new Exception("A method with the same signature has already been declared.");
                    }
                    return new Token(TokenType.Function, identifier);
                }
                if (char.IsDigit(_cur[0]))
                {
                    return new Token(TokenType.Int, ReadNumber());
                }
                token = new Token(TokenType.Illegal, _cur);
                break;
        }
        
        ReadChar();
        return token;
    }
    
    private string ReadIdentifier()
    {
        int tmp = _position;
        while (char.IsLetter(_cur[0]) || char.IsDigit(_cur[0]))
        {
            ReadChar();
        }
        return _input.Substring(tmp, _position - tmp);
    }

    private string ReadNumber()
    {
        int tmp = _position;
        while (char.IsDigit(_cur[0]))
        {
            ReadChar();
        }
        return _input.Substring(tmp, _position - tmp);
    }

    private char PeekChar() => _readPosition >= _input.Length ? '0' : _input[_readPosition];
    
    private void ReadChar()
    {
        _cur = _readPosition >= _input.Length ? "0" : _input[_readPosition].ToString();
        _position = _readPosition;
        _readPosition++;
    }
    
    private void ConsumeWhiteSpace()
    {
        while (_cur is " " or "\t" or "\n" or "\r")
        {
            ReadChar();
        }
    }
    
    private static TokenType LookUpIdentifier(string literal) => keyWords.GetValueOrDefault(literal, TokenType.Ident);
}
