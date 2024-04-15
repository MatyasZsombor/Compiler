namespace Compiler;

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

        SyntaxChecker syntaxChecker = new SyntaxChecker(node.Statements);

        if (syntaxChecker.Errors.Count != 0)
        {
            foreach (string error in syntaxChecker.Errors)
            {
                Console.WriteLine(error);
            }
            return;
        }

        Compiler compiler = new Compiler(node);

        foreach ((string, string ) instruction in compiler.Instructions)
        {
            Console.WriteLine(instruction.ToString());
        }

        compiler.Instructions.Add(("LDA", "y"));
        Vm vm = new Vm(compiler.Instructions, compiler.Identifiers);
        
        Console.WriteLine(vm.Top());
        Console.WriteLine("Finished");
    }
}