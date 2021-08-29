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

        public bool CheckCondition(FieldSchema field, Interpreter interpreter)
        {
            if (Conditions.TryGetValue(field, out var expression))
                return interpreter.Evaluate(expression) != 0;
            return true;
        }
        public bool CheckVersionCondition(FieldSchema field, Interpreter interpreter)
        {
            if (VersionConditions.TryGetValue(field, out var expression))
                return interpreter.Evaluate(expression) != 0;
            return true;
        }
        public int GetCount(FieldSchema field, Interpreter interpreter)
        {
            if (Dimensions.TryGetValue(field, out var dimensions))
            {
                return (int)dimensions
                    .Select(dimension => interpreter.Evaluate(dimension))
                    .Aggregate((a, b) => a * b);
            }
            return -1;
        }
        public long GetArgument(FieldSchema field, Interpreter interpreter)
        {
            if (Arguments.TryGetValue(field, out var argument))
            {
                return interpreter.Evaluate(argument);
            }
            return 0;
        }
    }
}
