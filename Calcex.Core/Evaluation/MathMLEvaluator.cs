using System;
using System.Text;
using Calcex.Parsing;
using Calcex.Parsing.Tokens;

namespace Calcex.Evaluation
{
    /// <summary>
    /// Converts an expression into MathML notation.
    /// </summary>
    public class MathMLEvaluator : Evaluator<string>
    {
        public MathMLEvaluator(Parser parser, EvaluationContext context) : base(parser, context) { }

        public override string EvaluateOperator(OperatorToken token)
        {
            if (token is SignOperatorToken)
                return evalSignOperator(token as SignOperatorToken);
            switch (token.Symbol)
            {
                // Arithmetic operators
                case ParserSymbols.Multiply:
                    return $"{evalSub(token, 0)}<mo>&sdot;</mo>{evalSub(token, 1)}";
                case ParserSymbols.Divide:
                    return $"<mfrac><mrow>{evalSub(token, 0)}</mrow><mrow>{evalSub(token, 1)}</mrow></mfrac>";
                case ParserSymbols.Power:
                    return $"<msup><mrow>{evalSub(token, 0)}</mrow><mrow>{evalSub(token, 1)}</mrow></msup>";
                case ParserSymbols.Modulo:
                    return $"{evalSub(token, 0)}<mo>mod</mo>{evalSub(token, 1)}";
                // Comparison operators
                case ParserSymbols.LessEqual:
                    return $"{evalSub(token, 0)}<mo>&le;</mo>{evalSub(token, 1)}";
                case ParserSymbols.GreaterEqual:
                    return $"{evalSub(token, 0)}<mo>&ge;</mo>{evalSub(token, 1)}";
                case ParserSymbols.NotEqual:
                    return $"{evalSub(token, 0)}<mo>&ne;</mo>{evalSub(token, 1)}";
                // Boolean/ bitwise operators
                case ParserSymbols.And:
                    return $"{evalSub(token, 0)}<mo>&and;</mo>{evalSub(token, 1)}";
                case ParserSymbols.Or:
                    return $"{evalSub(token, 0)}<mo>&or;</mo>{evalSub(token, 1)}";
                default:
                    return $"{evalSub(token, 0)}<mo>{token.Symbol}</mo>{evalSub(token, 1)}";
            }
        }

        private string evalSub(StructToken token, int subIndex)
        {
            TreeToken subToken = token.SubTokens[subIndex];
            int precedence = OperatorProperties.GetPrecedence(token);
            bool brackets = precedence > OperatorProperties.GetPrecedence(subToken);
            string expr = subToken.Evaluate(this);
            if (brackets) expr = $"<mo>(</mo>{expr}<mo>)</mo>";
            return expr;
        }

        private string evalSignOperator(SignOperatorToken token)
        {
            string expr = token.SubTokens[1].Evaluate(this);
            bool brackets = token.SubTokens[1] is OperatorToken;
            if (brackets) expr = $"<mo>(</mo>{expr}<mo>)</mo>";
            return $"<mo>{token.Symbol}</mo>" + expr;
        }

        public override string EvaluateFunction(FuncToken token)
        {
            switch (token.Symbol)
            {
                case ParserSymbols.Sqrt:
                    return $"<msqrt>{token.SubTokens[0].Evaluate(this)}</msqrt>";
                case ParserSymbols.Cbrt:
                    return $"<mroot><mrow>{token.SubTokens[0].Evaluate(this)}</mrow><mn>3</mn></mroot>";
                case ParserSymbols.Abs:
                    return $"<mo>|</mo>{token.SubTokens[0].Evaluate(this)}<mo>|</mo>";
                case ParserSymbols.Log10:
                    var sbLog10 = new StringBuilder();
                    sbLog10.AppendFormat("<msub><mi>{0}</mi><mn>10</mn></msub>", ParserSymbols.Log);
                    sbLog10.AppendFormat("<mo>(</mo>{0}<mo>)</mo>", evalSub(token, 0));
                    return sbLog10.ToString();
                case ParserSymbols.Log:
                    var sb = new StringBuilder();
                    sb.AppendFormat("<msub><mi>{0}</mi>{1}</msub>", ParserSymbols.Log, evalSub(token, 0));
                    sb.AppendFormat("<mo>(</mo>{0}<mo>)</mo>", evalSub(token, 1));
                    return sb.ToString();
                case ParserSymbols.Not:
                    return $"<mo>&not;</mo>{evalSub(token, 0)}";
                case ParserSymbols.Fact:
                    return $"{evalSub(token, 0)}<mo>!</mo>";
                default:
                    return evalFunctionHelper(token);
            }
        }

        public override string EvaluateVarFunction(VarFuncToken token)
        {
            string symbol = String.Empty;
            switch (token.Symbol)
            {
                case ParserSymbols.Sum:
                    symbol = "&sum;";
                    break;
                case ParserSymbols.Prod:
                    symbol = "&prod;";
                    break;
            }
            var sb = new StringBuilder($"<munderover><mo>{symbol}</mo>");
            sb.AppendFormat("<mrow><mi>{0}</mi><mo>=</mo>{1}</mrow>",
                token.VariableSymbol, evalSub(token, 1));
            sb.AppendFormat("<mrow>{0}</mrow></munderover>", evalSub(token, 2));
            sb.AppendFormat("<mrow>{0}</mrow>", evalSub(token, 3));
            return sb.ToString();
        }

        private string evalFunctionHelper(FuncToken token)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<mi>{0}</mi>", token.Symbol);
            sb.AppendFormat("<mfenced separators=\"{0}\">", parser.Tokenizer.ArgumentSeparator);
            foreach (TreeToken tok in token.SubTokens)
                sb.Append($"<mrow>{tok.Evaluate(this)}</mrow>");
            sb.Append("</mfenced>");
            return sb.ToString();
        }

        public override string EvaluateNumber(NumberToken token)
            => $"<mn>{token.Symbol}</mn>";

        public override string EvaluateVar(VarToken token)
            => $"<mi>{token.Symbol}</mi>";

        public override string EvaluateConstant(ConstantToken token)
        {
            string symbol = token.Symbol == ParserSymbols.Pi ? "&pi;" : token.Symbol;
            return $"<mi>{symbol}</mi>";
        }
    }
}
