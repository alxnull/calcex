using System.Collections.Generic;

namespace Calcex.Parsing
{
    class DescendingStringComparer : IComparer<string>
    {
        public int Compare(string x, string y) => y.CompareTo(x);
    }
}
