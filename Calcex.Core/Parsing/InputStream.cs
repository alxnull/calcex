using System.Text.RegularExpressions;

namespace Calcex.Parsing
{
    internal class InputStream
    {
        internal int Position { get; private set; }
        internal string InputString { get; private set; }
        internal bool AtEnd { get { if (InputString.Length == 0) return true; else return false; } }

        internal InputStream(string inputString)
        {
            InputString = inputString;
            Position = 0;
            MoveForward(0);
        }

        internal char FirstChar()
        {
            return InputString[0];
        }

        internal bool StartsWith(string check)
        {
            if (InputString.StartsWith(check))
            {
                MoveForward(check.Length);
                return true;
            }
            else return false;
        }

        internal void MoveForward(int pos)
        {
            // Skip whitespace.
            while (pos < InputString.Length && InputString[pos] == ' ')
                pos += 1;
            InputString = InputString.Substring(pos);
            Position += pos;
        }
    }
}
