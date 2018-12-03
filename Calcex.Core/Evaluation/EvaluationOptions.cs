using System;

namespace Bluegrams.Calcex.Evaluation
{
    /// <summary>
    /// Specifies options for the evaluation of expressions.
    /// </summary>
    public class EvaluationOptions
    {
        /// <summary>
        /// If set to true, evaluators will not use double evaluation as a fallback for unsupported operations.
        /// (See DecimalEvaluator.cs for example.)
        /// </summary>
        public bool StrictMode { get; set; } = false;

        /// <summary>
        /// Specifies an epsilon value used for comparing double values.
        /// </summary>
        public double DoubleCompareEpsilon { get; set; } = 1e-15;
    }
}