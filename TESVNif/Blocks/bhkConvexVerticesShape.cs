using System;
using SSE.TESVNif.Structures;
using SSE.TESVNif.BlockStructure;

namespace SSE.TESVNif.Blocks
{
    public class bhkConvexVerticesShape : bhkConvexShape
    {
        public bhkConvexVerticesShape(NIFReader.NIFFile file, BlockStructure.BlockData data) : base(file, data)
        {
        }
    }
}
