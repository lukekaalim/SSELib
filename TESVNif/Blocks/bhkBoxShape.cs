using System;
namespace SSE.TESVNif.Blocks
{
    public class bhkBoxShape : bhkConvexShape
    {
        public bhkBoxShape(NIFReader.NIFFile file, BlockStructure.BlockData data) : base(file, data)
        {
        }
    }
}
