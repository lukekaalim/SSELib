using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace BlockStructure.Schemas
{
    public class SchemaDocument
    {
        public static List<TokenSchema> StaticTokens = new List<TokenSchema>()
        {
            new TokenSchema() {
                Attributes = new List<string>() { "cond", "arr1", "arr2", "arg" },
                Entries = new List<TokenSchema.Entry>() {
                    new TokenSchema.Entry() { Identifier = "#ARG#", Content = "Argument" }
                },
                Name = "Meta",
            }
        };

        public int SchemaVersion { get; set; }

        // Readable
        public Dictionary<string, NiObjectSchema> NiObjects { get; set; }
        public Dictionary<string, CompoundSchema> Compounds { get; set; }
        public Dictionary<string, BasicSchema> Basics { get; set; }
        public Dictionary<string, EnumSchema> Enums { get; set; }
        public Dictionary<string, BitFieldSchema> Bitfields { get; set; }
        public Dictionary<string, BitflagsSchema> Bitflags { get; set; }

        // Meta
        public List<TokenSchema> Tokens { get; set; }
        public List<VersionSchema> Versions { get; set; }

        // Precomputed
        public TokenLookup TokenLookup { get; set; }
        public InheritanceLookup InheritanceLookup { get; set; }
        public ExpressionLookup ExpressionLookup { get; set; }

        public SchemaDocument(XElement element)
        {
            SchemaVersion = VersionParser.Parse(element.Attribute("version").Value);

            var elementsByName = element.Elements()
                .ToLookup(e => e.Name);

            Tokens = elementsByName["token"]
                .Select(e => new TokenSchema(e))
                .ToList();
            Versions = elementsByName["version"]
                .Select(e => new VersionSchema(e))
                .ToList();

            NiObjects = elementsByName["niobject"]
                .Select(e => new NiObjectSchema(e))
                .ToDictionary(s => s.Name);
            Compounds = elementsByName["compound"]
                .Select(e => new CompoundSchema(e))
                .ToDictionary(s => s.Name);
            Basics = elementsByName["basic"]
                .Select(e => new BasicSchema(e))
                .ToDictionary(s => s.Name);
            Enums = elementsByName["enum"]
                .Select(e => new EnumSchema(e))
                .ToDictionary(s => s.Name);
            Bitfields = elementsByName["bitfield"]
                .Select(e => new BitFieldSchema(e))
                .ToDictionary(s => s.Name);
            Bitflags = elementsByName["bitflags"]
                .Select(e => new BitflagsSchema(e))
                .ToDictionary(s => s.Name);

            TokenLookup = new TokenLookup(Tokens.Concat(StaticTokens));
            InheritanceLookup = new InheritanceLookup(NiObjects);
            ExpressionLookup = new ExpressionLookup(NiObjects.Values, Compounds.Values, TokenLookup);
        }
    }
}
