using System.Collections.Generic;

namespace Bluegrams.Calcex.Parsing.Tokens
{
    public abstract class StructToken : TreeToken
    {
        public List<TreeToken> SubTokens { get; set; }

        public StructToken(string symbol, int pos) : base(symbol, pos) { }
    }
}
