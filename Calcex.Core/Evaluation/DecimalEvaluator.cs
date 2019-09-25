using System;
using System.Linq;
using Calcex.Parsing;
using Calcex.Parsing.Tokens;
using System.Collections.Generic;
using Calcex.Numerics;

namespace Calcex.Evaluation
{
    /// <summary>
    /// Evaluates an expression to a decimal value.
    /// </summary>
    public class DecimalEvaluator : Evaluator<decimal>
    {
        // not supported: Power, Exp, Sqrt, Cbrt, Sin, Cos, Tan, Abs, Log10, LogE, ASin, ACos, ATan, Log, Sinh, Cosh, Tanh, 
        // UnsignedRightShift, Sum, Prod

        // Use DoubleEvaluator and cast to decimal if operation not supported.
        DoubleEvaluator fallback;

        public DecimalEvaluator(Parser parser, EvaluationContext context) : base(parser, context)
        {
            fallback = new DoubleEvaluator(parser, context);
        }

        public static Dictionary<string, decimal> ConstantsDict = new Dictionary<string, decimal>()
        {
            {ParserSymbols.Pi, 3.1415926535897932384626433833m },
            {ParserSymbols.E,  2.7182818284590452353602874713m },
            {ParserSymbols.True, 1},
            {ParserSymbols.False, 0}
        };

        private decimal eval(TreeToken token) => token.Evaluate(this);

        public override decimal EvaluateOperator(OperatorToken token)
        {
            switch (token.Symbol)
            {
                // Arithmetic operators
                case ParserSymbols.Add:
                    return eval(token.SubTokens[0]) + eval(token.SubTokens[1]);
                case ParserSymbols.Subtract:
                    return eval(token.SubTokens[0]) - eval(token.SubTokens[1]);
                case ParserSymbols.Multiply:
                    return eval(token.SubTokens[0]) * eval(token.SubTokens[1]);
                case ParserSymbols.Divide:
                    decimal divisor = eval(token.SubTokens[1]);
                    if (divisor == 0) throw new ParserArithmeticException(token.Position);
                    return eval(token.SubTokens[0]) / divisor;
                case ParserSymbols.Modulo:
                    return eval(token.SubTokens[0]) % eval(token.SubTokens[1]);
                // Comparison operators
                case ParserSymbols.Equal:
                    return eval(token.SubTokens[0]) == eval(token.SubTokens[1]) ? 1 : 0;
                case ParserSymbols.LessThan:
                    return eval(token.SubTokens[0]) < eval(token.SubTokens[1]) ? 1 : 0;
                case ParserSymbols.GreaterThan:
                    return eval(token.SubTokens[0]) > eval(token.SubTokens[1]) ? 1 : 0;
                case ParserSymbols.LessEqual:
                    return eval(token.SubTokens[0]) <= eval(token.SubTokens[1]) ? 1 : 0;
                case ParserSymbols.GreaterEqual:
                    return eval(token.SubTokens[0]) >= eval(token.SubTokens[1]) ? 1 : 0;
                case ParserSymbols.NotEqual:
                    return eval(token.SubTokens[0]) != eval(token.SubTokens[1]) ? 1 : 0;
                // Boolean/ bitwise operators
                case ParserSymbols.And:
                    var andRes = Convert.ToBoolean(eval(token.SubTokens[0])) && Convert.ToBoolean(eval(token.SubTokens[1]));
                    return andRes ? 1 : 0;
                case ParserSymbols.Or:
                    var orRes = Convert.ToBoolean(eval(token.SubTokens[0])) || Convert.ToBoolean(eval(token.SubTokens[1]));
                    return orRes ? 1 : 0;
                case ParserSymbols.BitwiseAnd:
                    return (long)eval(token.SubTokens[0]) & (long)eval(token.SubTokens[1]);
                case ParserSymbols.BitwiseOr:
                    return (long)eval(token.SubTokens[0]) | (long)eval(token.SubTokens[1]);
                case ParserSymbols.BitwiseXor:
                    return (long)eval(token.SubTokens[0]) ^ (long)eval(token.SubTokens[1]);
                case ParserSymbols.LeftShift:
                    return (long)eval(token.SubTokens[0]) << (int)eval(token.SubTokens[1]);
                case ParserSymbols.RightShift:
                    return (long)eval(token.SubTokens[0]) >> (int)eval(token.SubTokens[1]);
                default:
                    if (Context.Options.StrictMode) throw new UnsupportedOperationException(token.Symbol);
                    double result = token.Evaluate(fallback);
                    if (double.IsNaN(result)) throw new ParserArithmeticException(token.Position);
                    else return (decimal)result;
            }
        }

