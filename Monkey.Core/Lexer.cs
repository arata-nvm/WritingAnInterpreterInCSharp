namespace Monkey.Core
{
    public class Lexer
    {
        string _input;
        int _position;
        int _readPosition;
        int _line = 1;
        int _column = 1;
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
                this._column++;
                if (this._ch == '\n')
                {
                    this._line++;
                    this._column = 1;
                }
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
            int line = _line, column = _column;

            if (this._readPosition > this._input.Length)
            {
                return new Token(TokenType.Eof, '\0', line, column);
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
                        tok = new Token(TokenType.Eq, l, line, column);
                    }
                    else
                    {
                        tok = new Token(TokenType.Assign, this._ch, line, column);
                    }
                    break;
                case ':':
                    tok = new Token(TokenType.Colon, this._ch, line, column);
                    break;
                case ';':
                    tok = new Token(TokenType.Semicolon, this._ch, line, column);
                    break;
                case '(':
                    tok = new Token(TokenType.Lparen, this._ch, line, column);
                    break;
                case ')':
                    tok = new Token(TokenType.Rparen, this._ch, line, column);
                    break;
                case ',':
                    tok = new Token(TokenType.Comma, this._ch, line, column);
                    break;
                case '+':
                    tok = new Token(TokenType.Plus, this._ch, line, column);
                    break;
                case '-':
                    tok = new Token(TokenType.Minus, this._ch, line, column);
                    break;
                case '!':
                    if (PeekChar() == '=')
                    {
                        char ch = this._ch;
                        ReadChar();
                        string l = $"{ch}{this._ch}";
                        tok = new Token(TokenType.NotEq, l, line, column);
                    }
                    else
                    {
                        tok = new Token(TokenType.Bang, this._ch, line, column);
                    }
                    break;
                case '*':
                    tok = new Token(TokenType.Asterisc, this._ch, line, column);
                    break;
                case '/':
                    tok = new Token(TokenType.Slash, this._ch, line, column);
                    break;
                case '<':
                    if (PeekChar() == '=')
                    {
                        ReadChar();
                        tok = new Token(TokenType.Lte, "<=", line, column);
                    }
                    else
                    {
                        tok = new Token(TokenType.Lt, this._ch, line, column);
                    }
                    break;
                case '>':
                    if (PeekChar() == '=')
                    {
                        ReadChar();
                        tok = new Token(TokenType.Gte, ">=", line, column);
                    }
                    else
                    {
                        tok = new Token(TokenType.Gt, this._ch, line, column);
                    }
                    break;
                case '{':
                    tok = new Token(TokenType.Lbrace, this._ch, line, column);
                    break;
                case '}':
                    tok = new Token(TokenType.Rbrace, this._ch, line, column);
                    break;
                case '[':
                    tok = new Token(TokenType.Lbracket, this._ch, line, column);
                    break;
                case ']':
                    tok = new Token(TokenType.Rbracket, this._ch, line, column);
                    break;
                case '"':
                    tok = new Token(TokenType.String, ReadString(), line, column);
                    break;
                default: 
                    if (IsLetter(this._ch))
                    {
                        string l = ReadIdentifier();
                        TokenType t = Token.LookUpIdent(l);
                        return new Token(t, l, line, column);
                    }
                    else if (IsDigit(this._ch))
                    {
                        TokenType t = TokenType.Int;
                        string l = ReadNumber();
                        return new Token(t, l, line, column);
                    }
                    tok = new Token(TokenType.Illegal, this._ch, line, column);
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