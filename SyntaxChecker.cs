namespace BetterInterpreter;

public class SyntaxChecker
{
    private readonly ProgramNode _programNode;
    public List<string> Errors { get; }= [];
    private readonly Dictionary<string, string> _identifiers = [];

    public SyntaxChecker(ProgramNode programNode)
    {
        _programNode = programNode;

        foreach (IStatement statement in programNode.Statements)
        {
            CheckSyntax(statement);
        }
    }
    
    private void CheckSyntax(IStatement statement)
    {
        if (statement.GetType() == typeof(DeclarationStatement))
        {
            CheckDeclarationStatement((DeclarationStatement) statement);
        }

        if (statement.GetType() == typeof(ExpressionStatement))
        {
            CheckExpressionStatement((ExpressionStatement)statement);
        }
    }

    private void CheckDeclarationStatement(DeclarationStatement statement)
    {
        if (!_identifiers.TryAdd(statement.Name.ToString(), statement.TokenLiteral()))
        {
            Errors.Add($"Variable {statement.Name} is already defined");
        }
    }

    private void CheckExpressionStatement(ExpressionStatement statement)
    {
        Console.WriteLine(statement.TokenLiteral());
    }
}
