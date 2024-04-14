namespace BetterInterpreter;

public enum TokenType
{
    Illegal,
    Eof,
    
    Ident,
    Int,
    True,
    False,
    
    Assign,
    Plus,
    Minus,
    Bang,
    Asterisk,
    Slash,
    
    Lt,
    Gt,
    Eq,
    NotEq,
    
    Comma,
    Semicolon,
    
    Lparen,
    Rparen,
    Lbrace,
    Rbrace,
    
    Function, 
    Call,
    Return,
    If,
    Else,
    Type
}

public class Token(TokenType tokenType, string literal)
{
    public TokenType TokenType { get; } = tokenType;
    public string Literal { get; } = literal;
    
    public bool CompareTo(Token obj) => obj.TokenType == TokenType && obj.Literal == Literal;
}
