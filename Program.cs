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
        List<string> tmp = lines.ToList();
        tmp.Add("\0");
        lines = tmp.ToArray();
        
        Lexer lexer = new Lexer(string.Join("\n", lines));
        Parser parser = new Parser(lexer.LexedTokens);
        ProgramNode node = parser.ParseProgram();
        Console.WriteLine("Finished");
    }
}