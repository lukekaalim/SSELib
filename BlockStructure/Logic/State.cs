using System.Collections.Generic;
using System.Linq;

using BlockStructure.Schemas;

namespace BlockStructure.Logic
{
    /*
    /// <summary>
    /// Context for variables mentioned in expressions
    /// </summary>
    public class State
    {
        public Dictionary<string, Oldvalue> Values { get; set; }

        public State(VersionKey key)
        {
            Values = new Dictionary<string, Oldvalue>();
            Values.Add("Version", Oldvalue.From(key.NifVersion));

            if (key.BethesdaVersion != null)
                Values.Add("BS Header", new StructureValue(new Dictionary<string, Oldvalue>()
                {
                    { "BS Version", Oldvalue.From(key.BethesdaVersion.Value) }
                }));
            else
                Values.Add("BS Header", new StructureValue(new Dictionary<string, Oldvalue>()
                {
                    { "BS Version", Oldvalue.From(0) }
                }));

            if (key.UserVersion != null)
                Values.Add("User Version", Oldvalue.From(key.UserVersion.Value));
            else
                Values.Add("User Version", Oldvalue.From(0));
        }

        public State(IEnumerable<FieldSchema> fields, Oldvalue argument = null)
        {
            Values = fields
                .GroupBy(field => field.Name)
                .ToDictionary(l => l.Key, _ => Oldvalue.From(0));

            if (argument != null)
                Values.Add("Argument", argument);
        }

        public void Set(FieldSchema field, Oldvalue value)
        {
            Values[field.Name] = value;
        }
    }
    */
}
