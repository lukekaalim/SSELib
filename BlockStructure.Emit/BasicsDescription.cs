using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

using BlockStructure.Schemas;

namespace BlockStructure.Emit
{
    /// <summary>
    /// Description of basics in a document, and how they map to underlying
    /// primitive types
    /// </summary>
    public class BasicsDescription
    {
        public class BasicTypeDescription
        {
            public Type UnderlyingType { get; set; }
            public BasicSchema Schema { get; set; }

            public BasicTypeDescription(Type underlyingType, BasicSchema schema)
            {
                UnderlyingType = underlyingType;
                Schema = schema;
            }
        }

        public Dictionary<BasicSchema, BasicTypeDescription> BasicDescriptions { get; set; }

        public BasicsDescription(SchemaDocument document, Dictionary<string, Type> basicTypeMap)
        {
            BasicDescriptions = basicTypeMap
                .Select(kvp => new BasicTypeDescription(kvp.Value, document.Basics[kvp.Key]))
                .ToDictionary(desc => desc.Schema);
        }
    }
}
