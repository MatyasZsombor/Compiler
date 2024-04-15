namespace Compiler;

public class Compiler
{
    public List<string> Errors { get; } = [];
    public List<(string instruction, string operand)> Instructions { get; } = [];
    private uint _curOffset;
    
    public readonly Dictionary<string, (string type, uint offset)> Identifiers = [];

    public Compiler(INode node)
    {
        Compile(node);
    }

    private void Compile(INode node)
    {
        switch (node)
        {
            case ProgramNode programNode:
                foreach (IStatement statement in programNode.Statements)
                {
                    Compile(statement);
                }
                break;
            case InfixExpression infixExpression:
                Compile(infixExpression.Left);
                Compile(infixExpression.Right);
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
                }
                break;
            case PrefixExpression prefixExpression:
                Compile(prefixExpression.Right);
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
            case DeclarationStatement declarationStatement:
                Identifiers.Add(declarationStatement.Name.ToString(), (declarationStatement.TokenLiteral(), _curOffset));
                Compile(declarationStatement.Value);
                Instructions.Add(("STA", declarationStatement.Name.ToString()));
                if (declarationStatement.TokenLiteral() == "int")
                {
                    _curOffset += 4;
                }
                else if (declarationStatement.TokenLiteral() == "bool")
                {
                    _curOffset++;
                }
                break;
            case IntegerLiteral integerLiteral:
                Instructions.Add(("PUSH", integerLiteral.Value.ToString()));
                break;
            case BoolLiteral boolLiteral:
                Instructions.Add(("PUSH", boolLiteral.Value ? "1" : "0"));
                break;
            case Identifier identifier:
                Instructions.Add(("LDA", identifier.ToString()));
                break;
        }
    }
}
