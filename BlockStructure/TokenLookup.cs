using System;
using System.Linq;
using System.Collections.Generic;

namespace BlockStructure
{
    public class TokenLookup
    {
        public class AttributeEntryLookup
        {
            public Dictionary<string, string> Tokens { get; set; }
        }
        public Dictionary<string, AttributeEntryLookup> Attributes { get; set; }

        public TokenLookup(IEnumerable<Schemas.TokenSchema> tokens)
        {
            Attributes = new Dictionary<string, AttributeEntryLookup>();

            var entriesByAttribute = tokens
                .Reverse()
                .SelectMany(token =>
                    token.Attributes.SelectMany(attribute =>
                        token.Entries.Select(entry => (attribute, entry))))
                .GroupBy(ta => ta.attribute, ta => ta.entry);

            foreach (var attributeEntries in entriesByAttribute)
            {
                var attributeSubsitutions = new Dictionary<string, string>();
                var attributeLookup = new AttributeEntryLookup() {
                    Tokens = attributeSubsitutions
                };
                Attributes.Add(attributeEntries.Key, attributeLookup);
                foreach (var entry in attributeEntries)
                {
                    attributeSubsitutions.Add(
                        entry.Identifier.Substring(1, entry.Identifier.Length - 2),
                        ResolveSubsitutions(entry.Content, attributeEntries.Key)
                    );
                }
            }
        }

        public string ResolveSubsitutions(string content, string attribute)
        {
            var stringElements = content
                .Split('#')
                .Select((identifier, i) => i % 2 == 0 ?
                    identifier :
                    GetSubtitutionForIdentifier(identifier, attribute));
            return string.Join("", stringElements);
        }

        public string GetSubtitutionForIdentifier(string identifier, string attribute)
        {
            if (Attributes.TryGetValue(attribute, out var attributeLookup))
                if (attributeLookup.Tokens.TryGetValue(identifier, out var replacement))
                    return replacement;
            
            throw new Exception("Undefined subsitution");
        }
    }
}
