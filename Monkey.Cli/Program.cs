using System;
using monkey_csharp.Monkey.Core;

namespace monkey_csharp.Monkey.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            Repl.Start(Console.In, Console.Out, null);
        }
    }
}