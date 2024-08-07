﻿namespace Compiler;

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

public class CharLiteral(Token token, char value) : IExpression
{
    private Token Token { get; } = token;
    public char Value { get; } = value;

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

public class InfixExpression(Token token, string @operator, IExpression left, IExpression right = null!) : IExpression
{
    private Token Token { get; } = token;
    public string Operator { get; } = @operator;
    public IExpression Left { get;}= left;
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

public class AssigmentStatement(IExpression value, Token token = null!, Identifier name = null!) : IStatement
{
    public Token Token { get; set; } = token;
    public Identifier Name { get; set; } = name;
    public IExpression Value { get; } = value;
    
    public string TokenLiteral() => Token.Literal;
    
    public override string ToString() => Name + " = " + Value + ";";
}

public class ReturnStatement(Token token, IExpression returnValue = null!) : IStatement
{
    private Token Token { get; } = token;
    public IExpression ReturnValue { get; set; } = returnValue;
    public string TokenLiteral() => Token.Literal;
    public override string ToString() => TokenLiteral() + " " + ReturnValue.ToString() + ";";
}

public class PostFixStatement(string @operator, Identifier name = null!, Token token = null!) : IStatement
{
    public Token Token { get; set; } = token;
    public string Operator { get; } = @operator;
    public Identifier Name { get; set; } = name;

    public string TokenLiteral() => Token.Literal;
    public override string ToString() => Name + Operator + ";";
}

public class BlockStatement(Token token) : IStatement
{
    private Token Token { get; } = token;
    public List<IStatement> Statements { get; } = [];

    public string TokenLiteral() => Token.Literal;
    public override string ToString() => Statements.Aggregate("", (current, statement) => current + statement);
}

public class IfStatement(Token token, IExpression? condition, BlockStatement? consequence = null) : IStatement
{
    private Token Token { get; } = token;
    public IExpression? Condition { get; } = condition;
    public BlockStatement? Consequence { get; } = consequence;
    public BlockStatement? Alternative { get; set; }

    public string TokenLiteral() => Token.Literal;

    public override string ToString() =>
        "if" + Condition + "{" + Consequence + "}" + (Alternative != null ? " else {" + Alternative + "}": ""); 
}

public class WhileStatement(Token token, IExpression? condition, BlockStatement? consequence = null) : IStatement
{
    private Token Token { get; } = token;
    public IExpression? Condition { get; } = condition;
    public BlockStatement? Consequence { get; } = consequence;

    public string TokenLiteral() => Token.Literal;

    public override string ToString() =>
        "while" + Condition + "{" + Consequence + "}"; 
}

public class BreakStatement(Token token) : IStatement
{
    private Token Token { get; } = token;

    public string TokenLiteral() => Token.Literal;

    public override string ToString() => TokenLiteral() + ";"; 
}

public class Parameter(Token type, Token name) : IStatement
{
    private Token Token { get; } = type;
    public Token Name { get; } = name;

    public string TokenLiteral() => Token.Literal;
    public override string ToString() => TokenLiteral() + " " + Name.Literal;
}

public class FunctionLiteral(Token type, Token name, List<Parameter>? parameters = null, BlockStatement? body = null) : IStatement
{
    private Token Token { get; } = type;
    public Identifier Name { get; } = new(name, name.Literal);
    public List<Parameter>? Parameters { get; set; } = parameters;
    public BlockStatement? Body { get; set; } = body;
 
    public string TokenLiteral() => Token.Literal;
    public override string ToString() => TokenLiteral() + " " + Name + "(" + (Parameters != null ? string.Join(",", Parameters) : "") + ")" + "{" + Body + "}";
}

public class CallExpression(Token token, Identifier funcName, List<IExpression>? arguments) : IExpression
{
    private Token Token { get; } = token;
    public Identifier FuncName { get; } = funcName;
    public List<IExpression>? Arguments { get; } = arguments;

    public string TokenLiteral() => Token.Literal;
    public override string ToString() => FuncName + "(" + (Arguments != null ? string.Join(",", Arguments) : "") + ")";
}