using System;
using System.Collections.Generic;

namespace monkey_csharp.Monkey.Core
{
    using PrefixParseFn = Func<Ast.IExpression>;
    using InfixParseFn = Func<Ast.IExpression, Ast.IExpression>;
    
    public enum Precedence
    {
        Lowest,
        Equals,
        LessGreater,
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
            {TokenType.Eq, Precedence.Equals},
            {TokenType.NotEq, Precedence.Equals},
            {TokenType.Lt, Precedence.LessGreater},
            {TokenType.Gt, Precedence.LessGreater},
            {TokenType.Plus, Precedence.Sum},
            {TokenType.Minus, Precedence.Sum},
            {TokenType.Slash, Precedence.Product},
            {TokenType.Asterisc, Precedence.Prefix},
            {TokenType.Lparen, Precedence.Call},
            {TokenType.Lbracket, Precedence.Index}
        };
        
        private Lexer _l;

        public List<string> Errors { get; }
        
        private Token _curToken;
        private Token _peekToken;
        

        private Dictionary<TokenType, PrefixParseFn> _prefixParseFn; 
        private Dictionary<TokenType, InfixParseFn> _infixParseFn; 

        
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

            this._infixParseFn = new Dictionary<TokenType, InfixParseFn>();
            RegisterInfix(TokenType.Plus, ParseInfixExpression);
            RegisterInfix(TokenType.Minus, ParseInfixExpression);
            RegisterInfix(TokenType.Slash, ParseInfixExpression);
            RegisterInfix(TokenType.Asterisc, ParseInfixExpression);
            RegisterInfix(TokenType.Eq, ParseInfixExpression);
            RegisterInfix(TokenType.Plus, ParseInfixExpression);
            RegisterInfix(TokenType.NotEq, ParseInfixExpression);
            RegisterInfix(TokenType.Lt, ParseInfixExpression);
            RegisterInfix(TokenType.Gt, ParseInfixExpression);
            RegisterInfix(TokenType.Lparen, ParseCallExpression);
            RegisterInfix(TokenType.Lbracket, ParseIndexExpression);
            
