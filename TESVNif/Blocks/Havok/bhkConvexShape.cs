using System;
using SSE.TESVNif.Structures;


namespace SSE.TESVNif.Blocks.Havok
{
    public class bhkConvexShape : bhkSphereRepShape
    {
        public bhkConvexShape(NIFFile file, BlockStructure.NiObjectData data) : base(file, data)
        {
        }
    }
}
