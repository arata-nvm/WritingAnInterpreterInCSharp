using System;
using Monkey.Core;

namespace Monkey.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Hello! This is the Monkey programming language.");
                Console.WriteLine("Feel free to type in commands.");
                Repl.Start(Console.In, Console.Out, null);
                return;
            }

            Repl.Exec(args[0]);
            
        }
    }
}