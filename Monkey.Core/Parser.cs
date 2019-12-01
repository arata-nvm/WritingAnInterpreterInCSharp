using System;
using System.Collections.Generic;

namespace Monkey.Core
{
    using PrefixParseFn = Func<Ast.IExpression>;
    using InfixParseFn = Func<Ast.IExpression, Ast.IExpression>;
    
    public enum Precedence
    {
        Lowest,
        Assign,
        Equals,
        LessGreater,
        Or,
        Xor,
        And,
        Sum,
        Product,
        Prefix,
        Call,
        Index,
    }

    public class Parser
    {
        private readonly Dictionary<TokenType, Precedence> _precedences = new Dictionary<TokenType, Precedence>
        {
            {TokenType.Assign, Precedence.Assign},
            {TokenType.Eq, Precedence.Equals},
            {TokenType.NotEq, Precedence.Equals},
            {TokenType.Lt, Precedence.LessGreater},
            {TokenType.Gt, Precedence.LessGreater},
            {TokenType.Lte, Precedence.LessGreater},
            {TokenType.Gte, Precedence.LessGreater},
            {TokenType.Or, Precedence.Or},
            {TokenType.Xor, Precedence.Xor},
            {TokenType.And, Precedence.And},
            {TokenType.Plus, Precedence.Sum},
            {TokenType.Minus, Precedence.Sum},
            {TokenType.Slash, Precedence.Product},
            {TokenType.Asterisk, Precedence.Product},
            {TokenType.Percent, Precedence.Product},
            {TokenType.Lparen, Precedence.Call},
            {TokenType.Lbracket, Precedence.Index}
        };
        
        private readonly Lexer _l;

        public List<string> Errors { get; }
        
        private Token _curToken;
        private Token _peekToken;
        

        private readonly Dictionary<TokenType, PrefixParseFn> _prefixParseFn; 
        private readonly Dictionary<TokenType, InfixParseFn> _infixParseFn; 

        
        public Parser(Lexer l)
        {
            this._l = l;
            this.Errors = new List<string>();
            
            this._prefixParseFn = new Dictionary<TokenType, PrefixParseFn>();
            RegisterPrefix(TokenType.Ident, ParseIdentifier);
            RegisterPrefix(TokenType.Int, ParseIntegerLiteral);
            RegisterPrefix(TokenType.String, ParseStringLiteral);
            RegisterPrefix(TokenType.Bang, ParsePrefixExpression);
            RegisterPrefix(TokenType.Minus, ParsePrefixExpression);
            RegisterPrefix(TokenType.True, ParseBoolean);
            RegisterPrefix(TokenType.False, ParseBoolean);
            RegisterPrefix(TokenType.Lparen, ParseGroupedExpression);
            RegisterPrefix(TokenType.If, ParseIfExpression);
            RegisterPrefix(TokenType.Function, ParseFunctionLiteral);
            RegisterPrefix(TokenType.Lbracket, ParseArrayLiteral);
            RegisterPrefix(TokenType.Lbrace, ParseHashLiteral);
            RegisterPrefix(TokenType.While, ParseWhileExpression);

            this._infixParseFn = new Dictionary<TokenType, InfixParseFn>();
            RegisterInfix(TokenType.Plus, ParseInfixExpression);
            RegisterInfix(TokenType.Minus, ParseInfixExpression);
            RegisterInfix(TokenType.Slash, ParseInfixExpression);
            RegisterInfix(TokenType.Asterisk, ParseInfixExpression);
            RegisterInfix(TokenType.Percent, ParseInfixExpression);
            RegisterInfix(TokenType.Or, ParseInfixExpression);
            RegisterInfix(TokenType.Xor, ParseInfixExpression);
            RegisterInfix(TokenType.And, ParseInfixExpression);
            RegisterInfix(TokenType.Eq, ParseInfixExpression);
            RegisterInfix(TokenType.Plus, ParseInfixExpression);
            RegisterInfix(TokenType.NotEq, ParseInfixExpression);
            RegisterInfix(TokenType.Lt, ParseInfixExpression);
            RegisterInfix(TokenType.Gt, ParseInfixExpression);
            RegisterInfix(TokenType.Lte, ParseInfixExpression);
            RegisterInfix(TokenType.Gte, ParseInfixExpression);
            RegisterInfix(TokenType.Lparen, ParseCallExpression);
            RegisterInfix(TokenType.Lbracket, ParseIndexExpression);
            RegisterInfix(TokenType.Assign, ParseAssignExpression);
            
            NextToken();
            NextToken();
        }
        
