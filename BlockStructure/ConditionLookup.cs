using System;
using System.Linq;
using System.Collections.Generic;

using BlockStructure.Schemas;
using BlockStructure.Logic;

namespace BlockStructure
{
    public class ConditionLookup
    {
        public Dictionary<FieldSchema, Expression> Conditions { get; set; }
        public Dictionary<FieldSchema, Expression> VersionConditions { get; set; }

        public ConditionLookup(
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
                .ToDictionary(f => f, f => {
                    var conditionString = token.ResolveSubsitutions(f.Condition, "cond");
                    var expression = Parser.Parse(Lexer.ReadSource(conditionString));
                    return expression;
                });
            VersionConditions = fields
                .Where(f => f.VersionCondition != null)
                .ToDictionary(f => f, f => {
                    var conditionString = token.ResolveSubsitutions(f.Condition, "vercond");
                    var expression = Parser.Parse(Lexer.ReadSource(conditionString));
                    return expression;
                });
        }
    }
}
