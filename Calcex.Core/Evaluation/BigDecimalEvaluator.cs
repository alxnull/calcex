using System;
using Bluegrams.Calcex.Numerics;
using Bluegrams.Calcex.Parsing;
using Bluegrams.Calcex.Parsing.Tokens;

namespace Bluegrams.Calcex.Evaluation
{
    /// <summary>
    /// WARNING: This evaluator is experimental.
    /// </summary>
    public class BigDecimalEvaluator : Evaluator<BigDecimal>
    {
        // not supported: Power, Sqrt, Cbrt, Sin, Cos, Tan, Abs, Log10, LogE, ASin, ACos, ATan, Log, Ceil, Round, Sign, Trunc, Min, Max, Avg,
        // Sinh, Cosh, Tanh, And, Or, Xor, UnsignedRightShift

        // Use DoubleEvaluator and cast to BigDecimal if operation not supported.
        DoubleEvaluator fallback;

        public BigDecimalEvaluator(Parser parser, EvaluationOptions options) : base(parser, options)
        {
            fallback = new DoubleEvaluator(parser, options);
        }

        private BigDecimal eval(TreeToken token) => token.Evaluate(this);

        public override BigDecimal EvaluateOperator(OperatorToken token)
        {
            switch (token.Symbol)
            {
                case ParserSymbols.Add:
                    return eval(token.SubTokens[0]) + eval(token.SubTokens[1]);
                case ParserSymbols.Subtract:
                    return eval(token.SubTokens[0]) - eval(token.SubTokens[1]);
                case ParserSymbols.Multiply:
                    return eval(token.SubTokens[0]) * eval(token.SubTokens[1]);
                case ParserSymbols.Divide:
                    BigDecimal divisor = eval(token.SubTokens[1]);
                    if (divisor == 0) return double.NaN;
                    return eval(token.SubTokens[0]) / divisor;
                case ParserSymbols.Modulo:
                    return eval(token.SubTokens[0]) % eval(token.SubTokens[1]);
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
                case ParserSymbols.And:
                    var andRes = eval(token.SubTokens[0]) != 0 && eval(token.SubTokens[1]) != 0;
                    return andRes ? 1 : 0;
                case ParserSymbols.Or:
                    var orRes = eval(token.SubTokens[0]) != 0 || eval(token.SubTokens[1]) != 0;
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
                    if (options.StrictMode) throw new UnsupportedOperationException(token.Symbol);
                    double result = token.Evaluate(fallback);
                    if (double.IsNaN(result)) throw new ParserArithmeticException(token.Position);
                    else return (BigDecimal)result;
            }
        }

        public override BigDecimal EvaluateFunction(FuncToken token)
        {
            if (token is CallerFuncToken)
                return parser.FunctionsDict[token.Symbol].Invoke(fallback.EvaluateTokens(((FuncToken)token).SubTokens.ToArray()));
            switch (token.Symbol)
            {
                case ParserSymbols.Negate:
                    return eval(token.SubTokens[0]) * -1;
                case ParserSymbols.Rad:
                    return eval(token.SubTokens[0]) * Math.PI / 180;
                case ParserSymbols.Deg:
                    return eval(token.SubTokens[0]) * 180 / Math.PI;
                case ParserSymbols.Floor:
                    return eval(token.SubTokens[0]).Floor();
                case ParserSymbols.Not:
                    return eval(token.SubTokens[0]) == 0 ? 1 : 0;
                case ParserSymbols.Fact:
                    var v = eval(token.SubTokens[0]);
                    if (v < 0 || v % 1 != 0) return double.NaN;
                    BigDecimal fact = 1;
                    for (int i = (int)v; i > 0; i--)
                        fact *= i;
                    return fact;
                case ParserSymbols.Cond:
                    return eval(token.SubTokens[0]) != 0
                        ? eval(token.SubTokens[1])
                        : eval(token.SubTokens[2]);
                default:
                    if (options.StrictMode) throw new UnsupportedOperationException(token.Symbol);
                    double result = token.Evaluate(fallback);
                    if (double.IsNaN(result)) throw new ParserArithmeticException(token.Position);
                    else return (BigDecimal)result;
            }
        }

        public override BigDecimal EvaluateNumber(NumberToken token)
        {
            return BigDecimal.Parse(token.Symbol, parser.Tokenizer.NumberFormat);
        }

        public override BigDecimal EvaluateVar(VarToken token)
        {
            object val = parser.GetVariable(token.Symbol);
            if (val == null) throw new ParserUnassignedVariableException(token.Symbol);
            else if (val is BigDecimal) return (BigDecimal)val;
            else if (val is decimal) return (BigDecimal)Convert.ToDecimal(val);
            else return (BigDecimal)Convert.ToDouble(val);
        }

        public override BigDecimal EvaluateConstant(ConstantToken token)
        {
            return DecimalEvaluator.ConstantsDict[token.Symbol];
        }
    }
}
