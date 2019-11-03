using System;
using System.Collections.Generic;
using System.Linq;

namespace monkey_csharp.Monkey.Core
{
    
    using BuiltinFunction = Func<List<IObject>, IObject>;

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
        Array,
        Hash,
    }

    public interface IObject
    {
        Type getType();

        string Inspect();
    }
    
    public interface IHashable
    {
        HashKey HashKey();
    }

    public class Integer : IObject, IHashable
    {
        public int Value;

        public Type getType() => Type.Integer;

        public string Inspect()
        {
            return $"{this.Value}";
        }

        public HashKey HashKey()
        {
            return new HashKey {Type = getType(), Value = Value};
        }
    }
    
    public class Boolean : IObject, IHashable
    {
        public bool Value;

        public Type getType() => Core.Type.Boolean;

        public string Inspect()
        {
            return $"{this.Value}";
        }

        public HashKey HashKey()
        {
            return new HashKey {Type = getType(), Value = this.Value ? 1 : 0};
        }
    }
    
    public class Null : IObject
    {
        public Type getType() => Core.Type.Null;

        public string Inspect()
        {
            return "null";
        }
    }
    
    public class Return : IObject
    {
        public IObject Value;
        
        public Type getType() => Core.Type.Return;

        public string Inspect()
        {
            return Value.Inspect();
        }
    }
    
    public class Error : IObject
    {
        public string Message;
        
        public Type getType() => Core.Type.Error;

        public string Inspect()
        {
            return $"ERROR: {Message}";
        }
    }

    public class Function : IObject
    {
        public List<Ast.Identifier> Parameters;
        public Ast.BlockStatement Body;
        public Environment Env;

        public Type getType() => Core.Type.Function;

        public string Inspect()
        {
            var param = string.Join(',', Parameters);
            return $"func({param}) {{\n{Body}\n}}";
        }
    }

    public class String : IObject, IHashable
    {
        public string Value;

        public Type getType() => Core.Type.String;

        public string Inspect()
        {
            return this.Value;
        }

        public HashKey HashKey()
        {
            var s1 = Value.Substring(0, Value.Length / 2);
            var s2 = Value.Substring(Value.Length / 2);
            var hash = s1.GetHashCode() << 32 | s2.GetHashCode();
            return new HashKey { Type = getType(), Value = hash };
        }
    }
    
    public class Builtin : IObject
    {
        public BuiltinFunction Fn;

        public Type getType() => Core.Type.Builtin;

        public string Inspect()
        {
            return "builtin function";
        }
    }
    
    public class Array : IObject
    {
        public List<IObject> Elements;

        public Type getType() => Core.Type.Array;

        public string Inspect()
        {
            var elm = string.Join(',', this.Elements.Select(e => e.Inspect()));
            return $"[{elm}]";
        }

        public Array Clone()
        {
            return new Array {Elements = new List<IObject>(this.Elements)};
        }
    }
    
    public class HashKey
    {
        public Type Type;
        public int Value;

        public override bool Equals(object obj)
        {
            return this == obj as HashKey;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) Type * 397) ^ Value;
            }
        }

        public static bool operator ==(HashKey a, HashKey b)
        {
            if (a is null || b is null)
                return false;

            return a.Type == b.Type && a.Value == b.Value;
        }

        public static bool operator !=(HashKey a, HashKey b)
        {
            return !(a == b);
        }
    }

    public class HashPair
    {
        public IObject Key;
        public IObject Value;
    }

    public class Hash : IObject
    {
        public Dictionary<HashKey, HashPair> Pairs;
        
        public Type getType() => Type.Hash;

        public string Inspect()
        {
            var pairs = string.Join(
                ',', 
                Pairs.Select(p => $"{p.Value.Key.Inspect()} : {p.Value.Value.Inspect()}")
                );
            return $"{{{pairs}}}";
        }
    }
    
    
}