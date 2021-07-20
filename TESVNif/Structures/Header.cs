using System;
using System.Collections.Generic;
using System.Linq;

using SSE.TESVNif.BlockStructure;
using SSE.TESVNif.BlockStructure.Logic;

namespace SSE.TESVNif.Structures
{
    public class BSHeader
    {
        public uint BSVersion { get; set; }
        public string Author { get; set; }
        public string ProcessScript { get; set; }
        public string ExportScript { get; set; }

        public BSHeader(CompoundData data)
        {
            BSVersion = data.GetBasic<uint>("BS Version");
            Author = CharList.ReadString(data.GetCompound("Author")).Trim();
            ProcessScript = CharList.ReadString(data.GetCompound("Process Script")).Trim();
            ExportScript = CharList.ReadString(data.GetCompound("Export Script")).Trim();
        }
    }

    public class Header
    {
        public struct BlockDescription
        {
            public string Type { get; set; }
            public uint Size { get; set; }
        }

        public string HeaderString { get; set; }
        public int Version { get; set; }
        public uint UserVersion { get; set; }

        public BSHeader BSHeader { get; set; }

        public List<BlockDescription> Blocks { get; set; }
        public List<string> Strings { get; set; }
        public List<uint> Groups { get; set; }

        public Header(CompoundData data)
        {
            HeaderString = data.GetBasic<string>("Header String").Trim();
            Version = data.GetBasic<int>("Version");
            UserVersion = data.GetBasic<uint>("User Version");

            BSHeader = new BSHeader(data.GetCompound("BS Header"));

            var types = data.GetCompoundList("Block Types")
                .Select(CharList.ReadString)
                .ToArray();
            var blockTypes = data.GetBasicList<short>("Block Type Index");
            var blockSizes = data.GetBasicList<uint>("Block Size");

            Blocks = blockTypes.Select(i => new BlockDescription()
            {
                Type = types[i],
                Size = blockSizes[i]
            }).ToList();
            Strings = data.GetCompoundList("Strings")
                .Select(CharList.ReadString)
                .ToList();
            Groups = data.GetBasicList<uint>("Groups");
        }

        public Dictionary<string, Value> Globals =>
            new Dictionary<string, Value>()
            {
                { "Version", Value.From(Version) },
                { "User Version", Value.From(UserVersion) },
                { "BS Header", new StructureValue(new Dictionary<string, Value>() {
                    { "BS Version", Value.From(BSHeader.BSVersion) }
                }) },
            };
    }
}
