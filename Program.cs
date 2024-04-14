namespace BetterInterpreter;

internal static class Program
{
    private static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: ./compiler file.fe");
            return;
        }

        if (!File.Exists(args[0]))
        {
          Console.WriteLine("The file doesn't exists.");   
        }
        
        string[] lines = File.ReadAllLines(args[0]);
        
        Lexer lexer = new Lexer(string.Join("\n", lines));
        Parser parser = new Parser(lexer.LexedTokens);
        ProgramNode node = parser.ParseProgram();

        if (parser.Errors.Count != 0)
        {
            foreach (string error in parser.Errors)
            {
                Console.WriteLine(error);
            }
            return;
        }

        SyntaxChecker syntaxChecker = new SyntaxChecker(node);

        if (syntaxChecker.Errors.Count != 0)
        {
            foreach (string error in syntaxChecker.Errors)
            {
                Console.WriteLine(error);
            }
            return;
        }
        
        Console.WriteLine("Finished");
    }
}