using System;
using System.Collections.Generic;
using monkey_csharp.Monkey.Core;
using Type = monkey_csharp.Monkey.Core.Type;

namespace monkey_csharp.Monkey.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            Repl.Start(Console.In, Console.Out);
        }
    }
}