
namespace Calcex.Parsing.Tokens
{
    public abstract class ValueToken : TreeToken
    {
        public ValueToken(string value, int pos) : base(value, pos) { }
    }

    public class NumberToken : ValueToken
    {
        public NumberToken(string value, int pos) : base(value, pos) { }

        public override T Evaluate<T>(Evaluation.Evaluator<T> evaluator)
        {
            return evaluator.EvaluateNumber(this);
        }
    }

    public class VarToken : ValueToken
    {
        public VarToken(string name, int pos) : base(name, pos) { }

        public override T Evaluate<T>(Evaluation.Evaluator<T> evaluator)
        {
            return evaluator.EvaluateVar(this);
        }
    }

    public class ConstantToken : ValueToken
    {
        public ConstantToken(string value, int pos) : base(value, pos) { }

        public override T Evaluate<T>(Evaluation.Evaluator<T> evaluator)
        {
            return evaluator.EvaluateConstant(this);
        }
    }
}
