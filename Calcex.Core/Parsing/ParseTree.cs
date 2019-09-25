using System;
using System.Collections.Generic;
using Calcex.Parsing.Tokens;

namespace Calcex.Parsing
{
    internal static class ParseTree
    {
        /// <summary>
        /// Creates an abstract syntax tree using the Shunting-yard algorithm.
        /// </summary>
        /// <param name="tokens">The tokenized expression.</param>
        /// <returns>A syntax tree representing the given expression.</returns>
        public static TreeToken CreateTree(LinkedList<Token> tokens)
        {
            Stack<Token> operatorStack = new Stack<Token>();
            Stack<Token> outputStack = new Stack<Token>();
            LinkedListNode<Token> node = tokens.First;
            while (node != null)
            {
                Token token = node.Value;
                System.Diagnostics.Debug.WriteLine("> " + token);
                // Values
                if (token is ValueToken)
                {
                    outputStack.Push(token);
                }
                // Functions
                else if (token is FuncToken)
                {
                    operatorStack.Push(token);
                }
                // Function parameters
                else if (token is FuncParamToken)
                {
                    outputStack.Push(token);
                }
                // Operator
                else if (token is OperatorToken opToken)
                {
                    while (operatorStack.Count > 0 && operatorStack.Peek() is StructToken stackPeekOp
                        && (OperatorProperties.GetPrecedence(opToken) < OperatorProperties.GetPrecedence(stackPeekOp)
                            || !OperatorProperties.IsRightAssociative(opToken) 
                                && OperatorProperties.GetPrecedence(opToken) == OperatorProperties.GetPrecedence(stackPeekOp))
                        )
                    {
                        createTreeNode(outputStack, operatorStack.Pop() as StructToken);
                    }
                    operatorStack.Push(token);
                }
                // Left bracket
                else if (token is LeftBracketToken)
                {
                    operatorStack.Push(token);
                }
                // Right bracket
                else if (token is RightBracketToken)
                {
                    while (true)
                    {
                        if (operatorStack.Count < 1)
                            throw new ParserBracketException("A closing bracket has no matching opening bracket.", token.Position);
                        Token stackToken = operatorStack.Pop();
                        if (stackToken is LeftBracketToken) break;
                        else createTreeNode(outputStack, (StructToken)stackToken);
                    }
                }
                node = node.Next;
                System.Diagnostics.Debug.WriteLine("ops: " + String.Join("|", (object[])operatorStack.ToArray()));
                System.Diagnostics.Debug.WriteLine("out: " + String.Join("|", (object[])outputStack.ToArray()));
            }
            // Pop all operators left.
            while (operatorStack.Count > 0)
            {
                Token token = operatorStack.Pop();
                if (token is LeftBracketToken)
                    throw new ParserBracketException("An opening bracket has no matching closing bracket.", token.Position);
                createTreeNode(outputStack, (StructToken)token);
            }
            if (outputStack.Count > 1)
                throw new ParserSyntaxException(outputStack.Peek().Position);
            else if (outputStack.Count < 1)
                throw new ParserSyntaxException(0);
            return outputStack.Pop() as TreeToken;
        }

        private static void createTreeNode(Stack<Token> stack, StructToken token)
        {
            if (token is SignOperatorToken signToken)
            {
                if (stack.Count < 1) throw new ParserSyntaxException(token.Position);
                signToken.SubTokens[1] = (TreeToken)stack.Pop();
                signToken.SubTokens[0] = new NumberToken("0", signToken.Position);
                stack.Push(signToken);
            }
            else if (token is OperatorToken opToken)
            {
                if (stack.Count < 2) throw new ParserSyntaxException(token.Position);
                opToken.SubTokens[1] = (TreeToken)stack.Pop();
                opToken.SubTokens[0] = (TreeToken)stack.Pop();
                stack.Push(opToken);
            }
            else if (token is FuncToken funcToken)
            {
                if (stack.Count < 1) throw new ParserSyntaxException(token.Position);
                var paramToken = stack.Pop() as FuncParamToken;
                if (paramToken == null)
                    throw new ParserSyntaxException(token.Position);
                if (funcToken.ArgCount != -1 && paramToken.ParamList.Count != funcToken.ArgCount)
                    throw new ParserFunctionArgumentException(funcToken.Symbol, token.Position, paramToken.ParamList.Count);
                int i = 0;
                foreach (var subTokenList in paramToken.ParamList)
                {
                    funcToken.SubTokens.Add(CreateTree(subTokenList));
                    i++;
                }
                stack.Push(funcToken);
            }
        }
    }
}