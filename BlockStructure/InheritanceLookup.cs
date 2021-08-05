using System;
using System.Linq;
using System.Collections.Generic;

using BlockStructure.Schemas;

namespace BlockStructure
{
    public class InheritanceLookup
    {
        public Dictionary<string, List<NiObjectSchema>> NiObjectInheritance { get; set; }

        public InheritanceLookup(Dictionary<string, NiObjectSchema> NiObjects)
        {
            NiObjectInheritance = NiObjects.Values
                .ToDictionary(
                    ni => ni.Name,
                    ni => GetInheritenceChain(ni, NiObjects)
                );
        }

        public List<NiObjectSchema> GetInheritenceChain(
            NiObjectSchema schema,
            Dictionary<string, NiObjectSchema> niObjects)
        {
            if (schema.Inherits == null)
                return new List<NiObjectSchema>() { schema };

            var chain = GetInheritenceChain(niObjects[schema.Inherits], niObjects);
            chain.Add(schema);
            return chain;
        }
    }
}
