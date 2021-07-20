using System;
using SSE.TESVNif.Structures;
using SSE.TESVNif.BlockStructure;

namespace SSE.TESVNif.Blocks
{
    public class bhkConvexShape : bhkSphereRepShape
    {
        public bhkConvexShape(NIFReader.NIFFile file, BlockStructure.BlockData data) : base(file, data)
        {
        }
    }
}
