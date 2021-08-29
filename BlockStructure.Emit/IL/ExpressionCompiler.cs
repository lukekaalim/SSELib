using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

using BlockStructure.Schemas;
using BlockStructure.Logic;

namespace BlockStructure.Emit.IL
{
    public class ExpressionCompiler
    {
        public class State
        {
            public Dictionary<string, FieldInfo> FieldByName { get; set; }

            public State() { }
            public State(Dictionary<FieldSchema, FieldBuilder> buildersBySchema)
            {
                FieldByName = buildersBySchema
                    .ToDictionary(b => b.Key.Name, b => b.Value as FieldInfo);
            }
            public State(IEnumerable<(FieldSchema, FieldBuilder)> fieldByName)
            {
                FieldByName = fieldByName
                    .GroupBy(p => p.Item1.Name)
                    .Select(g => g.First())
                    .Select(p => (p.Item1.Name, p.Item2))
                    .ToDictionary(p => p.Item1, b => b.Item2 as FieldInfo);
            }
        }

        public ExpressionCompiler(State state)
        {
            CurrentState = state;
        }

        public State CurrentState { get; set; }

        public Instruction LoadExpression(Expression expr)
        {
            switch (expr)
            {
                case Expression.Parameter parameter:
                    return LoadParameter(parameter);
                case Expression.Operation.Binary bin:
                    return LoadBinaryOperationExpression(bin);
                case Expression.Literal literal:
                    return LoadNumberLiteralExpression(literal);
                default:
                    throw new Exception();
            }
        }

        Instruction LoadParameter(Expression.Parameter parameter)
        {
            if (CurrentState.FieldByName.TryGetValue(parameter.Name, out var field))
            {
                return new Instruction.Set(
                    new Instruction.LoadThis(),
                    new Instruction.LoadField(field)
                );
            }
            throw new Exception($"Field \"{parameter.Name}\" does not exist in state");
        }

        Instruction LoadBinaryOperationExpression(Expression.Operation.Binary bin)
        {
            var loadLeft = LoadExpression(bin.LeftOperand);
            var loadRight = LoadExpression(bin.RightOperand);
            switch (bin.Operator)
            {
                case Syntax.Operator.Numerical.Addition:
                    return new Instruction.Set(loadLeft, loadRight, new Instruction.Add());
                default:
                    throw new Exception();
            }
        }

        Instruction LoadNumberLiteralExpression(Expression.Literal literal)
        {
            return new Instruction.LoadInt32((int)literal.Value);
        }
    }
}
