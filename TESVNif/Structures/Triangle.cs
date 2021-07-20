using System;
using SSE.TESVNif.BlockStructure;

namespace SSE.TESVNif.Structures
{
    public struct Triangle
    {
        public ushort V1 { get; set; }
        public ushort V2 { get; set; }
        public ushort V3 { get; set; }

        public Triangle(CompoundData data)
        {
            V1 = data.GetBasic<ushort>("v1");
            V2 = data.GetBasic<ushort>("v2");
            V3 = data.GetBasic<ushort>("v3");
        }
    }
}
