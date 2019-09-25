
namespace Calcex
{
    /// <summary>
    /// The exception that is thrown if the snytax of the input expression is invalid.
    /// </summary>
    public class ParserSyntaxException : ParserException
    {
        public int Position { get; private set; }

        public ParserSyntaxException(int pos) : base($"Invalid mathematical expression. Error at position {pos}.")
        {
            Position = pos;
        }

        public ParserSyntaxException(string message, int pos) : base($"{message} Error at position {pos}.")
        {
            Position = pos;
        }
    }

    public class ParserBracketException : ParserSyntaxException
    {
        public ParserBracketException(string message, int pos) : base(message, pos) {}
    }

    public class ParserFunctionArgumentException : ParserSyntaxException
    {
        public ParserFunctionArgumentException(string name, int pos, int count) : base($"Function '{name}' cannot take {count} argument(s).", pos) {}
    }
}