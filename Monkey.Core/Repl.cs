using System;
using System.Collections.Generic;
using System.IO;

namespace monkey_csharp.Monkey.Core
{
    public class Repl
    {
        public static readonly string Prompt = ">> ";

        public static void Start(TextReader reader, TextWriter writer)
        {
            var env = new Environment();
            
            while (true)
            {
                writer.Write(Prompt);
                string line = reader.ReadLine();
                if (line == string.Empty)
                {
                    return;
                }
                
                var l = new Lexer(line);
                var p = new Parser(l);
                
                var code = p.ParseCode();
                if (p.Errors.Count != 0)
                {
                    PrintParserErrors(Console.Out, p.Errors);
                }

                var obj = Evaluation.Eval(code, env);
                if (obj != null)
                {
                    writer.WriteLine(obj.Inspect());
                }
            }
        }

        public static void PrintParserErrors(TextWriter writer, List<string> errors)
        {
            writer.WriteLine("Woops! We ran into some monkey business here!\n");
            writer.WriteLine("parser errors:\n");
            errors.ForEach(msg =>
            {
                writer.WriteLine($"\t{msg}");
            });
        }
    }
}