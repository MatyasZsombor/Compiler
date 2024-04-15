namespace Compiler;

public enum Precedence
{
    Lowest = 0,
    Equals = 2,
    Comparison = 3,
    Sum = 4,
    Product = 5,
    Prefix = 6,
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
    private readonly Dictionary<TokenType, IPostfixParser> _postfixParsers = [];

    public Parser(List<Token> tokens)
    {
        _tokens = tokens;
        RegisterPrefix(TokenType.Ident, new IdentifierParser());
        RegisterPrefix(TokenType.Int, new IntegerLiteralParser());
        RegisterPrefix(TokenType.Bang, new PrefixExpressionParser());
        RegisterPrefix(TokenType.Minus, new PrefixExpressionParser());
        RegisterPrefix(TokenType.True, new BoolLiteralParser());
        RegisterPrefix(TokenType.False, new BoolLiteralParser());
        RegisterPrefix(TokenType.Lparen, new GroupedExpressionParser());

        RegisterInfix(TokenType.Plus, new InfixExpressionParser(Precedence.Sum));
        RegisterInfix(TokenType.Minus, new InfixExpressionParser(Precedence.Sum));
        RegisterInfix(TokenType.Slash, new InfixExpressionParser(Precedence.Product));
        RegisterInfix(TokenType.Asterisk, new InfixExpressionParser(Precedence.Product));
        RegisterInfix(TokenType.Eq, new InfixExpressionParser(Precedence.Equals));
        RegisterInfix(TokenType.NotEq, new InfixExpressionParser(Precedence.Equals));
        RegisterInfix(TokenType.Lt, new InfixExpressionParser(Precedence.Comparison));
        RegisterInfix(TokenType.Gt, new InfixExpressionParser(Precedence.Comparison));

        RegisterPostfix(TokenType.PostfixPlus, new PostfixParser());
        RegisterPostfix(TokenType.PostfixMinus, new PostfixParser());
        RegisterPostfix(TokenType.Assign, new AssigmentParser());
        
        NextToken();
        NextToken();
    }

