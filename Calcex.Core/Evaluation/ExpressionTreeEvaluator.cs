using System;
using Calcex.Parsing.Tokens;
using Calcex.Parsing;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;

using OneFunc = System.Func<double, double>;
using TwoFunc = System.Func<double, double, double>;

namespace Calcex.Evaluation
{
    /// <summary>
    /// Evaluates an expression to an expression tree.
    /// </summary>
    public class ExpressionTreeEvaluator : Evaluator<Expression>
    {
        public ExpressionTreeEvaluator(Parser parser, EvaluationContext context) : base(parser, context) {}

        public static Func<double, double> CreateDelegate(TreeToken token, string variable, Parser parser, EvaluationOptions options)
        {
            var paramColl = new ParameterCollection<double>(variable);
            var context = new EvaluationContext(options, paramColl);
            var eval = new ExpressionTreeEvaluator(parser, context);
            var expr = token.Evaluate(eval);
            var parameters = paramColl.GetParameters();
            return Expression.Lambda<Func<double, double>>(expr, parameters).Compile();
        }

        public static Dictionary<string, Expression> ConstantsDict = new Dictionary<string, Expression>()
        {
            {ParserSymbols.Pi, Expression.Constant(Math.PI)},
            {ParserSymbols.E, Expression.Constant(Math.E)},
            {ParserSymbols.True, Expression.Constant(1.0)},
            {ParserSymbols.False, Expression.Constant(0.0)}
        };

        private Expression eval(TreeToken token) => token.Evaluate(this);

        private InvocationExpression makeFuncExpr<T>(Expression<T> f, params Expression[] expr)
            => Expression.Invoke(f, expr);

        private Expression toBoolExpr(Expression expr)
            => makeFuncExpr<Func<double, bool>>(p => Convert.ToBoolean(p), expr);

