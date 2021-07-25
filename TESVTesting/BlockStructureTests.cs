using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using NUnit.Framework;

using BlockStructure;

namespace TESVTesting
{
    public static class CharList
    {
        public static string ReadString(Data data)
        {
            var compound = (CompoundData)data;
            var charArray = compound
                .GetBasicList<char>("Value")
                .ToArray();
            return new string(charArray);
        }
    }

    public class BlockStructureTests
    {
        [Test]
        public void TestMorrowind()
        {
            var morrowindVersion = "V4_0_0_2";
            var mwFile = new FileInfo("/Users/lukekaalim/projects/SSE-Data-Lib/TestData/W_art_Ice_mace.NIF");
            using var mwStream = mwFile.OpenRead();

            var mwReader = new BlockStructure.Schemas
                .SchemaDocument(SSE.TESVNif.NIFReader.LoadDocument().Root)
                .Precompute(morrowindVersion)
                .Open(mwStream);

            var header = mwReader.ReadBlock("Header") as CompoundData;
            var blocks = new List<Data>();
            for (int i = 0; i < header.GetBasic<uint>("Num Blocks"); i++)
            {
                var type = CharList.ReadString(mwReader.ReadBlock("SizedString"));
                var block = mwReader.ReadBlock(type);
                blocks.Add(block);
            }
            var footer = mwReader.ReadBlock("Footer") as CompoundData;
            Console.WriteLine(blocks.Count);
        }

        [Test]
        async public Task TestSkyrim()
        {
            var skyVersion = "V20_2_0_7_SSE";
            var skyFile = new FileInfo("/Users/lukekaalim/projects/SSE-Data-Lib/TestData/axe01.nif");
            using var skyStream = skyFile.OpenRead();
            var bytes = new byte[skyStream.Length];
            await skyStream.ReadAsync(bytes);

            using var memStream = new MemoryStream(bytes);

            var skyReader = new BlockStructure.Schemas
                .SchemaDocument(SSE.TESVNif.NIFReader.LoadDocument().Root)
                .Precompute(skyVersion)
                .Open(memStream);

            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int x = 0; x < 10; x++)
            {
                memStream.Seek(0, SeekOrigin.Begin);

                var header = skyReader.ReadBlock("Header") as CompoundData;
                var types = header
                    .GetCompoundList("Block Types")
                    .Select(t => CharList.ReadString(t))
                    .ToList();
                var typeIndex = header
                    .GetBasicList<short>("Block Type Index");
                var blocks = new Data[typeIndex.Count];
                for (int i = 0; i < typeIndex.Count; i++)
                {
                    var type = types[typeIndex[i]];
                    blocks[i] = skyReader.ReadBlock(type);
                }
                var footer = skyReader.ReadBlock("Footer");
            }

            sw.Stop();
            Console.WriteLine("10*={0}", sw.Elapsed);
        }
    }
}
