using Compiler;
namespace UnitTests;

public class SyntaxTests
{
    private Lexer _lexer = null!;
    private Parser _parser = null!;
    private SyntaxChecker _syntaxChecker = null!;
    
    [Fact]
    private void Test1()
    {
        List<(string input, string expected)> tests = 
        [
            ("1 + (2 + 3) + 4;", "((1 + (2 + 3)) + 4)"),
            ("(5 + 5) * 2;", "((5 + 5) * 2)"),
            ("2 / (5 + 5);", "(2 / (5 + 5))"),
            ("-(5 + 5);", "(-(5 + 5))"),
            ("!(true == true);", "(!(true == true))"),
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
}
