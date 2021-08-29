using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

using NUnit.Framework;

using BlockStructure.Emit.IL;
using BlockStructure.Logic;

namespace TESVTesting
{
    public class ExpressionTest
    {
        [Test]
        public void TestLexer()
        {
            var tokens = Lexer.ReadSource("10.0.1.2 + 0x05b << 5");
            var parser = new Parser(tokens);
            var expression = parser.ParseUntilComplete();
        }
    }
}
