using System;
using System.Collections.Generic;
using Bluegrams.Calcex.Parsing.Tokens;

namespace Bluegrams.Calcex.Parsing
{
    static class OperatorProperties
    {
        private static Dictionary<string, byte> precedences
        {
            get
            {
                return new Dictionary<string, byte>()
                {
                    {ParserSymbols.Or, 2},
                    {ParserSymbols.And, 3},
                    {ParserSymbols.BitwiseOr, 4},
                    {ParserSymbols.BitwiseXor, 5},
                    {ParserSymbols.BitwiseAnd, 6},
                    {ParserSymbols.Equal, 7},
                    {ParserSymbols.NotEqual, 7},
                    {ParserSymbols.LessThan, 8},
                    {ParserSymbols.GreaterThan, 8},
                    {ParserSymbols.LessEqual, 8},
                    {ParserSymbols.GreaterEqual, 8},
                    {ParserSymbols.LeftShift, 9},
                    {ParserSymbols.RightShift, 9},
                    {ParserSymbols.UnsignedRightShift, 9},
                    {ParserSymbols.Add, 10},
                    {ParserSymbols.Subtract, 10},
                    {ParserSymbols.Multiply, 11},
                    {ParserSymbols.Divide, 11},
                    {ParserSymbols.Modulo, 11},
                    {ParserSymbols.Power, 12}
                };
            }
        }

        /// <summary>
        /// Returns the operator precedence of a given token (higher value means higher precedence).
        /// </summary>
        /// <param name="token">The (operator) token for which to determine the precedence.</param>
        /// <returns>The operator precedence for an operator token. Function and value tokens have a precedence of byte.MaxValue.</returns>
        public static byte GetPrecedence(Token token)
        {
            if (token is SignOperatorToken)
                return byte.MaxValue;
            else if (token is OperatorToken opToken)
                return precedences[opToken.Symbol];
            else return byte.MaxValue;
        }

        /// <summary>
        /// Determines if an operator is right associative.
        /// </summary>
        /// <param name="token">The operator token to check.</param>
        /// <returns>True for the power operator, fals otherwise.</returns>
        public static bool IsRightAssociative(OperatorToken token)
        {
            return token.Symbol == ParserSymbols.Power;
        }
    }
}
  