using System;
using SSE.TESVNif.Structures;
using SSE.TESVNif.BlockStructure;

namespace SSE.TESVNif.Blocks
{
    public class NiIntegerExtraData : NiExtraData
    {
        public NiIntegerExtraData(NIFReader.NIFFile file, BlockData data) : base(file, data)
        {
        }
    }
}
