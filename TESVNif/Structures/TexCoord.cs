using System;
using BlockStructure;

namespace SSE.TESVNif.Structures
{
    public readonly struct TexCoord
    {
        public readonly float U;
        public readonly float V;

        public TexCoord(CompoundData data)
        {
            U = data.GetBasic<float>("u");
            V = data.GetBasic<float>("v");
        }
    }
    public readonly struct HalfTexCoord
    {
        public readonly Half U;
        public readonly Half V;

        public HalfTexCoord(CompoundData data)
        {
            U = data.GetBasic<Half>("u");
            V = data.GetBasic<Half>("v");
        }
    }
}
