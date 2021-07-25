using System;
using System.Collections.Generic;
using static BlockStructure.Logic.Syntax.Operator;

namespace BlockStructure.Logic
{
    public static class Interpreter
    {
        public class State
        {
            public Dictionary<string, Value> Identifiers { get; set; }
            public HashSet<string> EmptyIdentifiers { get; set; }
        }

        public static Value Interpret (Expression expression, State state = null)
        {
            switch (expression)
            {
                case NestedExpression nested:
                    return Interpret(nested.Expression, state);
                case BinaryOperationExpression operation:
                    var left = Interpret(operation.LeftOperand, state);
                    var right = Interpret(operation.RightOperand, state);
                    if (BinaryOps.TryGetValue(operation.Operator, out var performOp))
                        return performOp(left, right);
                    throw new NotImplementedException();
                case UnaryOperationExpression unaryOperation:
                    var value = Interpret(unaryOperation.Operand, state);
                    if (UnaryOps.TryGetValue(unaryOperation.Operator, out var performUnaryOp))
                        return performUnaryOp(value);
                    throw new NotImplementedException();
                case TextExpression text:
                    return ReadTextExpression(text, state);
                case null:
                    return null;
                default:
                    throw new NotImplementedException();
            }
        }

        public delegate Value PerformUnaOp(Value v);
        public static Dictionary<Syntax.Operator, PerformUnaOp> UnaryOps = new Dictionary<Syntax.Operator, PerformUnaOp>(new Syntax.Comparer())
        {
            { new Logical.Not(), (v) => Value.From(!v.AsBoolean) },
        };
        public delegate Value PerformBinOp(Value l, Value r);
        public static Dictionary<Syntax.Operator, PerformBinOp> BinaryOps = new Dictionary<Syntax.Operator, PerformBinOp>(new Syntax.Comparer())
        {
            { new Logical.And(), (l, r) => Value.From(l.AsBoolean && r.AsBoolean) },
            { new Logical.Or(), (l, r) => Value.From(l.AsBoolean || r.AsBoolean) },

            { new Numerical.Addition(), (l, r) => Value.From(l.AsInterger + r.AsInterger) },
            { new Numerical.Subtraction(), (l, r) => Value.From(l.AsInterger - r.AsInterger) },
            { new Numerical.Multiplication(), (l, r) => Value.From(l.AsInterger * r.AsInterger) },
            { new Numerical.Division(), (l, r) => Value.From(l.AsInterger / r.AsInterger) },

            { new Equatable.LessThan(), (l, r) => Value.From(l.AsInterger < r.AsInterger) },
            { new Equatable.LessThanOrEqual(), (l, r) => Value.From(l.AsInterger <= r.AsInterger) },
            { new Equatable.GreaterThan(), (l, r) => Value.From(l.AsInterger > r.AsInterger) },
            { new Equatable.GreaterThanOrEqual(), (l, r) => Value.From(l.AsInterger >= r.AsInterger) },

            { new Equatable.Equal(), (l, r) => Value.From(l.AsString == r.AsString) },
            { new Equatable.NotEqual(), (l, r) => Value.From(l.AsString != r.AsString) },

            { new Structural.Member(), (l, r) => l.AsStructure[r.AsString] },

            { new Bitwise.And(), (l, r) => Value.From(l.AsInterger & r.AsInterger) },
            { new Bitwise.Or(), (l, r) => Value.From(l.AsInterger | r.AsInterger) },
            { new Bitwise.ShiftLeft(), (l, r) => Value.From(l.AsInterger << (int)r.AsInterger) },
            { new Bitwise.ShiftRight(), (l, r) => Value.From(l.AsInterger >> (int)r.AsInterger) },
        };

        public static Value ReadTextExpression (TextExpression expression, State state)
        {
            if (int.TryParse(expression.Text, out var interger))
                return new IntergerValue(interger);

            if (expression.Text.StartsWith("0x"))
                    return new IntergerValue(int.Parse(
                        expression.Text.Substring(2),
                        System.Globalization.NumberStyles.AllowHexSpecifier
                    ));

            if (state != null && state.Identifiers.ContainsKey(expression.Text))
                return state.Identifiers[expression.Text];

            if (expression.Text.Contains("."))
                return new IntergerValue(VersionParser.Parse(expression.Text));

            return new StringValue(expression.Text);
        }
    }
}
