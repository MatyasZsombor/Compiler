namespace Compiler;

public enum TokenType
{
    Illegal,
    Eof,
    
    Ident,
    Int,
    Char,
    True,
    False,
    
    Assign,
    Plus,
    PostfixPlus,
    Minus,
    PostfixMinus,
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
    Apostrophe,
    
    Function, 
    While,
    Break,
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
