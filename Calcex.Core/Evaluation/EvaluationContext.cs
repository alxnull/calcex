
namespace Calcex.Evaluation
{
    /// <summary>
    /// Provides context information for the evaluation of an expression.
    /// </summary>
    public class EvaluationContext
    {
        public EvaluationOptions Options { get; }
        public IParameterCollection Parameters { get; }

        public EvaluationContext(EvaluationOptions options, IParameterCollection parameters)
        {
            this.Options = options;
            this.Parameters = parameters;
        }
    }
}