        private void NextToken()
        {
            _curToken = _peekToken;
            _peekToken = _l.NextToken();
        }

        private bool CurTokenIs(TokenType t)
        {
            return _curToken.Type == t;
        }

        private bool PeekTokenIs(TokenType t)
        {
            return _peekToken.Type == t;
        }

        private bool ExpectPeek(TokenType t)
        {
            if (PeekTokenIs(t))
            {
                NextToken();
                return true;
            }
            PeekError(t);
            return false;
        }

        private void PeekError(TokenType t)
        {
            var msg = $"{_peekToken.TokenPosition()} expected next token to be {t}, got {_peekToken.Type} instead";
            Errors.Add(msg);
        }
        
        private void NoPrefixParseFnError(TokenType t)
        {
            var msg = $"{_peekToken.TokenPosition()} no prefix parse function for {t} found";
            Errors.Add(msg);
        }
        
        public Ast.Code ParseCode()
        {
            var code = new Ast.Code
            {
                Statements = new List<Ast.IStatement>()
            };

            while (!CurTokenIs(TokenType.Eof))
            {
                var statement = ParseStatement();
                if (statement != null)
                    code.Statements.Add(statement);
                NextToken();
            }

            return code;
        }
        
        private Ast.IStatement ParseStatement()
        {
            switch (_curToken.Type)
            {
                case TokenType.Var:
                    return ParseVarStatement();
                case TokenType.Val:
                    return ParseValStatement();
                case TokenType.Return:
                    return ParseReturnStatement();
                default:
                    return ParseExpressionStatement();
            }
        }
        
        private Ast.VarStatement ParseVarStatement()
        {
            var token = _curToken;
            var (name, value) = ParseDeclaration();
            if (name == null || value == null) return null;

            return new Ast.VarStatement
            {
                Token = token,
                Name = name,
                Value = value
            };
        }
        
        private Ast.ValStatement ParseValStatement()
        {
            var token = _curToken;
            var (name, value) = ParseDeclaration();
            if (name == null || value == null) return null;

            return new Ast.ValStatement
            {
                Token = token,
                Name = name,
                Value = value
            };
        }

        private (Ast.Identifier, Ast.IExpression) ParseDeclaration()
        {
            if (!ExpectPeek(TokenType.Ident)) return (null, null);

            var name = new Ast.Identifier {Token = _curToken, Value = _curToken.Literal};

            if (!ExpectPeek(TokenType.Assign)) return (null, null);
            
            NextToken();

            var value = ParseExpression(Precedence.Lowest);
            
            if (PeekTokenIs(TokenType.Semicolon))
                NextToken();

            return (name, value); 
        }

        private Ast.ReturnStatement ParseReturnStatement()
        {
            var statement = new Ast.ReturnStatement {Token = _curToken};
            NextToken();

            statement.ReturnValue = ParseExpression(Precedence.Lowest);
            
            if (PeekTokenIs(TokenType.Semicolon))
                NextToken();

            return statement;
        }

        private Ast.ExpressionStatement ParseExpressionStatement()
        {
            var statement = new Ast.ExpressionStatement
            {
                Token = _curToken,
                Expression = ParseExpression(Precedence.Lowest)
            };


            if (PeekTokenIs(TokenType.Semicolon))
                NextToken();

            return statement;
        }

        private Ast.IExpression ParseExpression(Precedence precedence)
        {
            if (!_prefixParseFn.TryGetValue(_curToken.Type, out var prefix))
            {
                NoPrefixParseFnError(_curToken.Type);
                return null;
            }

            var leftExp = prefix();

            while (!PeekTokenIs(TokenType.Semicolon) && precedence < PeekPrecedence())
            {
                if (!_infixParseFn.TryGetValue(_peekToken.Type, out var infix))
                    return leftExp;
                
                NextToken();

                leftExp = infix(leftExp);
            }

            return leftExp;
        }

