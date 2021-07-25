using System;
using System.Collections.Generic;
using System.Linq;
using static BlockStructure.Logic.Syntax.Operator;
using static BlockStructure.Logic.Syntax.Punchuator;

namespace BlockStructure.Logic
{
    public class Token { }

    public class SyntaxToken : Token
    {
        public Syntax Type { get; set; }

        public static Dictionary<string, Syntax> SyntaxBySource = new Dictionary<string, Syntax>()
        {
            { "+", new Numerical.Addition() },
            { "-", new Numerical.Subtraction() },
            { "*", new Numerical.Multiplication() },
            { "/", new Numerical.Division() },

            { "==", new Equatable.Equal() },
            { "!=", new Equatable.NotEqual() },

            { "<", new Equatable.LessThan() },
            { "<=", new Equatable.LessThanOrEqual() },
            { ">", new Equatable.GreaterThan() },
            { ">=", new Equatable.GreaterThanOrEqual() },

            { "&&", new Logical.And() },
            { "||", new Logical.Or() },
            { "!", new Logical.Not() },

            { "&", new Bitwise.And() },
            { "|", new Bitwise.Or() },
            { "<<", new Bitwise.ShiftLeft() },
            { ">>", new Bitwise.ShiftRight() },

            { "(", new OpenParen() },
            { ")", new CloseParen() },

            { "\\", new Structural.Member() },
        };

        public SyntaxToken(Syntax type)
        {
            Type = type;
        }

        public static bool IsSyntaxChar(char character)
        {
            return SyntaxBySource.Keys.Any(k => k.Contains(character));
        }

        public static (int, SyntaxToken) Read(string source, int offset)
        {
            if (source.Length > offset + 2)
                if (SyntaxBySource.TryGetValue(source.Substring(offset, 2), out var twoCharOperator))
                    return (offset + 1, new SyntaxToken(twoCharOperator));
            if (SyntaxBySource.TryGetValue(source.Substring(offset, 1), out var oneCharOperator))
                return (offset, new SyntaxToken(oneCharOperator));
            return (offset, null);
        }
    }

    public class TextToken : Token
    {
        public string Content { get; set; }

        public TextToken(List<char> chars)
        {
            Content = new string(chars.ToArray());
        }
        public TextToken(string content)
        {
            Content = content;
        }
        public TextToken() { }

        public static HashSet<char> SpecialChars = new HashSet<char>(new char[] {
            '.',
            '_'
        });

        public static bool IsTextChar(char character)
        {
            if (char.IsWhiteSpace(character))
                return false;
            if (char.IsLetterOrDigit(character))
                return true;
            if (SpecialChars.Contains(character))
                return true;
            return false;
        }

        public static TextToken Read(string source, ref int offset)
        {
            var endIndex = source.ToList().FindIndex(offset, c => !IsTextChar(c));
            var length = (endIndex == -1 ? source.Length : endIndex) - offset;
            var token = new TextToken(source.Substring(offset, length));
            offset += (length - 1);
            return token;
        }
    }
}
