namespace Compiler;
/// TODO IMPLEMENT CHECKING FOR BREAK AND RETURN; THEY CAN ONLY OCCUR IN A LOOP OR IN A METHOD
/// TODO IMPLEMENT CHECKING FOR FUNCTIONS IN FUNCTIONS

public class SyntaxChecker
{
    public List<string> Errors { get; } = [];
    private readonly Dictionary<string, string> _identifiers = [];
    private readonly Dictionary<string, (string returnType, string parameterTypes)> _functions = [];

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

    private readonly Dictionary<string, string[]> _resultTypes = new()
    {
        {"+", ["int"]},
        {"-", ["int"]},
        {"/", ["int"]},
        {"*", ["int"]},
        {"<", ["bool"]},
        {">", ["bool"]},
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
            CheckSyntax(statement, null);
        }

        if (!_functions.ContainsKey("main"))
        {
            Errors.Add("Couldn't find suitable entry point. ");
        }
    }
    
    private void CheckSyntax(IStatement statement, Dictionary<string, string>? bindings)
    {
        switch (statement)
        {
            case FunctionLiteral functionLiteral:
                CheckFunctionLiteral(functionLiteral);
                break;
            
            case DeclarationStatement declarationStatement:
                CheckDeclarationStatement(declarationStatement, bindings);
                break;
            
            case PostFixStatement postFixStatement:
                if (_identifiers.TryGetValue(postFixStatement.TokenLiteral(), out string? tmp) ||
                    (bindings != null && bindings.TryGetValue(postFixStatement.TokenLiteral(), out tmp)))
                {
                    CheckPostfixType(postFixStatement.Operator, tmp);
                    return;
                }

                Errors.Add($"Cannot resolve symbol '{postFixStatement.TokenLiteral()}'");
                break;
            
            case AssigmentStatement assigmentStatement:
                CheckAssigmentStatement(assigmentStatement, bindings);
                break;
                
            case IfStatement ifStatement:
                CheckIfStatement(ifStatement, bindings);
                break;
            case WhileStatement whileStatement:
                CheckWhileStatement(whileStatement, bindings);
                break;
        }
    }

    private void CheckFunctionLiteral(FunctionLiteral functionLiteral)
    {
        Dictionary<string, string> local = [];
        string[] parameterTypes = [];
        if (functionLiteral.Parameters != null)
        {
            parameterTypes = new string[functionLiteral.Parameters.Count];

            for (int i = 0; i < functionLiteral.Parameters.Count; i++)
            {
                parameterTypes[i] = functionLiteral.Parameters[i].TokenLiteral();
                if (_identifiers.ContainsKey(functionLiteral.Parameters[i].Name.Literal))
                {
                    Errors.Add($"Variable {functionLiteral.Parameters[i].Name.Literal} is already declared");
                    continue;
                }
                local.Add(functionLiteral.Parameters[i].Name.Literal, functionLiteral.Parameters[i].TokenLiteral());
            }
        }
        
        if(!_functions.TryAdd(functionLiteral.Name.ToString(), (functionLiteral.TokenLiteral(), string.Join(",", parameterTypes))))
        {
            Errors.Add("Function with the same signature is already declared");
        }

        if (functionLiteral.Body != null)
        {
            foreach (IStatement statement in functionLiteral.Body.Statements)
            {
                if (statement.GetType() != typeof(ReturnStatement))
                {
                    CheckSyntax(statement, local);
                    continue;
                }

                ReturnStatement returnStatement = (ReturnStatement) statement;
                string type = CheckExpression(returnStatement.ReturnValue, local);
                if (type != functionLiteral.TokenLiteral())
                {
                    Errors.Add($"Cannot convert expression type '{type} to return type {functionLiteral.TokenLiteral()}'");
                }
            }
        }
    }
    
    private void CheckIfStatement(IfStatement statement, Dictionary<string,string>? localBindings)
    {
        Dictionary<string, string> local = [];
        if (localBindings != null)
        {
            local = new Dictionary<string, string>(localBindings);
        }
        string ifType = CheckExpression(statement.Condition!, local);

        if (ifType != "bool" && ifType != "unresolved")
        {
            Errors.Add($"Cannot convert type '{ifType}' to 'bool'");
        }

        if (statement.Consequence != null)
        {
            foreach (IStatement blockStatement in statement.Consequence.Statements)
            {
                CheckSyntax(blockStatement, local);
            }
        }

        if (statement.Alternative == null)
        {
            return;
        }
        {
            Dictionary<string, string> localElse = [];
            if (localBindings != null)
            {
                localElse = new Dictionary<string, string>(localBindings);
            }
            foreach (IStatement blockStatement in statement.Alternative.Statements)
            {
                CheckSyntax(blockStatement, localElse);
            }
        }
    }
    
    private void CheckWhileStatement(WhileStatement statement, Dictionary<string,string>? localBindings)
    {
        Dictionary<string, string> local = [];
        if (localBindings != null)
        {
            local = new Dictionary<string, string>(localBindings);
        }
        string ifType = CheckExpression(statement.Condition!, local);

        if (ifType != "bool" && ifType != "unresolved")
        {
            Errors.Add($"Cannot convert type '{ifType}' to 'bool'");
        }

        if (statement.Consequence == null)
        {
            return;
        }

        foreach (IStatement blockStatement in statement.Consequence.Statements)
        {
            CheckSyntax(blockStatement, local);
        }
    }

    private void CheckDeclarationStatement(DeclarationStatement statement,  Dictionary<string,string>? localBindings)
    {
        if (localBindings == null && !_identifiers.TryAdd(statement.Name.ToString(), statement.TokenLiteral()))
        {
            Errors.Add($"Variable {statement.Name} is already declared");
        }
        
        if (localBindings != null && (_identifiers.ContainsKey(statement.Name.ToString()) || !localBindings.TryAdd(statement.Name.ToString(), statement.TokenLiteral())))
        {
            Errors.Add($"Variable {statement.Name} is already declared");
        }
        
        string type = CheckExpression(statement.Value, localBindings);
        if (type != statement.TokenLiteral() && type != "unresolved")
        {
            Errors.Add($"Cannot assign {type} to {statement.TokenLiteral()}");
        }
        
    }

    private void CheckAssigmentStatement(AssigmentStatement statement, Dictionary<string,string>? localBindings)
    {
        if (!_identifiers.TryGetValue(statement.TokenLiteral(), out string? variableType))
        {
            Errors.Add($"Cannot resolve symbol '{statement.TokenLiteral()}'");
        }

        variableType ??= "unresolved";
        string type = CheckExpression(statement.Value, localBindings);
        if (type != variableType && variableType != "unresolved" && type != "unresolved")
        {
            Errors.Add($"Cannot assign {type} to {variableType}");
        }
    }

    private string CheckExpression(IExpression expression,  Dictionary<string,string>? localBindings)
    {
        if (expression.GetType() == typeof(IntegerLiteral))
        {
            return "int";
        }

        if (expression.GetType() == typeof(BoolLiteral))
        {
            return "bool";
        }

        if (expression.GetType() == typeof(CallExpression))
        {
            CallExpression callExpression = (CallExpression)expression;
            if (!_functions.ContainsKey(callExpression.FuncName.TokenLiteral()))
            {
                Errors.Add($"Cannot resolve symbol {callExpression.FuncName.TokenLiteral()}");
                return "unresolved";
            }
            (string type, string returnTypes) = _functions[callExpression.FuncName.TokenLiteral()];
            string[] types = returnTypes.Split(",");

            if (callExpression.Arguments == null && types is [""])
            {
                return type;
            }
            if(callExpression.Arguments == null)
            {
                Errors.Add($"Function '{callExpression.FuncName}' has {types.Length} parameter(s) but is invoked with 0 argument(s)");
                return type;
            }
            if (types.Length != callExpression.Arguments!.Count)
            {
                Errors.Add($"Function '{callExpression.FuncName}' has {types.Length} parameter(s) but is invoked with {callExpression.Arguments.Count} argument(s)");
            }

            for (int i = 0; i < types.Length; i++)
            {
                string argType = CheckExpression(callExpression.Arguments[i], localBindings);
                if (types[i] != argType && argType != "unresolved")
                {
                    Errors.Add($"Cannot convert type '{argType}' to '{type}'");
                }
            }
            return type;
        }

        if (expression.GetType() == typeof(Identifier))
        {
            if (_identifiers.TryGetValue(expression.TokenLiteral(), out string? tmp) || (localBindings != null && localBindings.TryGetValue(expression.TokenLiteral(), out tmp)))
            {
                return tmp;
            }

            Errors.Add($"Cannot resolve symbol \'{expression.TokenLiteral()}\'");
            return "unresolved";
        }

        if (expression.GetType() == typeof(PrefixExpression))
        {
            PrefixExpression prefixExpression = (PrefixExpression) expression;
            return CheckPrefixType(prefixExpression.Operator, CheckExpression(prefixExpression.Right, localBindings));
        }
        
        InfixExpression infixExpression = (InfixExpression) expression;
        string typeL = CheckExpression(infixExpression.Left, localBindings);
        string typeR = CheckExpression(infixExpression.Right, localBindings);
        
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
        return _resultTypes[@operator][0];
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
