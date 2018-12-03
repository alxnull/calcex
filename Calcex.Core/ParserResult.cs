using System;
using System.Collections.Generic;
using Bluegrams.Calcex.Evaluation;
using Bluegrams.Calcex.Numerics;
using Bluegrams.Calcex.Parsing;
using Bluegrams.Calcex.Parsing.Tokens;
using System.Linq;

namespace Bluegrams.Calcex
{
    /// <summary>
    /// Represents the result of an expression parsed by the math parser.
    /// </summary>
    public class ParserResult
    {
        private Parser parser;
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
            Evaluator<T> eval = (Evaluator<T>)Activator.CreateInstance(typeof(U), parser, options ?? new EvaluationOptions());
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
        /// Evaluates the value of the parsed expression as BigDecimal.
        /// </summary>
        /// <param name="options">Specifies the used evaluation options.</param>
        /// <returns>The evaluated value as BigDecimal.</returns>
        public BigDecimal EvaluateBigDecimal(EvaluationOptions options = null)
        {
            return Evaluate<BigDecimal, BigDecimalEvaluator>(options);
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
            var evaluator = new DoubleEvaluator(parser, options ?? new EvaluationOptions());
            var results = new Dictionary<double, double>();
            foreach (double value in values)
            {
                parser.SetVariable(variable, value);
                var result = treeToken.Evaluate(evaluator);
                results.Add(value, result);
            }
            return results;
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
