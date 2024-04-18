namespace UnitTests;
using Compiler;

public class LexerTest
{
    private Lexer _lexer = null!;
    
    [Fact]
    public void Test1()
    {
        _lexer = new Lexer("=+(){},;");
        Token[] expected =
        [
            new Token(TokenType.Assign, "="),
            new Token(TokenType.Plus, "+"),
            new Token(TokenType.Lparen, "("),
            new Token(TokenType.Rparen, ")"),
            new Token(TokenType.Lbrace, "{"),
            new Token(TokenType.Rbrace, "}"),
            new Token(TokenType.Comma, ","),
            new Token(TokenType.Semicolon, ";"),
            new Token(TokenType.Eof, "")
        ];

        Assert.Equal(_lexer.LexedTokens.Count, expected.Length);
        for (int i = 0; i < expected.Length; i++)
        {
            Token expectedToken = expected[i]; 
            Assert.True(expectedToken.CompareTo(_lexer.LexedTokens[i]), $"Expected token type {expectedToken.TokenType} and token literal {expectedToken.Literal}");
        }
    }

    [Fact]
    public void Test2()
    {
        _lexer = new Lexer("int x = 5;");
        Token[] expected =
        [
            new Token(TokenType.Type, "int"),
            new Token(TokenType.Ident, "x"),
            new Token(TokenType.Assign, "="),
            new Token(TokenType.Int, "5"),
            new Token(TokenType.Semicolon, ";"),
            new Token(TokenType.Eof, "")
        ];

        Assert.Equal(_lexer.LexedTokens.Count, expected.Length);
        for (int i = 0; i < expected.Length; i++)
        {
            Token expectedToken = expected[i]; 
            Assert.True(expectedToken.CompareTo(_lexer.LexedTokens[i]), $"Expected token type {expectedToken.TokenType} and token literal {expectedToken.Literal}");
        }
    }
    
    [Fact]
    public void Test3()
    {
        _lexer = new Lexer("bool x = false;");
        Token[] expected =
        [
            new Token(TokenType.Type, "bool"),
            new Token(TokenType.Ident, "x"),
            new Token(TokenType.Assign, "="),
            new Token(TokenType.False, "false"),
            new Token(TokenType.Semicolon, ";"),
            new Token(TokenType.Eof, "")
        ];

        Assert.Equal(_lexer.LexedTokens.Count, expected.Length);
        for (int i = 0; i < expected.Length; i++)
        {
            Token expectedToken = expected[i]; 
            Assert.True(expectedToken.CompareTo(_lexer.LexedTokens[i]), $"Expected token type {expectedToken.TokenType} and token literal {expectedToken.Literal}");
        }
    }

    [Fact]
    public void Test4()
    {
        _lexer = new Lexer("!-/*5;\n5 < 10 > 5;");
        Token[] expected =
        [
            new Token(TokenType.Bang, "!"),
            new Token(TokenType.Minus, "-"),
            new Token(TokenType.Slash, "/"),
            new Token(TokenType.Asterisk, "*"),
            new Token(TokenType.Int, "5"),
            new Token(TokenType.Semicolon, ";"),
            new Token(TokenType.Int, "5"),
            new Token(TokenType.Lt, "<"),
            new Token(TokenType.Int, "10"),
            new Token(TokenType.Gt, ">"),
            new Token(TokenType.Int, "5"),
            new Token(TokenType.Semicolon, ";"),
            new Token(TokenType.Eof, "")
        ];

        Assert.Equal(_lexer.LexedTokens.Count, expected.Length);
        for (int i = 0; i < expected.Length; i++)
        {
            Token expectedToken = expected[i]; 
            Assert.True(expectedToken.CompareTo(_lexer.LexedTokens[i]), $"Expected token type {expectedToken.TokenType} and token literal {expectedToken.Literal}");
        }
    }
    
    [Fact]
    public void Test5()
    {
        _lexer = new Lexer("==\n!=\nif\nelse\ntrue\nfalse\n");
        Token[] expected =
        [
            new Token(TokenType.Eq, "=="),
            new Token(TokenType.NotEq, "!="),
            new Token(TokenType.If, "if"), 
            new Token(TokenType.Else, "else"), 
            new Token(TokenType.True, "true"),
            new Token(TokenType.False, "false"),
            new Token(TokenType.Eof, "")
        ];
        
        Assert.Equal(_lexer.LexedTokens.Count, expected.Length);
        for (int i = 0; i < expected.Length; i++)
        {
            Token expectedToken = expected[i]; 
            Assert.True(expectedToken.CompareTo(_lexer.LexedTokens[i]), $"Expected token type {expectedToken.TokenType} and token literal {expectedToken.Literal}");
        }
    }

    [Fact]
    public void Test6()
    {
        _lexer = new Lexer("int myFunc(int x, int y); myFunc(1, 2);");
        Token[] expected =
        [
            new Token(TokenType.Type, "int"),
            new Token(TokenType.Function, "myFunc"),
            new Token(TokenType.Lparen, "("), 
            new Token(TokenType.Type, "int"), 
            new Token(TokenType.Ident, "x"),
            new Token(TokenType.Comma, ","),
            new Token(TokenType.Type, "int"),
            new Token(TokenType.Ident, "y"),
            new Token(TokenType.Rparen, ")"),
            new Token(TokenType.Semicolon, ";"), 
            new Token(TokenType.Ident, "myFunc"), 
            new Token(TokenType.Lparen, "("),
            new Token(TokenType.Int, "1"),
            new Token(TokenType.Comma, ","),
            new Token(TokenType.Int, "2"),
            new Token(TokenType.Rparen, ")"),
            new Token(TokenType.Semicolon, ";"),
            new Token(TokenType.Eof, "")
        ];
        
        Assert.Equal(_lexer.LexedTokens.Count, expected.Length);
        for (int i = 0; i < expected.Length; i++)
        {
            Token expectedToken = expected[i]; 
            Assert.True(expectedToken.CompareTo(_lexer.LexedTokens[i]), $"Expected token type {expectedToken.TokenType} and token literal {expectedToken.Literal}");
        }
    }
    
    [Fact]
    public void Test7()
    {
        _lexer = new Lexer("while(x < 5){ x++; break; }");
        Token[] expected =
        [
            new Token(TokenType.While, "while"),
            new Token(TokenType.Lparen, "("), 
            new Token(TokenType.Ident, "x"),
            new Token(TokenType.Lt, "<"),
            new Token(TokenType.Int, "5"),
            new Token(TokenType.Rparen, ")"),
            new Token(TokenType.Lbrace, "{"),
            new Token(TokenType.Ident, "x"),
            new Token(TokenType.PostfixPlus, "++"),
            new Token(TokenType.Semicolon, ";"),
            new Token(TokenType.Break, "break"),
            new Token(TokenType.Semicolon, ";"),
            new Token(TokenType.Rbrace, "}"),
            new Token(TokenType.Eof, "")
        ];
        
        Assert.Equal(_lexer.LexedTokens.Count, expected.Length);
        for (int i = 0; i < expected.Length; i++)
        {
            Token expectedToken = expected[i]; 
            Assert.True(expectedToken.CompareTo(_lexer.LexedTokens[i]), $"Expected token type {expectedToken.TokenType} and token literal {expectedToken.Literal}");
        }
    }
}
