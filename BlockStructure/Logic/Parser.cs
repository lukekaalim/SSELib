using System;
using System.Linq;
using System.Collections.Generic;

namespace BlockStructure.Logic
{
    public static class Parser
    {
        public static Expression Parse(List<Token> tokens)
        {
            var (_, parts) = ParseParts(tokens);
            var expression = AssembleExpression(parts);

            return expression;
        }

        public static (int, List<ExpressionPart>) ParseParts(List<Token> tokens, int startOffset = 0, int depth = 0)
        {
            var parts = new List<ExpressionPart>();
            for (int offset = startOffset; offset < tokens.Count; offset++)
            {
                var token = tokens[offset];
                switch (token)
                {
                    case SyntaxToken syntax:
                        switch (syntax.Type)
                        {
                            // Punchuators
                            case Syntax.Punchuator.OpenParen open:
                                var (nestedOffset, nestedParts) = ParseParts(tokens, offset + 1, depth + 1);
                                offset = nestedOffset;
                                var nested = AssembleExpression(nestedParts);
                                parts.Add(new OperandPart(new NestedExpression(nested)));
                                continue;
                            case Syntax.Punchuator.CloseParen close:
                                if (depth == 0)
                                    throw new Exception("Closing parent on root!");
                                else
                                    return (offset, parts);
                            // Operators
                            case Syntax.Operator operatorType:
                                parts.Add(new OperatorPart(operatorType));
                                continue;
                            default:
                                throw new Exception("Unknown Syntax type");
                        }
                    case TextToken text:
                        if (
                            parts.Count > 0 &&
                            parts.Last() is OperandPart operand &&
                            operand.Operand is TextExpression identifier
                        )
                            identifier.Text += " " + text.Content;
                        else
                            parts.Add(new OperandPart(new TextExpression(text.Content)));
                        continue;
                }
            }
            return (tokens.Count - 1, parts);
        }

        public static Expression AssembleExpression(List<ExpressionPart> parts)
        {
            if (parts.Count == 0)
                return null;
            if (parts.Count == 1)
                if (parts[0] is OperandPart operand)
                    return operand.Operand;
                else
                    throw new Exception("Operator with no arguments");

            OperatorPart priorityOperator = null;
            int index = -1;

            for (int i = 0; i < parts.Count; i++)
            {
                var part = parts[i];
                if (part is OperatorPart operatorPart)
                {
                    if (priorityOperator == null || (operatorPart.Precedence > priorityOperator.Precedence))
                    {
                        index = i;
                        priorityOperator = operatorPart;
                    }
                }
            }

            switch (priorityOperator.Operator)
            {
                case Syntax.Operator.Logical.Not not:
                    return new UnaryOperationExpression()
                    {
                        Operator = priorityOperator.Operator,
                        Operand = AssembleExpression(parts.Skip(index + 1).ToList())
                    };
                default:
                    var leftParts = parts.Take(index).ToList();
                    var rightParts = parts.Skip(index + 1).ToList();
                    return new BinaryOperationExpression()
                    {
                        Operator = priorityOperator.Operator,
                        LeftOperand = AssembleExpression(leftParts),
                        RightOperand = AssembleExpression(rightParts),
                    };

            }
        }
    }
}
