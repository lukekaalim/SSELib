using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace BlockStructure.Schemas
{
    public class VersionSchema
    {
        public string Id { get; set; }
        public int Num { get; set; }

        public bool Supported { get; set; }

        public List<int> BethesdaVersions { get; set; }
        public List<int> UserVersions { get; set; }
        public List<string> NIFExtensions { get; set; }

        public VersionSchema(XElement element)
        {
            Id = element.Attribute("id").Value;
            Num = VersionParser.Parse(element.Attribute("num").Value);
            Supported = bool.Parse(element.Attribute("supported")?.Value ?? "true");

            BethesdaVersions = (element.Attribute("bsver")?.Value ?? "")
                .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(n => (int)Utils.ParseLong(n))
                .ToList();
            UserVersions = (element.Attribute("user")?.Value ?? "")
                .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(n => (int)Utils.ParseLong(n))
                .ToList();
            NIFExtensions = (element.Attribute("ext")?.Value ?? "")
                .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .ToList();
        }
    }
}
