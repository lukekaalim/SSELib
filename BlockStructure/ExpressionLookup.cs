using System;
using System.Linq;
using System.Collections.Generic;

using BlockStructure.Schemas;
using BlockStructure.Logic;

namespace BlockStructure
{
    public class ExpressionLookup
    {
        public Dictionary<FieldSchema, Expression> Conditions { get; set; }
        public Dictionary<FieldSchema, Expression> VersionConditions { get; set; }
        public Dictionary<FieldSchema, List<Expression>> Dimensions { get; set; }
        public Dictionary<FieldSchema, Expression> Arguments { get; set; }

        public ExpressionLookup(
            ICollection<NiObjectSchema> niObjects,
            ICollection<CompoundSchema> compounds,
            TokenLookup token
            )
        {
            var fields = niObjects
                .SelectMany(ni => ni.Fields)
                .Concat(compounds.SelectMany(c => c.Fields));

            Conditions = fields
                .Where(f => f.Condition != null)
                .ToDictionary(f => f, f =>
                    ParseAttributeExpression(token, f.Condition, "cond"));
            VersionConditions = fields
                .Where(f => f.VersionCondition != null)
                .ToDictionary(f => f, f =>
                    ParseAttributeExpression(token, f.VersionCondition, "vercond"));
            Arguments = fields
                .Where(f => f.Argument != null)
                .ToDictionary(f => f, f =>
                    ParseAttributeExpression(token, f.Argument, "arg"));

            Dimensions = fields
                .Where(f => f.IsMultiDimensional)
                .ToDictionary(f => f, f =>
                    f.Dimensions
                    .Select((d, i) =>
                        ParseAttributeExpression(token, d, $"arr{i + 1}"))
                    .ToList());
        }

        public Expression ParseAttributeExpression(TokenLookup token, string source, string attribute)
        {
            var subsitutedSource = token.ResolveSubsitutions(source, attribute);
            var expression = Parser.Parse(Lexer.ReadSource(subsitutedSource));
            return expression;
        }

        public bool CheckCondition(FieldSchema field, State state)
        {
            if (Conditions.TryGetValue(field, out var expression))
                return Interpreter.Interpret(expression, state).AsBoolean;
            return true;
        }
        public bool CheckVersionCondition(FieldSchema field, VersionKey version)
        {
            if (VersionConditions.TryGetValue(field, out var expression))
                return Interpreter.Interpret(expression, version.State.Value).AsBoolean;
            return true;
        }
        public int GetCount(FieldSchema field, State state)
        {
            if (Dimensions.TryGetValue(field, out var dimensions))
            {
                return (int)dimensions
                    .Select(dimension => Interpreter.Interpret(dimension, state).AsInterger)
                    .Aggregate((a, b) => a * b);
            }
            return -1;
        }
        public Value GetArgument(FieldSchema field, State state)
        {
            if (Arguments.TryGetValue(field, out var argument))
            {
                return Interpreter.Interpret(argument, state);
            }
            return null;
        }
    }
}
