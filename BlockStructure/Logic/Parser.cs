using System;
using System.Linq;
using System.Collections.Generic;

using static BlockStructure.Logic.Expression;
using static BlockStructure.Logic.Syntax;

namespace BlockStructure.Logic
{
    /// <summary>
    /// We assume that a list of tokens is essentially a soup of operators and
    /// operands, which we must sort into an expression tree.
    /// </summary>
    public class Parser
    {
        Stack<List<Part>> Parts;
        Stack<Token> Tokens;

        List<Part> CurrentParts => Parts.Peek();

        public Parser(List<Token> tokens)
        {
            Tokens = new Stack<Token>(tokens.Reverse<Token>());
            Parts = new Stack<List<Part>>();
            Parts.Push(new List<Part>());
        }

        public Expression ParseUntilComplete()
        {
            while (Tokens.Count > 0)
                ParseNextToken();
            return AssembleExpression(CurrentParts);
        }

        public static Expression Parse(List<Token> tokens)
        {
            return new Parser(tokens).ParseUntilComplete();
        }

        void ParseNextToken()
        {
            var nextToken = Tokens.Pop();
            switch (nextToken)
            {
                case TextToken text:
                    ParseTextToken(text);
                    return;
                case SyntaxToken syntax:
                    ParseSyntaxToken(syntax);
                    return;
                default:
                    throw new Exception("Unknown token type");
            }
        }
        void ParseSyntaxToken(SyntaxToken token)
        {
            switch (token.Type)
            {
                // Punchuators
                case Punchuator.OpenParen _:
                    Parts.Push(new List<Part>());
                    return;
                case Punchuator.CloseParen _:
                    if (Parts.Count == 1)
                        throw new Exception("Closing paren on root!");
                    var expression = AssembleExpression(Parts.Pop());
                    CurrentParts.Add(new Part.Operand(new Nested(expression)));
                    return;
                // Operators
                case Operator operatorType:
                    CurrentParts.Add(new Part.Operator(operatorType));
                    return;
                default:
                    throw new Exception("Unknown Syntax type");
            }
        }
        void ParseTextToken(TextToken token)
        {
            var lastPart = CurrentParts.LastOrDefault();

            if (lastPart is Part.Operator operatorPart)
            {
                if (operatorPart.Value is Syntax.Operator.Structural.Member)
                {
                    var memberExpression = new Expression.MemberName(token.Content);
                    CurrentParts.Add(new Part.Operand(memberExpression));
                    return;
                }
            }

            if (Utils.TryParseLong(token.Content, out var literalValue))
            {
                CurrentParts.Add(new Part.Operand(new Literal(literalValue)));
                return;
            }
            if (NIFVersion.TryParse(token.Content, out var nifVersion))
            {
                CurrentParts.Add(new Part.Operand(new Literal(nifVersion)));
                return;
            }

            var paramtereExpression = new Expression.Parameter(token.Content);
            CurrentParts.Add(new Part.Operand(paramtereExpression));
            return;
        }

        public static Expression AssembleExpression(List<Part> parts)
        {
            if (parts.Count == 0)
                return null;
            if (parts.Count == 1)
                if (parts[0] is Part.Operand operand)
                    return operand.Value;
                else
                    throw new Exception("Operator with no arguments");

            var (index, highest) = FindHighestPrecedenceIndex(parts);

            switch (highest.Value)
            {
                // all unary cases should be caught here
                case Operator.Logical.Not _:
                    return new Operation.Unary()
                    {
                        Operator = highest.Value,
                        Operand = AssembleExpression(parts.Skip(index + 1).ToList())
                    };
                default:
                    var leftParts = parts.Take(index).ToList();
                    var rightParts = parts.Skip(index + 1).ToList();
                    return new Operation.Binary()
                    {
                        Operator = highest.Value,
                        LeftOperand = AssembleExpression(leftParts),
                        RightOperand = AssembleExpression(rightParts),
                    };

            }
        }

        public static (int, Part.Operator) FindHighestPrecedenceIndex(List<Part> parts)
        {
            Part.Operator highest = null;
            int index = -1;

            for (int i = 0; i < parts.Count; i++)
            {
                var part = parts[i];
                if (part is Part.Operator operatorPart)
                {
                    if (highest == null)
                    {
                        index = i;
                        highest = operatorPart;
                    }
                    // this gives us our associativity; we scan left-to-right,
                    // overrwriting only if we see something higher
                    else if (operatorPart.Precedence > highest.Precedence)
                    {
                        index = i;
                        highest = operatorPart;
                    }
                }
            }

            return (index, highest);
        }
    }
}
