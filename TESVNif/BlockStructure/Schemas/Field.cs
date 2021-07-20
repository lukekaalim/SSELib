using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml.Linq;

namespace SSE.TESVNif.BlockStructure.Schemas
{
    public class FieldSchema
    {
        public string Name { get; set; }
        public string Type { get; set; }

        public int? MinVersion { get; set; }
        public int? MaxVersion { get; set; }

        public List<string> Dimensions { get; set; }

        public string Condition { get; set; }
        public string VersionCondition { get; set; }
        public string OnlyT { get; set; }

        public string Argument { get; set; }
        public string Template { get; set; }

        public FieldSchema(XElement element)
        {
            Name = element.Attribute("name").Value;
            Type = element.Attribute("type").Value;

            MinVersion = VersionParser.Parse(element.Attribute("ver1"));
            MaxVersion = VersionParser.Parse(element.Attribute("ver2"));

            Dimensions = new string[]
            {
                element.Attribute("arr1")?.Value,
                element.Attribute("arr2")?.Value,
            }.Where(a => a != null).ToList();

            Condition = element.Attribute("cond")?.Value;
            VersionCondition = element.Attribute("vercond")?.Value;
            OnlyT = element.Attribute("onlyT")?.Value;

            Argument = element.Attribute("arg")?.Value;
            Template = element.Attribute("template")?.Value;
        }
    }
}