        public override Expression EvaluateOperator(OperatorToken token)
        {
            switch (token.Symbol)
            {
                // Arithmetic operators
                case ParserSymbols.Add:
                    return Expression.Add(eval(token.SubTokens[0]), eval(token.SubTokens[1]));
                case ParserSymbols.Subtract:
                    return Expression.Subtract(eval(token.SubTokens[0]), eval(token.SubTokens[1]));
                case ParserSymbols.Multiply:
                    return Expression.Multiply(eval(token.SubTokens[0]), eval(token.SubTokens[1]));
                case ParserSymbols.Divide:
                    return makeFuncExpr<TwoFunc>((p1, p2) => p2 == 0 ? double.NaN : p1 / p2,
                        eval(token.SubTokens[0]), eval(token.SubTokens[1]));
                case ParserSymbols.Power:
                    return Expression.Power(eval(token.SubTokens[0]), eval(token.SubTokens[1]));
                case ParserSymbols.Modulo:
                    return Expression.Modulo(eval(token.SubTokens[0]), eval(token.SubTokens[1]));
                // Comparison operators
                case ParserSymbols.Equal:
                    return makeFuncExpr<TwoFunc>((p1, p2) => Math.Abs(p1 - p2) <= Context.Options.DoubleCompareEpsilon ? 1 : 0,
                        eval(token.SubTokens[0]), eval(token.SubTokens[1]));
                case ParserSymbols.LessThan:
                    return makeFuncExpr<TwoFunc>((p1, p2) => p1 < p2 - Context.Options.DoubleCompareEpsilon ? 1 : 0,
                        eval(token.SubTokens[0]), eval(token.SubTokens[1]));
                case ParserSymbols.GreaterThan:
                    return makeFuncExpr<TwoFunc>((p1, p2) => p1 > p2 + Context.Options.DoubleCompareEpsilon ? 1 : 0,
                        eval(token.SubTokens[0]), eval(token.SubTokens[1]));
                case ParserSymbols.LessEqual:
                    return makeFuncExpr<TwoFunc>((p1, p2) => p1 <= p2 + Context.Options.DoubleCompareEpsilon ? 1 : 0,
                        eval(token.SubTokens[0]), eval(token.SubTokens[1]));
                case ParserSymbols.GreaterEqual:
                    return makeFuncExpr<TwoFunc>((p1, p2) => p1 >= p2 - Context.Options.DoubleCompareEpsilon ? 1 : 0,
                        eval(token.SubTokens[0]), eval(token.SubTokens[1]));
                case ParserSymbols.NotEqual:
                    return makeFuncExpr<TwoFunc>((p1, p2) => Math.Abs(p1 - p2) > Context.Options.DoubleCompareEpsilon ? 1 : 0,
                        eval(token.SubTokens[0]), eval(token.SubTokens[1]));
                // Boolean/ bitwise operators
                case ParserSymbols.And:
                    return Expression.And(
                        toBoolExpr(eval(token.SubTokens[0])),
                        toBoolExpr(eval(token.SubTokens[1]))
                        );
                case ParserSymbols.Or:
                    return Expression.Or(
                        toBoolExpr(eval(token.SubTokens[0])),
                        toBoolExpr(eval(token.SubTokens[1]))
                        );
                case ParserSymbols.BitwiseAnd:
                    return Expression.Convert(Expression.And(
                        Expression.Convert(eval(token.SubTokens[0]), typeof(long)),
                        Expression.Convert(eval(token.SubTokens[1]), typeof(long))
                        ), typeof(double));
                case ParserSymbols.BitwiseOr:
                    return Expression.Convert(Expression.Or(
                        Expression.Convert(eval(token.SubTokens[0]), typeof(long)),
                        Expression.Convert(eval(token.SubTokens[1]), typeof(long))
                        ), typeof(double));
                case ParserSymbols.BitwiseXor:
                    return Expression.Convert(Expression.ExclusiveOr(
                        Expression.Convert(eval(token.SubTokens[0]), typeof(long)),
                        Expression.Convert(eval(token.SubTokens[1]), typeof(long))
                        ), typeof(double));
                case ParserSymbols.LeftShift:
                    return Expression.Convert(Expression.LeftShift(
                        Expression.Convert(eval(token.SubTokens[0]), typeof(long)),
                        Expression.Convert(eval(token.SubTokens[1]), typeof(int))
                        ), typeof(double));
                case ParserSymbols.RightShift:
                    return Expression.Convert(Expression.RightShift(
                        Expression.Convert(eval(token.SubTokens[0]), typeof(long)),
                        Expression.Convert(eval(token.SubTokens[1]), typeof(int))
                        ), typeof(double));
                case ParserSymbols.UnsignedRightShift:
                    return Expression.Convert(Expression.RightShift(
                        Expression.Convert(eval(token.SubTokens[0]), typeof(uint)),
                        Expression.Convert(eval(token.SubTokens[1]), typeof(int))
                        ), typeof(double));
                default:
                    if (Context.Options.StrictMode) throw new UnsupportedOperationException(token.Symbol);
                    return Expression.Constant(double.NaN);
            }
        }

