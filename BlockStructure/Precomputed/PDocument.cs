using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using BlockStructure.Schemas;

namespace BlockStructure.Precomputed
{
    /// <summary>
    /// The "PDocument" is the precomputed document, prepared to parse a specific
    /// version of a BlockStructured file.
    /// </summary>
    public class PDocumentSchema
    {
        public int SchemaVersion { get; set; }

        public VersionKey Key { get; set; }

        public int NifVersion { get; set; }
        public uint? BethesdaVersion { get; set; }
        public uint? UserVersion { get; set; }

        public SchemaDocument BaseDocument { get; set; }

        public Dictionary<string, PBlockSchema> Blocks { get; set; }

        public Dictionary<string, Logic.Value> StaticGlobals;

        public Dictionary<TypeReferenceKey, TypeReference> References;

        public Dictionary<string, Dictionary<string, string>> Tokens { get; set; }
        public Dictionary<string, List<NiObjectSchema>> NiObjectInheritance { get; set; }

        public List<NiObjectSchema> GetInheritenceChain(
            NiObjectSchema schema,
            Dictionary<string, NiObjectSchema> niObjects)
        {
            if (schema.Inherits == null)
                return new List<NiObjectSchema>() { schema };

            var chain = GetInheritenceChain(niObjects[schema.Inherits], niObjects);
            chain.Add(schema);
            return chain;
        }

        public PDocumentSchema(SchemaDocument baseDocument, VersionKey key)
        {
            BaseDocument = baseDocument;
            Key = key;

            NifVersion = key.NifVersion;
            BethesdaVersion = key.BethesdaVersion;
            UserVersion = key.UserVersion;

            References = new Dictionary<TypeReferenceKey, TypeReference>();

            // Global Stuff
            StaticGlobals = new Dictionary<string, Logic.Value>()
            {
                { "Version", Logic.Value.From(NifVersion) },
                { "User Version", Logic.Value.From(0) },
                { "BS Header", new Logic.StructureValue(new Dictionary<string, Logic.Value>()
                {
                    { "BS Version", Logic.Value.From(0) }
                }) },
            };

            if (BethesdaVersion is uint bsVersion)
                StaticGlobals["BS Header"] = new Logic.StructureValue(new Dictionary<string, Logic.Value>()
                {
                    { "BS Version", Logic.Value.From(bsVersion) }
                });
            if (UserVersion is uint userVersion)
                StaticGlobals["User Version"] = Logic.Value.From(userVersion);

            NiObjectInheritance = baseDocument.NiObjects.Values
                .ToDictionary(
                    ni => ni.Name,
                    ni => GetInheritenceChain(ni, baseDocument.NiObjects)
                );

            // Token stuff
            Tokens = new Dictionary<string, Dictionary<string, string>>();

            var attributes = baseDocument.Tokens
                .Reverse<TokenSchema>()
                .SelectMany(token =>
                    token.Attributes.SelectMany(attribute =>
                        token.Entries.Select(entry => (attribute, entry))))
                .GroupBy(ta => ta.attribute);

            foreach (var attribute in attributes)
            {
                var attributeSubsitutions = new Dictionary<string, string>();
                foreach (var (attr, entry) in attribute)
                {
                    attributeSubsitutions.Add(
                        entry.Identifier.Substring(1, entry.Identifier.Length - 2),
                        TokenLookup.ResolveSubsitutions(entry.Content, attributeSubsitutions)
                    );
                }
                Tokens.Add(attribute.Key, attributeSubsitutions);
            }
            // make this an implicit thing
            Tokens["cond"].Add("ARG", "Argument");
            Tokens["arr1"].Add("ARG", "Argument");
            Tokens["arr2"].Add("ARG", "Argument");
            Tokens["arg"].Add("ARG", "Argument");

            var compoundBlocks = baseDocument.Compounds
                .Where(c => !c.Value.Generic)
                .Select(c => new PBlockSchema(c.Value, new TypeContext() { Document = this }));
            var niBlocks = baseDocument.NiObjects
                .Select(n => new PBlockSchema(n.Value, new TypeContext() { Document = this, TName = n.Value.Name }));

            Blocks = compoundBlocks.Concat(niBlocks)
                .ToDictionary(b => b.Name);
        }

        public PReader Open(Stream stream)
        {
            return new PReader()
            {
                BasicReader = new BasicReader() { Version = NifVersion },
                Reader = new BinaryReader(stream),
                Document = this,
            };
        }
    }
}
