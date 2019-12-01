using System.Collections.Generic;
using System.Linq;

namespace Monkey.Core
{
    public static class Evaluation
    {
        public static readonly Boolean True = new Boolean {Value = true};
        public static readonly Boolean False = new Boolean {Value = false}; 
        public static readonly Null Null = new Null();

        private static Boolean FromNativeBoolean(bool input)
        {
            return input ? True : False;
        }
         
        public static IObject Eval(Ast.INode node, Environment env)
        {
            if (node == null) return null;
            var type = node.GetType();

            if (type == typeof(Ast.Code))
                return EvalCode((Ast.Code) node, env);
            
            else if (type == typeof(Ast.ExpressionStatement))
                return Eval(((Ast.ExpressionStatement) node).Expression, env);
            
            else if (type == typeof(Ast.BlockStatement))
                return EvalBlockStatement((Ast.BlockStatement) node, env);
            
            else if (type == typeof(Ast.ReturnStatement))
            {
                var val = Eval(((Ast.ReturnStatement) node).ReturnValue, env);
                if (IsError(val)) return val;
                return new Return {Value = val};
            }
            
            else if (type == typeof(Ast.VarStatement))
            {
                var val = Eval(((Ast.VarStatement) node).Value, env);
                if (IsError(val)) return val;
                val = env.SetVariable(((Ast.VarStatement) node).Name.Value, val);
                if (IsError(val)) return val;
            }
            else if (type == typeof(Ast.ValStatement))
            {
                var val = Eval(((Ast.ValStatement) node).Value, env);
                if (IsError(val)) return val;
                val = env.SetConstant(((Ast.ValStatement) node).Name.Value, val);
                if (IsError(val)) return val;
            }
            
            else if (type == typeof(Ast.IntegerLiteral))
                return new Integer {Value = ((Ast.IntegerLiteral) node).Value};
            
            else if (type == typeof(Ast.StringLiteral))
                return new String {Value = ((Ast.StringLiteral) node).Value};
            
            else if (type == typeof(Ast.Boolean))
                return FromNativeBoolean(((Ast.Boolean) node).Value);
            
            else if (type == typeof(Ast.PrefixExpression))
            {
                var prefix = (Ast.PrefixExpression) node;
                var right = Eval(prefix.Right, env);
                if (IsError(right)) return right;
                return EvalPrefixExpression(prefix.Operator, right);
            }
            else if (type == typeof(Ast.InfixExpression))
            {
                var infix = (Ast.InfixExpression) node;
                var left = Eval(infix.Left, env);
                if (IsError(left)) return left;
                var right = Eval(infix.Right, env);
                if (IsError(left)) return right;
                return EvalInfixExpression(infix.Operator, left, right);
            }
            else if (type == typeof(Ast.AssignExpression))
            {
                var assign = (Ast.AssignExpression) node;
                var val = Eval(assign.Value, env);
                if (IsError(val)) return val;

                if (env.ExistsVariable(assign.Name.Value))
                    return env.SetVariable(assign.Name.Value, val);

                if (env.ExistsConstant(assign.Name.Value))
                    return env.SetConstant(assign.Name.Value, val);

                return new Error {Message = $"cannot resolve identifier: {assign.Name.Value}"};
            }
            else if (type == typeof(Ast.IfExpression))
                return EvalIfExpression((Ast.IfExpression) node, env);
            else if (type == typeof(Ast.WhileExpression))
                return EvalWhileExpression((Ast.WhileExpression) node, env);

            else if (type == typeof(Ast.Identifier))
                return EvalIdentifier((Ast.Identifier) node, env);
            
            else if (type == typeof(Ast.FunctionLiteral))
            {
                var func = (Ast.FunctionLiteral) node;
                var param = func.Parameters;
                var body = func.Body;
                return new Function {Parameters = param, Env = env, Body = body};
            }
            else if (type == typeof(Ast.CallExpression))
            {
                var call = (Ast.CallExpression) node;
                var func = Eval(call.Function, env);
                if (IsError(func)) return func;
                var args = EvalExpressions(call.Arguments, env);
                if (args.Count == 1 && IsError(args[0]))
                    return args[0];
                return ApplyFunction(func, args);
            }
            else if (type == typeof(Ast.ArrayLiteral))
            {
                var elements = EvalExpressions(((Ast.ArrayLiteral) node).Elements, env);
                if (elements.Count == 1 && IsError(elements[0]))
                    return elements[0];

                return new Array {Elements = elements};
            }
            else if (type == typeof(Ast.IndexExpression))
            {
                var indexExp = (Ast.IndexExpression) node;
                
                var left = Eval(indexExp.Left, env);
                if (IsError(left)) return left;
                
                var index = Eval(indexExp.Index, env);
                if (IsError(index)) return index;

                return EvalIndexExpression(left, index);
            }
            else if (type == typeof(Ast.HashLiteral))
                return EvalHashLiteral((Ast.HashLiteral) node, env);
            
            return null;
        }