    public ProgramNode ParseProgram()
    {
        ProgramNode programNode = new ProgramNode();

        while (_curToken.TokenType != TokenType.Eof)
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

    private IStatement? ParseStatement()
    {
        switch (_curToken.TokenType)
        {
            case TokenType.Type:
                return ParseDeclaration();
            case TokenType.Return:
                return ParseReturnStatement();
            case TokenType.Ident:
                return ParsePostfixStatement();
            default:
                Errors.Add( "Only assigment, call, increment and decrement can be used as statement");
                while (_curToken.TokenType != TokenType.Semicolon)
                {
                    if (_curToken.TokenType == TokenType.Eof)
                    {
                        Errors.Add($"Expected next token to be {TokenType.Semicolon}, got {_peekToken.TokenType} instead");
                        break;
                    }
                    NextToken();
                }
                return null;
        }
    }

    private IStatement? ParsePostfixStatement()
    {
        if (_curToken.TokenType != TokenType.Ident)
        {
            Errors.Add("");
            return null;
        }

        Token tmpToken = _curToken;
        Identifier tmp = new Identifier(_curToken, _curToken.Literal);
        NextToken();
        
        _postfixParsers.TryGetValue(_curToken.TokenType, out var postfixParser);
        if (postfixParser== null)
        { 
            Errors.Add("Couldn't find Postfix Parser for " + _curToken.TokenType);
            return null;
        }
        
        IStatement? statement = postfixParser.Parse(this, _curToken);

        if (statement == null)
        {
            return null;
        } 
        if (statement.GetType() == typeof(PostFixStatement))
        {
            PostFixStatement postFixStatement = (PostFixStatement) statement;
            postFixStatement.Token = tmpToken;
            postFixStatement.Name = tmp;

            return postFixStatement;
        }

        AssigmentStatement assigmentStatement = (AssigmentStatement) statement;
        assigmentStatement.Token = tmpToken;
        assigmentStatement.Name = tmp;

        return assigmentStatement;
    }

    private IExpression? ParseExpression(Precedence precedence)
    {
        _prefixParsers.TryGetValue(_curToken.TokenType, out var prefixParser);
        if (prefixParser == null)
        {
            Errors.Add("Couldn't find Prefix Parser for " + _curToken.TokenType);

            return null;
        }

        IExpression? left = prefixParser.Parse(this, _curToken);
        if (left == null)
        {
            return null;
        }

        while (_peekToken.TokenType != TokenType.Semicolon && precedence < PeekPrecedence())
        {
            _infixParsers.TryGetValue(_peekToken.TokenType, out var infixParser);
            if (infixParser == null)
            {
                return left;
            }

            NextToken();

            left = infixParser.Parse(this, left, _curToken);
        }

        return left;
    }

    private ReturnStatement? ParseReturnStatement()
    {
        ReturnStatement statement = new ReturnStatement(_curToken);
        NextToken();

        IExpression? tmp = ParseExpression(Precedence.Lowest);
        ExpectPeek(TokenType.Semicolon);

        if (tmp == null)
        {
            return null;
        }

        statement.ReturnValue = tmp;

        return statement;
    }

    private DeclarationStatement? ParseDeclaration()
    {
        Token tmpToken = _curToken;
        if (!ExpectPeek(TokenType.Ident))
        {
            return null;
        }

        DeclarationStatement statement = new(tmpToken, new Identifier(_curToken, _curToken.Literal));

        if (!ExpectPeek(TokenType.Assign))
        {
            return null;
        }

        NextToken();
        IExpression? tmp = ParseExpression(Precedence.Lowest);
        ExpectPeek(TokenType.Semicolon);

        if (tmp == null)
        {
            return null;
        }

        statement.Value = tmp;

        return statement;
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
    
    private void RegisterPostfix(TokenType tokenType, IPostfixParser postfixParser)
    {
        _postfixParsers.Add(tokenType, postfixParser);
    }

    private void RegisterInfix(TokenType tokenType, IInfixParser infixParser)
    {
        _infixParsers.Add(tokenType, infixParser);
    }

    private Precedence PeekPrecedence()
    {
        if (!_infixParsers.ContainsKey(_peekToken.TokenType))
        {
            return Precedence.Lowest;
        }

        IInfixParser parslet = _infixParsers[_peekToken.TokenType];

        return parslet.Precedence;
    }

    private interface IPrefixParser
    {
        IExpression? Parse(Parser parser, Token token);
    }

    private interface IInfixParser
    {
        public Precedence Precedence { get; }
        IExpression? Parse(Parser parser, IExpression? left, Token token);
    }
    
    private interface IPostfixParser
    { 
        IStatement? Parse(Parser parser, Token token);
    }

    private class InfixExpressionParser(Precedence precedence) : IInfixParser
    {
        public Precedence Precedence { get; } = precedence;

        public IExpression? Parse(Parser parser, IExpression? left, Token token)
        {
            InfixExpression expression = new InfixExpression(token, token.Literal, left!);

            parser.NextToken();
            IExpression? right = parser.ParseExpression(Precedence);
            if (right == null)
            {
                return null;
            }

            expression.Right = right;

            return expression;
        }
    }

    private class IdentifierParser : IPrefixParser
    {
        public IExpression Parse(Parser parser, Token token) => new Identifier(token, token.Literal);
    }

    private class IntegerLiteralParser : IPrefixParser
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

    private class BoolLiteralParser : IPrefixParser
    {
        public IExpression Parse(Parser parser, Token token) =>
            new BoolLiteral(parser._curToken, parser._curToken.Literal == "true");
    }

    private class GroupedExpressionParser : IPrefixParser
    {
        public IExpression? Parse(Parser parser, Token token)
        {
            parser.NextToken();

            IExpression? expression = parser.ParseExpression(Precedence.Lowest);

            return parser.ExpectPeek(TokenType.Rparen) ? expression : null;
        }
    }

    private class PrefixExpressionParser : IPrefixParser
    {
        public IExpression? Parse(Parser parser, Token token)
        {
            PrefixExpression prefixExpression = new PrefixExpression(token, token.Literal);
            parser.NextToken();

            IExpression? expression = parser.ParseExpression(Precedence.Prefix);
            if (expression == null)
            {
                return null;
            }

            prefixExpression.Right = expression;

            return prefixExpression;
        }
    }
    
    private class PostfixParser : IPostfixParser
    {
        public IStatement? Parse(Parser parser, Token token)
        {
            Token tmp = token;
           return parser.ExpectPeek(TokenType.Semicolon) ? new PostFixStatement(tmp.Literal) : null;
        }
    }

    private class AssigmentParser : IPostfixParser
    {
        public IStatement? Parse(Parser parser, Token token)
        {
            parser.NextToken();
            IExpression? tmp = parser.ParseExpression(Precedence.Lowest);
            parser.ExpectPeek(TokenType.Semicolon);

            return tmp == null ? null : new AssigmentStatement(tmp);
        }
    }
}