        public override Expression EvaluateFunction(FuncToken token)
        {
            if (token is CallerFuncToken)
            {
                Delegate func = parser.FunctionsDict[token.Symbol];
                return Expression.Invoke(Expression.Constant(func), EvaluateTokens((token).SubTokens.ToArray()));
            }
            switch (token.Symbol)
            {
                case ParserSymbols.Negate:
                    return Expression.Negate(eval(token.SubTokens[0]));
                case ParserSymbols.Sqrt:
                    return Expression.Power(eval(token.SubTokens[0]), Expression.Constant(0.5));
                case ParserSymbols.Cbrt:
                    return Expression.Power(eval(token.SubTokens[0]), Expression.Constant(1 / 3.0));
                case ParserSymbols.Exp:
                    return Expression.Power(Expression.Constant(Math.E), eval(token.SubTokens[0]));
                case ParserSymbols.Sin:
                    return makeFuncExpr<OneFunc>(p => Math.Sin(p), eval(token.SubTokens[0]));
                case ParserSymbols.Cos:
                    return makeFuncExpr<OneFunc>(p => Math.Cos(p), eval(token.SubTokens[0]));
                case ParserSymbols.Tan:
                    return makeFuncExpr<OneFunc>(p => Math.Tan(p), eval(token.SubTokens[0]));
                case ParserSymbols.Abs:
                    return makeFuncExpr<OneFunc>(p => Math.Abs(p), eval(token.SubTokens[0]));
                case ParserSymbols.Log10:
                    return makeFuncExpr<OneFunc>(p => Math.Log10(p), eval(token.SubTokens[0]));
                case ParserSymbols.LogE:
                    return makeFuncExpr<OneFunc>(p => Math.Log(p), eval(token.SubTokens[0]));
                case ParserSymbols.ASin:
                    return makeFuncExpr<OneFunc>(p => Math.Asin(p), eval(token.SubTokens[0]));
                case ParserSymbols.ACos:
                    return makeFuncExpr<OneFunc>(p => Math.Acos(p), eval(token.SubTokens[0]));
                case ParserSymbols.ATan:
                    return makeFuncExpr<OneFunc>(p => Math.Atan(p), eval(token.SubTokens[0]));
                case ParserSymbols.Sinh:
                    return makeFuncExpr<OneFunc>(p => Math.Sinh(p), eval(token.SubTokens[0]));
                case ParserSymbols.Cosh:
                    return makeFuncExpr<OneFunc>(p => Math.Cosh(p), eval(token.SubTokens[0]));
                case ParserSymbols.Tanh:
                    return makeFuncExpr<OneFunc>(p => Math.Tanh(p), eval(token.SubTokens[0]));
                case ParserSymbols.Log:
                    return makeFuncExpr<TwoFunc>((p1, p2) => Math.Log(p1, p2), eval(token.SubTokens[1]), eval(token.SubTokens[0]));
                case ParserSymbols.Rad:
                    return makeFuncExpr<OneFunc>(p => p * Math.PI / 180, eval(token.SubTokens[0]));
                case ParserSymbols.Deg:
                    return makeFuncExpr<OneFunc>(p => p * 180 / Math.PI, eval(token.SubTokens[0]));
                case ParserSymbols.Ceil:
                    return makeFuncExpr<OneFunc>(p => Math.Ceiling(p), eval(token.SubTokens[0]));
                case ParserSymbols.Floor:
                    return makeFuncExpr<OneFunc>(p => Math.Floor(p), eval(token.SubTokens[0]));
                case ParserSymbols.Round:
                    return makeFuncExpr<OneFunc>(p => Math.Round(p), eval(token.SubTokens[0]));
                case ParserSymbols.Sign:
                    return makeFuncExpr<OneFunc>(p => Math.Sign(p), eval(token.SubTokens[0]));
                case ParserSymbols.Trunc:
                    return makeFuncExpr<OneFunc>(p => Math.Truncate(p), eval(token.SubTokens[0]));
                case ParserSymbols.Min:
                    return makeFuncExpr<ArrayFunc>(
                        a => a.Min(),
                        Expression.NewArrayInit(typeof(double), EvaluateTokens(token.SubTokens.ToArray())));
                case ParserSymbols.Max:
                    return makeFuncExpr<ArrayFunc>(
                        a => a.Max(),
                        Expression.NewArrayInit(typeof(double), EvaluateTokens(token.SubTokens.ToArray())));
                case ParserSymbols.Avg:
                    return makeFuncExpr<ArrayFunc>(
                        a => a.Average(),
                        Expression.NewArrayInit(typeof(double), EvaluateTokens(token.SubTokens.ToArray())));
                case ParserSymbols.AndFunc:
                    return makeFuncExpr<ArrayFunc>(
                        a => a.Select(v1 => Convert.ToBoolean(v1)).Aggregate((v1, v2) => v1 & v2) ? 1 : 0,
                        Expression.NewArrayInit(typeof(double), EvaluateTokens(token.SubTokens.ToArray())));
                case ParserSymbols.OrFunc:
                    return makeFuncExpr<ArrayFunc>(
                        a => a.Select(v1 => Convert.ToBoolean(v1)).Aggregate((v1, v2) => v1 | v2) ? 1 : 0,
                        Expression.NewArrayInit(typeof(double), EvaluateTokens(token.SubTokens.ToArray())));
                case ParserSymbols.XorFunc:
                    return makeFuncExpr<ArrayFunc>(
                        a => a.Select(v1 => Convert.ToBoolean(v1)).Aggregate((v1, v2) => v1 ^ v2) ? 1 : 0,
                        Expression.NewArrayInit(typeof(double), EvaluateTokens(token.SubTokens.ToArray())));
                case ParserSymbols.Not:
                    return Expression.Not(toBoolExpr(eval(token.SubTokens[0])));
                case ParserSymbols.Fact:
                    return makeFuncExpr<OneFunc>(p => fact(p), eval(token.SubTokens[0]));
                case ParserSymbols.Cond:
                    return Expression.Condition(toBoolExpr(eval(token.SubTokens[0])),
                        eval(token.SubTokens[1]),
                        eval(token.SubTokens[2]));
                default:
                    if (Context.Options.StrictMode) throw new UnsupportedOperationException(token.Symbol);
                    return Expression.Constant(double.NaN);
            }
        }

