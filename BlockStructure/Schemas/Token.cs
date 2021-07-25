using System;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;

namespace BlockStructure.Schemas
{
    public class TokenSchema
    {
        public class Entry
        {
            public string Identifier { get; set; }
            public string Content { get; set; }

            public Entry (XElement element)
            {
                Identifier = element.Attribute("token").Value;
                Content = element.Attribute("string").Value;
            }
        }

        public List<Entry> Entries { get; set; }
        public List<string> Attributes { get; set; }
        public string Name { get; set; }

        public TokenSchema(XElement element)
        {
            Name = element.Attribute("name").Value;
            Attributes = element.Attribute("attrs").Value
                .Split(' ')
                .ToList();
            Entries = element.Descendants()
                .Select(d => new Entry(d))
                .ToList();
        }
    }
}
