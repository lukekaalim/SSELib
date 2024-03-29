﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml.Linq;

namespace BlockStructure.Schemas
{
    public class NiObjectSchema : IContainsFields
    {
        public string Inherits { get; set; }
        public string Name { get; set; }
        public string Versions { get; set; }

        public List<FieldSchema> Fields { get; set; }

        public NiObjectSchema(XElement element)
        {
            Name = element.Attribute("name").Value;
            Versions = element.Attribute("versions")?.Value;
            Inherits = element.Attribute("inherit")?.Value;

            Fields = element.Elements()
                .Select(e => new FieldSchema(e))
                .ToList();
        }
    }
}
