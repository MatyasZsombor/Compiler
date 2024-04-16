using Compiler;

namespace UnitTests;

public class ParserTests
{
    private Lexer _lexer = null!;
    private Parser _parser = null!;
    
    [Fact]
    private void Test1()
    {
        List<(string input, string expected)> tests = 
        [
            ("int x = 5;", "int x = 5;"),
            ("bool y = true;", "bool y = true;"),
            ("int foobar = y;", "int foobar = y;")
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

    [Fact]
    private void Test2()
    {
        List<(string input, string expected)> tests = 
        [
            ("int x = -a * b;", "((-a) * b)"),
            ("int x = !-a;", "(!(-a))"),
            ("int x = a + b + c;", "((a + b) + c)"),
            ("int x = a + b - c;", "((a + b) - c)"),
            ("int x = a * b * c;", "((a * b) * c)"),
            ("int x = a * b * c;", "((a * b) * c)"),
            ("int x = a + b / c;", "(a + (b / c))"),
            ("int x = a + b * c + d / e - f;", "(((a + (b * c)) + (d / e)) - f)"),
            ("int x = (3 + 4) -5 * 5;", "((3 + 4) - (5 * 5))"),
            ("int x = 5 > 4 == 3 < 4;", "((5 > 4) == (3 < 4))"),
            ("int x = 5 < 4 != 3 > 4;", "((5 < 4) != (3 > 4))"),
            ("int x = 3 + 4 * 5 == 3 * 1 + 4 * 5;", "((3 + (4 * 5)) == ((3 * 1) + (4 * 5)))"),
            ("int x = 3 + 4 * 5 == 3 * 1 + 4 * 5;", "((3 + (4 * 5)) == ((3 * 1) + (4 * 5)))")
        ];

        foreach (var test in tests)
        {
            _lexer = new Lexer(test.input);
            _parser = new Parser(_lexer.LexedTokens);

            ProgramNode programNode = _parser.ParseProgram();
            
            Assert.Empty(_parser.Errors);
            
            Assert.Equal("int x = " + test.expected + ";", programNode.ToString());
        }
    }

    [Fact]
    private void Test3()
    {
        List<(string input, string expected)> tests = 
        [
            ("bool x = true;", "true"),
            ("bool x = false;", "false"),
            ("bool x = 3 > 5 == false;", "((3 > 5) == false)"),
            ("bool x = 3 < 5 == true;", "((3 < 5) == true)"),
            ("bool x = true == true;", "(true == true)"),
            ("bool x = !true;", "(!true)")
        ];

        foreach (var test in tests)
        {
            _lexer = new Lexer(test.input);
            _parser = new Parser(_lexer.LexedTokens);

            ProgramNode programNode = _parser.ParseProgram();
            
            Assert.Empty(_parser.Errors);
            
            Assert.Equal("bool x = " + test.expected + ";", programNode.ToString());
        }
    }
    
    [Fact]
    private void Test4()
    {
        List<(string input, string expected)> tests = 
        [
            ("int x = 1 + (2 + 3) + 4;", "((1 + (2 + 3)) + 4)"),
            ("int x = (5 + 5) * 2;", "((5 + 5) * 2)"),
            ("int x = 2 / (5 + 5);", "(2 / (5 + 5))"),
            ("int x = -(5 + 5);", "(-(5 + 5))"),
            ("int x = !(true == true);", "(!(true == true))"),
        ];

        foreach (var test in tests)
        {
            _lexer = new Lexer(test.input);
            _parser = new Parser(_lexer.LexedTokens);

            ProgramNode programNode = _parser.ParseProgram();
            
            Assert.Empty(_parser.Errors);
            
            Assert.Equal("int x = " + test.expected + ";", programNode.ToString());
        }
    }
    
    [Fact]
    private void Test5()
    {
        List<(string input, string expected)> tests = 
        [
            ("return 5;", "return 5;"),
            ("return false == true;", "return (false == true);")
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
    
    [Fact]
    private void Test6()
    {
        List<(string input, string expected)> tests = 
        [
            ("x++;", "x++;"),
            ("x--;", "x--;")
        ];

        foreach (var test in tests)
        {
            _lexer = new Lexer(test.input);
            _parser = new Parser(_lexer.LexedTokens);

            ProgramNode programNode = _parser.ParseProgram();
            
            Assert.Empty(_parser.Errors);
            Assert.Single(programNode.Statements);
            
            Assert.Equal(test.expected, programNode.ToString());
        }
    }

    [Fact]
    private void Test7()
    {
        List<(string input, string expected)> tests = 
        [
            ("if(x == 5){}", "if(x == 5){}"),
            ("if (x < y) { x++; }", "if(x < y){x++;}"),
            ("if(x < y) { x++; } else { y++; }", "if(x < y){x++;} else {y++;}")
        ];

        foreach (var test in tests)
        {
            _lexer = new Lexer(test.input);
            _parser = new Parser(_lexer.LexedTokens);

            ProgramNode programNode = _parser.ParseProgram();
            
            Assert.Empty(_parser.Errors);
            Assert.Single(programNode.Statements);
            
            Assert.Equal(test.expected, programNode.ToString());
        }
    }
}
