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
