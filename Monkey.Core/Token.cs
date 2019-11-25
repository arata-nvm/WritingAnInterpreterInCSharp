using System.Collections.Generic;

namespace Monkey.Core
{
    public enum TokenType
    {
        Illegal,
        Eof,
        
        Ident,
        Int,
        String,
        
        Assign,
        Plus,
        Minus,
        Bang,
        Asterisc,
        Slash,
        
        Lt,
        Gt,
        
        Eq,
        NotEq,
        
        Comma,
        Semicolon,
        Colon,
        
        Lparen,
        Rparen,
        Lbrace,
        Rbrace,
        Lbracket,
        Rbracket,
        
        Function,
        Var,
        True,
        False,
        If,
        Else,
        Return
    }
    
    public class Token
    {
        private static readonly Dictionary<string, TokenType> Keywords = new Dictionary<string, TokenType>
        {
            {"func", TokenType.Function},
            {"var", TokenType.Var},
            {"true", TokenType.True},
            {"false", TokenType.False},
            {"if", TokenType.If},
            {"else", TokenType.Else},
            {"return", TokenType.Return}
        };
        
        public TokenType Type;
        public string Literal;

        public Token(TokenType tokenType, char ch)
        {
            this.Type = tokenType;
            this.Literal = ch.ToString();
        }
        
        public Token(TokenType tokenType, string str)
        {
            this.Type = tokenType;
            this.Literal = str;
        }

        public override string ToString()
        {
            return $"Token(Type: {this.Type}, Literal: {this.Literal})";
        }
        
        public static TokenType LookUpIdent(string ident)
        {
            if (Keywords.ContainsKey(ident))
                return Keywords[ident];

            return TokenType.Ident;
        }
    }
}