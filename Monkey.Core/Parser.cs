using System;
using System.Collections.Generic;

namespace monkey_csharp.Monkey.Core
{
    using PrefixParseFn = Func<AST.IExpression>;
    using InfixParseFn = Func<AST.IExpression, AST.IExpression>;
    
    public enum Precedence
    {
        Lowest,
        Equals,
        LessGreater,
        Sum,
        Product,
        Prefix,
        Call,
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
            {TokenType.Lparen, Precedence.Call}
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
        
        public AST.Code ParseCode()
        {
            var code = new AST.Code
            {
                Statements = new List<AST.IStatement>()
            };

            while (!CurTokenIs(TokenType.Eof))
            {
                var statement = ParseStatement();
                if (statement != null)
                {
                    code.Statements.Add(statement);
                }
                NextToken();
            }

            return code;
        }
        
        private AST.IStatement ParseStatement()
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
        
        private AST.LetStatement ParseLetStatement()
        {
            var statement = new AST.LetStatement {Token = this._curToken};
            if (!ExpectPeek(TokenType.Ident)) return null;

            statement.Name = new AST.Identifier {Token = this._curToken, Value = this._curToken.Literal};

            if (!ExpectPeek(TokenType.Assign)) return null;
            
            NextToken();

            statement.Value = ParseExpression(Precedence.Lowest);
            
            while (PeekTokenIs(TokenType.Semicolon))
                NextToken();

            return statement;
        }

        private AST.ReturnStatement ParseReturnStatement()
        {
            var statement = new AST.ReturnStatement {Token = this._curToken};
            NextToken();

            statement.ReturnValue = ParseExpression(Precedence.Lowest);
            
            if (PeekTokenIs(TokenType.Semicolon))
                NextToken();

            return statement;
        }

        private AST.ExpressionStatement ParseExpressionStatement()
        {
            var statement = new AST.ExpressionStatement
            {
                Token = this._curToken, Expression = ParseExpression(Precedence.Lowest)
            };


            if (PeekTokenIs(TokenType.Semicolon))
                NextToken();

            return statement;
        }

        private AST.IExpression ParseExpression(Precedence precedence)
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

                {
                    return leftExp;
                }
                
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

        private AST.IExpression ParseIdentifier()
        {
            return new AST.Identifier {Token = this._curToken, Value = this._curToken.Literal};
        }

        private AST.IExpression ParseIntegerLiteral()
        {
            var lit = new AST.IntegerLiteral {Token = this._curToken};

            if (!int.TryParse(this._curToken.Literal, out var value))
            {
                var msg = $"could not parse {this._curToken.Literal} as integer";
                this.Errors.Add(msg);
                return null;
            }

            lit.Value = value;

            return lit;
        }

        private AST.IExpression ParseStringLiteral()
        {
            return new AST.StringLiteral {Token = this._curToken, Value = this._curToken.Literal};
        }

        private AST.IExpression ParsePrefixExpression()
        {
            var expression = new AST.PrefixExpression
            {
                Token = this._curToken,
                Operator = this._curToken.Literal
            };

            NextToken();

            expression.Right = ParseExpression(Precedence.Prefix);

            return expression;
        }
        
        private AST.IExpression ParseInfixExpression(AST.IExpression left)
        {
            var expression = new AST.InfixExpression
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

        private AST.IExpression ParseBoolean()
        {
            return new AST.Boolean
            {
                Token = this._curToken,
                Value = CurTokenIs(TokenType.True)
            };
        }

        private AST.IExpression ParseGroupedExpression()
        {
            NextToken();

            var exp = ParseExpression(Precedence.Lowest);

            if (!ExpectPeek(TokenType.Rparen))
                return null;

            return exp;
        }

        private AST.IExpression ParseIfExpression()
        {
            var expression = new AST.IfExpression
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

        private AST.BlockStatement ParseBlockStatement()
        {
            var block = new AST.BlockStatement
            {
                Token = this._curToken,
                Statements = new List<AST.IStatement>()
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

        private AST.IExpression ParseFunctionLiteral()
        {
            var lit = new AST.FunctionLiteral
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

        private List<AST.Identifier> ParseFunctionParameters()
        {
            var identifiers = new List<AST.Identifier>();

            if (PeekTokenIs(TokenType.Rparen))
            {
                NextToken();
                return identifiers;
            }
            
            NextToken();

            var ident = new AST.Identifier
            {
                Token = this._curToken,
                Value = this._curToken.Literal
            };
            identifiers.Add(ident);

            while (PeekTokenIs(TokenType.Comma))
            {
                NextToken();
                NextToken();
                ident = new AST.Identifier
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

        private AST.IExpression ParseCallExpression(AST.IExpression function)
        {
            var exp = new AST.CallExpression
            {
                Token = this._curToken,
                Function = function,
                Arguments = ParseCallArguments()
            };

            return exp;
        }

        private List<AST.IExpression> ParseCallArguments()
        {
            var args = new List<AST.IExpression>();

            if (PeekTokenIs(TokenType.Rparen))
            {
                NextToken();
                return args;
            }
            
            NextToken();
            args.Add(ParseExpression(Precedence.Lowest));

            while (PeekTokenIs(TokenType.Comma))
            {
                NextToken();
                NextToken();
                args.Add(ParseExpression(Precedence.Lowest));
            }

            if (!ExpectPeek(TokenType.Rparen))
                return null;

            return args;
        }
        
        private void RegisterPrefix(TokenType tokenType, PrefixParseFn fn)
        {
            this._prefixParseFn[tokenType] = fn;
        }

        private void RegisterInfix(TokenType tokenType, InfixParseFn fn)
        {
            this._infixParseFn[tokenType] = fn;
        }
    }
}