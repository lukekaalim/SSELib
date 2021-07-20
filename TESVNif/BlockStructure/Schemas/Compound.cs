using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml.Linq;

namespace SSE.TESVNif.BlockStructure.Schemas
{
    public class CompoundSchema
    {
        public string Name { get; set; }
        public bool Generic { get; set; }

        public List<FieldSchema> Fields { get; set; }

        public CompoundSchema(XElement element)
        {
            Name = element.Attribute("name").Value;
            Generic = bool.Parse(element.Attribute("generic")?.Value ?? "false");
            
            Fields = element.Elements()
                .Select(e => new FieldSchema(e))
                .ToList();
        }
    }
}
