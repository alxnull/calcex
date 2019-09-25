using System;
using System.Linq;
using System.Text;
using Calcex.Parsing;
using Calcex.Parsing.Tokens;

namespace Calcex.Evaluation
{
    public class StringEvaluator : Evaluator<string>
    {
        public StringEvaluator(Parser parser, EvaluationContext context) : base(parser, context) { }

        public override string EvaluateOperator(OperatorToken token)
        {
            if (token is SignOperatorToken)
                return evalSignOperator(token as SignOperatorToken);
            int precedence = OperatorProperties.GetPrecedence(token);
            string exprLeft = evalOperatorSubToken(precedence, token.SubTokens[0]);
            string exprRight = evalOperatorSubToken(precedence, token.SubTokens[1]);
            return exprLeft + token.Symbol + exprRight;
        }

        private string evalOperatorSubToken(int precedence, TreeToken subToken)
        {
            bool brackets = precedence > OperatorProperties.GetPrecedence(subToken);
            string expr = subToken.Evaluate(this);
            if (brackets) expr = $"{ParserSymbols.LBracket}{expr}{ParserSymbols.RBracket}";
            return expr;
        }

        private string evalSignOperator(SignOperatorToken token)
        {
            string expr = token.SubTokens[1].Evaluate(this);
            bool brackets = token.SubTokens[1] is OperatorToken;
            if (brackets) expr = String.Format("({0})", expr);
            return token.Symbol + expr;
        }

        public override string EvaluateFunction(FuncToken token)
        {
            var sb = new StringBuilder(token.Symbol);
            sb.Append("(");
            sb.Append(String.Join(
                parser.Tokenizer.ArgumentSeparator.ToString(),
                token.SubTokens.Select(t => t.Evaluate(this))
                ));
            sb.Append(")");
            return sb.ToString();
        }

        public override string EvaluateVarFunction(VarFuncToken token)
            => EvaluateFunction(token);

        public override string EvaluateNumber(NumberToken token) => token.Symbol;
        public override string EvaluateVar(VarToken token) => token.Symbol;
        public override string EvaluateConstant(ConstantToken token) => token.Symbol;
    }
}
