using System;
using System.Collections.Generic;

namespace SSE.TESVNif.BlockStructure.Logic
{
    public static class Lexer
    {
        public static Token ReadNextToken(string source, ref int offset)
        {
            if (SyntaxToken.IsSyntaxChar(source[offset]))
            {
                var (nextOffset, nextToken) = SyntaxToken.Read(source, offset);
                offset = nextOffset;
                return nextToken;
            }
            else if (TextToken.IsTextChar(source[offset]))
                return TextToken.Read(source, ref offset);
            else
                return null;
        }

        public static List<Token> ReadSource(string source, int offset = 0)
        {
            var tokens = new List<Token>();
            for (int i = offset; i < source.Length; i++)
            {
                if (char.IsWhiteSpace(source[i]))
                    continue;
                var token = ReadNextToken(source, ref i);
                if (token != null)
                    tokens.Add(token);
            }
            return tokens;
        }
    }
}
