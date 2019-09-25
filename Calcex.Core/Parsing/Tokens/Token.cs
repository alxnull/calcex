
namespace Calcex.Parsing.Tokens
{
    /// <summary>
    /// The base class of all tokens used in the parser.
    /// </summary>
    public abstract class Token
    {
        /// <summary>
        /// The position of this token in the original input string.
        /// </summary>
        public int Position { get; private set; }

        public Token(int pos)
        {
            Position = pos;
        }
    }
}
