using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Calcex.Parsing.Tokens;
using System.Globalization;

namespace Calcex.Parsing
{
    internal class Tokenizer
    {
        internal char DecimalSeparator { get; private set; }
        internal char ArgumentSeparator { get; private set; }
        internal SortedDictionary<string, Tuple<Type, int>> TokenDict;
        internal NumberFormatInfo NumberFormat;
        
        private string NumberPattern 
        {
            get 
            {
                var sp = DecimalSeparator;
                return $@"^((\d|\s)*\{sp}\d+|\d(\d|\s)*)(E[+-]?\d(\d|\s)*)?";
            }
        }

        private static Regex numberRegex;
        private static readonly Regex varFuncRegex = new Regex(@"^\(\s*([a-zA-Z]+[0-9]*)\s*,");

        internal Tokenizer()
        {
            TokenDict = new SortedDictionary<string, Tuple<Type, int>>(new DescendingStringComparer())
            {
                // Operators
                {ParserSymbols.Add, token(typeof(OperatorToken))},
                {ParserSymbols.Subtract, token(typeof(OperatorToken))},
                {ParserSymbols.Multiply, token(typeof(OperatorToken))},
                {ParserSymbols.Divide, token(typeof(OperatorToken))},
                {ParserSymbols.Power, token(typeof(OperatorToken))},
                {ParserSymbols.Modulo, token(typeof(OperatorToken))},
                {ParserSymbols.Equal, token(typeof(OperatorToken))},
                {ParserSymbols.LessThan, token(typeof(OperatorToken))},
                {ParserSymbols.GreaterThan, token(typeof(OperatorToken))},
                {ParserSymbols.LessEqual, token(typeof(OperatorToken))},
                {ParserSymbols.GreaterEqual, token(typeof(OperatorToken))},
                {ParserSymbols.NotEqual, token(typeof(OperatorToken))},
                {ParserSymbols.And, token(typeof(OperatorToken))},
                {ParserSymbols.Or, token(typeof(OperatorToken))},
                {ParserSymbols.BitwiseAnd, token(typeof(OperatorToken))},
                {ParserSymbols.BitwiseOr, token(typeof(OperatorToken))},
                {ParserSymbols.BitwiseXor, token(typeof(OperatorToken))},
                {ParserSymbols.LeftShift, token(typeof(OperatorToken))},
                {ParserSymbols.RightShift, token(typeof(OperatorToken))},
                {ParserSymbols.UnsignedRightShift, token(typeof(OperatorToken))},
                // Functions
                {ParserSymbols.Negate, token(typeof(FuncToken), 1)},
                {ParserSymbols.Sqrt, token(typeof(FuncToken), 1)},
                {ParserSymbols.Cbrt, token(typeof(FuncToken), 1)},
                {ParserSymbols.Exp, token(typeof(FuncToken), 1)},
                {ParserSymbols.Sin, token(typeof(FuncToken), 1)},
                {ParserSymbols.Cos, token(typeof(FuncToken), 1)},
                {ParserSymbols.Tan, token(typeof(FuncToken), 1)},
                {ParserSymbols.Abs, token(typeof(FuncToken), 1)},
                {ParserSymbols.Log10, token(typeof(FuncToken), 1)},
                {ParserSymbols.LogE, token(typeof(FuncToken), 1)},
                {ParserSymbols.ASin, token(typeof(FuncToken), 1)},
                {ParserSymbols.ACos, token(typeof(FuncToken), 1)},
                {ParserSymbols.ATan, token(typeof(FuncToken), 1)},
                {ParserSymbols.Rad, token(typeof(FuncToken), 1)},
                {ParserSymbols.Deg, token(typeof(FuncToken), 1)},
                {ParserSymbols.Ceil, token(typeof(FuncToken), 1)},
                {ParserSymbols.Floor, token(typeof(FuncToken), 1)},
                {ParserSymbols.Round, token(typeof(FuncToken), 1)},
                {ParserSymbols.Sign, token(typeof(FuncToken), 1)},
                {ParserSymbols.Trunc, token(typeof(FuncToken), 1)},
                {ParserSymbols.Log, token(typeof(FuncToken), 2)},
                {ParserSymbols.Min, token(typeof(FuncToken), -1)},
                {ParserSymbols.Max, token(typeof(FuncToken), -1)},
                {ParserSymbols.Avg, token(typeof(FuncToken), -1)},
                {ParserSymbols.Sinh, token(typeof(FuncToken), 1) },
                {ParserSymbols.Cosh, token(typeof(FuncToken), 1) },
                {ParserSymbols.Tanh, token(typeof(FuncToken), 1) },
                {ParserSymbols.AndFunc, token(typeof(FuncToken), -1)},
                {ParserSymbols.OrFunc, token(typeof(FuncToken), -1)},
                {ParserSymbols.Not, token(typeof(FuncToken), 1)},
                {ParserSymbols.XorFunc, token(typeof(FuncToken), -1)},
                {ParserSymbols.Fact, token(typeof(FuncToken), 1)},
                {ParserSymbols.Cond, token(typeof(FuncToken), 3)},
                {ParserSymbols.Sum, token(typeof(VarFuncToken), 4)},
                {ParserSymbols.Prod, token(typeof(VarFuncToken), 4)},
                // Brackets / Constants
                {ParserSymbols.LBracket, token(typeof(LeftBracketToken))},
                {ParserSymbols.RBracket, token(typeof(RightBracketToken))},
                {ParserSymbols.Pi, token(typeof(ConstantToken))},
                {ParserSymbols.E, token(typeof(ConstantToken))},
                {ParserSymbols.True, token(typeof(ConstantToken))},
                {ParserSymbols.False, token(typeof(ConstantToken))}
            };
        }

