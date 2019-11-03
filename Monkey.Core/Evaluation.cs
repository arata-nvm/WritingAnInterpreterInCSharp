using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace monkey_csharp.Monkey.Core
{
    public class Evaluation
    {
        public static readonly Boolean True = new Boolean {Value = true};
        public static readonly Boolean False = new Boolean {Value = false}; 
        public static readonly Null Null = new Null();

        private static Boolean FromNativeBoolean(bool input)
        {
            return input ? True : False;
        }
         
        public static Object Eval(AST.INode node, Environment env)
        {
            var type = node.GetType();

            if (type == typeof(AST.Code))
                return EvalCode((AST.Code) node, env);
            
            else if (type == typeof(AST.ExpressionStatement))
                return Eval(((AST.ExpressionStatement) node).Expression, env);
            
            else if (type == typeof(AST.BlockStatement))
                return EvalBlockStatement((AST.BlockStatement) node, env);
            
            else if (type == typeof(AST.ReturnStatement))
            {
                var val = Eval(((AST.ReturnStatement) node).ReturnValue, env);
                if (IsError(val)) return val;
                return new Return {Value = val};
            }
            
            else if (type == typeof(AST.LetStatement))
            {
                var val = Eval(((AST.LetStatement) node).Value, env);
                if (IsError(val)) return val;
                env.Set(((AST.LetStatement) node).Name.Value, val);
            }
            

            else if (type == typeof(AST.IntegerLiteral))
                return new Integer {Value = ((AST.IntegerLiteral) node).Value};
            
            else if (type == typeof(AST.StringLiteral))
                return new String {Value = ((AST.StringLiteral) node).Value};
            
            else if (type == typeof(AST.Boolean))
                return FromNativeBoolean(((AST.Boolean) node).Value);
            
            else if (type == typeof(AST.PrefixExpression))
            {
                var prefix = (AST.PrefixExpression) node;
                var right = Eval(prefix.Right, env);
                if (IsError(right)) return right;
                return EvalPrefixExpression(prefix.Operator, right);
            }
            else if (type == typeof(AST.InfixExpression))
            {
                var infix = (AST.InfixExpression) node;
                var left = Eval(infix.Left, env);
                if (IsError(left)) return left;
                var right = Eval(infix.Right, env);
                if (IsError(left)) return right;
                return EvalInfixExpression(infix.Operator, left, right);
            }
            else if (type == typeof(AST.IfExpression))
                return EvalIfExpression((AST.IfExpression) node, env);

            else if (type == typeof(AST.Identifier))
                return EvalIdentifier((AST.Identifier) node, env);
            
            else if (type == typeof(AST.FunctionLiteral))
            {
                var func = (AST.FunctionLiteral) node;
                var param = func.Parameters;
                var body = func.Body;
                return new Function {Parameters = param, Env = env, Body = body};
            }
            else if (type == typeof(AST.CallExpression))
            {
                var call = (AST.CallExpression) node;
                var func = Eval(call.Function, env);
                if (IsError(func)) return func;
                var args = EvalExpressions(call.Arguments, env);
                if (args.Count == 1 && IsError(args[0]))
                    return args[0];
                return ApplyFunction(func, args);
            }
            
            return null;
        }

        private static Object EvalCode(AST.Code code, Environment env)
        {
            Object result = null;

            foreach (var s in code.Statements)
            {
                result = Eval(s, env);

                if (result == null) continue;
                if (result.getType() == Type.Return)
                    return ((Return) result).Value;
                else if (result.getType() == Type.Error)
                    return result;
            }

            return result;
        }

        private static Object EvalStatements(List<AST.IStatement> statements, Environment env)
        {
            Object result = null;
            
            foreach (var s in statements)
            {
                result = Eval(s, env);
                
                if (result == null) continue;
                if (result.getType() == Type.Return)
                {
                    return ((Return) result).Value;
                }
            }
            
            return result;
        }

        private static Object EvalBlockStatement(AST.BlockStatement block, Environment env)
        {
            Object result = null;
            
            foreach (var s in block.Statements)
            {
                result = Eval(s, env);

                if (result == null) continue;
                var rt = result.getType();
                if (rt == Type.Return | rt == Type.Error)
                    return result;
            }

            return result;
        }

        private static Object EvalPrefixExpression(string op, Object right)
        {
            switch (op)
            {
                case "!":
                    return EvalBangOperatorExpression(right);
                case "-":
                    return EvalMinusPrefixOperatorExpression(right);
                default:
                    return new Error {Message = $"unknown operator: {op}{right.getType()}"};
            }
        }

        private static Object EvalBangOperatorExpression(Object right)
        {
            if (right == True)
                return False;
            else if (right == False)
                return True;
            else if (right == Null)
                return True;
            return False;
        }

        private static Object EvalMinusPrefixOperatorExpression(Object right)
        {
            if (right.getType() != Type.Integer)
                return new Error {Message = $"unknown operator: -{right.getType()}"};


            var value = ((Integer) right).Value;
            return new Integer {Value = -value};
        }

        private static Object EvalInfixExpression(string op, Object left, Object right)
        {
            if (left.getType() == Type.Integer && right.getType() == Type.Integer)
                return EvalIntegerInfixExpression(op, left, right);
            else if (left.getType() == Type.String && right.getType() == Type.String)
                return EvalStringInfixExpression(op, left, right);
            else if (op == "==")
                return FromNativeBoolean(left == right);
            else if (op == "!=")
                return FromNativeBoolean(left != right);
            else if (left.getType() != right.getType())
                return new Error {Message = $"type mismatch: {left.getType()} {op} {right.getType()}"};
            else
                return new Error {Message = $"unknown operator: {left.getType()} {op} {right.getType()}"};
        }

        private static Object EvalIntegerInfixExpression(string op, Object left, Object right)
        {
            var leftVal = ((Integer) left).Value;
            var rightVal = ((Integer) right).Value;

            switch (op)
            {
                case "+":
                    return new Integer {Value = leftVal + rightVal};
                case "-":
                    return new Integer {Value = leftVal - rightVal};
                case "*":
                    return new Integer {Value = leftVal * rightVal};
                case "/":
                    return new Integer {Value = leftVal / rightVal};
                case "<":
                    return FromNativeBoolean(leftVal < rightVal);
                case ">":
                    return FromNativeBoolean(leftVal > rightVal);
                case "==":
                    return FromNativeBoolean(leftVal == rightVal);
                case "!=":
                    return FromNativeBoolean(leftVal != rightVal);
                default:
                    return Null;
            }
        }

        private static Object EvalStringInfixExpression(string op, Object left, Object right)
        {
            if (op != "+")
                return new Error {Message = $"unknown operator: {left.getType()} {op} {right.getType()}"};

            var leftVal = ((String) left).Value;
            var rightVal = ((String) right).Value;
            return new String {Value = leftVal + rightVal};
        }

        private static Object EvalIfExpression(AST.IfExpression ie, Environment env)
        {
            var condition = Eval(ie.Condition, env);
            if (IsError(condition))
                return condition;
            
            if (IsTruthy(condition))
                return Eval(ie.Consequence, env);
            else if (ie.Alternative != null)
                return Eval(ie.Alternative, env);

            return Null;
        }

        private static bool IsTruthy(Object obj)
        {
            if (obj == Null)
                return false;
            else if (obj == True)
                return true;
            else if (obj == False)
                return false;

            return true;
        }

        private static bool IsError(Object obj)
        {
            if (obj != null)
                return obj.getType() == Type.Error;

            return false;
        }

        private static Object EvalIdentifier(AST.Identifier node, Environment env)
        {
            var val = env.Get(node.Value);
            if (val != null)
                return val;

            if (Builtins.BuiltinFunctions.ContainsKey(node.Value))
                return Builtins.BuiltinFunctions[node.Value];

            return new Error {Message = $"identifier not found: {node.Value}"};
        }

        private static List<Object> EvalExpressions(List<AST.IExpression> exps, Environment env)
        {
            List<Object> result = new List<Object>();
            
            foreach (AST.IExpression e in exps)
            {
                var evaluated = Eval(e, env);
                if (IsError(evaluated)) return new List<Object> {evaluated};

                result.Add(evaluated);
            }

            return result;
        }

        private static Object ApplyFunction(Object fn, List<Object> args)
        {
            if (fn.GetType() == typeof(Function))
            {
                var func = (Function) fn;
                var env = ExtendFunctionEnv(func, args);
                return UnwrapReturnValue(Eval(func.Body, env));
            }
            else if (fn.GetType() == typeof(Builtin))
                return ((Builtin) fn).Fn(args);

            return new Error {Message = $"not a function: {fn.getType()}"};
        }

        private static Environment ExtendFunctionEnv(Function fn, List<Object> args)
        {
            var env = fn.Env.Clone();

            for (int i = 0; i < fn.Parameters.Count; i++)
            {
                env.Set(fn.Parameters[i].Value, args[i]);
            }

            return env;
        }

        private static Object UnwrapReturnValue(Object obj)
        {
            if (obj.GetType() != typeof(Return)) return obj;
            return ((Return) obj).Value;
        }
    }
}