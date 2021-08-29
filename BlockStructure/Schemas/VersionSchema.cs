using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace BlockStructure.Schemas
{
    public class VersionSchema
    {
        public string Id { get; set; }
        public int NifVersion { get; set; }

        public bool Supported { get; set; }

        public List<uint> BethesdaVersions { get; set; }
        public List<uint> UserVersions { get; set; }
        public List<string> NIFExtensions { get; set; }

        public VersionSchema(XElement element)
        {
            Id = element.Attribute("id").Value;
            NifVersion = NIFVersion.Parse(element.Attribute("num").Value);
            Supported = bool.Parse(element.Attribute("supported")?.Value ?? "true");

            if (element.Attribute("bsver") != null)
                BethesdaVersions = element.Attribute("bsver")
                    .Value
                    .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(n => (uint)Utils.ParseLong(n))
                    .ToList();
            if (element.Attribute("user") != null)
                UserVersions = element.Attribute("user")
                    .Value
                    .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(n => (uint)Utils.ParseLong(n))
                    .ToList();

            NIFExtensions = (element.Attribute("ext")?.Value ?? "")
                .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .ToList();
        }

        public List<VersionKey> GetVersionKeys()
        {
            var bsVersions = BethesdaVersions == null ?
                new List<uint?> { null } :
                BethesdaVersions.Select(v => new uint?(v));

            var userVersions = UserVersions == null ?
                new List<uint?> { null } :
                UserVersions.Select(v => new uint?(v));

            return bsVersions
                .SelectMany(bsVer => userVersions
                    .Select(userVer => new VersionKey(NifVersion, bsVer, userVer)))
                .ToList();
        }
    }
}
