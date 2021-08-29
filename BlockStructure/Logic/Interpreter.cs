using System;
using System.Collections.Generic;

using static BlockStructure.Logic.Syntax.Operator;

namespace BlockStructure.Logic
{
    public class Interpreter
    {
        delegate bool EvaluateLogicalBinaryOperation(long left, long right);
        static Dictionary<Syntax.Operator, EvaluateLogicalBinaryOperation> LogicalBinaryEvaluators = new Dictionary<Syntax.Operator, EvaluateLogicalBinaryOperation>(new Syntax.Comparer())
        {
            { new Logical.And(),                    (l, r) => (l != 0) && (r != 0) },
            { new Logical.Or(),                     (l, r) => (l != 0) || (r != 0) },

            { new Equatable.LessThan(),             (l, r) => l < r },
            { new Equatable.LessThanOrEqual(),      (l, r) => l <= r },
            { new Equatable.GreaterThan(),          (l, r) => l > r },
            { new Equatable.GreaterThanOrEqual(),   (l, r) => l >= r },

            { new Equatable.Equal(),                (l, r) => l == r },
            { new Equatable.NotEqual(),             (l, r) => l != r },
        };

        delegate long EvaluatNumericalBinaryOperation(long left, long right);
        static Dictionary<Syntax.Operator, EvaluatNumericalBinaryOperation> NumericalBinaryEvaluators = new Dictionary<Syntax.Operator, EvaluatNumericalBinaryOperation>(new Syntax.Comparer())
        {
            { new Numerical.Addition(),         (l, r) => l + r },
            { new Numerical.Subtraction(),      (l, r) => l - r },
            { new Numerical.Multiplication(),   (l, r) => l * r },
            { new Numerical.Division(),         (l, r) => l / r },

            { new Bitwise.And(),                (l, r) => l & r },
            { new Bitwise.Or(),                 (l, r) => l | r },
            { new Bitwise.ShiftLeft(),          (l, r) => l << (int)r },
            { new Bitwise.ShiftRight(),         (l, r) => r >> (int)r },
        };

        public class State
        {
            public Dictionary<string, long> ParameterValues { get; set; }
            public Dictionary<string, State> ParameterCompoundStates { get; set; }
        }

        public State CurrentState { get; }

        public Interpreter(State state)
        {
            CurrentState = state;
        }

        public long Evaluate(Expression expression)
        {
            switch (expression)
            {
                case Expression.Nested nested:
                    return Evaluate(nested.Expression);
                case Expression.Parameter parameter:
                    return CurrentState.ParameterValues[parameter.Name];
                case Expression.Literal literal:
                    return literal.Value;
                case Expression.Operation.Binary binaryOperation:
                    return EvaluateBinaryOperation(binaryOperation);
                case Expression.Operation.Unary unaryOperation:
                    return EvaluateUnaryOperation(unaryOperation);
                default:
                    throw new Exception();
            }
        }

        long EvaluateUnaryOperation(Expression.Operation.Unary unary)
        {
            long left = Evaluate(unary.Operand);
            var op = unary.Operator;

            switch (op)
            {
                case Logical.Not _:
                    return (left != 0) ? 0 : 1;
                default:
                    throw new Exception();
            }
        }

        long EvaluateBinaryOperation(Expression.Operation.Binary binary)
        {
            var op = binary.Operator;

            if (op is Structural.Member member)
            {
                if (binary.LeftOperand is Expression.Parameter compParam &&
                    binary.RightOperand is Expression.MemberName memberName)
                {
                    return CurrentState
                        .ParameterCompoundStates[compParam.Name]
                        .ParameterValues[memberName.Name];
                }
                throw new Exception();
            }
            long left = Evaluate(binary.LeftOperand);
            long right = Evaluate(binary.RightOperand);

            switch (op)
            {
                case Numerical _:
                case Bitwise _:
                    return NumericalBinaryEvaluators[op](left, right);
                case Logical _:
                case Equatable _:
                    return LogicalBinaryEvaluators[op](left, right) ? 1 : 0;
                default:
                    throw new Exception();
            }
        }
    }
}
