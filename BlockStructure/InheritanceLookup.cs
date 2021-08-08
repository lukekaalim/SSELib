using System;
using System.Linq;
using System.Collections.Generic;

using BlockStructure.Schemas;

namespace BlockStructure
{
    public class InheritanceLookup
    {
        public Dictionary<NiObjectSchema, List<NiObjectSchema>> NiObjectInheritance { get; set; }
        public Dictionary<NiObjectSchema, List<FieldSchema>> NiObjectFields { get; set; }

        public InheritanceLookup(Dictionary<string, NiObjectSchema> niObjects)
        {
            NiObjectInheritance = niObjects.Values
                .ToDictionary(
                    ni => ni,
                    ni => GetInheritenceChain(ni, niObjects)
                );
            NiObjectFields = niObjects.Values
                .ToDictionary(
                    ni => ni,
                    ni => NiObjectInheritance[ni]
                        .SelectMany(inheritedNi => inheritedNi.Fields)
                        .ToList()
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

        public List<FieldSchema> GetFields(NiObjectSchema niObjectSchema)
        {
            if (NiObjectFields.TryGetValue(niObjectSchema, out var fields))
            {
                return fields;
            }
            throw new Exception();
        }

        public bool CheckIncludedInType(FieldSchema fieldSchema, NiObjectSchema parent)
        {
            if (parent == null)
                return true;

            if (NiObjectInheritance.TryGetValue(parent, out var inheritance))
            {
                if (fieldSchema.OnlyT != null)
                    if (!inheritance.Any(inheritedNi => inheritedNi.Name == fieldSchema.OnlyT))
                        return false;

                if (fieldSchema.ExcludeT != null)
                    if (inheritance.Any(inheritedNi => inheritedNi.Name == fieldSchema.ExcludeT))
                        return false;

                return true;
            }
            return true;
        }
    }
}
