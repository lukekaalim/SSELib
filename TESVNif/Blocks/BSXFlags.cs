using System;
using SSE.TESVNif.Structures;
using SSE.TESVNif.BlockStructure;

namespace SSE.TESVNif.Blocks
{
    public class BSXFlags : NiIntegerExtraData
    {
        public BSXFlags(NIFReader.NIFFile file, BlockData data) : base(file, data)
        {
        }
    }
}
