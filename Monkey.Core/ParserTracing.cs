using System;

namespace monkey_csharp.Monkey.Core
{
    public class ParserTracing
    {
        private static int _traceLevel;
        private static readonly char _traceIdentPlaceholder = '\t';

        private string IdentLevel()
        {
            return new string(_traceIdentPlaceholder, _traceLevel - 1);
        }

        private void TracePrint(string fs)
        {
            Console.WriteLine($"{IdentLevel()}{fs}");
        }

        private void IncIdent()
        {
            _traceLevel++;
        }

        private void DecIdent()
        {
            _traceLevel--;
        }

        private string Trace(string msg)
        {
            IncIdent();
            TracePrint($"BEGIN {msg}");
            return msg;
        }
        
        private void Untrace(string msg)
        {
            TracePrint($"END {msg}");
            DecIdent();
        }
    }
}