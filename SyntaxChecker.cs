namespace Compiler;

public class SyntaxChecker
{
    private readonly ProgramNode _programNode;
    public List<string> Errors { get; } = [];
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
    }

    private void CheckDeclarationStatement(DeclarationStatement statement)
    {
        if (!_identifiers.TryAdd(statement.Name.ToString(), statement.TokenLiteral()))
        {
            Errors.Add($"Variable {statement.Name} is already defined");
        }

        string type = CheckExpression(statement.Value);
        if (type != statement.TokenLiteral())
        {
            Errors.Add($"Cannot assign {type} to {statement.TokenLiteral()}");
        }
    }

    private string CheckExpression(IExpression expression)
    {
        if (expression.GetType() == typeof(IntegerLiteral))
        {
            return "int";
        }

        if (expression.GetType() == typeof(BoolLiteral))
        {
            return "bool";
        }

        if (expression.GetType() == typeof(Identifier))
        {
            if (_identifiers.TryGetValue(expression.TokenLiteral(), out string? tmp))
            {
                return tmp;
            }

            Errors.Add($"Cannot resolve symbol \'{expression.TokenLiteral()}\'");
            return "unknown";
        }

        if (expression.GetType() == typeof(PrefixExpression))
        {
            PrefixExpression prefixExpression = (PrefixExpression) expression;
            Console.WriteLine(prefixExpression);
            switch (prefixExpression.Operator)
            {
                case "!":
                {
                    string typeRight = CheckExpression(prefixExpression.Right);
                    if (typeRight != "bool" && typeRight != "unknown")
                    {
                        AddOperatorError("!", typeRight);
                    }
                    return "bool";
                }
                case "-":
                {
                    string typeRight = CheckExpression(prefixExpression.Right);
                    if (typeRight != "int" && typeRight != "unknown")
                    {
                        AddOperatorError("-", typeRight);
                    }
                    return "bool";
                }
            }
        }

        return null!;
    }

    private void AddOperatorError(string @operator, string type)
    {
        Errors.Add($"Cannot apply operator '{@operator}' to operand of type '{type}'");
    }
}
