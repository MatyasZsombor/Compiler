using System.Reflection.Metadata;

namespace Compiler;

public class Compiler
{
    public List<string> Errors { get; } = [];
    public List<(string instruction, string operand)> Instructions { get; } = [];
    private int _globalOffset = 0x100;
    public int FunctionOffset;
    private int _localOffset;

    private readonly Dictionary<string, string> _globals = [];
    public readonly Dictionary<string, int> Functions = [];

    public Compiler(INode node)
    {
        CompileFunctions((ProgramNode) node);
        Instructions.Add(("call", "main"));
    }

    private void CompileFunctions(ProgramNode programNode)
    {
        foreach (IStatement node in programNode.Statements.Where(node => node.GetType() == typeof(FunctionLiteral)))
        {
            _localOffset = 0;
            FunctionLiteral function = (FunctionLiteral) node;
            Functions.Add(function.Name.Value, FunctionOffset);
            Dictionary<string, string> locals = [];
            
            if (function.Parameters != null)
            {
                int parameterOffset = 3;
                function.Parameters.Reverse();
                foreach (Parameter parameter in function.Parameters)
                {
                    locals.Add(parameter.Name.Literal, $"ebp+{parameterOffset}");
                    parameterOffset++;
                }
            }
            
            if (function.Body == null)
            {
                continue;
            }

            Compile(function.Body, locals);
            FunctionOffset = Instructions.Count;
        }
    }

    private void Compile(INode node, Dictionary<string, string>? locals = null)
    {
        switch (node)
        {
            case BlockStatement blockStatement:
                foreach (IStatement statement in blockStatement.Statements)
                {
                    Compile(statement, locals);
                }
                break;
            
            case DeclarationStatement declarationStatement:
                Compile(declarationStatement.Value, locals);
                if (locals == null)
                {
                    _globals.Add(declarationStatement.Name.ToString(), _globalOffset.ToString());
                    Instructions.Add(("sta", _globals[declarationStatement.Name.ToString()]));
                    _globalOffset += 1;
                    break;
                }

                locals.Add(declarationStatement.Name.ToString(), $"ebp-{_localOffset}");
                Instructions.Add(("push", "eax"));
                _localOffset += 1;
                break;
            
            case AssigmentStatement assigmentStatement:
                Console.WriteLine(assigmentStatement);
                break;
            
            case IntegerLiteral integerLiteral:
                Instructions.Add(("mov", integerLiteral.Value + ",eax"));
                break;
            
            case InfixExpression infixExpression:
                Compile(infixExpression.Left, locals);
                Instructions.Add(("push", "eax"));
                Compile(infixExpression.Right, locals);
                Instructions.Add(("pop", "ecx"));
                Instructions.Add((LookUpOperator(infixExpression.Operator), "ecx,eax"));
                break;
            
            case Identifier identifier:
                if (locals != null && locals.ContainsKey(identifier.Value))
                {
                    Instructions.Add(("lda", locals[identifier.TokenLiteral()]));
                    break;   
                }
                Instructions.Add(("lda", _globals[identifier.TokenLiteral()]));
                break;
            
            case ReturnStatement returnStatement:
                Compile(returnStatement.ReturnValue, locals);
                Instructions.Add(("ret", ""));
                break;
            
            case IfStatement ifStatement:
                Compile(ifStatement.Condition!, locals);
                Instructions.Add(("jne", ""));

                int beforeCount = Instructions.Count - 1;
                int scopeBefore = locals?.Count ?? 0;
                if (ifStatement.Consequence != null)
                {
                    Compile(ifStatement.Consequence, locals);
                }
                
                Instructions[beforeCount] = ("jne", (Instructions.Count - 1).ToString());
                for (int i = 0; i < ((locals?.Count ?? 0) - scopeBefore); i++)
                {
                    Instructions.Add(("pop", ""));
                }
                
                break; 
            
            case CallExpression callExpression:
                if (callExpression.Arguments != null)
                {
                    foreach (IExpression statement in callExpression.Arguments)
                    {
                        Compile(statement, locals);
                        Instructions.Add(("push", "eax"));
                    }
                }
                Instructions.Add(("call", callExpression.FuncName.Value));

                for (int i = 0; i < (callExpression.Arguments?.Count ?? 0); i++)
                {
                    Instructions.Add(("pop", ""));
                }
                break;
            
            default:
                Console.WriteLine(node.GetType());
                break;
        }
    }

    private static string LookUpOperator(string @operator) =>
        @operator switch
        {
            "+"  => "add",
            "-"  => "sub",
            "*"  => "mul",
            "/"  => "div",
            "==" => "cmp",

            _ => throw new NotImplementedException("Operator isn't implemented: " + @operator)
        };
}
