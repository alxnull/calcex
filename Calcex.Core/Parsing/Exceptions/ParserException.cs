using System;

namespace Bluegrams.Calcex.Parsing
{
    /// <summary>
    /// The base class of all parsing exceptions.
    /// </summary>
    public abstract class ParserException : Exception
    {
        public ParserException(string message) : base(message) {}

        public ParserException(string message, Exception innerException) : base(message, innerException) {}
    }

    /// <summary>
    /// The exception that is thrown when the expression string contains an invalid character.
    /// </summary>
    public class ParserCharException : ParserException
    {
        public ParserCharException(int pos, char ch) : base($"Unexpected character '{ch}' at position {pos}.") { }
    }

    /// <summary>
    /// The exception that is thrown when a variable is unassigned during evaluation.
    /// </summary>
    public class ParserUnassignedVariableException : ParserException
    {
        public string VariableName { get; private set; }

        public ParserUnassignedVariableException(string name) : base($"Unassigned variable '{name}' used.")
        {
            VariableName = name;
        }
    }
}