        private static IObject EvalCode(Ast.Code code, Environment env)
        {
            IObject result = null;

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

        private static IObject EvalStatements(IEnumerable<Ast.IStatement> statements, Environment env)
        {
            IObject result = null;
            
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

        private static IObject EvalBlockStatement(Ast.BlockStatement block, Environment env)
        {
            IObject result = null;
            
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

        private static IObject EvalPrefixExpression(string op, IObject right)
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

        private static IObject EvalBangOperatorExpression(IObject right)
        {
            if (right == True)
                return False;
            else if (right == False)
                return True;
            else if (right == Null)
                return True;
            return False;
        }

        private static IObject EvalMinusPrefixOperatorExpression(IObject right)
        {
            if (right.getType() != Type.Integer)
                return new Error {Message = $"unknown operator: -{right.getType()}"};


            var value = ((Integer) right).Value;
            return new Integer {Value = -value};
        }

        private static IObject EvalInfixExpression(string op, IObject left, IObject right)
        {
            if (left.getType() == Type.Integer && right.getType() == Type.Integer)
                return EvalIntegerInfixExpression(op, left, right);
            else if (left.getType() == Type.String && right.getType() == Type.String)
                return EvalStringInfixExpression(op, left, right);
            else if (op == "*" && left.getType() == Type.String && right.getType() == Type.Integer)
                return EvalStringAndIntegerInfixExpression(op, left, right);
            else if (op == "*" && left.getType() == Type.Integer && right.getType() == Type.String)
                return EvalStringAndIntegerInfixExpression(op, right, left);
            else if (op == "==")
                return FromNativeBoolean(left == right);
            else if (op == "!=")
                return FromNativeBoolean(left != right);
            else if (left.getType() != right.getType())
                return new Error {Message = $"type mismatch: {left.getType()} {op} {right.getType()}"};
            else
                return new Error {Message = $"unknown operator: {left.getType()} {op} {right.getType()}"};
        }

        private static IObject EvalStringAndIntegerInfixExpression(string op, IObject left, IObject right)
        {
            if (op != "*")
                return new Error {Message = $"unknown operator: {left.getType()} {op} {right.getType()}"};

            var leftVal = ((String) left).Value;
            var rightVal = ((Integer) right).Value;

            var repeatedVal = string.Concat(Enumerable.Repeat(leftVal, rightVal));
            
            return new String {Value = repeatedVal};
        }

        private static IObject EvalIntegerInfixExpression(string op, IObject left, IObject right)
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
                case "%":
                    return new Integer {Value = leftVal % rightVal};
                case "&":
                    return new Integer {Value = leftVal & rightVal};
                case "|":
                    return new Integer {Value = leftVal | rightVal};
                case "^":
                    return new Integer {Value = leftVal ^ rightVal};
                case "/":
                    return new Integer {Value = leftVal / rightVal};
                case "<":
                    return FromNativeBoolean(leftVal < rightVal);
                case "<=":
                    return FromNativeBoolean(leftVal <= rightVal);
                case ">":
                    return FromNativeBoolean(leftVal > rightVal);
                case ">=":
                    return FromNativeBoolean(leftVal >= rightVal);
                case "==":
                    return FromNativeBoolean(leftVal == rightVal);
                case "!=":
                    return FromNativeBoolean(leftVal != rightVal);
                default:
                    return Null;
            }
        }

        private static IObject EvalStringInfixExpression(string op, IObject left, IObject right)
        {
            if (op != "+")
                return new Error {Message = $"unknown operator: {left.getType()} {op} {right.getType()}"};

            var leftVal = ((String) left).Value;
            var rightVal = ((String) right).Value;
            return new String {Value = leftVal + rightVal};
        }

        private static IObject EvalIfExpression(Ast.IfExpression ie, Environment env)
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

        private static IObject EvalWhileExpression(Ast.WhileExpression we, Environment env)
        {
            var condition = Eval(we.Condition, env);
            if (IsError(condition))
                return condition;

            IObject result = Null;
            while (IsTruthy(condition))
            {
                result = Eval(we.Consequence, env);
                condition = Eval(we.Condition, env);
            }

            return result;
        }
        
        private static bool IsTruthy(IObject obj)
        {
            if (obj == Null)
                return false;
            else if (obj == True)
                return true;
            else if (obj == False)
                return false;

            return true;
        }

        private static bool IsError(IObject obj)
        {
            if (obj != null)
                return obj.getType() == Type.Error;

            return false;
        }

        private static IObject EvalIdentifier(Ast.Identifier node, Environment env)
        {
            var val = env.Get(node.Value);
            if (val != null)
                return val;

            if (Builtins.BuiltinFunctions.ContainsKey(node.Value))
                return Builtins.BuiltinFunctions[node.Value];

            return new Error {Message = $"identifier not found: {node.Value}"};
        }

        private static List<IObject> EvalExpressions(List<Ast.IExpression> exps, Environment env)
        {
            List<IObject> result = new List<IObject>();
            
            foreach (Ast.IExpression e in exps)
            {
                var evaluated = Eval(e, env);
                if (IsError(evaluated)) return new List<IObject> {evaluated};

                result.Add(evaluated);
            }

            return result;
        }

        private static IObject ApplyFunction(IObject fn, List<IObject> args)
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

        private static Environment ExtendFunctionEnv(Function fn, List<IObject> args)
        {
            var env = fn.Env.CreateEnclosedEnvironment();

            for (int i = 0; i < fn.Parameters.Count; i++)
            {
                env.SetVariable(fn.Parameters[i].Value, args[i]);
            }

            return env;
        }

        private static IObject UnwrapReturnValue(IObject obj)
        {
            if (obj.GetType() != typeof(Return)) return obj;
            return ((Return) obj).Value;
        }

        private static IObject EvalIndexExpression(IObject left, IObject index)
        {
            if (left.getType() == Type.Array && index.getType() == Type.Integer)
                return EvalArrayExpression(left, index);
            if (left.getType() == Type.Hash)
                return EvalHashIndexExpression(left, index);
            if (left.getType() == Type.String)
                return EvalStringIndexExpression(left, index);

            return new Error {Message = $"index operator not supported: {left.getType()}"};
        }

        private static IObject EvalHashIndexExpression(IObject hash, IObject index)
        {
            var hashObject = (Hash) hash;

            if (!(index is IHashable key))
                return new Error {Message = $"unusable as hash key: {index.getType()}"};
            
            if (!hashObject.Pairs.ContainsKey(key.HashKey()))
                return Null;

            return hashObject.Pairs[key.HashKey()].Value;
        }

        private static IObject EvalArrayExpression(IObject array, IObject index)
        {
            var arrayObject = (Array) array;
            var idx = ((Integer) index).Value;
            var max = arrayObject.Elements.Count - 1;

            if (idx < 0 || idx > max)
                return Null;

            return arrayObject.Elements[idx];
        }
        
        private static IObject EvalStringIndexExpression(IObject str, IObject index)
        {
            var stringObject = (String) str;
            var idx = ((Integer) index).Value;
            var max = stringObject.Value.Length - 1;

            if (idx < 0 || idx > max)
                return Null;

            return new String {Value = $"{stringObject.Value[idx]}"};
        }

        private static IObject EvalHashLiteral(Ast.HashLiteral node, Environment env)
        {
            var pairs = new Dictionary<HashKey, HashPair>();

            foreach (var pair in node.Pairs)
            {
                var key = Eval(pair.Key, env);
                if (IsError(key)) return key;

                if (!(key is IHashable hashKey))
                    return new Error {Message = $"unusable as hash key: {key.getType()}"};

                var value = Eval(pair.Value, env);
                if (IsError(value)) return value;

                var hashed = hashKey.HashKey();
                pairs[hashed] = new HashPair {Key = key, Value = value};
            }

            return new Hash {Pairs = pairs};
        }
    }
}