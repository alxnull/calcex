using System;

namespace Bluegrams.Calcex.Parsing.Tokens
{
    public abstract class BracketToken : Token
    {
        public BracketToken(int pos) : base(pos) { }
    }

    public class LeftBracketToken : BracketToken
    {
        public LeftBracketToken(int pos) : base(pos) { }
    }

    public class RightBracketToken : BracketToken
    {
        public RightBracketToken(int pos) : base(pos) { }
    }
}