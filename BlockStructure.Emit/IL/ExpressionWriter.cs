using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

using BlockStructure.Schemas;
using BlockStructure.Logic;

namespace BlockStructure.Emit.IL
{
    public class ExpressionWriter
    {
        public class State
        {
            public Dictionary<string, FieldInfo> FieldByName { get; set; }
            public Dictionary<string, int> ArgumentsByName { get; set; }

            public State() { }
            public State(IEnumerable<(FieldSchema, FieldBuilder)> fieldByName, IEnumerable<string> arguments)
            {
                FieldByName = fieldByName
                    .GroupBy(p => p.Item1.Name)
                    .Select(g => g.First())
                    .Select(p => (p.Item1.Name, Field: p.Item2 as FieldInfo))
                    .ToDictionary(p => p.Name, p => p.Field);

                ArgumentsByName = arguments
                    .Select((a, i) => (Name: a, Index: i + 1))
                    .ToDictionary(p => p.Name, p => p.Index);
            }
        }

        State state;
        ILGenerator il;

        public ExpressionWriter(ILGenerator il, State state)
        {
            this.il = il;
            this.state = state;
        }

        public void WriteExpression(Expression expression)
        {
            switch (expression)
            {
                case Expression.Nested nested:
                    WriteExpression(nested.Expression);
                    return;
                case Expression.Operation.Binary binary:
                    WriteBinaryOperationExpression(binary);
                    return;
                case Expression.Operation.Unary unary:
                    WriteUnaryOperationExpression(unary);
                    return;
                case Expression.Parameter parameter:
                    WriteParameterExpression(parameter);
                    return;
                case Expression.Literal literal:
                    il.Emit(OpCodes.Ldc_I8, literal.Value);
                    return;
            }
        }

        void WriteParameterExpression(Expression.Parameter parameter)
        {
            il.Emit(OpCodes.Ldarg_0);

            if (state.ArgumentsByName.TryGetValue(parameter.Name, out var argumentIndex))
            {
                il.Emit(OpCodes.Ldarga_S, argumentIndex);
                return;
            }

            if (state.FieldByName.TryGetValue(parameter.Name, out var fieldInfo))
            {
                il.Emit(OpCodes.Ldfld, fieldInfo);
                if (fieldInfo.FieldType != typeof(long))
                    il.Emit(OpCodes.Conv_I8);
                return;
            }

            throw new Exception();
        }

        void WriteBinaryOperationExpression(Expression.Operation.Binary binary)
        {
            switch (binary.Operator)
            {
                case Syntax.Operator.Logical.And:
                    {
                        var end = il.DefineLabel();
                        var shortCircut = il.DefineLabel();
                        WriteExpression(binary.LeftOperand);
                        il.Emit(OpCodes.Brfalse, shortCircut);
                        WriteExpression(binary.RightOperand);
                        il.Emit(OpCodes.Br_S, end);

                        il.MarkLabel(shortCircut);
                        il.Emit(OpCodes.Ldc_I4_0);

                        il.MarkLabel(end);
                        return;
                    }
                case Syntax.Operator.Logical.Or:
                    {
                        var end = il.DefineLabel();
                        var shortCircut = il.DefineLabel();
                        WriteExpression(binary.LeftOperand);
                        il.Emit(OpCodes.Brtrue, shortCircut);
                        WriteExpression(binary.RightOperand);
                        il.Emit(OpCodes.Br_S, end);

                        il.MarkLabel(shortCircut);
                        il.Emit(OpCodes.Ldc_I4_1);

                        il.MarkLabel(end);
                        return;
                    }
            }
            WriteExpression(binary.RightOperand);
            WriteExpression(binary.LeftOperand);

            switch (binary.Operator)
            {
                // Numerical
                case Syntax.Operator.Numerical.Addition _:
                    il.Emit(OpCodes.Add); return;
                case Syntax.Operator.Numerical.Subtraction _:
                    il.Emit(OpCodes.Sub); return;
                case Syntax.Operator.Numerical.Division _:
                    il.Emit(OpCodes.Div); return;
                case Syntax.Operator.Numerical.Multiplication _:
                    il.Emit(OpCodes.Mul); return;

                // Equatable
                case Syntax.Operator.Equatable.Equal _:
                    il.Emit(OpCodes.Ceq); return;
                case Syntax.Operator.Equatable.NotEqual:
                    il.Emit(OpCodes.Ceq);
                    InvertStackVal();
                    return;
                case Syntax.Operator.Equatable.GreaterThan _:
                    il.Emit(OpCodes.Cgt); return;
                case Syntax.Operator.Equatable.LessThan _:
                    il.Emit(OpCodes.Clt); return;
                case Syntax.Operator.Equatable.GreaterThanOrEqual:
                    il.Emit(OpCodes.Clt);
                    InvertStackVal();
                    return;
                case Syntax.Operator.Equatable.LessThanOrEqual:
                    il.Emit(OpCodes.Cgt);
                    InvertStackVal();
                    return;

                case Syntax.Operator.Bitwise.And:
                    il.Emit(OpCodes.And); return;
                case Syntax.Operator.Bitwise.Or:
                    il.Emit(OpCodes.Or); return;
                default:
                    throw new Exception();
            }
        }
        void WriteUnaryOperationExpression(Expression.Operation.Unary unary)
        {

        }

        void InvertStackVal()
        {
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ceq);
        }
        void CircutBreak()
        {

        }
    }
}
