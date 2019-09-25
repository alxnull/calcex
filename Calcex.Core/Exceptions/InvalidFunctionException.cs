using System;

namespace Calcex
{
    public class InvalidFunctionException : ArgumentException
    {
        public InvalidFunctionException(string name, Exception inner) : base($"Expression of function {name} is not valid.", inner) { }
    }
}