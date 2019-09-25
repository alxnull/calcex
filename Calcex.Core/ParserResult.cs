using System;
using System.Collections.Generic;
using Calcex.Evaluation;
using Calcex.Parsing.Tokens;
using System.Linq;
using System.Linq.Expressions;

namespace Calcex
{
    /// <summary>
    /// Represents the result of an expression parsed by the math parser.
    /// </summary>
    public class ParserResult
    {
        private readonly Parser parser;
        private TreeToken treeToken;

        internal ParserResult(Parser parser, TreeToken treeToken)
        {
            this.parser = parser;
            this.treeToken = treeToken;
        }
        
        /// <summary>
        /// Evaluates the parsed expression with a given evaluator.
        /// </summary>
        /// <param name="options">Specifies the used evaluation options.</param>
        /// <typeparam name="T">The type of the evaluation result.</typeparam>
        /// <typeparam name="U">The type of the evaluator to be used.</typeparam>
        /// <returns>The evaluated value of type T.</returns>
        public T Evaluate<T,U>(EvaluationOptions options = null) where U : Evaluator<T>
        {
            var context = new EvaluationContext(options ?? new EvaluationOptions(), null);
            Evaluator<T> eval = (Evaluator<T>)Activator.CreateInstance(typeof(U), parser, context);
            return treeToken.Evaluate(eval);
        }
        
        /// <summary>
        /// Evaluates the value of the parsed expression as double.
        /// </summary>
        /// <param name="options">Specifies the used evaluation options.</param>
        /// <returns>The evaluated value as double.</returns>
        public double Evaluate(EvaluationOptions options = null)
        {
            return Evaluate<double, DoubleEvaluator>(options);
        }

        /// <summary>
        /// Evaluates the value of the parsed expression as decimal.
        /// </summary>
        /// <param name="options">Specifies the used evaluation options.</param> 
        /// <returns>The evaluated value as decimal.</returns>
        public decimal EvaluateDecimal(EvaluationOptions options = null)
        {
            return Evaluate<decimal, DecimalEvaluator>(options);
        }

        /// <summary>
        /// Evaluates the value of the parsed expression as boolean.
        /// </summary>
        /// <param name="options">Specifies the used evaluation options.</param>
        /// <returns>The evaluated value as boolean.</returns>
        public bool EvaluateBool(EvaluationOptions options = null)
        {
            var result = Evaluate<double, DoubleEvaluator>(options);
            double epsilon = options?.DoubleCompareEpsilon ?? 0;
            if (Math.Abs(result - 1) <= epsilon) return true;
            else if (Math.Abs(result - 0) <= epsilon) return false;
            else throw new EvaluationException($"Cannot convert '{result}' to boolean.");
        }

        /// <summary>
        /// Compiles the parsed expression into an executable delegate with one parameter.
        /// </summary>
        /// <param name="param1">The name of the variable used as first parameter.</param>
        /// <param name="options">Specifies the used evaluation options.</param>
        /// <returns>The compiled delegate for the parsed expression.</returns>
        public Func<double, double> Compile(string param1,
            EvaluationOptions options = null)
        {
            var paramColl = new ParameterCollection<double>(param1);
            Expression expr = evalDelegate(options, paramColl);
            var parameters = paramColl.GetParameters();
            return Expression.Lambda<Func<double, double>>(expr, parameters).Compile();
        }

        /// <summary>
        /// Compiles the parsed expression into an executable delegate with two parameters.
        /// </summary>
        /// <param name="param1">The name of the variable used as first parameter.</param>
        /// <param name="param2">The name of the variable used as second parameter.</param>
        /// <param name="options">Specifies the used evaluation options.</param>
        /// <returns>The compiled delegate for the parsed expression.</returns>
        public Func<double, double, double> Compile(string param1, string param2,
            EvaluationOptions options = null)
        {
            var paramColl = new ParameterCollection<double>(param1, param2);
            Expression expr = evalDelegate(options, paramColl);
            var parameters = paramColl.GetParameters();
            return Expression.Lambda<Func<double, double, double>>(expr, parameters).Compile();
        }

