using System;
using Bluegrams.Calcex.Parsing;
using Bluegrams.Calcex.Parsing.Tokens;

namespace Bluegrams.Calcex.Evaluation
{
    public class StringEvaluator : Evaluator<string>
    {
        public StringEvaluator(Parser parser, EvaluationOptions options) : base(parser, options) { }

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
            string expr = token.Symbol + "(";
            foreach (TreeToken tok in token.SubTokens)
                expr += tok.Evaluate(this) + parser.Tokenizer.ArgumentSeparator;
            expr = expr.Trim();
            expr = expr.TrimEnd(parser.Tokenizer.ArgumentSeparator);
            expr += ")";
            return expr;
        }

        public override string EvaluateNumber(NumberToken token) => token.Symbol;
        public override string EvaluateVar(VarToken token) => token.Symbol;
        public override string EvaluateConstant(ConstantToken token) => token.Symbol;
    }
}
