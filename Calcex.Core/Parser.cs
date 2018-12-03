using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Bluegrams.Calcex.Parsing;
using Bluegrams.Calcex.Parsing.Tokens;

namespace Bluegrams.Calcex
{
    /// <summary>
    /// A simple math parser for parsing and evaluating mathematical expressions.
    /// </summary>
    public class Parser
    {
        internal Tokenizer Tokenizer;
        private SeparatorStyle separatorStyle = SeparatorStyle.Dot;

        /// <summary>
        /// An array of all user-added variables.
        /// </summary>
        public string[] CustomVariables { get { return variables.Keys.ToArray(); }}

        private Dictionary<string, object> variables;

        /// <summary>
        /// An array of all user-added function symbols.
        /// </summary>
        public string[] CustomFunctions { get { return FunctionsDict.Keys.ToArray(); } }

        internal Dictionary<string, MathFunc> FunctionsDict { get; private set; }

        /// <summary>
        /// Gets or sets the separator style used for this parser.
        /// </summary>
        public SeparatorStyle SeparatorStyle
        {
            get { return separatorStyle; }
            set
            {
                Tokenizer.SetSeparatorStyle(value);
                separatorStyle = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the Parser class.
        /// </summary>
        public Parser()
        {
            Tokenizer = new Tokenizer();
            Tokenizer.SetSeparatorStyle(SeparatorStyle);
            variables = new Dictionary<string, object>();
            FunctionsDict = new Dictionary<string, MathFunc>();
        }

        /// <summary>
        /// Initializes a new instance of the Parser class with a list of variables.
        /// </summary>
        /// <param name="variables">A list of variables names.</param>
        public Parser(params string[] variables) : this()
        {
            foreach (string va in variables)
                AddVariable(va);
        }

        /// <summary>
        /// Parses a mathematical expression.
        /// </summary>
        /// <param name="input">The expression string to be parsed.</param>
        /// <returns>The result of the parsing as an instance of ParserResult.</returns>
        public ParserResult Parse(string input)
        {
            if (String.IsNullOrWhiteSpace(input)) return new ParserResult(this, new NumberToken("0", 0));
            //Read
            var tokenList = Tokenizer.Read(new InputStream(input));
            // Build tree
            TreeToken treeToken = ParseTree.CreateTree(tokenList);
            return new ParserResult(this, treeToken);
        }

        /// <summary>
        /// Checks if the given name is already used as a variable, function or operator.
        /// </summary>
        /// <param name="name">The name to be checked.</param>
        /// <returns>true if the given name is already defined, otherwise false.</returns>
        public bool IsDefined(string name)
        {
            return Tokenizer.TokenDict.ContainsKey(name);
        }

        /// <summary>
        /// Checks if the given string represents a valid parser item.
        /// </summary>
        /// <param name="input">The string item to be checked.</param>
        /// <returns>true if the item is valid, otherwise false.</returns>
        public bool IsValidItem(string input)
        {
            input = input.Replace(" ", "");
            if (String.IsNullOrEmpty(input)) return true;
            bool valid = Tokenizer.IsValidNumber(input);
            valid |= IsDefined(input);
            valid |= input == Tokenizer.ArgumentSeparator.ToString() || input == Tokenizer.DecimalSeparator.ToString();
            return valid;
        }

        /// <summary>
        /// Checks if a given mathematical expression is valid for the parser.
        /// </summary>
        /// <param name="input">The expression string to be checked.</param>
        /// <returns>true if the expression is valid, otherwise false.</returns>
        public bool IsValid(string input)
        {
            Exception o;
            return IsValid(input, out o);
        }

        /// <summary>
        /// Checks if a given mathematical expression is valid for the parser.
        /// </summary>
        /// <param name="input">The expression string to be checked.</param>
        /// <param name="ex">Gives out an exception if the expression is invalid, otherwise null.</param>
        /// <returns>true if the expression is valid, otherwise false.</returns>
        public bool IsValid(string input, out Exception ex)
        {
            ex = null;
            if (input.Length == 0) return true;
            try
            {
                var tokenList = Tokenizer.Read(new InputStream(input));
                ParseTree.CreateTree(tokenList);
            }
            catch (Exception e)
            {
                ex = e;
                return false;
            }
            return true;
        }
        
        #region Variables

        private bool var_match(string input)
        {
            if (IsDefined(input) && !variables.ContainsKey(input)) return false;
            Regex reg = new Regex(@"^[a-zA-Z]+[0-9]*");
            if (reg.Match(input).Length == input.Length)
            {
                return true;
            }
            else return false;
        }

        /// <summary>
        /// Defines a custom variable for the parser.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        public void AddVariable(string name)
        {
            if (variables.ContainsKey(name) || !var_match(name))
                throw new ArgumentException($"Given variable name '{name}' is either invalid or already exists.");
            variables.Add(name, null);
            Tokenizer.TokenDict.Add(name, new Tuple<Type, int>(typeof(VarToken), 0));
        }

        /// <summary>
        /// Returns the currently set value of a custom variable.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <returns>The value of the variable or null if the variable if not defined.</returns>
        public object GetVariable(string name)
        {
            variables.TryGetValue(name, out object value);
            return value;
        }

        /// <summary>
        /// Sets a custom variable to a new value or adds a new variable.
        /// </summary>
        /// <param name="name">The name of the variable to be set.</param>
        /// <param name="value">An object that implements the IConvertible interface.</param>
        public void SetVariable(string name, object value)
        {
            if (!var_match(name))
                throw new ArgumentException($"Given variable name '{name}' is invalid.");
            variables[name] = value;
            if (!IsDefined(name))
                Tokenizer.TokenDict.Add(name, new Tuple<Type, int>(typeof(VarToken), 0));
        }

        /// <summary>
        /// Removes a custom variable.
        /// </summary>
        /// <param name="name">The name of the custom variable to be removed.</param>
        public void RemoveVariable(string name)
        {
            if (!variables.ContainsKey(name))
                throw new ArgumentException($"No variable named '{name}' was found.");
            Tokenizer.TokenDict.Remove(name);
            variables.Remove(name);
        }

        /// <summary>
        /// Removes all custom variables.
        /// </summary>
        public void RemoveAllVariables()
        {
            foreach (var variable in variables.Keys)
            {
                Tokenizer.TokenDict.Remove(variable);
            }
            variables.Clear();
        }

        #endregion

        #region Functions

        private void add_MathFunc(string name, MathFunc func, int paramNum = -1)
        {
            if (variables.ContainsKey(name) || !var_match(name))
                throw new ArgumentException($"Given function name '{name}' is either invalid or already exists.");
            FunctionsDict.Add(name, func);
            Tokenizer.TokenDict.Add(name, new Tuple<Type, int>(typeof(CallerFuncToken), paramNum));
        }

        /// <summary>
        /// Adds a custom function with one parameter.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="method">The delegate of the functions method with one parameter.</param>
        public void AddOneParamFunction(string name, Func<double, double> method)
        {
            add_MathFunc(name, v => method(v[0]), 1);
        }

        /// <summary>
        /// Adds a custom function with two parameters.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="method">The delegate of the function's method with two parameters.</param>
        public void AddTwoParamFunction(string name, Func<double, double, double> method)
        {
            add_MathFunc(name, v => method(v[0], v[1]), 2);
        }

        /// <summary>
        /// Adds a custom function with three parameters.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="method">The delegate of the function's method with three parameters.</param>
        public void AddThreeParamFunction(string name, Func<double, double, double, double> method)
        {
            add_MathFunc(name, v => method(v[0], v[1], v[2]), 3);
        }

        /// <summary>
        /// Adds a custom function with multiple parameters.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="method">The MathFunc-delegate of the function's method.</param>
        public void AddMultiParamFunction(string name, MathFunc method)
        {
            add_MathFunc(name, method);
        }

        /// <summary>
        /// Adds a custom function with a mathematical expression string.
        /// </summary>
        /// <param name="name">The name of the function.</param>
        /// <param name="expression">The method of the function as expression string</param>
        /// <param name="args">A list of the names of the function's arguments.</param>
        public void AddFunction(string name, string expression, params string[] args)
        {
            if (variables.ContainsKey(name) || !var_match(name)) 
                throw new ArgumentException($"Given function name '{name}' is either invalid or already exists.");
            Tokenizer.TokenDict.Add(name, new Tuple<Type, int>(typeof(CallerFuncToken), args.Length));
            Parser funcParser = new Parser(args);
            try 
            {
                ParserResult funcResult = funcParser.Parse(expression);
                MathFunc func = delegate (double[] v)
                {
                    for (int i = 0; i < args.Length; i++)
                        funcParser.SetVariable(args[i], v[i]);
                    return funcResult.Evaluate();
                };
                FunctionsDict.Add(name, func);
            }
            catch (Exception ex)
            { 
                throw new InvalidFunctionException(name, ex); 
            }
        }

        /// <summary>
        /// Removes a custom function.
        /// </summary>
        /// <param name="name">The name of the custom function to be removed.</param>
        public void RemoveFunction(string name)
        {
            if (!FunctionsDict.Keys.Contains(name)) 
                throw new ArgumentException($"No function named '{name}' was found.");
            Tokenizer.TokenDict.Remove(name);
            FunctionsDict.Remove(name);
        }

        /// <summary>
        /// Removes all custom functions.
        /// </summary>
        public void RemoveAllFunctions()
        {
            foreach (string func in FunctionsDict.Keys)
            {
                Tokenizer.TokenDict.Remove(func);
            }
            FunctionsDict.Clear();
        }
        #endregion

        #region "Shortcut methods"
        /// <summary>
        /// A static method that parses and evaluates a given expression string in one step.
        /// </summary>
        /// <param name="input">An expression string to be parsed and evaluated.</param>
        /// <returns>The result of the evaluation as double.</returns>
        public static double Evaluate(string input)
        {
            return Evaluate(input, SeparatorStyle.Dot);
        }

        /// <summary>
        /// A static method that parses and evaluates a given expression string in one step.
        /// </summary>
        /// <param name="input">An expression string to be parsed and evaluated.</param>
        /// <param name="separator">The separator style to used to parse the expression.</param>
        /// <returns>The result of the evaluation as double.</returns>
        public static double Evaluate(string input, SeparatorStyle separator)
        {
            var parser = new Parser();
            parser.SeparatorStyle = separator;
            return parser.Parse(input).Evaluate();
        }

        /// <summary>
        /// Tries to parse and evaluate a given expression string in one step.
        /// </summary>
        /// <param name="input">An expression string to be parsed and evaluated.</param>
        /// <param name="result">Holds the result of the evaluation if it was successful, otherwiese NaN.</param>
        /// <returns>true if parsing and evaluating were succesful and the result is valid, otherwise false.</returns>
        public static bool TryEvaluate(string input, out double result)
        {
            result = 0;
            return TryEvaluate(input, SeparatorStyle.Dot, out result);
        }

        /// <summary>
        /// Tries to parse and evaluate a given expression string in one step.
        /// </summary>
        /// <param name="input">An expression string to be parsed and evaluated.</param>
        /// <param name="separator">The separator style to used to parse the expression.</param>
        /// <param name="result">Holds the result of the evaluation if it was successful, otherwiese NaN.</param>
        /// <returns>true if parsing and evaluating were succesful and the result is valid, otherwise false.</returns>
        public static bool TryEvaluate(string input, SeparatorStyle separator, out double result)
        {
            var parser = new Parser();
            parser.SeparatorStyle = separator;
            try
            {
                var parserResult = parser.Parse(input);
                result = parserResult.Evaluate();
            }
            catch
            {
                result = double.NaN;
                return false;
            }
            if (double.IsNaN(result)) return false;
            else return true;
        }
        #endregion
    }

    /// <summary>
    /// A delegate for a method calculating a result out of a given array of doubles.
    /// </summary>
    /// <param name="args">An array of doubles given as input.</param>
    /// <returns>A double value.</returns>
    public delegate double MathFunc(double[] args);
}
