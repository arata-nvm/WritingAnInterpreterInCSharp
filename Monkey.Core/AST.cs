using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Monkey.Core
{
    public static class Ast
    {
        public interface INode
        {
            string TokenLiteral();
        }

        public interface IStatement : INode { }

        public interface IExpression : INode { }

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
                Statements.ForEach(s => { sb.Append(s); });
                return sb.ToString();
            }
        }

        public class VarStatement : IStatement
        {
            public Token Token;
            public Identifier Name;
            public IExpression Value;


            public string TokenLiteral()
            {
                return Token.Literal;
            }
            
            public override string ToString()
            {
                return $"{TokenLiteral()} {Name} = {Value};";
            }
        }
        
        public class ValStatement : IStatement
        {
            public Token Token;
            public Identifier Name;
            public IExpression Value;


            public string TokenLiteral()
            {
                return Token.Literal;
            }

            public override string ToString()
            {
                return $"{TokenLiteral()} {Name} = {Value};";
            }
        }

        public class ReturnStatement : IStatement
        {
            public Token Token;
            public IExpression ReturnValue;

            public string TokenLiteral()
            {
                return Token.Literal;
            }

            public override string ToString()
            {
                return $"{TokenLiteral()} {ReturnValue};";
            }
        }

        public class ExpressionStatement : IStatement
        {
            public Token Token;
            public IExpression Expression;

            public string TokenLiteral()
            {
                return Token.Literal;
            }

            public override string ToString()
            {
                return $"{Expression}";
            }
        }

        public class BlockStatement : IStatement
        {
            public Token Token;
            public List<IStatement> Statements = new List<IStatement>();

            public string TokenLiteral()
            {
                return Token.Literal;
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
                return Token.Literal;
            }
 
            public override string ToString()
            {
                return Value;
            }
        }
        
        public class Boolean : IExpression
        {
            public Token Token;
            public bool Value;

            public string TokenLiteral()
            {
                return Token.Literal;
            }

            public override string ToString()
            {
                return Token.Literal;
            }
        }
        
        public class IntegerLiteral : IExpression
        {
            public Token Token;
            public int Value;

            public string TokenLiteral()
            {
                return Token.Literal;
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
                return Token.Literal;
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
                return Token.Literal;
            }

            public override string ToString()
            {
                return $"({Operator}{Right})";
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
                return Token.Literal;
            }

            public override string ToString()
            {
                return $"({Left} {Operator} {Right})";
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
                return Token.Literal;
            }
            
            public override string ToString()
            {
                return Alternative == null ?
                    $"if{Condition} {Consequence}"
                    : $"if{Condition} {Consequence}else {Alternative}";
            }
        }
        
        public class WhileExpression : IExpression
        {
            public Token Token;
            public IExpression Condition;
            public BlockStatement Consequence;

            public string TokenLiteral()
            {
                return Token.Literal;
            }
            
            public override string ToString()
            {
                return $"while {Condition} {Consequence}";
            }
        }

        public class AssignExpression : IExpression
        {
            public Token Token;
            public Identifier Name;
            public IExpression Value;


            public string TokenLiteral()
            {
                return Token.Literal;
            }

            public override string ToString()
            {
                return $"{Name} = {Value};";
            }
        }
        
        public class FunctionLiteral : IExpression
        {
            public Token Token;
            public List<Identifier> Parameters = new List<Identifier>();
            public BlockStatement Body;

            public string TokenLiteral()
            {
                return Token.Literal;
            }

            public override string ToString()
            {
                return $"{TokenLiteral()}({string.Join(",", Parameters)}) {{{Body}}}";
            }
        }
        
        public class CallExpression : IExpression
        {
            public Token Token;
            public IExpression Function;
            public List<IExpression> Arguments = new List<IExpression>();

            public string TokenLiteral()
            {
                return Token.Literal;
            }

            public override string ToString()
            {
                return $"{Function}({string.Join(",", Arguments)})";
            }
        }
        
        public class ArrayLiteral : IExpression
        {
            public Token Token;
            public List<IExpression> Elements = new List<IExpression>();

            public string TokenLiteral()
            {
                return Token.Literal;
            }

            public override string ToString()
            {
                var elem = string.Join(",", Elements.Select(e => e.ToString()));
                return $"[{elem}]";
            }
        }
        
        public class IndexExpression : IExpression
        {
            public Token Token;
            public IExpression Left;
            public IExpression Index;

            public string TokenLiteral()
            {
                return Token.Literal;
            }

            public override string ToString()
            {
                return $"({Left}[{Index}])";
            }
        }
        
        public class HashLiteral : IExpression
        {
            public Token Token;
            public Dictionary<IExpression, IExpression> Pairs = new Dictionary<IExpression, IExpression>();

            public string TokenLiteral()
            {
                return Token.Literal;
            }

            public override string ToString()
            {
                var pairs = string.Join(
                    ",",
                    Pairs.Select(p => $"{p.Key} : {p.Value}")
                );
                return $"{{{pairs}}}";
            }
        }
    }
}