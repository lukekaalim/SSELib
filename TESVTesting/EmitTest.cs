using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;

using NUnit.Framework;

using BlockStructure;
using BlockStructure.Emit;

using SSE.TESVNif;

namespace TESVTesting
{
    public class Emit
    {
        [Test]
        public void TestEmit()
        {
            
            var doc = NifSchema.Doc;
            var basicDescription = new BasicsDescription(doc, new Dictionary<string, Type>()
            {
                { "uint64", typeof(ulong) },
                { "int64", typeof(long) },
                { "ulittle32", typeof(uint) },
                { "uint", typeof(uint) },
                { "int", typeof(int) },
                { "ushort", typeof(ushort) },
                { "short", typeof(short) },
                { "char", typeof(char) },
                { "byte", typeof(byte) },
                { "bool", typeof(bool) },
                { "BlockTypeIndex", typeof(int) },
                { "FileVersion", typeof(int) },
                { "float", typeof(float) },
                { "hfloat", typeof(float) },
                { "HeaderString", typeof(string) },
                { "LineString", typeof(string) },
                { "Ptr", typeof(int) },
                { "Ref", typeof(int) },
                { "StringOffset", typeof(uint) },
                { "NiFixedString", typeof(uint) },
            });
            var version = doc.Versions
                .Find(v => v.Id == "V20_2_0_7_SSE")
                .GetVersionKeys()
                .First();

            var assembly = SchemaDocumentBuilder.CreateNewAssembly(NifSchema.Doc,
                                                                   basicDescription,
                                                                   version);
            //var a = assembly.GetReferencedAssemblies();
            //var b = Assembly.GetExecutingAssembly();
            //var c = b.GetReferencedAssemblies();
            //var d = typeof(Enum);
            var generator = new Lokad.ILPack.AssemblyGenerator();
            //generator.GenerateAssembly(assembly, "/Users/lukekaalim/projects/SSE-Data-Lib/test.dll");
            
        }

        [Test]
        public void TestLoad()
        {
            //var a = AccumFlags.ACCUM_NEG_FRONT;
        }
    }
}
