using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;

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
            var doc = NifSchema.LoadEmbedded();
            var basics = new Dictionary<string, Type>()
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
            };
            var version = doc.Versions
                .Find(v => v.Id == "V20_2_0_7_SSE")
                .GetVersionKeys()
                .First();

            var name = new AssemblyName("MyNonStandardLibrary");
            var access = AssemblyBuilderAccess.Run;
            var assembly = AssemblyBuilder.DefineDynamicAssembly(name, access);
            var module = assembly.DefineDynamicModule("MyNonStandardLibrary.dll");

            var schema = new SchemaDocumentBuilder(module, doc, basics, version);

            var generator = new Lokad.ILPack.AssemblyGenerator();
            generator.GenerateAssembly(assembly, "./MyNonStandardLibrary.dll");
        }

        [Test]
        public void TestLoad()
        {
            //var reader = new DefaultReader();
            //var sizedString = new SizedString(reader);
            //var assembly = Assembly.LoadFile("/Users/lukekaalim/projects/SSE-Data-Lib/test.dll");
            //var sizedT = assembly.DefinedTypes.First(t => t.Name == "SizedString");
            //var sized = Activator.CreateInstance(sizedT);
            //var sized = Activator.CreateInstance(typeof(SizedString));
        }
    }
}
