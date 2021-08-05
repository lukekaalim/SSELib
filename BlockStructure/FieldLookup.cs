using System;
using System.Linq;
using System.Collections.Generic;

using BlockStructure.Schemas;

namespace BlockStructure
{
    public class FieldLookup
    {
        public Dictionary<TypeKey, List<FieldSchema>> FieldsByKey { get; set; }

        public FieldLookup(
            Dictionary<string, CompoundSchema> compounds,
            Dictionary<string, NiObjectSchema> niObjects,
            TypeReferenceLookup typeLookup)
        {

        }
    }
}