            NextToken();
            NextToken();
        }
        
        private void NextToken()
        {
            this._curToken = this._peekToken;
            this._peekToken = this._l.NextToken();
        }

        private bool CurTokenIs(TokenType t)
        {
            return this._curToken.Type == t;
        }

        private bool PeekTokenIs(TokenType t)
        {
            return this._peekToken.Type == t;
        }

        private bool ExpectPeek(TokenType t)
        {
            if (this.PeekTokenIs(t))
            {
                NextToken();
                return true;
            }
            PeekError(t);
            return false;
        }

        private void PeekError(TokenType t)
        {
            var msg = $"expected next token to be {t}, got {_peekToken.Type} instead";
            this.Errors.Add(msg);
        }
        
        private void NoPrefixParseFnError(TokenType t)
        {
            var msg = $"no prefix parse function for {t} found";
            this.Errors.Add(msg);
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
            switch (this._curToken.Type)
            {
                case TokenType.Let:
                    return ParseLetStatement();
                case TokenType.Return:
                    return ParseReturnStatement();
                default:
                    return ParseExpressionStatement();
            }
        }
        
        private Ast.LetStatement ParseLetStatement()
        {
            var statement = new Ast.LetStatement {Token = this._curToken};
            if (!ExpectPeek(TokenType.Ident)) return null;

            statement.Name = new Ast.Identifier {Token = this._curToken, Value = this._curToken.Literal};

            if (!ExpectPeek(TokenType.Assign)) return null;
            
            NextToken();

            statement.Value = ParseExpression(Precedence.Lowest);
            
            if (PeekTokenIs(TokenType.Semicolon))
                NextToken();

            return statement;
        }

        private Ast.ReturnStatement ParseReturnStatement()
        {
            var statement = new Ast.ReturnStatement {Token = this._curToken};
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
                Token = this._curToken,
                Expression = ParseExpression(Precedence.Lowest)
            };


            if (PeekTokenIs(TokenType.Semicolon))
                NextToken();

            return statement;
        }

        private Ast.IExpression ParseExpression(Precedence precedence)
        {
            if (!this._prefixParseFn.TryGetValue(this._curToken.Type, out var prefix))
            {
                NoPrefixParseFnError(this._curToken.Type);
                return null;
            }

            var leftExp = prefix();

            while (!PeekTokenIs(TokenType.Semicolon) && precedence < PeekPrecedence())
            {
                if (!this._infixParseFn.TryGetValue(this._peekToken.Type, out var infix))
                    return leftExp;
                
                NextToken();

                leftExp = infix(leftExp);
            }

            return leftExp;
        }

        private Precedence PeekPrecedence()
        {
            if (_precedences.TryGetValue(this._peekToken.Type, out var p))
            {
                return p;
            }

            return Precedence.Lowest;
        }
        
        private Precedence CurPrecedence()
        {
            if (_precedences.TryGetValue(this._curToken.Type, out var p))
            {
                return p;
            }

            return Precedence.Lowest;
        }

        private Ast.IExpression ParseIdentifier()
        {
            return new Ast.Identifier {Token = this._curToken, Value = this._curToken.Literal};
        }

        private Ast.IExpression ParseIntegerLiteral()
        {
            var lit = new Ast.IntegerLiteral {Token = this._curToken};

            if (!int.TryParse(this._curToken.Literal, out var value))
            {
                var msg = $"could not parse {this._curToken.Literal} as integer";
                this.Errors.Add(msg);
                return null;
            }

            lit.Value = value;

            return lit;
        }

        private Ast.IExpression ParseStringLiteral()
        {
            return new Ast.StringLiteral {Token = this._curToken, Value = this._curToken.Literal};
        }

        private Ast.IExpression ParsePrefixExpression()
        {
            var expression = new Ast.PrefixExpression
            {
                Token = this._curToken,
                Operator = this._curToken.Literal
            };

            NextToken();

            expression.Right = ParseExpression(Precedence.Prefix);

            return expression;
        }
        
        private Ast.IExpression ParseInfixExpression(Ast.IExpression left)
        {
            var expression = new Ast.InfixExpression
            {
                Token = this._curToken,
                Operator = this._curToken.Literal,
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
                Token = this._curToken,
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
                Token = this._curToken
            };

            if (!ExpectPeek(TokenType.Lparen))
                return null;
            
            NextToken();
            expression.Condition = ParseExpression(Precedence.Lowest);

            if (!ExpectPeek(TokenType.Rparen))
                return null;

            if (!ExpectPeek(TokenType.Lbrace))
                return null;

            expression.Consequence = ParseBlockStatement();

            if (PeekTokenIs(TokenType.Else))
            {
                NextToken();
                if (!ExpectPeek(TokenType.Lbrace))
                    return null;

                expression.Alternative = ParseBlockStatement();
            }

            return expression;
        }

        private Ast.BlockStatement ParseBlockStatement()
        {
            var block = new Ast.BlockStatement
            {
                Token = this._curToken,
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
                Token = this._curToken
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
                Token = this._curToken,
                Value = this._curToken.Literal
            };
            identifiers.Add(ident);

            while (PeekTokenIs(TokenType.Comma))
            {
                NextToken();
                NextToken();
                ident = new Ast.Identifier
                {
                    Token = this._curToken,
                    Value = this._curToken.Literal
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
                Token = this._curToken,
                Function = function,
                Arguments = ParseExpressionList(TokenType.Rparen)
            };

            return exp;
        }

        private void RegisterPrefix(TokenType tokenType, PrefixParseFn fn)
        {
            this._prefixParseFn[tokenType] = fn;
        }

        private void RegisterInfix(TokenType tokenType, InfixParseFn fn)
        {
            this._infixParseFn[tokenType] = fn;
        }

        private Ast.IExpression ParseArrayLiteral()
        {
            var array = new Ast.ArrayLiteral
            {
                Token = this._curToken, 
                Elements = ParseExpressionList(TokenType.Rbracket)
            };

            return array;
        }
        
        private List<Ast.IExpression> ParseExpressionList(TokenType end)
        {
            var list = new List<Ast.IExpression>();

            if (PeekTokenIs(TokenType.Rparen))
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
                return null;

            return list;
        }

        private Ast.IExpression ParseIndexExpression(Ast.IExpression left)
        {
            var exp = new Ast.IndexExpression {Token = this._curToken, Left = left};
            
            NextToken();
            exp.Index = ParseExpression(Precedence.Lowest);

            if (!ExpectPeek(TokenType.Rbracket))
                return null;

            return exp;
        }

        private Ast.IExpression ParseHashLiteral()
        {
            var hash = new Ast.HashLiteral {
                Token = this._curToken,
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