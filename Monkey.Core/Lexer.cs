using System.Collections.Generic;

namespace monkey_csharp.Monkey.Core
{
    public class Lexer
    {
        string _input;
        int _position;
        int _readPosition;
        char _ch;

        public Lexer(string input)
        {
            this._input = input;
            ReadChar();
        }
        
        private bool IsDigit(char ch)
        {
            return char.IsDigit(ch);
        }

        private bool IsLetter(char ch)
        {
            return char.IsLetter(ch);
        }

        private void ReadChar()
        {
            if (this._readPosition >= _input.Length)
            {
                this._ch = ' ';
            }
            else
            {
                this._ch = this._input[this._readPosition];
            }

            this._position = this._readPosition;
            this._readPosition++;
        }
        
        private char PeekChar()
        {
            if (this._readPosition >= this._input.Length)
            {
                return ' ';
            }
            return this._input[this._readPosition];
        }
        
                public Token NextToken()
        {
            Token tok;
            
            if (this._readPosition > this._input.Length)
            {
                return new Token(TokenType.Eof, '\0');
            }
            
            SkipWhiteSpace();

            switch (this._ch)
            {
                case '=':
                    if (PeekChar() == '=')
                    {
                        char ch = this._ch;
                        ReadChar();
                        string l = $"{ch}{this._ch}";
                        tok = new Token(TokenType.Eq, l);
                    }
                    else
                    {
                        tok = new Token(TokenType.Assign, this._ch);
                    }
                    break;
                case ':':
                    tok = new Token(TokenType.Colon, this._ch);
                    break;
                case ';':
                    tok = new Token(TokenType.Semicolon, this._ch);
                    break;
                case '(':
                    tok = new Token(TokenType.Lparen, this._ch);
                    break;
                case ')':
                    tok = new Token(TokenType.Rparen, this._ch);
                    break;
                case ',':
                    tok = new Token(TokenType.Comma, this._ch);
                    break;
                case '+':
                    tok = new Token(TokenType.Plus, this._ch);
                    break;
                case '-':
                    tok = new Token(TokenType.Minus, this._ch);
                    break;
                case '!':
                    if (PeekChar() == '=')
                    {
                        char ch = this._ch;
                        ReadChar();
                        string l = $"{ch}{this._ch}";
                        tok = new Token(TokenType.NotEq, l);
                    }
                    else
                    {
                        tok = new Token(TokenType.Bang, this._ch);
                    }
                    break;
                case '*':
                    tok = new Token(TokenType.Asterisc, this._ch);
                    break;
                case '/':
                    tok = new Token(TokenType.Slash, this._ch);
                    break;
                case '<':
                    tok = new Token(TokenType.Lt, this._ch);
                    break;
                case '>':
                    tok = new Token(TokenType.Gt, this._ch);
                    break;
                case '{':
                    tok = new Token(TokenType.Lbrace, this._ch);
                    break;
                case '}':
                    tok = new Token(TokenType.Rbrace, this._ch);
                    break;
                case '[':
                    tok = new Token(TokenType.Lbracket, this._ch);
                    break;
                case ']':
                    tok = new Token(TokenType.Rbracket, this._ch);
                    break;
                case '"':
                    tok = new Token(TokenType.String, ReadString());
                    break;
                default: 
                    if (IsLetter(this._ch))
                    {
                        string l = ReadIdentifier();
                        TokenType t = Token.LookUpIdent(l);
                        return new Token(t, l);
                    }
                    else if (IsDigit(this._ch))
                    {
                        TokenType t = TokenType.Int;
                        string l = ReadNumber();
                        return new Token(t, l);
                    }
                    tok = new Token(TokenType.Illegal, this._ch);
                    break;
                    
            }
            
            ReadChar();
            return tok;
        }

        private string ReadIdentifier()
        {
            var position = this._position;
            while (IsLetter(this._ch))
                ReadChar();

            return this._input.Substring(position, this._position - position);
        }

        private string ReadNumber()
        {
            var position = this._position;
            while (IsDigit(this._ch))
                ReadChar();

            return this._input.Substring(position, this._position - position);
        }
        
        private string ReadString()
        {
            var pos = this._position + 1;
            while (true)
            {
                ReadChar();
                if (this._ch == '"' || this._readPosition > this._input.Length)
                    break;
            }

            return this._input.Substring(pos, this._position - pos);
        }
        
        private void SkipWhiteSpace()
        {
            while (this._ch == ' ' || this._ch == '\t' || this._ch == '\n')
                ReadChar();
        }
    }
}