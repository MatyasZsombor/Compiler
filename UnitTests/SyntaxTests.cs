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
            ("int x = 5;", ""),
            ("int x = false;", "Cannot assign bool to int"),
            ("int x = 5; int x = 6;", "Variable x is already declared"),
            ("int x = -y;", "Cannot resolve symbol 'y'"),
            ("int x = !false;", "Cannot assign bool to int"),
            ("bool x = -false;", $"Cannot apply operator '-' to operand of type 'bool'"),
        ];

        foreach (var test in tests)
        {
            _lexer = new Lexer(test.input);
            _parser = new Parser(_lexer.LexedTokens);

            ProgramNode programNode = _parser.ParseProgram();
            _syntaxChecker = new SyntaxChecker(programNode.Statements);
            
            Assert.Empty(_parser.Errors);
            if (test.expected != "")
            {
                Assert.Single(_syntaxChecker.Errors);
                Assert.Equal(test.expected, _syntaxChecker.Errors[0]);
            }
            else
            {
                Assert.Empty(_syntaxChecker.Errors);
            }
        }
    }
    
    //TODO IMPLEMENT TESTING FOF INFIX OPERATORS
}
