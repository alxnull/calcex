using System.Collections.Generic;

namespace Calcex.Parsing.Tokens
{
    public class OperatorToken : StructToken
    {
        public OperatorToken(string symbol, int pos) : base(symbol, pos)
        {
            SubTokens = new List<TreeToken>(2) { null, null };
        }

        public override T Evaluate<T>(Evaluation.Evaluator<T> evaluator) 
        {
            return evaluator.EvaluateOperator(this);
        }
    }

    public class SignOperatorToken : OperatorToken
    {
        public SignOperatorToken(int pos) : base(ParserSymbols.Subtract, pos) { }
    }
}