        /// <summary>
        /// Compiles the parsed expression into an executable delegate with three parameters.
        /// </summary>
        /// <param name="param1">The name of the variable used as first parameter.</param>
        /// <param name="param2">The name of the variable used as second parameter.</param>
        /// <param name="param3">The name of the variable used as third parameter.</param>
        /// <param name="options">Specifies the used evaluation options.</param>
        /// <returns>The compiled delegate for the parsed expression.</returns>
        public Func<double, double, double, double> Compile(string param1, string param2, string param3,
            EvaluationOptions options = null)
        {
            var paramColl = new ParameterCollection<double>(param1, param2, param3);
            Expression expr = evalDelegate(options, paramColl);
            var parameters = paramColl.GetParameters();
            return Expression.Lambda<Func<double, double, double, double>>(expr, parameters).Compile();
        }

        /// <summary>
        /// Compiles the parsed expression into an executable delegate taking an array as parameter.
        /// </summary>
        /// <param name="paramVars">An array of variavles names used as parameters.</param>
        /// <param name="options">Specifies the used evaluation options.</param>
        /// <returns>The compiled delegate taking values for the given variables in the specified order.</returns>
        public ArrayFunc Compile(string[] paramVars, EvaluationOptions options = null)
        {
            var paramColl = new IndexedParameterExpression(paramVars);
            Expression expr = evalDelegate(options, paramColl);
            var parameters = paramColl.GetParameters();
            return Expression.Lambda<ArrayFunc>(expr, parameters).Compile();
        }

        private Expression evalDelegate(EvaluationOptions options, IParameterCollection paramColl)
        {
            var context = new EvaluationContext(options ?? new EvaluationOptions(), paramColl);
            var eval = new ExpressionTreeEvaluator(parser, context);
            return treeToken.Evaluate(eval);
        }

        /// <summary>
        /// Evaluates a list of double values by setting the specified variable to each of the given argument values.
        /// </summary>
        /// <param name="variable">The argument variable for which the list is created.</param>
        /// <param name="start">The start index of the argument values.</param>
        /// <param name="length">The number of list items to be evaluated.</param>
        /// <param name="options">Specifies the used evaluation options.</param>
        /// <returns>A dictionary containing the given values and the evaluated results.</returns>
        public Dictionary<double, double> EvaluateList(string variable, int start, int length, EvaluationOptions options = null)
        {
            return EvaluateList(variable, Enumerable.Range(start, length).Select(x => (double)x), options);
        }

        /// <summary>
        /// Evaluates a list of double values by setting the specified variable to each of the given argument values.
        /// </summary>
        /// <param name="variable">The argument variable for which the list is created.</param>
        /// <param name="values">A collection of argument values.</param>
        /// <param name="options">Specifies the used evaluation options.</param>
        /// <returns>A dictionary containing the given values and the evaluated results.</returns>
        public Dictionary<double, double> EvaluateList(string variable, IEnumerable<double> values, EvaluationOptions options = null)
        {
            var context = new EvaluationContext(options ?? new EvaluationOptions(), null);
            var func = Compile(variable, options);
            return values.ToDictionary(v => v, v => func(v));
        }

        /// <summary>
        /// Gets the expression string of the parsed expression.
        /// </summary>
        /// <returns>The expression of the ParserResult as string.</returns>
        public string GetExpression() => Evaluate<string, StringEvaluator>();

        /// <summary>
        /// Gets the expression string of the parsed expression in postfix notation.
        /// </summary>
        /// <returns>The expression of the ParserResult as string in postfix notation.</returns>
        public string GetPostfixExpression() => Evaluate<string, PostfixStringEvaluator>();
    }
}
