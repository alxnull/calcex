using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Bluegrams.Calcex;
using Bluegrams.Calcex.Parsing;
using System.IO;
using System.Globalization;

namespace Bluegrams.Calcex.CalcexConsole
{
    enum EvalMode
    {
        Double = 'd',
        Decimal = 'm',
        Bool = 'b',
        BigDecimal = 'g'
    }

    class Program
    {
        private static bool interactive;
        private static EvalMode EvalMode = EvalMode.Double;
        private static Parser parser = new Parser();
        private static int count = 0;

        private static CommandParser commandParser = new CommandParser(new Command[] {
            new Command("help", null, command_help, "Displays this help text."),
            new Command("ref", null, command_ref, "Prints a list of all operators, functions and symbols currently supported by the parser."),
            new Command("load", new[] {"FILE"}, command_load, "Loads commands from a file and executes them."),
            new Command("eval", new[] {"EXPR"}, command_eval, "Evaluates the result of an expression."),
            new Command("postfix", new[] {"EXPR"}, command_postfix, "Converts an expression to postfix notation."),
            new Command("conv", new[] {"NUMBER", "FROM", "TO"}, command_conv, "Converts a number from one base to another."),
            new Command("list", new[] {"EXPR", "INDEX", "START", "LEN"}, command_list,
                    "Evaluates a list of values for an expression by using an index variable, a start index and a length."),
            new Command("mode", new[] {"MODE"}, command_mode, "Sets the evaluation mode: d (double), m (decimal), b (bool), g (big decimal).", true),
            new Command("func", new[] {"NAME", "EXPR", "PARAMS"}, command_func,
                    "Defines a function by giving a name, an expression and a comma-separated parameter list.", true),
            new Command("set", new[] {"NAME", "VAL"}, command_set, "Sets/Adds a variable with name and (optionally) a value.", true) {MinArgs=1},
            new Command("unset", new[] {"NAME"}, command_unset, "Removes a previously set variable or function definition.", true),
            new Command("exit", null, command_exit, "Exits the console.", true)
        });

        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                try
                {
                    commandParser.ParseCommand(args, isInteractive: false);
                }
                catch (ParserException ex)
                {
                    Console.WriteLine("Parser exception: {0}", ex.Message);
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine("Error: {0}", ex.Message);
                }
            }
            else
            {
                interactive = true;
                printVersion();
                Console.WriteLine("\"#help\" for help; \"#exit\" to exit");
                loop();
            }
        }

        static void loop()
        {
            string input;
            while (true)
            {
                Console.Write(">> ");
                input = Console.ReadLine().Trim();
                processStatement(input);
            }
        }

        static void processStatement(string input)
        {
            if (input.StartsWith("#")) input = input.Substring(1);
            else input = $"eval {input}";
            try
            {
                commandParser.ParseCommand(input);
            }
            catch (ParserException ex)
            {
                Console.WriteLine("Parser exception: {0}", ex.Message);
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
            }
        }

        static void printVersion()
        {
            string consoleVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            string calcexVersion = Assembly.GetAssembly(typeof(Parser)).GetName().Version.ToString();
            Console.WriteLine("Calcex Console v.{0} [using Calcex.Core v.{1}]", consoleVersion, calcexVersion);
        }

        static void command_help(string[] __)
        {
            printVersion();
            Console.WriteLine();
            var commands = commandParser.Commands
                                        .OrderBy(v => v.Name)
                                        .Where(v => interactive || !v.InteractiveOnly);
            foreach (var command in commands)
            {
                Console.WriteLine("{2}{0,-35}{1}", command, command.HelpText, interactive ? "#" : "");
            }
        }

        static void command_load(string[] args)
        {
            if (!File.Exists(args[0]))
            {
                Console.WriteLine($"Error: File {args[0]} does not exist.");
                return;
            }
            interactive = true;
            foreach (string line in File.ReadAllLines(args[0]))
            {
                processStatement(line.Trim());
            }
        }

        static void command_eval(string[] args)
        {
            var result = evaluate(args[0]);
            if (interactive)
            {
                parser.SetVariable($"r{count}", result);
                Console.WriteLine($"r{count} := {result}");
                count++;
            }
            else Console.WriteLine(result);
        }

        static object evaluate(string expression)
        {
            var res = parser.Parse(expression);
            switch (EvalMode)
            {
                case EvalMode.Decimal:
                    return res.EvaluateDecimal();
                case EvalMode.BigDecimal:
                    return res.EvaluateBigDecimal();
                case EvalMode.Bool:
                    return res.EvaluateBool();
                default:
                    return res.Evaluate();
            }
        }

        static void command_mode(string[] args) => EvalMode = (EvalMode)char.Parse(args[0]);

        static void command_ref(string[] __)
        {
            string refString = String.Empty;
            foreach (var keyValue in ParserSymbols.GetParserSymbols())
            {
                refString += String.Format("{0,-7}{1}\n", keyValue.Key, keyValue.Value?.Description);
            }
            Console.Write(refString);
        }
        static void command_postfix(string[] args) => Console.WriteLine(parser.Parse(args[0]).GetPostfixExpression());

        static void command_set(string[] args)
        {
            if (args.Length == 1) parser.AddVariable(args[0]);
            else 
            {
                var val = double.Parse(args[1]);
                parser.SetVariable(args[0], val);
                Console.WriteLine($"{args[0]} := {val}");
            }
        }

        static void command_unset(string[] args)
        {
            if (parser.GetVariable(args[0]) != null) 
            {
                parser.RemoveVariable(args[0]);
            }
            else parser.RemoveFunction(args[0]);
        }

        static void command_func(string[] args)
        {
            string[] funcArgs = args[2].Split(',');
            parser.AddFunction(args[0], args[1], funcArgs);
        }

        static void command_list(string[] args)
        {
            bool removeIterator = false;
            if (!parser.IsDefined(args[1]))
            {
                parser.AddVariable(args[1]);
                removeIterator = true;
            }
            var dict = parser.Parse(args[0]).EvaluateList(args[1], int.Parse(args[2]), int.Parse(args[3]));
            if (removeIterator) parser.RemoveVariable(args[1]);
            foreach(var item in dict)
            {
                Console.WriteLine("{0,-10}{1}", item.Key, item.Value);
            }
        }

        static void command_conv(string[] args)
        {
            double base10 = Utils.ConvertToBase10(args[0], int.Parse(args[1]));
            Console.WriteLine(Utils.ConvertFromBase10((int)base10, int.Parse(args[2])));
        }

        static void command_exit(string[] __) => Environment.Exit(0);
    }
}