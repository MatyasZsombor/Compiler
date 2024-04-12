namespace BetterInterpreter;

public enum Precedence
{
    Lowest,
    Equals,
    LessGreater,
    Sum,
    Product,
    Prefix,
    Call,
}

public class Parser
{
    private Token _curToken = null!;
    private Token _peekToken = null!;
    private int _read = -1;
    private readonly List<Token> _tokens;
    public readonly List<string> Errors = [];
    private readonly Dictionary<TokenType, IInfixParser> _infixParsers = [];
    private readonly Dictionary<TokenType, IPrefixParser> _prefixParsers = [];

    public Parser(List<Token> tokens)
    {
        _tokens = tokens;
        RegisterPrefix(TokenType.Ident, new IdentifierParser());
        RegisterPrefix(TokenType.Int, new IntegerLiteralParser());
        
        NextToken();
        NextToken();
    }

    public ProgramNode ParseProgram()
    {
        ProgramNode programNode = new ProgramNode();

        while (_peekToken.TokenType != TokenType.Eof)
        {
            IStatement? statement = ParseStatement();
            if (statement != null)
            {
                programNode.Statements.Add(statement);
            }
            NextToken();
        }

        return programNode;
    }

    private IStatement? ParseStatement() =>
        _curToken.TokenType switch
        {
            TokenType.Type => ParseDeclaration(),
            TokenType.Return => ParseReturnStatement(),
            _              => ParseExpressionStatement()
        };

    private ExpressionStatement? ParseExpressionStatement()
    {
        IExpression? parsed = ParseExpression(Precedence.Lowest);
        if (parsed == null)
        {
            return null;
        }
        ExpressionStatement statement = new ExpressionStatement(_curToken, parsed);

        if (_peekToken.TokenType == TokenType.Semicolon)
        {
            NextToken();
        }
        return statement;
    }

    private IExpression? ParseExpression(Precedence precedence)
    {
        _prefixParsers.TryGetValue(_curToken.TokenType, out var prefixParser);
        if (prefixParser == null)
        {
            Errors.Add("Couldn't find Prefix Parser for " + _curToken.TokenType);
        }
        
        return prefixParser?.Parse(this, _curToken);
    }
    
    private ReturnStatement ParseReturnStatement()
    {
        Token tmp = _curToken;
        NextToken();

        while (_curToken.TokenType != TokenType.Semicolon)
        {
            NextToken();
        }
        return new ReturnStatement(tmp);
    }
    
    private DeclarationStatement? ParseDeclaration()
    {
        Token tmp = _curToken;
        if (!ExpectPeek(TokenType.Ident))
        {
            return null;
        }

        Identifier name = new Identifier(_curToken, _curToken.Literal);

        if (!ExpectPeek(TokenType.Assign))
        {
            return null;
        }

        while (_curToken.TokenType != TokenType.Semicolon)
        {
            NextToken();
        }

        return new DeclarationStatement(tmp, name);
    }


    private bool ExpectPeek(TokenType tokenType)
    {
        if (_peekToken.TokenType != tokenType)
        {
            Errors.Add($"Expected next token to be {tokenType}, got {_peekToken.TokenType} instead");
            return false;
        }

        NextToken();
        return true;
    }
    
    private void NextToken()
    {
        _curToken = _peekToken;
        _read++;
        if (_read < _tokens.Count)
        {
            _peekToken = _tokens[_read];
        }
    }

    private void RegisterPrefix(TokenType tokenType, IPrefixParser prefixParser)
    {
        _prefixParsers.Add(tokenType, prefixParser);
    }
    
    private void RegisterInfix(TokenType tokenType, IInfixParser infixParser)
    {
        _infixParsers.Add(tokenType, infixParser);
    }
    
    private interface IPrefixParser
    {
        IExpression? Parse(Parser parser, Token token);
    }

    private interface IInfixParser
    {
        IExpression Parse(Parser parser, IExpression function, Token token);
    }
    
    private class IdentifierParser : IPrefixParser
    {
        public IExpression Parse(Parser parser, Token token) => new Identifier(token, token.Literal);
    }
    
    private class IntegerLiteralParser: IPrefixParser
    {
        public IExpression? Parse(Parser parser, Token token)
        {
            IntegerLiteral literal = new IntegerLiteral(token);

            if (!int.TryParse(token.Literal, out int tmp))
            {
                parser.Errors.Add("Could not parse " + token.Literal + "as integer");
                return null;
            }

            literal.Value = tmp;
            return literal;
        }
    }
}
