namespace Monkey.Core
{
    public class Lexer
    {
        private readonly string _input;
        private int _position;
        private int _readPosition;
        private int _line = 1;
        private int _column = 1;
        private char _ch;

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
            if (_readPosition >= _input.Length)
            {
                _ch = ' ';
            }
            else
            {
                _ch = _input[_readPosition];
                _column++;
                if (_ch == '\n')
                {
                    _line++;
                    _column = 1;
                }
            }

            _position = _readPosition;
            _readPosition++;
        }
        
        private char PeekChar()
        {
            if (_readPosition >= _input.Length)
            {
                return ' ';
            }
            return _input[_readPosition];
        }
        
        public Token NextToken()
        {
            Token tok;
            int line = _line, column = _column;

            if (_readPosition > _input.Length)
            {
                return new Token(TokenType.Eof, '\0', line, column);
            }
            
            SkipWhiteSpace();

            switch (_ch)
            {
                case '=':
                    if (PeekChar() == '=')
                    {
                        char ch = _ch;
                        ReadChar();
                        string l = $"{ch}{_ch}";
                        tok = new Token(TokenType.Eq, l, line, column);
                    }
                    else
                    {
                        tok = new Token(TokenType.Assign, _ch, line, column);
                    }
                    break;
                case ':':
                    tok = new Token(TokenType.Colon, _ch, line, column);
                    break;
                case ';':
                    tok = new Token(TokenType.Semicolon, _ch, line, column);
                    break;
                case '(':
                    tok = new Token(TokenType.Lparen, _ch, line, column);
                    break;
                case ')':
                    tok = new Token(TokenType.Rparen, _ch, line, column);
                    break;
                case ',':
                    tok = new Token(TokenType.Comma, _ch, line, column);
                    break;
                case '+':
                    tok = new Token(TokenType.Plus, _ch, line, column);
                    break;
                case '-':
                    tok = new Token(TokenType.Minus, _ch, line, column);
                    break;
                case '!':
                    if (PeekChar() == '=')
                    {
                        char ch = _ch;
                        ReadChar();
                        string l = $"{ch}{_ch}";
                        tok = new Token(TokenType.NotEq, l, line, column);
                    }
                    else
                    {
                        tok = new Token(TokenType.Bang, _ch, line, column);
                    }
                    break;
                case '*':
                    tok = new Token(TokenType.Asterisk, _ch, line, column);
                    break;
                case '%':
                    tok = new Token(TokenType.Percent, _ch, line, column);
                    break;
                case '&':
                    tok = new Token(TokenType.And, _ch, line, column);
                    break;
                case '|':
                    tok = new Token(TokenType.Or, _ch, line, column);
                    break;
                case '^':
                    tok = new Token(TokenType.Xor, _ch, line, column);
                    break;
                case '/':
                    tok = new Token(TokenType.Slash, _ch, line, column);
                    break;
                case '<':
                    if (PeekChar() == '=')
                    {
                        ReadChar();
                        tok = new Token(TokenType.Lte, "<=", line, column);
                    }
                    else
                    {
                        tok = new Token(TokenType.Lt, _ch, line, column);
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
                        tok = new Token(TokenType.Gt, _ch, line, column);
                    }
                    break;
                case '{':
                    tok = new Token(TokenType.Lbrace, _ch, line, column);
                    break;
                case '}':
                    tok = new Token(TokenType.Rbrace, _ch, line, column);
                    break;
                case '[':
                    tok = new Token(TokenType.Lbracket, _ch, line, column);
                    break;
                case ']':
                    tok = new Token(TokenType.Rbracket, _ch, line, column);
                    break;
                case '"':
                    tok = new Token(TokenType.String, ReadString(), line, column);
                    break;
                default: 
                    if (IsLetter(_ch))
                    {
                        string l = ReadIdentifier();
                        TokenType t = Token.LookUpIdent(l);
                        return new Token(t, l, line, column);
                    }
                    else if (IsDigit(_ch))
                    {
                        TokenType t = TokenType.Int;
                        string l = ReadNumber();
                        return new Token(t, l, line, column);
                    }
                    tok = new Token(TokenType.Illegal, _ch, line, column);
                    break;
                    
            }
            
            ReadChar();
            return tok;
        }

        private string ReadIdentifier()
        {
            var position = _position;
            while (IsLetter(_ch))
                ReadChar();

            return _input.Substring(position, _position - position);
        }

        private string ReadNumber()
        {
            var position = _position;
            while (IsDigit(_ch))
                ReadChar();

            return _input.Substring(position, _position - position);
        }
        
        private string ReadString()
        {
            var pos = _position + 1;
            while (true)
            {
                ReadChar();
                if (_ch == '"' || _readPosition > _input.Length)
                    break;
            }

            return _input.Substring(pos, _position - pos);
        }
        
        private void SkipWhiteSpace()
        {
            while (_ch == ' ' || _ch == '\t' || _ch == '\n')
                ReadChar();
        }
    }
}