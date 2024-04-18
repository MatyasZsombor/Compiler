namespace Compiler;

public class Compiler
{
    public List<string> Errors { get; } = [];
    public List<(string instruction, string operand)> Instructions { get; } = [];
    private uint _curOffset;
    private uint _localOffset;
    public int ProgramStart;

    private readonly Dictionary<string, (string type, string offset)> _identifiers = [];
    private readonly Dictionary<string, string> _functions = [];

    public Compiler(INode node)
    {
        CompileFunctions((ProgramNode)node);
        Compile(node);
        
        Instructions.Add(("LDA", "0,4"));
        Instructions.Add(("BRK", ""));
    }

    private void CompileFunctions(ProgramNode node)
    {
        foreach (IStatement statement in node.Statements)
        {
            if (statement.GetType() != typeof(FunctionLiteral))
            {
                continue;
            }

            FunctionLiteral functionLiteral = (FunctionLiteral) statement;
            _functions.Add(functionLiteral.Name.ToString(), Instructions.Count.ToString());
            
            Dictionary<string, (string type, string offset)> locals = [];
            if (functionLiteral.Parameters != null)
            {
                List<(string, string)> tmp = [];
                foreach (Parameter parameter in functionLiteral.Parameters)
                {
                    locals.Add(parameter.Name.Literal,
                               (parameter.TokenLiteral(), ""));
                    tmp.Add(("STA", $"{_localOffset},{CalculateWidth(parameter.TokenLiteral())}"));
                    _localOffset += parameter.TokenLiteral() == "int" ? (uint) 4 : 1;
                }

                tmp.Reverse();
                Instructions.AddRange(tmp);
            }

            List<int> jumps = [];
            if (functionLiteral.Body != null)
            {
                foreach (IStatement stat in functionLiteral.Body.Statements)
                {
                    Compile(stat, locals, jumps);
                }
            }
        }
        ProgramStart = Instructions.Count;
    }

