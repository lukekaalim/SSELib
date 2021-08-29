using System;
using System.Linq;
using System.Collections.Generic;

using BlockStructure.Schemas;
using BlockStructure.Logic;

namespace BlockStructure
{
    public class ParameterLookup
    {
        public IReadOnlyDictionary<CompoundSchema, IReadOnlyList<string>> CompoundConditionParameterNames { get; }

        public ParameterLookup(IEnumerable<CompoundSchema> schemas, ExpressionLookup expressionLookup)
        {
            CompoundConditionParameterNames = schemas
                .Select(s => (s, s.Fields
                    .Where(f => f.Condition != null)
                    .SelectMany(f => expressionLookup.Conditions[f].FindAll<Expression.Parameter>())))
                .ToDictionary(
                    sp => sp.s,
                    sp => sp.Item2
                        .GroupBy(p => p.Name)
                        .Select(g => g.First().Name)
                        .ToList() as IReadOnlyList<string>
                );
        }

        public Interpreter.State BuildCompoundState(CompoundSchema compound, long argument)
        {
            return null;
        }
        public Interpreter.State BuildNiObjectState(NiObjectSchema compound)
        {
            return null;
        }
    }
}
