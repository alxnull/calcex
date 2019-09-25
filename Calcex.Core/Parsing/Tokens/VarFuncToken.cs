
namespace Calcex.Parsing.Tokens
{
    public class VarFuncToken : FuncToken
    {
        public string VariableSymbol => SubTokens[0].Symbol;

        public VarFuncToken(string symbol, int pos, int argCount) : base(symbol, pos, argCount) { }

        public override T Evaluate<T>(Evaluation.Evaluator<T> evaluator)
        {
            return evaluator.EvaluateVarFunction(this);
        }
    }
}
