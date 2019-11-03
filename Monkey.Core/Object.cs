using System;
using System.Collections.Generic;
using System.Text;

namespace monkey_csharp.Monkey.Core
{
    
    using BuiltinFunction = Func<List<Object>, Object>;

    public enum Type
    {
        Integer,
        String,
        Boolean,
        Null,
        Return,
        Error,
        Function,
        Builtin,
    }

    public interface Object
    {
        Type getType();

        string Inspect();
    }

    public class Integer : Object
    {
        public int Value;

        public Type getType() => Type.Integer;

        public string Inspect()
        {
            return $"{this.Value}";
        }
    }
    
    public class Boolean : Object
    {
        public bool Value;

        public Type getType() => Type.Boolean;

        public string Inspect()
        {
            return $"{this.Value}";
        }
    }
    
    public class Null : Object
    {
        public Type getType() => Type.Null;

        public string Inspect()
        {
            return "null";
        }
    }
    
    public class Return : Object
    {
        public Object Value;
        
        public Type getType() => Type.Return;

        public string Inspect()
        {
            return Value.Inspect();
        }
    }
    
    public class Error : Object
    {
        public string Message;
        
        public Type getType() => Type.Error;

        public string Inspect()
        {
            return $"ERROR: {Message}";
        }
    }

    public class Function : Object
    {
        public List<AST.Identifier> Parameters;
        public AST.BlockStatement Body;
        public Environment Env;

        public Type getType() => Type.Function;

        public string Inspect()
        {
            var param = string.Join(',', Parameters);
            return $"func({param}) {{\n{Body}\n}}";
        }
    }

    public class String : Object
    {
        public string Value;

        public Type getType() => Type.String;

        public string Inspect()
        {
            return this.Value;
        }
    }
    
    public class Builtin : Object
    {
        public BuiltinFunction Fn;

        public Type getType() => Type.Builtin;

        public string Inspect()
        {
            return "builtin function";
        }
    }
}