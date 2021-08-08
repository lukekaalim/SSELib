using System;
using System.Collections.Generic;
using System.Linq;

using BlockStructure;
using BlockStructure.Logic;

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

    public abstract class BlockReadStrategy
    {
        public VersionKey Version { get; set; }

        public class TypeIndexed : BlockReadStrategy
        {
            public struct BlockDescription
            {
                public string Type { get; set; }
                public uint Size { get; set; }

                public BlockDescription(Header header, int index)
                {
                    var typeIndex = header.BlockTypeIndexByBlockIndex[index];
                    Type = header.BlockTypes[typeIndex];
                    Size = header.BlockSizes[index];
                }
            }

            public List<BlockDescription> BlockDescriptions { get; set; }

            public TypeIndexed(Header header)
            {
                BlockDescriptions = Enumerable.Range(0, (int)header.BlockCount)
                    .Select(index => new BlockDescription(header, index))
                    .ToList();
                Version = header.VersionKey;
            }
        }

        public class TypePrefixed : BlockReadStrategy
        {
            public uint BlockCount { get; set; }

            public TypePrefixed(Header header)
            {
                BlockCount = (uint)header.BlockCount;
                Version = header.VersionKey;
            }
        }
    }

    public class Header
    {
        public string HeaderString { get; set; }
        public int Version { get; set; }
        public uint? UserVersion { get; set; }

        public BSHeader BSHeader { get; set; }

        public uint? BlockCount { get; set; }
        public List<string> BlockTypes { get; set; }
        public List<uint> BlockSizes { get; set; }
        public List<short> BlockTypeIndexByBlockIndex { get; set; }

        public List<string> Strings { get; set; }
        public List<uint> Groups { get; set; }

        public VersionKey VersionKey => new VersionKey(Version, BSHeader?.BSVersion, UserVersion);

        public Header(CompoundData data)
        {
            HeaderString = data.GetBasic<string>("Header String").Trim();
            Version = data.GetBasic<int>("Version");
            UserVersion = data.TryGetBasic<uint?>("User Version", null);

            if (data.Fields.ContainsKey("BS Header"))
                BSHeader = new BSHeader(data.GetCompound("BS Header"));

            BlockCount = data.TryGetBasic<uint?>("Num Blocks", null);
            BlockTypes = data.TryGetBasicList<string>("Block Types", null);
            BlockTypeIndexByBlockIndex = data.TryGetBasicList<short>("Block Type Index", null);
            BlockSizes = data.TryGetBasicList<uint>("Block Size", null);

            Strings = data.TryGetBasicList<string>("Strings", null);
            Groups = data.TryGetBasicList<uint>("Groups", null);
        }

        public Dictionary<string, Value> BuildGlobals()
        {
            var globals = new Dictionary<string, Value>();
            globals.Add("Version", Value.From(Version));
            if (UserVersion != null)
                globals.Add("User Version", Value.From((uint)UserVersion));
            if (BSHeader != null)
                globals.Add("BS Header", new StructureValue(new Dictionary<string, Value>() {
                    { "BS Version", Value.From(BSHeader.BSVersion) }
                }));

            return globals;
        }

        public BlockReadStrategy GetBlockStrategy()
        {
            if (BlockTypeIndexByBlockIndex != null)
                return new BlockReadStrategy.TypeIndexed(this);
            if (BlockCount != null)
                return new BlockReadStrategy.TypePrefixed(this);

            throw new NotImplementedException();
        }
    }
}
