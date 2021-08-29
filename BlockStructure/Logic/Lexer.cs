using System;
using System.Linq;
using System.Collections.Generic;

namespace BlockStructure.Logic
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
            var collapsedTokens = CollapseAdjacentTextTokens(tokens);
            return collapsedTokens;
        }

        static List<Token> CollapseAdjacentTextTokens(List<Token> input)
        {
            var output = new List<Token>();
            foreach (var token in input)
            {
                var prevToken = output.LastOrDefault();
                if (token is TextToken textToken)
                {
                    if (prevToken != null && prevToken is TextToken prevTextToken)
                    {
                        output[output.Count - 1] = TextToken.Merge(prevTextToken, textToken);
                        continue;
                    }
                }
                output.Add(token);
            }
            return output;
        }
    }
}