        public override decimal EvaluateFunction(FuncToken token)
        {
            if (token is CallerFuncToken)
                return (decimal)parser.FunctionsDict[token.Symbol].Invoke(fallback.EvaluateTokens((token).SubTokens.ToArray()));
            switch (token.Symbol)
            {
                case ParserSymbols.Negate:
                    return eval(token.SubTokens[0]) * -1;
                case ParserSymbols.Rad:
                    return eval(token.SubTokens[0]) * (decimal)Math.PI / 180;
                case ParserSymbols.Deg:
                    return eval(token.SubTokens[0]) * 180 / (decimal)Math.PI;
                case ParserSymbols.Ceil:
                    return Math.Ceiling(eval(token.SubTokens[0]));
                case ParserSymbols.Floor:
                    return Math.Floor(eval(token.SubTokens[0]));
                case ParserSymbols.Round:
                    return Math.Round(eval(token.SubTokens[0]));
                case ParserSymbols.Sign:
                    return Math.Sign(eval(token.SubTokens[0]));
                case ParserSymbols.Trunc:
                    return Math.Truncate(eval(token.SubTokens[0]));
                case ParserSymbols.Min:
                    return EvaluateTokens(token.SubTokens.ToArray()).Min();
                case ParserSymbols.Max:
                    return EvaluateTokens(token.SubTokens.ToArray()).Max();
                case ParserSymbols.Avg:
                    return EvaluateTokens(token.SubTokens.ToArray()).Average();
                case ParserSymbols.AndFunc:
                    return EvaluateTokens(token.SubTokens.ToArray())
                        .Select(v1 => Convert.ToBoolean(v1))
                        .Aggregate((v1, v2) => v1 & v2) ? 1 : 0;
                case ParserSymbols.OrFunc:
                    return EvaluateTokens(token.SubTokens.ToArray())
                        .Select(v1 => Convert.ToBoolean(v1))
                        .Aggregate((v1, v2) => v1 | v2) ? 1 : 0;
                case ParserSymbols.XorFunc:
                    return EvaluateTokens(token.SubTokens.ToArray())
                        .Select(v1 => Convert.ToBoolean(v1))
                        .Aggregate((v1, v2) => v1 ^ v2) ? 1 : 0;
                case ParserSymbols.Not:
                    return !Convert.ToBoolean(eval(token.SubTokens[0])) ? 1 : 0;
                case ParserSymbols.Fact:
                    decimal v = eval(token.SubTokens[0]);
                    if (v < 0 || v % 1 != 0) throw new ParserArithmeticException(token.Position);
                    decimal fact = 1;
                    for (int i = (int)v; i > 0; i--)
                        fact *= i;
                    return fact;          
                case ParserSymbols.Cond:
                    return Convert.ToBoolean(eval(token.SubTokens[0]))
                        ? eval(token.SubTokens[1])
                        : eval(token.SubTokens[2]);
                default:
                    if (Context.Options.StrictMode) throw new UnsupportedOperationException(token.Symbol);
                    double result = token.Evaluate(fallback);
                    if (double.IsNaN(result)) throw new ParserArithmeticException(token.Position);
                    else return (decimal)result;
            }
        }

        public override decimal EvaluateVarFunction(VarFuncToken token)
        {
            if (Context.Options.StrictMode) throw new UnsupportedOperationException(token.Symbol);
            double result = token.Evaluate(fallback);
            if (double.IsNaN(result)) throw new ParserArithmeticException(token.Position);
            else return (decimal)result;
        }

        public override decimal EvaluateNumber(NumberToken token)
        {
            return decimal.Parse(token.Symbol, parser.Tokenizer.NumberFormat);
        }

        public override decimal EvaluateVar(VarToken token)
        {
            object val = parser.GetVariable(token.Symbol);
            if (val == null) throw new ParserUnassignedVariableException(token.Symbol);
            else if (val is BigDecimal) return (decimal)(BigDecimal)val;
            else return Convert.ToDecimal(val);
        }

        public override decimal EvaluateConstant(ConstantToken token) => ConstantsDict[token.Symbol];
    }
}
