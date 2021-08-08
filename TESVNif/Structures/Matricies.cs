using System;
using BlockStructure;

namespace SSE.TESVNif.Structures
{
    public readonly struct Matrix3x3
    {
        public readonly float[] Elements;

        public Matrix3x3(CompoundData data)
        {
            Elements = new float[]
            {
                data.GetBasic<float>("m11"),
                data.GetBasic<float>("m21"),
                data.GetBasic<float>("m31"),
                data.GetBasic<float>("m12"),
                data.GetBasic<float>("m22"),
                data.GetBasic<float>("m32"),
                data.GetBasic<float>("m13"),
                data.GetBasic<float>("m23"),
                data.GetBasic<float>("m33"),
            };
        }
    }
}
