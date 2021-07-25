using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;

namespace BlockStructure.Schemas
{
    public class EnumSchema
    {
        public class Option
        {
            public long Value { get; set; }
            public string Name { get; set; }

            public Option(XElement element)
            {
                Name = element.Attribute("name").Value;
                Value = Utils.ParseLong(element.Attribute("value").Value);
            }
        }

        public List<string> Versions { get; set; }
        public string Name { get; set; }
        public string Storage { get; set; }
        public List<Option> Options { get; set; }

        public EnumSchema(XElement element)
        {
            Name = element.Attribute("name").Value;
            Storage = element.Attribute("storage").Value;
            Versions = (element.Attribute("versions")?.Value ?? "")
                .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .ToList();
            Options = element.Elements()
                .Select(c => new Option(c))
                .ToList();
        }
    }

    public class BitflagsSchema
    {
        public struct Option
        {

        }

        public string Name { get; set; }
        public string Storage { get; set; }
        public List<string> Versions { get; set; }

        public List<Option> Options { get; set; }

        public BitflagsSchema(XElement element)
        {
            Name = element.Attribute("name").Value;
            Storage = element.Attribute("storage").Value;
            Versions = (element.Attribute("versions")?.Value ?? "")
                .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .ToList();
            Options = new List<Option>();
        }
    }

    public class BitFieldSchema
    {
        public class Member
        {
            public int Width { get; set; }
            public int Position { get; set; }
            public int Mask { get; set; }
            public string Type { get; set; }
        }

        public string Name { get; set; }
        public string Storage { get; set; }
        public List<string> Versions { get; set; }

        public List<Member> Members { get; set; }

        public BitFieldSchema(XElement element)
        {
            Name = element.Attribute("name").Value;
            Storage = element.Attribute("storage").Value;
            Versions = (element.Attribute("versions")?.Value ?? "")
                .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .ToList();
            Members = new List<Member>();
        }
    }
}
