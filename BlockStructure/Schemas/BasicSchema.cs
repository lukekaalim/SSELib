using System;
using System.Linq;
using System.Xml.Linq;

namespace BlockStructure.Schemas
{
    public class BasicSchema
    {
        public string Name { get; set; }

        public bool Intergral { get; set; }
        public bool Countable { get; set; }
        public bool Boolean { get; set; }
        public bool Generic { get; set; }

        public int Size { get; set; }

        public BasicSchema(XElement element)
        {
            Name = element.Attribute("name").Value;

            Intergral = bool.Parse(element.Attribute("integral")?.Value ?? "false");
            Countable = bool.Parse(element.Attribute("countable")?.Value ?? "false");
            Boolean = bool.Parse(element.Attribute("boolean")?.Value ?? "false");
            Generic = bool.Parse(element.Attribute("generic")?.Value ?? "false");

            Size = int.Parse(element.Attribute("size")?.Value ?? "-1");
        }
    }
}
