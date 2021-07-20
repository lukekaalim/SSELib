using System;
using System.Collections.Generic;

namespace SSE.TESVNif.BlockStructure.Logic
{
    public abstract class Expression { }

    public class NestedExpression : Expression
    {
        public Expression Expression { get; set; }
        public NestedExpression(Expression expression)
        {
            Expression = expression;
        }
    }

    public class TextExpression : Expression
    {
        public string Text { get; set; }
        public TextExpression(string identifer)
        {
            Text = identifer;
        }
    }

    public class OperationExpression : Expression
    {
        public Syntax.Operator Operator { get; set; }
    }

    public class UnaryOperationExpression : OperationExpression
    {
        public Expression Operand { get; set; }
    }

    public class BinaryOperationExpression : OperationExpression
    {
        public Expression LeftOperand { get; set; }
        public Expression RightOperand { get; set; }
    }

    public class ExpressionPart { }

    public class OperatorPart : ExpressionPart
    {
        public Syntax.Operator Operator { get; set; }
        public int Precedence
        {
            get
            {
                if (OperatorPrecedence.TryGetValue(Operator, out var precedence))
                    return precedence;
                else
                    return 0;
            }
        }
        public OperatorPart(Syntax.Operator operatorType)
        {
            Operator = operatorType;
        }

        public static Dictionary<Syntax.Operator, int> OperatorPrecedence = new Dictionary<Syntax.Operator, int>(new Syntax.Comparer())
            {
                { new Syntax.Operator.Structural.Member(), -2 },
                { new Syntax.Operator.Logical.Not(), -1 },
                { new Syntax.Operator.Logical.And(), 1 },
                { new Syntax.Operator.Logical.Or(), 2 },
            };
    }
    public class OperandPart : ExpressionPart
    {
        public Expression Operand { get; set; }
        public OperandPart(Expression operand)
        {
            Operand = operand;
        }
    }
}
