using System;

namespace Bluegrams.Calcex
{
    /// <summary>
    /// Additional mathematical functions not included in the parser.
    /// </summary>
    public static class Utils
    {
        #region Number Conversion
        private static char[] chs = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz".ToCharArray();

        /// <summary>
        /// Converts a string that represents a number with a base from 2 to 62 to a decimal number.
        /// </summary>
        /// <param name="input">A string that represents the number to be converted.</param>
        /// <param name="frombase">The base from 2 to 62 of the number to be converted.</param>
        /// <returns>The converted decimal number as long integer.</returns>
        public static long ConvertToBase10(string input, int frombase)
        {
            if (frombase > chs.Length) throw new ArgumentException(String.Format("Given base can not be greater than {0}.", chs.Length), "frombase");
            else if (frombase < 2) throw new ArgumentException("Given base can not be lower than 2.", "frombase");
            bool neg = false;
            long dec = 0;
            if (input.StartsWith("-")) { input = input.Substring(1); neg = true; }
            for (int i = input.Length - 1, j = 0; i >= 0; i--, j++)
            {
                char c = Convert.ToChar(input.Substring(j, 1));
                int val = Array.IndexOf(chs, c);
                if (val == -1 || val >= frombase) { throw new FormatException("String contains invalid characters."); }
                    dec = dec + val * (long)Math.Pow(frombase, i);
            }
            if (neg) dec *= -1;
            return dec;
        }

        /// <summary>
        /// Converts a decimal number to a number with another base from 2 to 62.
        /// </summary>
        /// <param name="input">The number to be converted as long integer.</param>
        /// <param name="tobase">The base from 2 to 62 to which the number should be converted.</param>
        /// <returns>A string representing the converted number.</returns>
        public static string ConvertFromBase10(long input, int tobase)
        {
            if (tobase > chs.Length) throw new ArgumentException(String.Format("Given base can not be greater than {0}.", chs.Length), "tobase");
            else if (tobase < 2) throw new ArgumentException("Given base can not be lower than 2.", "tobase");
            if (input == 0) return "0";
            long s = Math.Abs(input);
            string result = "";
            while (s > 0)
            {
                long rest = s % tobase;
                result = chs[rest] + result;
                s /= tobase;
            }
            if (input < 0) result = "-" + result;
            return result;
        }
        #endregion
    }
}
