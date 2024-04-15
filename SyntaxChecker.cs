namespace Compiler;

public class SyntaxChecker
{
    public List<string> Errors { get; } = [];
    private readonly Dictionary<string, string> _identifiers = [];

    private readonly Dictionary<string, string[]> _expectedTypes = new()
    {
        {"+", ["int"]},
        {"-", ["int"]},
        {"/", ["int"]},
        {"*", ["int"]},
        {"<", ["int"]},
        {">", ["int"]},
        {"==", ["bool","int"]},
        {"!=", ["bool","int"]},
        {"!", ["bool"]},
        {"++", ["int"]},
        {"--", ["int"]}
    };

    public SyntaxChecker(List<IStatement> statements)
    {
        foreach (IStatement statement in statements)
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

        if (statement.GetType() == typeof(PostFixStatement))
        {
            PostFixStatement postFixStatement = (PostFixStatement) statement;
            if (_identifiers.TryGetValue(postFixStatement.TokenLiteral(), out string? tmp))
            {
                CheckPostfixType(postFixStatement.Operator, tmp);
                return;
            }

            Errors.Add($"Cannot resolve symbol \'{postFixStatement.TokenLiteral()}\'");
        }

        if (statement.GetType() == typeof(AssigmentStatement))
        {
            CheckAssigmentStatement((AssigmentStatement)statement);
        }
    }

    private void CheckDeclarationStatement(DeclarationStatement statement)
    {
        if (!_identifiers.TryAdd(statement.Name.ToString(), statement.TokenLiteral()))
        {
            Errors.Add($"Variable {statement.Name} is already declared");
        }
        
        string type = CheckExpression(statement.Value);
        if (type != statement.TokenLiteral())
        {
            Errors.Add($"Cannot assign {type} to {statement.TokenLiteral()}");
        }
        
    }

    private void CheckAssigmentStatement(AssigmentStatement statement)
    {
        if (!_identifiers.TryGetValue(statement.TokenLiteral(), out string? variableType))
        {
            Errors.Add($"Cannot resolve symbol '{statement.TokenLiteral()}'");
        }

        variableType ??= "unresolved";
        string type = CheckExpression(statement.Value);
        if (type != variableType && variableType != "unresolved")
        {
            Errors.Add($"Cannot assign {type} to {variableType}");
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
            return "unresolved";
        }

        if (expression.GetType() == typeof(PrefixExpression))
        {
            PrefixExpression prefixExpression = (PrefixExpression) expression;
            return CheckPrefixType(prefixExpression.Operator, CheckExpression(prefixExpression.Right));
        }
        
        InfixExpression infixExpression = (InfixExpression) expression;
        string typeL = CheckExpression(infixExpression.Left);
        string typeR = CheckExpression(infixExpression.Right);
        
        return CheckInfixType(infixExpression.Operator, typeL, typeR);
    }

    private void AddOperatorError(string @operator, string type)
    {
        Errors.Add($"Cannot apply operator '{@operator}' to operand of type '{type}'");
    }
    
    private void AddOperatorErrorInfix(string @operator, string type1, string type2)
    {
        Errors.Add($"Cannot apply operator '{@operator}' to operands of type '{type1}' and '{type2}'");
    }

    private string CheckInfixType(string @operator, string typeL, string typeR)
    {
        string[] expected = _expectedTypes[@operator];
        if (typeR != "unresolved" && typeL != "unresolved" && (!expected.Contains(typeL) || !expected.Contains(typeR)))
        {
            AddOperatorErrorInfix(@operator, typeL, typeR);
        }
        return expected[0];
    }

    private void CheckPostfixType(string @operator, string type)
    {
        if (type != _expectedTypes[@operator][0])
        {
            AddOperatorError(@operator, type);
        }
    }

    private string CheckPrefixType(string @operator, string type)
    {
        string expected = _expectedTypes[@operator][0];
        if (type != expected && type != "unresolved")
        {
            AddOperatorError(@operator, type);
        }
        return type != "unresolved" ? type : "int";
    }
}
