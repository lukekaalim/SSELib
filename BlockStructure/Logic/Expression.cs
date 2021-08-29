using System;
using System.Linq;
using System.Collections.Generic;

namespace BlockStructure.Logic
{
    public abstract class Expression
    {
        public class Literal : Expression
        {
            public long Value { get; set; }
            public Literal(long value) =>
                Value = value;
        }
        public class Parameter : Expression
        {
            public string Name { get; set; }
            public Parameter(string name) =>
                Name = name;
        }
        public class Nested : Expression
        {
            public Expression Expression { get; set; }
            public Nested(Expression expression) =>
                Expression = expression;
        }
        public class MemberName : Expression
        {
            public string Name { get; set; }
            public MemberName(string name) =>
                Name = name;
        }
        public abstract class Operation : Expression
        {
            public Syntax.Operator Operator { get; set; }
            public abstract List<Expression> Operands { get; }

            public class Unary : Operation
            {
                public Expression Operand { get; set; }

                public override List<Expression> Operands =>
                    new List<Expression> { Operand };
            }

            public class Binary : Operation
            {
                public Expression LeftOperand { get; set; }
                public Expression RightOperand { get; set; }

                public override List<Expression> Operands =>
                    new List<Expression> { LeftOperand, RightOperand };
            }
        }
        /// <summary>
        /// Parts! Used for ordering expression precedence.
        /// </summary>
        public class Part
        {
            public class Operator : Part
            {
                public Syntax.Operator Value { get; set; }
                public int Precedence =>
                    OperatorPrecedence.ContainsKey(Value) ?
                    OperatorPrecedence[Value] : 0;

                public Operator(Syntax.Operator operatorType) =>
                    Value = operatorType;

                public static Dictionary<Syntax.Operator, int> OperatorPrecedence = new Dictionary<Syntax.Operator, int>(new Syntax.Comparer())
                {
                    { new Syntax.Operator.Structural.Member(), -2 },
                    { new Syntax.Operator.Logical.Not(), -1 },
                    { new Syntax.Operator.Logical.And(), 1 },
                    { new Syntax.Operator.Logical.Or(), 2 },
                };
            }

            public class Operand : Part
            {
                public Expression Value { get; set; }
                public Operand(Expression operand) =>
                    Value = operand;
            }
        }

        public List<T> FindAll<T>() where T : Expression
        {
            var children = FindInChildren();

            if (this is T foundT)
                children.Add(foundT);

            return children;

            List<T> FindInChildren()
            {
                switch (this)
                {
                    case Nested nested:
                        return nested.Expression.FindAll<T>();
                    case Operation operation:
                        return operation.Operands
                            .SelectMany(op => op.FindAll<T>())
                            .ToList();
                    default:
                        return new List<T>();
                }
            }
        }


    }
}
