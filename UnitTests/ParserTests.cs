using BetterInterpreter;

namespace UnitTests;

public class ParserTests
{
    private Lexer _lexer = null!;
    private Parser _parser = null!;
    
    [Fact]
    private void Test1()
    {
        _lexer = new Lexer("int x = 5;\nint y = 10; int foobar = 838383;");
        _parser = new Parser(_lexer.LexedTokens);

        ProgramNode programNode = _parser.ParseProgram();
        
        Assert.Equal(3, programNode.Statements.Count);

        string[] expectedIdentifiers =
        [
            "x",
            "y",
            "foobar"
        ];

        for (int i = 0; i < expectedIdentifiers.Length; i++)
        {
            Assert.True(TestDeclaration(programNode.Statements[i], "int", expectedIdentifiers[i]));
        }
    }
    
    [Fact]
    private void Test2()
    {
        _lexer = new Lexer("foobar;");
        _parser = new Parser(_lexer.LexedTokens);

        ProgramNode programNode = _parser.ParseProgram();
        
        Assert.Single(programNode.Statements);
        Assert.Empty(_parser.Errors);
        
        Assert.Equal(typeof(ExpressionStatement), programNode.Statements[0].GetType());
        ExpressionStatement statement = (ExpressionStatement) programNode.Statements[0];
        
        Assert.Equal(typeof(Identifier), statement.Expression.GetType());
        Identifier identifier = (Identifier) statement.Expression;
        
        Assert.Equal("foobar", identifier.Value);
        Assert.Equal("foobar", identifier.TokenLiteral());
    }

    [Fact]
    private void Test3()
    {
        _lexer = new Lexer("5;");
        _parser = new Parser(_lexer.LexedTokens);

        ProgramNode programNode = _parser.ParseProgram();
        
        Assert.Single(programNode.Statements);
        Assert.Empty(_parser.Errors);
        
        Assert.Equal(typeof(ExpressionStatement), programNode.Statements[0].GetType());
        ExpressionStatement statement = (ExpressionStatement) programNode.Statements[0];
        
        Assert.Equal(typeof(IntegerLiteral), statement.Expression.GetType());
        IntegerLiteral identifier = (IntegerLiteral) statement.Expression;
        
        Assert.Equal(5, identifier.Value);
        Assert.Equal("5", identifier.TokenLiteral());
    }
    
    [Fact]
    private void Test4()
    {
        _lexer = new Lexer("!5;\n-15;");
        _parser = new Parser(_lexer.LexedTokens);

        ProgramNode programNode = _parser.ParseProgram();
        
        Assert.Equal(2,programNode.Statements.Count());
        Assert.Empty(_parser.Errors);
        
        Assert.Equal(typeof(ExpressionStatement), programNode.Statements[0].GetType());
        ExpressionStatement statement = (ExpressionStatement) programNode.Statements[0];
        
        Assert.Equal(typeof(PrefixExpression), statement.Expression.GetType());
        PrefixExpression prefixExpression = (PrefixExpression) statement.Expression;
        
        Assert.Equal("!", prefixExpression.TokenLiteral());
        Assert.Equal("!", prefixExpression.Operator);
        Assert.Equal("(!5)", prefixExpression.ToString());
        
        Assert.Equal(typeof(ExpressionStatement), programNode.Statements[1].GetType());
        ExpressionStatement statement2 = (ExpressionStatement) programNode.Statements[1];
        
        Assert.Equal(typeof(PrefixExpression), statement2.Expression.GetType());
        PrefixExpression prefixExpression2 = (PrefixExpression) statement2.Expression;
        
        Assert.Equal("-", prefixExpression2.TokenLiteral());
        Assert.Equal("-", prefixExpression2.Operator);
        Assert.Equal("(-15)", prefixExpression2.ToString());
    }

    [Fact]
    private void Test5()
    {
        List<(string input, string expected)> tests = 
        [
            ("-a * b;", "((-a) * b)"),
            ("!-a;", "(!(-a))"),
            ("a + b + c;", "((a + b) + c)"),
            ("a + b - c;", "((a + b) - c)"),
            ("a * b * c;", "((a * b) * c)"),
            ("a * b * c;", "((a * b) * c)"),
            ("a + b / c;", "(a + (b / c))"),
            ("a + b * c + d / e - f;", "(((a + (b * c)) + (d / e)) - f)"),
            ("3 + 4; -5 * 5;", "(3 + 4)((-5) * 5)"),
            ("5 > 4 == 3 < 4;", "((5 > 4) == (3 < 4))"),
            ("5 < 4 != 3 > 4;", "((5 < 4) != (3 > 4))"),
            ("3 + 4 * 5 == 3 * 1 + 4 * 5;", "((3 + (4 * 5)) == ((3 * 1) + (4 * 5)))"),
            ("3 + 4 * 5 == 3 * 1 + 4 * 5;", "((3 + (4 * 5)) == ((3 * 1) + (4 * 5)))")
        ];

        foreach (var test in tests)
        {
            _lexer = new Lexer(test.input);
            _parser = new Parser(_lexer.LexedTokens);

            ProgramNode programNode = _parser.ParseProgram();
            
            Assert.Empty(_parser.Errors);
            
            Assert.Equal(test.expected, programNode.ToString());
        }
    }

    private static bool TestDeclaration(INode node, string type, string name)
    {
        if (node.TokenLiteral() != type || node.GetType() != typeof(DeclarationStatement))
        {
            return false;
        }

        DeclarationStatement statement = (DeclarationStatement)node;

        return statement.Name.Value == name && statement.Name.TokenLiteral() == name;
    }
}
