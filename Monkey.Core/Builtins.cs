using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Array = Monkey.Core.Array;
using String = Monkey.Core.String;
using Type = Monkey.Core.Type;

namespace Monkey.Core
{
    public static class Builtins
    {
        public static readonly Dictionary<string, Builtin> BuiltinFunctions = new Dictionary<string, Builtin> {
            {"len", new Builtin {Name = "len", Fn = Len}},
            {"first", new Builtin {Name = "first", Fn = First}},
            {"last", new Builtin {Name = "last", Fn = Last}},
            {"rest", new Builtin {Name = "rest", Fn = Rest}},
            {"push", new Builtin {Name = "push", Fn = Push}},
            {"pop", new Builtin {Name = "pop", Fn = Pop}},
            {"print", new Builtin {Name = "print", Fn = Print}},
            {"input", new Builtin {Name = "input", Fn = Input}},
            {"exit", new Builtin {Name = "exit", Fn = Exit}},
            {"read", new Builtin {Name = "read", Fn = Read}},
            {"write", new Builtin {Name = "write", Fn = Write}}
        };

        private static IObject Len(List<IObject> args)
        {
            if (args.Count != 1)
                return new Error {Message = $"wrong number of arguments. got={args.Count}, want=1"};
                
            var type = args[0].GetType();
            if (type == typeof(String))
                return new Integer {Value = ((String) args[0]).Value.Length};
            else if (type == typeof(Array))
                return new Integer {Value = ((Array) args[0]).Elements.Count};
                
            return new Error {Message = $"argument to `len` not supported, got {args[0].getType()}"};
        }
        
        private static IObject First(List<IObject> args)
        {
            if (args.Count != 1)
                return new Error {Message = $"wrong number of arguments. got={args.Count}, want=1"};

            if (args[0].getType() != Type.Array)
                return new Error {Message = $"argument to `first` must be ARRAY. got {args[0].getType()}"};

            var arr = (Array) args[0];
            if (arr.Elements.Count > 0)
                return arr.Elements.First();

            return Evaluation.Null;
            
        }
        
        private static IObject Last(List<IObject> args)
        {
            if (args.Count != 1)
                return new Error {Message = $"wrong number of arguments. got={args.Count}, want=1"};

            if (args[0].getType() != Type.Array)
                return new Error {Message = $"argument to `last` must be ARRAY. got {args[0].getType()}"};

            var arr = (Array) args[0];
            var length = arr.Elements.Count;
            if (length > 0)
                return arr.Elements.Last();

            return Evaluation.Null;
        }
        
        private static IObject Rest(List<IObject> args)
        {
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
        }
        
        private static IObject Push(List<IObject> args)
        {
            if (args.Count != 2)
                return new Error {Message = $"wrong number of arguments. got={args.Count}, want=2"};

            if (args[0].getType() != Type.Array)
                return new Error {Message = $"argument to `push` must be ARRAY. got {args[0].getType()}"};

            var arr = ((Array) args[0]).Clone();
            arr.Elements.Add(args[1]);

            return arr;
        }
        
        private static IObject Pop(List<IObject> args)
        {
            if (args.Count != 1)
                return new Error {Message = $"wrong number of arguments. got={args.Count}, want=1"};

            if (args[0].getType() != Type.Array)
                return new Error {Message = $"argument to `push` must be ARRAY. got {args[0].getType()}"};

            var arr = (Array) args[0];
            var length = arr.Elements.Count;

            if (length == 0)
                return new Error {Message = "cannot pop from an empty array"};

            var element = arr.Elements[length - 1];
            arr.Elements.RemoveAt(length - 1);

            return element;
        }
        
        private static IObject Print(List<IObject> args)
        {
            args.ForEach(arg => Console.WriteLine(arg.Inspect()));

            return Evaluation.Null;
        }
        
        private static IObject Input(List<IObject> args)
        {
            if (args.Count != 0)
            {
                if (args[0].getType() != Type.String)
                    return new Error {Message = $"argument to `input` not supported. got {args[0]}"};
                Console.Out.Write(((String)args[0]).Value);
            }

            var line = Console.In.ReadLine();
            return new String {Value = line};
        }
        
        private static IObject Exit(List<IObject> args)
        {
            if (args.Count != 1) System.Environment.Exit(0);
            if (args[0].getType() != Type.Integer)
                return new Error {Message = $"argument to `exit` must be INTEGER. got {args[0].getType()}"};
            System.Environment.Exit(((Integer)args[0]).Value);
            return null;
        }

        private static IObject Read(List<IObject> args)
        {
            if (args.Count != 1)
                return new Error {Message = $"wrong number of arguments. got={args.Count}, want=1"};

            if (args[0].getType() != Type.String)
                return new Error {Message = $"argument to `read` must be STRING. got {args[0].getType()}"};

            var filename = ((String) args[0]).Value;
            if (!File.Exists(filename))
                return new Error {Message = $"file not found: {filename}"};

            var data = File.ReadAllText(filename);
            return new String {Value = data};
        }
        
        private static IObject Write(List<IObject> args)
        {
            if (args.Count != 2)
                return new Error {Message = $"wrong number of arguments. got={args.Count}, want=2"};

            if (args[0].getType() != Type.String)
                return new Error {Message = $"argument1 to `read` must be STRING. got {args[0].getType()}"};

            if (args[1].getType() != Type.String)
                return new Error {Message = $"argument2 to `read` must be STRING. got {args[0].getType()}"};

            
            var filename = ((String) args[0]).Value;
            var data = ((String) args[1]).Value;
            
            File.WriteAllText(filename, data);

            return null;
        }
    }
}
