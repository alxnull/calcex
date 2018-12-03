using System;
using Bluegrams.Calcex.Parsing.Tokens;

namespace Bluegrams.Calcex.Evaluation
{
    public class PostfixStringEvaluator : StringEvaluator
    {
        public PostfixStringEvaluator(Parser parser, EvaluationOptions options) : base(parser, options) { }

        public override string EvaluateOperator(OperatorToken token)
        {
            if (token is SignOperatorToken)
            {
                return String.Format("{0} {1}",
                    token.SubTokens[1].Evaluate(this), token.Symbol);
            }
            return String.Format("{0} {1} {2}", 
                token.SubTokens[0].Evaluate(this), token.SubTokens[1].Evaluate(this), token.Symbol);
        }

        public override string EvaluateFunction(FuncToken token)
        {
            string ret = "";
            foreach (TreeToken tok in token.SubTokens)
                ret += tok.Evaluate(this) + " ";
            return String.Format("{0}{1}", ret, token.Symbol);
        }
    }
}