        private Precedence PeekPrecedence()
        {
            if (_precedences.TryGetValue(_peekToken.Type, out var p))
            {
                return p;
            }

            return Precedence.Lowest;
        }
        
        private Precedence CurPrecedence()
        {
            if (_precedences.TryGetValue(_curToken.Type, out var p))
            {
                return p;
            }

            return Precedence.Lowest;
        }

        private Ast.IExpression ParseIdentifier()
        {
            return new Ast.Identifier {Token = _curToken, Value = _curToken.Literal};
        }

        private Ast.IExpression ParseIntegerLiteral()
        {
            var lit = new Ast.IntegerLiteral {Token = _curToken};

            if (!int.TryParse(_curToken.Literal, out var value))
            {
                var msg = $"could not parse {_curToken.Literal} as integer";
                Errors.Add(msg);
                return null;
            }

            lit.Value = value;

            return lit;
        }

        private Ast.IExpression ParseStringLiteral()
        {
            return new Ast.StringLiteral {Token = _curToken, Value = _curToken.Literal};
        }

        private Ast.IExpression ParsePrefixExpression()
        {
            var expression = new Ast.PrefixExpression
            {
                Token = _curToken,
                Operator = _curToken.Literal
            };

            NextToken();

            expression.Right = ParseExpression(Precedence.Prefix);

            return expression;
        }
        
        private Ast.IExpression ParseInfixExpression(Ast.IExpression left)
        {
            var expression = new Ast.InfixExpression
            {
                Token = _curToken,
                Operator = _curToken.Literal,
                Left = left
            };

            var precedence = CurPrecedence();
            NextToken();
            expression.Right = ParseExpression(precedence);

            return expression;
        }

        private Ast.IExpression ParseBoolean()
        {
            return new Ast.Boolean
            {
                Token = _curToken,
                Value = CurTokenIs(TokenType.True)
            };
        }

        private Ast.IExpression ParseGroupedExpression()
        {
            NextToken();

            var exp = ParseExpression(Precedence.Lowest);

            if (!ExpectPeek(TokenType.Rparen))
                return null;

            return exp;
        }

        private Ast.IExpression ParseIfExpression()
        {
            var expression = new Ast.IfExpression
            {
                Token = _curToken
            };
            
            NextToken();
            expression.Condition = ParseExpression(Precedence.Lowest);

            if (!ExpectPeek(TokenType.Lbrace))
                return null;

            expression.Consequence = ParseBlockStatement();

            if (PeekTokenIs(TokenType.Else))
            {
                NextToken();

                if (PeekTokenIs(TokenType.If))
                {
                    NextToken();
                    expression.Alternative = new Ast.BlockStatement
                    {
                        Statements = new List<Ast.IStatement>
                        {
                            new Ast.ExpressionStatement
                            {
                                Expression = ParseIfExpression()
                            }
                        }
                    };
                    return expression;
                }
                
                if (!ExpectPeek(TokenType.Lbrace))
                    return null;

                expression.Alternative = ParseBlockStatement();
            }

            return expression;
        }
        
        private Ast.WhileExpression ParseWhileExpression()
        {
            var expression = new Ast.WhileExpression()
            {
                Token = _curToken
            };
            
            NextToken();

            expression.Condition = ParseExpression(Precedence.Lowest);

            if (!ExpectPeek(TokenType.Lbrace))
                return null;

            expression.Consequence = ParseBlockStatement();

            return expression;
        }

        private Ast.AssignExpression ParseAssignExpression(Ast.IExpression left)
        {
            if (left.GetType() != typeof(Ast.Identifier))
            {
                var msg = $"{_peekToken.TokenPosition()} expected identifier on left but got {left}";
                Errors.Add(msg);
                return null;
            }
                
            var expression = new Ast.AssignExpression
            {
                Token = _curToken,
                Name = (Ast.Identifier) left
            };

            NextToken();

            expression.Value = ParseExpression(Precedence.Lowest);

            return expression;
        }

