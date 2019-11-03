using System;
using System.Collections.Generic;
using System.Linq;

namespace monkey_csharp.Monkey.Core
{
    public class Builtins
    {
        public static readonly Dictionary<string, Builtin> BuiltinFunctions = new Dictionary<string, Builtin>
        {
            {"len", new Builtin {Fn = args => {
                if (args.Count != 1)
                    return new Error {Message = $"wrong number of arguments. got={args.Count}, want=1"};
                
                var type = args[0].GetType();
                if (type == typeof(String))
                    return new Integer {Value = ((String) args[0]).Value.Length};
                else if (type == typeof(Array))
                    return new Integer {Value = ((Array) args[0]).Elements.Count};
                
                return new Error {Message = $"argument to `len` not supported, got {args[0].getType()}"}; 
            }}},
            {"first", new Builtin {Fn = args => {
               if (args.Count != 1)
                   return new Error {Message = $"wrong number of arguments. got={args.Count}, want=1"};

               if (args[0].getType() != Type.Array)
                   return new Error {Message = $"argument to `first` must be ARRAY. got {args[0].getType()}"};

               var arr = (Array) args[0];
               if (arr.Elements.Count > 0)
                   return arr.Elements.First();

               return Evaluation.Null;
            }}},
            {"last", new Builtin {Fn = args => {
                if (args.Count != 1)
                    return new Error {Message = $"wrong number of arguments. got={args.Count}, want=1"};

                if (args[0].getType() != Type.Array)
                    return new Error {Message = $"argument to `last` must be ARRAY. got {args[0].getType()}"};

                var arr = (Array) args[0];
                var length = arr.Elements.Count;
                if (length > 0)
                    return arr.Elements.Last();

                return Evaluation.Null;
            }}},
            {"rest", new Builtin {Fn = args => {
                if (args.Count != 1)
                    return new Error {Message = $"wrong number of arguments. got={args.Count}, want=1"};

                if (args[0].getType() != Type.Array)
                    return new Error {Message = $"argument to `rest` must be ARRAY. got {args[0].getType()}"};

                var arr = ((Array) args[0]).Clone();
                var length = arr.Elements.Count;
                if (length > 0)
                {
                    arr.Elements.RemoveAt(0);
                    return arr;
                }

                return Evaluation.Null;
            }}},
            {"push", new Builtin {Fn = args => {
                if (args.Count != 2)
                    return new Error {Message = $"wrong number of arguments. got={args.Count}, want=2"};

                if (args[0].getType() != Type.Array)
                    return new Error {Message = $"argument to `push` must be ARRAY. got {args[0].getType()}"};

                var arr = ((Array) args[0]).Clone();
                arr.Elements.Add(args[1]);

                return arr;
            }}},
            {"puts", new Builtin {Fn = args => {
                args.ForEach(arg => Console.WriteLine(arg.Inspect()));

                return Evaluation.Null;
            }}},
        };
    }
}