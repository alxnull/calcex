using System;
using Calcex.Parsing.Tokens;
using Calcex.Parsing;
using System.Linq;
using System.Collections.Generic;
using Calcex.Numerics;

namespace Calcex.Evaluation
{
    /// <summary>
    /// Evaluates an expression to a double value.
    /// </summary>
    public class DoubleEvaluator : Evaluator<double>
    {
        public DoubleEvaluator(Parser parser, EvaluationContext context) : base(parser, context) {}

        public static Dictionary<string, double> ConstantsDict = new Dictionary<string, double>()
        {
            {ParserSymbols.Pi, Math.PI },
            {ParserSymbols.E, Math.E },
            {ParserSymbols.True, 1},
            {ParserSymbols.False, 0}
        };

        private double eval(TreeToken token) => token.Evaluate(this);

        public override double EvaluateOperator(OperatorToken token)
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
                    double divisor = eval(token.SubTokens[1]);
                    if (divisor == 0) return double.NaN;
                    return eval(token.SubTokens[0]) / divisor;
                case ParserSymbols.Power:
                    return Math.Pow(eval(token.SubTokens[0]), eval(token.SubTokens[1]));
                case ParserSymbols.Modulo:
                    return eval(token.SubTokens[0]) % eval(token.SubTokens[1]);
                // Comparison operators
                case ParserSymbols.Equal:
                    return Math.Abs(eval(token.SubTokens[0]) - eval(token.SubTokens[1])) <= Context.Options.DoubleCompareEpsilon ? 1 : 0;
                case ParserSymbols.LessThan:
                    return eval(token.SubTokens[0]) < eval(token.SubTokens[1]) - Context.Options.DoubleCompareEpsilon ? 1 : 0;
                case ParserSymbols.GreaterThan:
                    return eval(token.SubTokens[0]) > eval(token.SubTokens[1]) + Context.Options.DoubleCompareEpsilon ? 1 : 0;
                case ParserSymbols.LessEqual:
                    return eval(token.SubTokens[0]) <= eval(token.SubTokens[1]) + Context.Options.DoubleCompareEpsilon ? 1 : 0;
                case ParserSymbols.GreaterEqual:
                    return eval(token.SubTokens[0]) >= eval(token.SubTokens[1]) - Context.Options.DoubleCompareEpsilon ? 1 : 0;
                case ParserSymbols.NotEqual:
                    return Math.Abs(eval(token.SubTokens[0]) - eval(token.SubTokens[1])) > Context.Options.DoubleCompareEpsilon ? 1 : 0;
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
                case ParserSymbols.UnsignedRightShift:
                    return (uint)eval(token.SubTokens[0]) >> (int)eval(token.SubTokens[1]);
                default:
                    if (Context.Options.StrictMode) throw new UnsupportedOperationException(token.Symbol);
                    return double.NaN;
            }
        }

        public override double EvaluateFunction(FuncToken token)
        {
            if (token is CallerFuncToken)
                return parser.FunctionsDict[token.Symbol].Invoke(EvaluateTokens((token).SubTokens.ToArray()));
            switch (token.Symbol)
            {
                case ParserSymbols.Negate:
                    return eval(token.SubTokens[0]) * -1;
                case ParserSymbols.Sqrt:
                    return Math.Sqrt(eval(token.SubTokens[0]));
                case ParserSymbols.Cbrt:
                    return Math.Pow(eval(token.SubTokens[0]), (double)1 / 3);
                case ParserSymbols.Exp:
                    return Math.Exp(eval(token.SubTokens[0]));
                case ParserSymbols.Sin:
                    return Math.Sin(eval(token.SubTokens[0]));
                case ParserSymbols.Cos:
                    return Math.Cos(eval(token.SubTokens[0]));
                case ParserSymbols.Tan:
                    return Math.Tan(eval(token.SubTokens[0]));
                case ParserSymbols.Abs:
                    return Math.Abs(eval(token.SubTokens[0]));
                case ParserSymbols.Log10:
                    return Math.Log10(eval(token.SubTokens[0]));
                case ParserSymbols.LogE:
                    return Math.Log(eval(token.SubTokens[0]));
                case ParserSymbols.ASin:
                    return Math.Asin(eval(token.SubTokens[0]));
                case ParserSymbols.ACos:
                    return Math.Acos(eval(token.SubTokens[0]));
                case ParserSymbols.ATan:
                    return Math.Atan(eval(token.SubTokens[0]));
                case ParserSymbols.Sinh:
                    return Math.Sinh(eval(token.SubTokens[0]));
                case ParserSymbols.Cosh:
                    return Math.Cosh(eval(token.SubTokens[0]));
                case ParserSymbols.Tanh:
                    return Math.Tanh(eval(token.SubTokens[0]));
                case ParserSymbols.Log:
                    return Math.Log(eval(token.SubTokens[1]), eval(token.SubTokens[0]));
                case ParserSymbols.Rad:
                    return eval(token.SubTokens[0]) * Math.PI / 180;
                case ParserSymbols.Deg:
                    return eval(token.SubTokens[0]) * 180 / Math.PI;
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
                    double v = eval(token.SubTokens[0]);
                    if (v < 0 || v % 1 != 0) return double.NaN;
                    double fact = 1;
                    for (int i = (int)v; i > 0; i--)
                        fact *= i;
                    return fact;
                case ParserSymbols.Cond:
                    return Convert.ToBoolean(eval(token.SubTokens[0]))
                        ? eval(token.SubTokens[1])
                        : eval(token.SubTokens[2]);
                default:
                    if (Context.Options.StrictMode) throw new UnsupportedOperationException(token.Symbol);
                    return double.NaN;
            }
        }

        public override double EvaluateVarFunction(VarFuncToken token)
        {
            Func<double, double> func;
            int start;
            switch (token.Symbol)
            {
                case ParserSymbols.Sum:
                    func = ExpressionTreeEvaluator.CreateDelegate(token.SubTokens[3],
                        token.VariableSymbol, parser, Context.Options);
                    start = (int)eval(token.SubTokens[1]);
                    return Enumerable.Range(start, (int)eval(token.SubTokens[2]) - start + 1)
                                .Select(v => func(v)).Sum(); 
                case ParserSymbols.Prod:
                    func = ExpressionTreeEvaluator.CreateDelegate(token.SubTokens[3],
                        token.VariableSymbol, parser, Context.Options);
                    start = (int)eval(token.SubTokens[1]);
                    return Enumerable.Range(start, (int)eval(token.SubTokens[2]) - start + 1)
                                .Select(v => func(v))
                                .Aggregate((v1, v2) => v1 * v2);
                default:
                    if (Context.Options.StrictMode) throw new UnsupportedOperationException(token.Symbol);
                    return double.NaN;
            }
        }

        public override double EvaluateNumber(NumberToken token)
        {
            return double.Parse(token.Symbol, parser.Tokenizer.NumberFormat);
        }

        public override double EvaluateVar(VarToken token)
        {
            object val = parser.GetVariable(token.Symbol);
            if (val == null) throw new ParserUnassignedVariableException(token.Symbol);
            else if (val is BigDecimal) return (double)(BigDecimal)val;
            else return Convert.ToDouble(val);
        }

        public override double EvaluateConstant(ConstantToken token) => ConstantsDict[token.Symbol];
    }
}
