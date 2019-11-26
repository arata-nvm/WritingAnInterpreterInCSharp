using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Monkey.Core
{
    public class Repl
    {
        public static readonly string Prompt = ">> ";
        public static readonly string BlockPrompt = ".. ";

        public static Environment Exec(string fileName)
        {
            var env = new Environment();
            
            if (!File.Exists(fileName))
            {
                Console.WriteLine($"File not found:{fileName}");
                return env;
            }

            var b = File.ReadAllText(fileName);
            
            var l = new Lexer(b);
            var p = new Parser(l);

            var code = p.ParseCode();
            if (p.Errors.Count != 0)
            {
                PrintParserErrors(Console.Out, p.Errors);
            }

            Evaluation.Eval(code, env);
            return env;
        }

        public static void Start(TextReader reader, TextWriter writer, Environment env)
        {
            if (env == null)
                env = new Environment();
            
            while (true)
            {
                writer.Write(Prompt);
                var line = reader.ReadLine();
                
                if (line == string.Empty)
                    return;
                
                while (CountOf(line, '{') - CountOf(line, '}') > 0)
                {
                    writer.Write(BlockPrompt);
                    line += reader.ReadLine();
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

        private static void PrintParserErrors(TextWriter writer, List<string> errors)
        {
            writer.WriteLine("Woops! We ran into some monkey business here!\n");
            writer.WriteLine("parser errors:\n");
            errors.ForEach(msg =>
            {
                writer.WriteLine($"\t{msg}");
            });
        }
        
        private static int CountOf(string self, char charToCount)
        {
            return self.Count(c => c == charToCount);
        }
    }
}