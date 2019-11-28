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
        Percent,
        
        And,
        Or,
        Xor,

        Lt,
        Lte,
        
        Gt,
        Gte,
        
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
        Val,
        True,
        False,
        If,
        Else,
        While,
        Return
    }
    
    public class Token
    {
        private static readonly Dictionary<string, TokenType> Keywords = new Dictionary<string, TokenType>
        {
            {"func", TokenType.Function},
            {"var", TokenType.Var},
            {"val", TokenType.Val},
            {"true", TokenType.True},
            {"false", TokenType.False},
            {"if", TokenType.If},
            {"else", TokenType.Else},
            {"while", TokenType.While},
            {"return", TokenType.Return}
        };
        
        public TokenType Type;
        public string Literal;
        public int Line;
        public int Column;

        public Token(TokenType tokenType, char ch, int line, int column)
        {
                this.Type = tokenType;
                this.Literal = ch.ToString();
                this.Line = line;
                this.Column = column;
        }
        
        public Token(TokenType tokenType, string str, int line, int column)
        {
            this.Type = tokenType;
            this.Literal = str;
            this.Line = line;
            this.Column = column;
        }

        public override string ToString()
        {
            return $"Token(Type: {this.Type}, Literal: {this.Literal})";
        }

        public string TokenPosition()
        {
            return $"({this.Line}, {this.Column})";
        }
        
        public static TokenType LookUpIdent(string ident)
        {
            if (Keywords.ContainsKey(ident))
                return Keywords[ident];

            return TokenType.Ident;
        }
    }
}