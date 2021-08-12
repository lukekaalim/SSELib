using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

using BlockStructure.Schemas;

namespace BlockStructure.Emit
{
    public class SchemaInheritanceBuilder
    {
        SchemaTypeBuilder SchemaTypes { get; set; }
        public Dictionary<NiObjectSchema, Type> ParentsBySchema { get; set; }

        public SchemaInheritanceBuilder(SchemaTypeBuilder schemaType)
        {
            SchemaTypes = schemaType;
            ParentsBySchema = schemaType
                .NiObjectTypes
                .ToDictionary(kvp => kvp.Key, kvp =>
                    SetParentBySchema(kvp.Value, kvp.Key));
        }


        public Type SetParentBySchema(TypeBuilder typeBuilder, NiObjectSchema schema)
        {
            if (schema.Inherits != null)
                typeBuilder.SetParent(SchemaTypes.SchemaTypesByName[schema.Inherits]);

            return typeBuilder.BaseType;
        }
    }
}
