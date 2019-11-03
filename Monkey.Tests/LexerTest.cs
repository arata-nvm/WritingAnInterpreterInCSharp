using System;
using System.Collections.Generic;
using monkey_csharp.Monkey.Core;

namespace monkey_csharp.Monkey.Tests
{
    class Program
    {
        static void Main(string[] args) {
            Repl.Start(Console.In, Console.Out);
        }
    }
}