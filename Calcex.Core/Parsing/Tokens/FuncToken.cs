using System.Collections.Generic;

namespace Bluegrams.Calcex.Parsing.Tokens
{
    public class FuncToken : StructToken
    {
        public int ArgCount { get; private set; }

        public FuncToken(string symbol, int pos, int argCount) : base(symbol, pos)
        {
            ArgCount = argCount;
            SubTokens = new List<TreeToken>();
        }

        public override T Evaluate<T>(Evaluation.Evaluator<T> evaluator)
        {
            return evaluator.EvaluateFunction(this);
        }
    }

    public class CallerFuncToken : FuncToken
    {
        public CallerFuncToken(string symbol, int pos, int argCount) : base(symbol, pos, argCount) { }
    }

    public class FuncParamToken : Token
    {
        public List<LinkedList<Token>> ParamList { get; set; }

        public FuncParamToken(int pos) : base(pos)
        {
            ParamList = new List<LinkedList<Token>>();
        }
    }
}
