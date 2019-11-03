using System.Collections.Generic;

namespace monkey_csharp.Monkey.Core
{
    public class Builtins
    {
        public static readonly Dictionary<string, Builtin> BuiltinFunctions = new Dictionary<string, Builtin>
        {
            {"len", new Builtin {Fn = args =>
                {
                    if (args.Count != 1)
                        return new Error {Message = $"wrong number of arguments. got={args.Count}, want=1"};
                    
                    var type = args[0].GetType();
                    if (type == typeof(String))
                        return new Integer {Value = ((String) args[0]).Value.Length};
                    return new Error {Message = $"argument to `len` not supported, got {args[0].getType()}"};
                }
            }}
        };
    }
}