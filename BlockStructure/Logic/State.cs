using System.Collections.Generic;
using System.Linq;

using BlockStructure.Schemas;

namespace BlockStructure.Logic
{
    /// <summary>
    /// Context for variables mentioned in expressions
    /// </summary>
    public class State
    {
        public Dictionary<string, Value> Values { get; set; }

        public State(VersionKey key)
        {
            Values = new Dictionary<string, Value>();
            Values.Add("Version", Value.From(key.NifVersion));

            if (key.BethesdaVersion != null)
                Values.Add("BS Header", new StructureValue(new Dictionary<string, Value>()
                {
                    { "BS Version", Value.From(key.BethesdaVersion.Value) }
                }));
            else
                Values.Add("BS Header", new StructureValue(new Dictionary<string, Value>()
                {
                    { "BS Version", Value.From(0) }
                }));

            if (key.UserVersion != null)
                Values.Add("User Version", Value.From(key.UserVersion.Value));
            else
                Values.Add("User Version", Value.From(0));
        }

        public State(IEnumerable<FieldSchema> fields, Value argument = null)
        {
            Values = fields
                .GroupBy(field => field.Name)
                .ToDictionary(l => l.Key, _ => Value.From(0));

            if (argument != null)
                Values.Add("Argument", argument);
        }

        public void Set(FieldSchema field, Value value)
        {
            Values[field.Name] = value;
        }
    }
}
