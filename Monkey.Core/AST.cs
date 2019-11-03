using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Sockets;
using System.Text;

namespace monkey_csharp.Monkey.Core
{
    public class AST
    {
        public interface INode
        {
            string TokenLiteral();
        }
        
        public interface IStatement : INode
        {
            void StatementNode();
        }
        
        public interface IExpression : INode
        {
            void ExpressionNode();
        }

        public class Code : INode
        {
            public List<IStatement> Statements;

            public string TokenLiteral()
            {
                if (Statements.Count > 0)
                {
                    return Statements[0].TokenLiteral();
                }

                return "";
            }

            public override string ToString()
            {
                var sb = new StringBuilder();
                this.Statements.ForEach(s => { sb.Append(s); });
                return sb.ToString();
            }
        }

        public class LetStatement : IStatement
        {
            public Token Token;
            public Identifier Name;
            public IExpression Value;


            public string TokenLiteral()
            {
                return this.Token.Literal;
            }

            public void StatementNode()
            {
                throw new System.NotImplementedException();
            }

            public override string ToString()
            {
                return $"{this.TokenLiteral()} {this.Name} = {this.Value};";
            }
        }

        public class ReturnStatement : IStatement
        {
            public Token Token;
            public IExpression ReturnValue;

            public string TokenLiteral()
            {
                return this.Token.Literal;
            }

            public void StatementNode()
            {
                throw new System.NotImplementedException();
            }
            
            public override string ToString()
            {
                return $"{this.TokenLiteral()} {this.ReturnValue};";
            }
        }

        public class ExpressionStatement : IStatement
        {
            public Token Token;
            public IExpression Expression;

            public string TokenLiteral()
            {
                return this.Token.Literal;
            }

            public void StatementNode()
            {
                throw new System.NotImplementedException();
            }
            
            public override string ToString()
            {
                return $"{this.Expression}";
            }
        }

        public class BlockStatement : IStatement
        {
            public Token Token;
            public List<IStatement> Statements;

            public string TokenLiteral()
            {
                return this.Token.Literal;
            }

            public void StatementNode()
            {
                throw new System.NotImplementedException();
            }
            
            public override string ToString()
            {
                var sb = new StringBuilder();
                Statements.ForEach(s => sb.Append(s));
                return sb.ToString();
            }
        }
        
        public class Identifier : IExpression
        {
            public Token Token;
            public string Value;

            public string TokenLiteral()
            {
                return this.Token.Literal;
            }

            public void ExpressionNode()
            {
                throw new System.NotImplementedException();
            }
            
            public override string ToString()
            {
                return this.Value;
            }
        }
        
        public class Boolean : IExpression
        {
            public Token Token;
            public bool Value;

            public string TokenLiteral()
            {
                return this.Token.Literal;
            }

            public void ExpressionNode()
            {
                throw new System.NotImplementedException();
            }
            
            public override string ToString()
            {
                return this.Token.Literal;
            }
        }
        
        public class IntegerLiteral : IExpression
        {
            public Token Token;
            public int Value;

            public string TokenLiteral()
            {
                return this.Token.Literal;
            }

            public void ExpressionNode()
            {
                throw new System.NotImplementedException();
            }

            public override string ToString()
            {
                return TokenLiteral();
            }
        }
        
        public class StringLiteral : IExpression
        {
            public Token Token;
            public string Value;

            public string TokenLiteral()
            {
                return this.Token.Literal;
            }

            public void ExpressionNode()
            {
                throw new System.NotImplementedException();
            }

            public override string ToString()
            {
                return TokenLiteral();
            }
        }
        
        public class PrefixExpression : IExpression
        {
            public Token Token;
            public string Operator;
            public IExpression Right;

            public string TokenLiteral()
            {
                return this.Token.Literal;
            }

            public void ExpressionNode()
            {
                throw new System.NotImplementedException();
            }

            public override string ToString()
            {
                return $"({this.Operator}{this.Right})";
            }
        }
        
        public class InfixExpression : IExpression
        {
            public Token Token;
            public IExpression Left;
            public string Operator;
            public IExpression Right;

            public string TokenLiteral()
            {
                return this.Token.Literal;
            }

            public void ExpressionNode()
            {
                throw new System.NotImplementedException();
            }

            public override string ToString()
            {
                return $"({this.Left} {this.Operator} {this.Right})";
            }
        }
        
        public class IfExpression : IExpression
        {
            public Token Token;
            public IExpression Condition;
            public BlockStatement Consequence;
            public BlockStatement Alternative;

            public string TokenLiteral()
            {
                return this.Token.Literal;
            }

            public void ExpressionNode()
            {
                throw new System.NotImplementedException();
            }

            public override string ToString()
            {
                return this.Alternative == null ?
                    $"if{this.Condition} {this.Consequence}"
                    : $"if{this.Condition} {this.Consequence}else {this.Alternative}";
            }
        }
        
        public class FunctionLiteral : IExpression
        {
            public Token Token;
            public List<Identifier> Parameters;
            public BlockStatement Body;

            public string TokenLiteral()
            {
                return this.Token.Literal;
            }

            public void ExpressionNode()
            {
                throw new System.NotImplementedException();
            }

            public override string ToString()
            {
                return $"{TokenLiteral()}({string.Join(',', Parameters)}) {{{this.Body}}}";
            }
        }
        
        public class CallExpression : IExpression
        {
            public Token Token;
            public IExpression Function;
            public List<IExpression> Arguments;

            public string TokenLiteral()
            {
                return this.Token.Literal;
            }

            public void ExpressionNode()
            {
                throw new System.NotImplementedException();
            }

            public override string ToString()
            {
                return $"{this.Function}({string.Join(',', this.Arguments)})";
            }
        }
    }
}