using Bluegrams.Calcex.Evaluation;

namespace Bluegrams.Calcex.Parsing.Tokens
{
     /// <summary>
    /// The base class of all tokens that can be part of the parsed syntax tree.
    /// </summary>
    public abstract class TreeToken : Token
    {
        /// <summary>
        /// The string that represents this token.
        /// </summary>
        public string Symbol { get; private set; }

        public TreeToken(string symbol, int pos) : base(pos)
        {
            Symbol = symbol;
        }

        /// <summary>
        /// Evaluates this token.
        /// </summary>
        /// <param name="evaluator">The evaluator to be called.</param>
        /// <returns>The result of the evaluation.</returns>
        public abstract T Evaluate<T>(Evaluator<T> evaluator);

        public override string ToString() => Symbol;
    }
}