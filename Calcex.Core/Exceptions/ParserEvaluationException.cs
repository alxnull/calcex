
namespace Calcex
{
    public class EvaluationException : ParserException
    {
        public EvaluationException(string message) : base(message) { }
    }
    
    public class ParserArithmeticException : EvaluationException
    {
        public ParserArithmeticException(int pos) : base($"Invalid arithmetic operation at position {pos}.") { }
    }

    public class UnsupportedOperationException : EvaluationException
    {
        public UnsupportedOperationException(string name) : base($"The operation '{name}' is not supported by this evaluator.") { }
    }
}