        private double fact(double v)
        {
            if (v < 0 || v % 1 != 0) return double.NaN;
            double fact = 1;
            for (int i = (int)v; i > 0; i--)
                fact *= i;
            return fact;
        }

        public override Expression EvaluateVarFunction(VarFuncToken token)
        {
            Func<double, double> func;
            switch (token.Symbol)
            {
                case ParserSymbols.Sum:
                    func = ExpressionTreeEvaluator.CreateDelegate(token.SubTokens[3],
                        token.VariableSymbol, parser, Context.Options);
                    return iterateExpr(0, token.SubTokens[1], token.SubTokens[2], (v, i) => v + func(i));
                case ParserSymbols.Prod:
                    func = ExpressionTreeEvaluator.CreateDelegate(token.SubTokens[3],
                        token.VariableSymbol, parser, Context.Options);
                    return iterateExpr(1, token.SubTokens[1], token.SubTokens[2], (v, i) => v * func(i));
                default:
                    if (Context.Options.StrictMode) throw new UnsupportedOperationException(token.Symbol);
                    return Expression.Constant(double.NaN);
            }
        }

        private Expression iterateExpr(double init, TreeToken start, TreeToken end,
            Expression<Func<double, double, double>> funcExpr)
        {
            var iteratorExpr = Expression.Parameter(typeof(double));
            var resultExpr = Expression.Parameter(typeof(double));
            var returnLabel = Expression.Label(typeof(double));
            return Expression.Block(
                new[] { iteratorExpr, resultExpr },
                Expression.Assign(resultExpr, Expression.Constant(init)),
                Expression.Assign(iteratorExpr, eval(start)),
                Expression.Loop(
                    Expression.IfThenElse(
                        Expression.GreaterThan(iteratorExpr, eval(end)),
                        Expression.Return(returnLabel, resultExpr),
                        Expression.Assign(resultExpr,
                            Expression.Invoke(funcExpr, resultExpr, Expression.PostIncrementAssign(iteratorExpr))
                        )
                    ),
                    returnLabel
                )
            );
        }

        public override Expression EvaluateNumber(NumberToken token)
            => Expression.Constant(double.Parse(token.Symbol, parser.Tokenizer.NumberFormat));

        public override Expression EvaluateVar(VarToken token)
            => Context.Parameters[token.Symbol] ?? resolveVar(token.Symbol);

        private Expression resolveVar(string symbol)
        {
            object val = parser.GetVariable(symbol);
            if (val == null) throw new ParserUnassignedVariableException(symbol);
            else return Expression.Constant(Convert.ToDouble(val));
        }

        public override Expression EvaluateConstant(ConstantToken token)
            => ConstantsDict[token.Symbol];
    }
}