        private Tuple<Type, int> token(Type type, int argCount = 0) => new Tuple<Type, int>(type, argCount);

        internal void SetSeparatorStyle(SeparatorStyle style)
        {
            switch (style)
            {
                case SeparatorStyle.Dot:
                    DecimalSeparator = '.';
                    ArgumentSeparator = ',';
                    break;
                default:
                    DecimalSeparator = ',';
                    ArgumentSeparator = ';';
                    break;
            }
            NumberFormat = new NumberFormatInfo();
            NumberFormat.NumberDecimalSeparator = DecimalSeparator.ToString();
            numberRegex = new Regex(NumberPattern);
        }

        internal bool IsValidNumber(string str)
        {
            Regex reg = new Regex(NumberPattern + "$");
            if(!reg.Match(str).Success) return false;
            return true;
        }

        internal LinkedList<Token> Read(InputStream str)
        {
            LinkedList<Token> tokenlist = new LinkedList<Token>();
            while (!str.AtEnd)
            {
                Token tok = readNextToken(tokenlist, tokenlist.Last?.Value, str);
                if (tok is FuncToken funcToken) tokenlist.AddLast(readFuncParams(funcToken, str));
            }
            return tokenlist;
        }

        /// <summary>
        /// Reads the arguments given to a function and puts them into a FuncParamToken.
        /// </summary>
        /// <returns>The created FuncParamToken conatining the arguments given to the function.</returns>
        private Token readFuncParams(FuncToken funcToken, InputStream str)
        {
            if (str.AtEnd) throw new ParserSyntaxException("Invalid function usage.", funcToken.Position);
            FuncParamToken paramtok = new FuncParamToken(str.Position);
            LinkedList<Token> subTokenList = new LinkedList<Token>();
            int open = 0, close = 0;
            bool withBrackets = false;
            string removeVar = null;
            // Match the index variable if it's a VarFuncToken.
            if (funcToken is VarFuncToken varFuncToken)
            {
                var match = varFuncRegex.Match(str.InputString);
                if (match.Success)
                {
                    withBrackets = true; open++;
                    var varToken = new VarToken(match.Groups[1].ToString(), str.Position);
                    paramtok.ParamList.Add(new LinkedList<Token>(new[] { varToken }));
                    str.MoveForward(match.Length);
                    if (!TokenDict.ContainsKey(varToken.Symbol))
                    {
                        removeVar = varToken.Symbol;
                        TokenDict.Add(removeVar, new Tuple<Type, int>(typeof(VarToken), 0));
                    }
                }
                else throw new ParserSyntaxException("Invalid index variable definition.", str.Position);
            }
            Token tok = funcToken;
            do
            {
                if (str.StartsWith(ArgumentSeparator.ToString()))
                {
                    paramtok.ParamList.Add(subTokenList);
                    subTokenList = new LinkedList<Token>();
                    tok = funcToken;
                }
                tok = readNextToken(subTokenList, tok, str);
                if (tok is LeftBracketToken)
                {
                    open++;
                    if (!withBrackets)
                    { withBrackets = true; subTokenList.RemoveLast(); };
                } 
                else if (tok is RightBracketToken) close++;
                if (tok is FuncToken func) subTokenList.AddLast(tok = readFuncParams(func, str));
            }
            while (!str.AtEnd && (open != close));
            if (open != close) throw new ParserBracketException("Unequal number of opening and closing brackets.", str.Position);
            paramtok.ParamList.Add(subTokenList);
            if (withBrackets) paramtok.ParamList[paramtok.ParamList.Count - 1].RemoveLast();
            if (removeVar != null)
                TokenDict.Remove(removeVar);
            return paramtok;
        }