        private Ast.BlockStatement ParseBlockStatement()
        {
            var block = new Ast.BlockStatement
            {
                Token = _curToken,
                Statements = new List<Ast.IStatement>()
            };
            
            NextToken();

            while (!CurTokenIs(TokenType.Rbrace) && !CurTokenIs(TokenType.Eof))
            {
                var statement = ParseStatement();
                if (statement != null)
                    block.Statements.Add(statement);
                NextToken();
            }

            return block;
        }

        private Ast.IExpression ParseFunctionLiteral()
        {
            var lit = new Ast.FunctionLiteral
            {
                Token = _curToken
            };

            if (!ExpectPeek(TokenType.Lparen))
                return null;

            lit.Parameters = ParseFunctionParameters();

            if (!ExpectPeek(TokenType.Lbrace))
                return null;

            lit.Body = ParseBlockStatement();

            return lit;
        }

        private List<Ast.Identifier> ParseFunctionParameters()
        {
            var identifiers = new List<Ast.Identifier>();

            if (PeekTokenIs(TokenType.Rparen))
            {
                NextToken();
                return identifiers;
            }
            
            NextToken();

            var ident = new Ast.Identifier
            {
                Token = _curToken,
                Value = _curToken.Literal
            };
            identifiers.Add(ident);

            while (PeekTokenIs(TokenType.Comma))
            {
                NextToken();
                NextToken();
                ident = new Ast.Identifier
                {
                    Token = _curToken,
                    Value = _curToken.Literal
                };
                identifiers.Add(ident);
            }

            if (!ExpectPeek(TokenType.Rparen))
                return null;

            return identifiers;
        }

        private Ast.IExpression ParseCallExpression(Ast.IExpression function)
        {
            var exp = new Ast.CallExpression
            {
                Token = _curToken,
                Function = function,
                Arguments = ParseExpressionList(TokenType.Rparen)
            };

            return exp;
        }

        private void RegisterPrefix(TokenType tokenType, PrefixParseFn fn)
        {
            _prefixParseFn[tokenType] = fn;
        }

        private void RegisterInfix(TokenType tokenType, InfixParseFn fn)
        {
            _infixParseFn[tokenType] = fn;
        }

        private Ast.IExpression ParseArrayLiteral()
        {
            var array = new Ast.ArrayLiteral
            {
                Token = _curToken, 
                Elements = ParseExpressionList(TokenType.Rbracket)
            };

            return array;
        }
        
        private List<Ast.IExpression> ParseExpressionList(TokenType end)
        {
            var list = new List<Ast.IExpression>();

            if (PeekTokenIs(end))
            {
                NextToken();
                return list;
            }
            
            NextToken();
            list.Add(ParseExpression(Precedence.Lowest));

            while (PeekTokenIs(TokenType.Comma))
            {
                NextToken();
                NextToken();
                list.Add(ParseExpression(Precedence.Lowest));
            }

            if (!ExpectPeek(end))
                return new List<Ast.IExpression>();

            return list;
        }

        private Ast.IExpression ParseIndexExpression(Ast.IExpression left)
        {
            var exp = new Ast.IndexExpression {Token = _curToken, Left = left};
            
            NextToken();
            exp.Index = ParseExpression(Precedence.Lowest);

            if (!ExpectPeek(TokenType.Rbracket))
                return null;

            return exp;
        }

        private Ast.IExpression ParseHashLiteral()
        {
            var hash = new Ast.HashLiteral {
                Token = _curToken,
                Pairs = new Dictionary<Ast.IExpression, Ast.IExpression>()
            };

            while (!PeekTokenIs(TokenType.Rbrace))
            {
                NextToken();
                var key = ParseExpression(Precedence.Lowest);

                if (!ExpectPeek(TokenType.Colon))
                    return null;
                
                NextToken();
                var value = ParseExpression(Precedence.Lowest);

                hash.Pairs[key] = value;

                if (!PeekTokenIs(TokenType.Rbrace) && !ExpectPeek(TokenType.Comma))
                    return null;
            }

            if (!ExpectPeek(TokenType.Rbrace))
                return null;

            return hash;
        }
    }
}