    private void Compile(INode node, Dictionary<string, (string type, string offset)>? locals = null, List<int>? jumpsReturns = null)
    {
        switch (node)
        {
            case ProgramNode programNode:
                foreach (IStatement statement in programNode.Statements)
                {
                    Compile(statement);
                }
                break;
            
            case CallExpression callExpression:
                Instructions.Add(("PUSH", ""));
                int retJump = Instructions.Count - 1;
                if (callExpression.Arguments != null)
                {
                    foreach (IExpression expression in callExpression.Arguments)
                    {
                        Compile(expression, locals);
                    }   
                }
                Instructions[retJump] = ("PUSH", Instructions.Count.ToString());
                Instructions.Add(("JMP", _functions[callExpression.FuncName.ToString()]));
                break;
                
            case InfixExpression infixExpression:
                Compile(infixExpression.Left, locals);
                Compile(infixExpression.Right, locals);
                switch (infixExpression.Operator)
                {
                    case "+":
                        Instructions.Add(("ADD", ""));

                        break;
                    case "-":
                        Instructions.Add(("NEG", ""));
                        Instructions.Add(("ADD", ""));

                        break;
                    case "*":
                        Instructions.Add(("MULT", ""));

                        break;
                    case "/":
                        Instructions.Add(("DIV", ""));
                        break;
                    
                    case "==":
                        Instructions.Add(("CMP", ""));
                        break;
                    
                    case "!=":
                        Instructions.Add(("CMP", ""));
                        Instructions.Add(("NOT", ""));
                        break;
                    case "<":
                        Instructions.Add(("LCMP", ""));
                        break;
                    case ">":
                        Instructions.Add(("GCMP", ""));
                        break;
                }

                break;

            case PrefixExpression prefixExpression:
                Compile(prefixExpression.Right, locals);
                switch (prefixExpression.Operator)
                {
                    case "-":
                        Instructions.Add(("NEG", ""));

                        break;
                    case "!":
                        Instructions.Add(("NOT", ""));

                        break;
                }

                break;

            case PostFixStatement postFixStatement:
                Compile(postFixStatement.Name, locals);
                Instructions.Add(("PUSH", "1"));

                string type;
                string offset;
                if (_identifiers.ContainsKey(postFixStatement.Name.ToString()))
                {
                    (type, offset) = _identifiers[postFixStatement.Name.ToString()];
                }
                else
                {
                    (type, offset) = locals![postFixStatement.Name.ToString()];
                }

                if (postFixStatement.Operator == "++")
                {
                    Instructions.Add(("ADD", ""));
                    Instructions.Add(("STA", $"{offset},{CalculateWidth(type)}"));

                    break;
                }

                Instructions.Add(("NEG", ""));
                Instructions.Add(("ADD", ""));
                Instructions.Add(("STA", $"{offset},{CalculateWidth(type)}"));

                break;

            case DeclarationStatement declarationStatement:
                if (locals == null)
                {
                    _identifiers.Add(declarationStatement.Name.ToString(),
                                     (declarationStatement.TokenLiteral(), _curOffset.ToString()));
                }
                else
                {
                    locals.Add(declarationStatement.Name.ToString(),
                               (declarationStatement.TokenLiteral(), "ebs," + _localOffset));
                }

                Compile(declarationStatement.Value, locals);

                Instructions.Add(locals == null
                                     ? ("STA", $"{_curOffset},{CalculateWidth(declarationStatement.TokenLiteral())}")
                                     : ("STA", $"{_localOffset},{CalculateWidth(declarationStatement.TokenLiteral())}"));

                if (locals == null)
                {
                    _curOffset += declarationStatement.TokenLiteral() == "int" ? (uint) 4 : 1;
                }
                else
                {
                    _localOffset += declarationStatement.TokenLiteral() == "int" ? (uint) 4 : 1;
                }

                break;

            case IfStatement ifStatement:
                Dictionary<string, (string type, string offset)> localsWithScope = [];
                if (locals != null)
                {
                    localsWithScope = new Dictionary<string, (string, string)>(locals);
                }

                Compile(ifStatement.Condition!, localsWithScope);
                int jump1 = Instructions.Count;
                Instructions.Add(("BRZ", ""));

                if (ifStatement.Consequence != null)
                {
                    foreach (IStatement statement in ifStatement.Consequence.Statements)
                    {
                        Compile(statement, localsWithScope, jumpsReturns);
                    }

                    if (locals == null)
                    {
                        _localOffset = 0xFFF;
                    }

                    Instructions[jump1] = ("BRZ", (Instructions.Count - 1).ToString());
                }

                if (ifStatement.Alternative != null)
                {
                    int jump2 = Instructions.Count;
                    Instructions.Add(("JMP", ""));
                    Instructions[jump1] = ("BRZ", (Instructions.Count - 1).ToString());

                    localsWithScope = [];
                    if (locals != null)
                    {
                        localsWithScope = new Dictionary<string, (string type, string offset)>(locals);
                    }

                    foreach (IStatement statement in ifStatement.Alternative.Statements)
                    {
                        Compile(statement, localsWithScope, jumpsReturns);
                    }

                    if (locals == null)
                    {
                        _localOffset = 0xFFF;
                    }

                    Instructions[jump2] = ("JMP", (Instructions.Count - 1).ToString());
                }
                break;
            
            case WhileStatement whileStatement: 
                localsWithScope = [];
                if (locals != null)
                {
                    localsWithScope = new Dictionary<string, (string, string)>(locals);
                }

                jump1 = Instructions.Count;
                Compile(whileStatement.Condition!, localsWithScope);
                int branch = Instructions.Count;
                Instructions.Add(("BRZ", ""));

                if (whileStatement.Consequence != null)
                {
                    List<int> jumps = [];
                    foreach (IStatement statement in whileStatement.Consequence.Statements)
                    {
                        Compile(statement, localsWithScope, jumps);
                    }

                    if (locals == null)
                    {
                        _localOffset = 0xFFF;
                    }

                    Instructions[branch] = ("BRZ", Instructions.Count.ToString());
                    foreach (int jump in jumps)
                    {
                        Instructions[jump] = ("JMP", (Instructions.Count + 1).ToString());
                    }
                    Instructions.Add(("JMP", jump1.ToString()));
                }
                break;

            case AssigmentStatement assigmentStatement:
                Compile(assigmentStatement.Value, locals);
                if (_identifiers.ContainsKey(assigmentStatement.Name.ToString()))
                {
                    (type, offset) = _identifiers[assigmentStatement.Name.ToString()];
                }
                else
                {
                    (type, offset) = locals![assigmentStatement.Name.ToString()];
                }

                Instructions.Add(("STA", $"{offset},{CalculateWidth(type)}"));
                break;

            case IntegerLiteral integerLiteral:
                Instructions.Add(("PUSH", integerLiteral.Value.ToString()));
                break;

            case BoolLiteral boolLiteral:
                Instructions.Add(("PUSH", boolLiteral.Value ? "1" : "0"));
                break;
            
            case BreakStatement:
                if (jumpsReturns == null)
                {
                    Errors.Add("Unresolved jump");
                    break;
                }
                Instructions.Add(("JMP", ""));
                jumpsReturns.Add(Instructions.Count);
                break;
            
            case ReturnStatement returnStatement:
                if (jumpsReturns == null)
                {
                    Errors.Add("Unresolved jump");
                    break;
                }
                Console.WriteLine(returnStatement);
                Compile(returnStatement.ReturnValue, locals, jumpsReturns);
                Instructions.Add(("RTSV", ""));
                break;

            case Identifier identifier:
                if (_identifiers.ContainsKey(identifier.ToString()))
                {
                    (type, offset) = _identifiers[identifier.ToString()];
                }
                else
                {
                    (type, offset) = locals![identifier.ToString()];
                }

                Instructions.Add(("LDA", $"{offset},{CalculateWidth(type)}"));

                break;
        }
    }

    private static int CalculateWidth(string type) =>
        type switch
        {
            "int" => 4,
            _     => 1
        };
}