        /// <summary>
        /// Reads the next token from the input stream and adds it to the given token list.
        /// </summary>
        private Token readNextToken(LinkedList<Token> tokenList, Token lastToken, InputStream str)
        {
            if (str.AtEnd) throw new ParserSyntaxException(str.Position);
            // Check for numbers
            if (numberReader(str, out Token outtok))
            {
                tokenList.AddLast(outtok);
                return outtok;
            }
            // Check for operators, functions, vars
            foreach (var item in TokenDict)
            {
                int pos = str.Position;
                if (str.StartsWith(item.Key))
                {
                    Token newToken;
                    if (item.Value.Item2 != 0)
                        newToken = (Token)Activator.CreateInstance(item.Value.Item1, item.Key, pos, item.Value.Item2);
                    else if (item.Value.Item1 == typeof(LeftBracketToken) || item.Value.Item1 == typeof(RightBracketToken))
                        newToken = (Token)Activator.CreateInstance(item.Value.Item1, pos);
                    else newToken = (Token)Activator.CreateInstance(item.Value.Item1, item.Key, pos);
                    // Special handling for '+' and '-' signs
                    if (lastToken is null || lastToken is StructToken || lastToken is LeftBracketToken)
                    {
                        if (item.Key == ParserSymbols.Add)
                        {
                            Token nextToken = readNextToken(tokenList, newToken, str);
                            if (nextToken is OperatorToken)
                                throw new ParserSyntaxException("Invalid operator sequence.", pos);
                            return nextToken;
                        }
                        else if (item.Key == ParserSymbols.Subtract)
                        {
                            var signToken = new SignOperatorToken(pos);
                            tokenList.AddLast(signToken);
                            return signToken;
                        }
                    }
                    // Special handling for syntax that omits the '*' operator
                    if (newToken is VarToken || newToken is ConstantToken)
                    {
                        if (lastToken is NumberToken)
                            tokenList.AddLast(new OperatorToken(ParserSymbols.Multiply, pos));
                    }
                    tokenList.AddLast(newToken);
                    return newToken;
                }
            }
            // If no valid valid token was found
            throw new ParserCharException(str.Position, str.FirstChar());
        }

        private bool numberReader(InputStream str, out Token outtok)
        {
            Match match = numberRegex.Match(str.InputString);
            if (match.Success)
            {
                outtok = new NumberToken(match.Value.Replace(" ", ""), str.Position);
                str.MoveForward(match.Length);
                return true;
            }
            outtok = null;
            return false;
        }
    }
}
