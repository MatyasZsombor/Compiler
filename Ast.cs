namespace Compiler;

public interface INode
{
    public string TokenLiteral();
    public string ToString();
}

public interface IStatement : INode;
    
public interface IExpression : INode;

public class ProgramNode : INode
{
    public List<IStatement> Statements { get; } = [];

    public string TokenLiteral() => Statements.Count == 0 ? "" : Statements[0].TokenLiteral();

    public override string ToString() => Statements.Aggregate("", (current, statement) => current + statement.ToString());
    
}

public class Identifier(Token token, string value) : IExpression
{
    private Token Token { get; } = token;
    public string Value { get; } = value;

    public string TokenLiteral() => Token.Literal;

    public override string ToString() => Value;
}

public class IntegerLiteral(Token token, int value = 0) : IExpression
{
    private Token Token { get; } = token;
    public int Value { get; set; } = value;
    
    public string TokenLiteral() => Token.Literal;
    public override string ToString() => Value.ToString();
}

public class BoolLiteral(Token token, bool value) : IExpression
{
    private Token Token { get; } = token;
    public bool Value { get; } = value;

    public string TokenLiteral() => Token.Literal;
    public override string ToString() => Token.Literal;
}

public class PrefixExpression(Token token, string @operator, IExpression right = null!) : IExpression
{
    private Token Token { get; } = token;
    public string Operator { get; }= @operator;
    public IExpression Right { get; set; } = right;

    public string TokenLiteral() => Token.Literal;
    public override string ToString() => "(" + Operator + Right + ")";
}

public class InfixExpression(Token token, string @operator, IExpression? left, IExpression right = null!) : IExpression
{
    private Token Token { get; } = token;
    public string Operator { get; } = @operator;
    public IExpression? Left { get;}= left;
    public IExpression Right { get; set; }= right;
    
    public string TokenLiteral() => Token.Literal;
    public override string ToString() => "(" + Left  + " " + Operator + " " + Right + ")";
}

public class DeclarationStatement(Token token, Identifier name, IExpression value = null!) : IStatement
{
    private Token Token { get; } = token;
    public Identifier Name { get; } = name;
    public IExpression Value { get; set; } = value;
    
    public string TokenLiteral() => Token.Literal;
    
    public override string ToString() => TokenLiteral() + " " + Name + " = " + Value + ";";
    
}

public class ReturnStatement(Token token, IExpression returnValue = null!) : IStatement
{
    private Token Token { get; } = token;
    public IExpression ReturnValue { get; set; } = returnValue;
    public string TokenLiteral() => Token.Literal;
    public override string ToString() => TokenLiteral() + " " + ReturnValue.ToString() + ";";
}

public class ExpressionStatement(Token token, IExpression expression = null!) : IStatement
{
    private Token Token { get; } = token;
    public IExpression Expression { get; } = expression;
    public string TokenLiteral() => Token.Literal;
    public override string ToString() => Expression.ToString();
}