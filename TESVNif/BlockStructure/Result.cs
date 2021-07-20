using System;
using System.Collections.Generic;

using SSE.TESVNif.BlockStructure.Schemas;

namespace SSE.TESVNif.BlockStructure
{
    public abstract class Block { }

    public class NiObjectBlock : Block
    {
        public string Name { get; set; }
        public Dictionary<string, Block> Fields { get; set; }
    }

    public class BasicBlock : Block
    {
        public object Value { get; set; }
        public BasicBlock(object value) => Value = value;
    }

    public class CompoundBlock : Block
    {
        public Dictionary<string, Block> Fields { get; set; }
        public string Name { get; set; }
    }

    public class EnumBlock : Block
    {
        public long Value { get; set; }
        public EnumBlock(long value) => Value = value;
    }

    public class ListBlock : Block
    {
        public List<Block> Contents { get; set; }
        public ListBlock(List<Block> contents) => Contents = contents;
    }
}
