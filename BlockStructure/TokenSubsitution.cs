using System;
using System.Linq;
using System.Collections.Generic;

namespace BlockStructure
{
    public static class TokenSubsitution
    {
        public static string ResolveSubsitutions(string content, Dictionary<string, string> tokens)
        {
            var stringElements = content
                .Split('#')
                .Select((identifier, i) => i % 2 == 0 ?
                    identifier :
                    GetSubtitutionForIdentifier(identifier, tokens));
            return string.Join("", stringElements);
        }

        public static string GetSubtitutionForIdentifier(string identifier, Dictionary<string, string> tokens)
        {
            if (tokens.TryGetValue(identifier, out var globalIdentifer))
                return globalIdentifer;
            throw new Exception("Undefined subsitution");
        }
    }
}
